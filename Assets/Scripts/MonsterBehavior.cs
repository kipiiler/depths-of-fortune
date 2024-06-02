using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour, IHear
{
    public enum MonsterState
    {
        Exploratory,
        Suspicious,
        Aggressive
    }

    // Constants
    static int MAX_SEGMENT_WEIGHT = 2147483647;
    static float EXPLORE_AMBIENT_SOUND = 20f;
    static float SUSPICIOUS_AMBIENT_SOUND = 8f;
    static float AGGRESSIVE_AMBIENT_SOUND = 1f;
    static float SOUND_FALL_OFF = 0.6f;
    static float MAX_SOUND_INTENSITY = 9f;
    static int MOVE_SPEED = 4;
    static float MIN_SETPOINT_DIST = 0.4f;
    static float MIN_ATTACK_DIST = 1f;
    static float EXPLORE_TO_AGGRESSIVE_SOUND_THRESHOLD = 3f;
    static float SUSPICIOUS_TO_AGGRESSIVE_SOUND_THRESHOLD = 2f;
    static int AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD = 10;

    public MonsterState CurrentState;
    LinkedList<Map.Segment> visited;
    Stack<Vector3> setpoints;
    public Vector3 playerPosition;
    public Map.Segment lastPlayerSegment;


    // Sound stuff
    Sound lastSound;
    bool newSoundFlag = false;
    float aggressiveLastSoundTimeElapsed = AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD;


    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetTrigger("StartWalk");
        CurrentState = MonsterState.Exploratory;
        visited = new LinkedList<Map.Segment>();
        setpoints = new Stack<Vector3>();
        setpoints.Push(transform.position);
        lastSound = new Sound(new Vector3(0, 0, 0), 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Process the state
        switch (CurrentState)
        {
            case MonsterState.Exploratory:
                Explore();
                break;

            case MonsterState.Suspicious:
                Investigate();
                break;
            case MonsterState.Aggressive:
                Attack();
                break;
        }

        if (setpoints.Count > 0)
        {
            transform.LookAt(setpoints.Peek());
            transform.position += transform.forward * MOVE_SPEED * Time.deltaTime;
        }
    }

    void Explore() {
        if (setpoints.Count > 0 &&
            Vector3.Distance(transform.position, setpoints.Peek()) < MIN_SETPOINT_DIST) {
            // Remove the old setpoint
            setpoints.Pop();

            // Our new setpoint will be the adjacent segment visited the longest ago
            List<Map.Segment> adj = Map.FindSegment(transform.position).Adjacent;
            List<Map.Segment> oldestVisited = new List<Map.Segment>();
            int highestWeight = -1;
            foreach (Map.Segment s in adj) 
            {
                // Search through the visited LinkedList, weighting later nodes higher
                var count = 0;
                var found = false;
                for (var node = visited.First; node != null; node = node.Next, count++)
                {
                    if (s.Equals(node.Value))
                    {
                        found = true;
                        if (count > highestWeight) {
                            oldestVisited.Clear();
                            oldestVisited.Add(s);
                            highestWeight = count;
                        } else if (count == highestWeight) {
                            oldestVisited.Add(s);
                        }
                        break;
                    }
                }

                // If a segment has not been visited, it is weighted the max
                if (!found) {
                    if (highestWeight < MAX_SEGMENT_WEIGHT) {
                        oldestVisited.Clear();
                        highestWeight = MAX_SEGMENT_WEIGHT;
                    }
                    oldestVisited.Add(s);
                }
            }
            // If there are multiple old/non-visited segments, randomly choose one
            Map.Segment newSetpoint = oldestVisited[Random.Range(0, oldestVisited.Count)];

            // Add this segment to visited
            visited.AddFirst(newSetpoint);

            // Add new setpoint
            setpoints.Push(newSetpoint.GetUnityPosition());
        }

        // State transition
        if (newSoundFlag)
        {
            float soundIntensity = GetMonsterRelativeSoundIntensity(lastSound);
            if (soundIntensity > EXPLORE_TO_AGGRESSIVE_SOUND_THRESHOLD)
            {
                CurrentState = MonsterState.Aggressive;
                Debug.Log("Monster is now aggressive....");
            }
            else
            {
                CurrentState = MonsterState.Suspicious;
                Debug.Log("Monster is now suspicious....");
            }
        }
    }

    void Investigate()
    {
        if (newSoundFlag)
        {
            Stack<Map.Segment> reverse = new Stack<Map.Segment>(
                Map.FindPath(Map.FindSegment(transform.position), Map.FindSegment(lastSound.pos))
            );
            setpoints.Clear();
            while (reverse.Count > 0)
            {
                setpoints.Push(reverse.Pop().GetUnityPosition());
            }
            newSoundFlag = false;  // We have now acted upon the sound
        }

        // Check if we're in the segment already
        if (Map.FindSegment(transform.position) == Map.FindSegment(lastSound.pos))
        {
            if (Vector3.Distance(transform.position, lastSound.pos) < MIN_SETPOINT_DIST)
            {
                // State transition to explore
                CurrentState = MonsterState.Exploratory;
                Debug.Log("Monster is now exploratory...");
                setpoints.Push(transform.position);
            }
            else
            {
                setpoints.Clear();
                setpoints.Push(lastSound.pos);
            }
        }
        else if (setpoints.Count > 0 &&
                 Vector3.Distance(transform.position, setpoints.Peek()) < MIN_SETPOINT_DIST)
        {
            visited.AddFirst(Map.FindSegment(setpoints.Pop()));
        }

        // State transition
        if (GetMonsterRelativeSoundIntensity(lastSound) > SUSPICIOUS_TO_AGGRESSIVE_SOUND_THRESHOLD)
        {
            CurrentState = MonsterState.Aggressive;
            Debug.Log("Monster is now aggressive...");
        }
    }

    void Attack()
    {
        // Acknowledge new sounds
        if (newSoundFlag) newSoundFlag = false;

        if (Vector3.Distance(transform.position, playerPosition) < MIN_ATTACK_DIST)
        {
            // Monster will attack the player
            Debug.Log("Attack!");
        }
        else
        {
            if (Map.FindSegment(playerPosition) != Map.FindSegment(transform.position))
            {
                // Continue traveling to the player
                Map.Segment currentPlayerSegment = Map.FindSegment(playerPosition);
                if (currentPlayerSegment != lastPlayerSegment)
                {
                    Debug.Log("Updating path...");
                    // Update the path
                    Stack<Map.Segment> reverse = new Stack<Map.Segment>(
                        Map.FindPath(Map.FindSegment(transform.position), Map.FindSegment(playerPosition))
                    );
                    setpoints.Clear();
                    while (reverse.Count > 0)
                    {
                        setpoints.Push(reverse.Pop().GetUnityPosition());
                    }
                    lastPlayerSegment = currentPlayerSegment;
                }

                // If we've reached setpoint, pop it from setpoints
                if (setpoints.Count > 0 &&
                    Vector3.Distance(transform.position, setpoints.Peek()) < MIN_SETPOINT_DIST)
                {
                    Debug.Log("Reached a setpoint");
                    visited.AddFirst(Map.FindSegment(setpoints.Pop()));
                }
            }
            else
            {
                // Go directly towards the player
                setpoints.Clear();
                setpoints.Push(playerPosition);
            }
        }

        // Decrememt aggressive-to-suspicious timer
        aggressiveLastSoundTimeElapsed -= Time.deltaTime;

        // State transition
        if (aggressiveLastSoundTimeElapsed < 0)
        {
            aggressiveLastSoundTimeElapsed = AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD;
            CurrentState = MonsterState.Suspicious;
        }
    }

    float GetMonsterRelativeSoundIntensity(Sound sound)
    {
        if (Vector3.Distance(transform.position, lastSound.pos) == 0) return MAX_SOUND_INTENSITY;

        return Mathf.Min(
            Mathf.Max(sound.intensity / Mathf.Pow(Vector3.Distance(transform.position, sound.pos), SOUND_FALL_OFF), 0),
            MAX_SOUND_INTENSITY);
    }

    public void RespondToSound(Sound sound)
    {
        // Decide if sound is acknowledged by the monster
        float soundIntensity = GetMonsterRelativeSoundIntensity(sound);
        float ambientSound = 0f;
        switch (CurrentState)
        {
            case MonsterState.Exploratory:
                ambientSound = EXPLORE_AMBIENT_SOUND;
                break;
            case MonsterState.Suspicious:
                ambientSound = SUSPICIOUS_AMBIENT_SOUND;
                break;
            case MonsterState.Aggressive:
                ambientSound = AGGRESSIVE_AMBIENT_SOUND;
                break;
        }
        float soundCoeff = soundIntensity / (ambientSound + soundIntensity);
        if (Random.value < soundCoeff)
        {
            // Sound is acknowledged by the monster, setpoint must change
            Debug.Log("Acknowledged a sound of intensity " + soundIntensity + " that was " + Vector3.Distance(transform.position, sound.pos) + " away");
            lastSound = sound;
            newSoundFlag = true;

            // If sound heard, reset timer
            if (CurrentState == MonsterState.Aggressive) aggressiveLastSoundTimeElapsed = AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD;
        }
    }
}
