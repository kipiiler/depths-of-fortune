using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MonsterBehavior : MonoBehaviour, IHear
{
    public enum MonsterState
    {
        Exploratory,
        Suspicious,
        Aggressive
    }

    // Constants
    static float EXPLORE_AMBIENT_SOUND = 20f;
    static float SUSPICIOUS_AMBIENT_SOUND = 8f;
    static float AGGRESSIVE_AMBIENT_SOUND = 1f;
    static float SOUND_FALL_OFF = 0.6f;
    static float MAX_SOUND_INTENSITY = 9f;
    static int MOVE_SPEED = 4;
    static float MIN_SETPOINT_DIST = 0.4f;
    static float MIN_ATTACK_DIST = 3f;
    static float EXPLORE_TO_AGGRESSIVE_SOUND_THRESHOLD = 3f;
    static float SUSPICIOUS_TO_AGGRESSIVE_SOUND_THRESHOLD = 2f;
    static int AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD = 10;

    public MonsterState CurrentState;
    LinkedList<Map.Segment> visited;
    Map.Segment lastSegment;
    public Vector3 playerPosition;
    public FirstPersonController player;

    public bool isStunned = false;
    private float stunDuration = 3f;

    private float attackCooldown;

    // Sound stuff
    Sound lastSound;
    bool newSoundFlag = false;
    float aggressiveLastSoundTimeElapsed = AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD;



    Animator anim;
    bool pathfinderIsInstantiated = false;
    Pathfinder pathfinder;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetTrigger("StartWalk");
        CurrentState = MonsterState.Exploratory;
        visited = new LinkedList<Map.Segment>();
        lastSound = new Sound(new Vector3(0, 0, 0), 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!pathfinderIsInstantiated)
        {
            pathfinder = new Pathfinder(Map.MAP_DIMENSION * Map.MODULE_WIDTH, Map.MAP_DIMENSION * Map.MODULE_WIDTH, 1000, 1000);
            pathfinderIsInstantiated = true;
        }

        if (!isStunned)
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

            Map.Segment currSegment = Map.FindSegment(transform.position);
            if (currSegment != lastSegment)
            {
                visited.AddFirst(currSegment);
                lastSegment = currSegment;
            }

            pathfinder.Update(transform.position);
            if (pathfinder.HasNextSetpoint())
            {
                transform.LookAt(pathfinder.GetNextSetpoint());
                transform.position += transform.forward * MOVE_SPEED * Time.deltaTime;
            }
        }
        else
        {
            stunDuration -= Time.deltaTime;
            if (stunDuration < 0)
            {
                stunDuration = 2.5f;
                isStunned = false;
            }
        }

        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime;

            if (attackCooldown <= 0f)
            {
                attackCooldown = 0f;
            }
        }
    }

    void Explore() {
        Map.Segment currSegment = Map.FindSegment(transform.position);
        while (!pathfinder.HasNextSetpoint()) {
            // Our new setpoint will be the adjacent segment visited the longest ago
            List<Map.Segment> adj = currSegment.Adjacent;
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
                    if (highestWeight < System.Int32.MaxValue) {
                        oldestVisited.Clear();
                        highestWeight = System.Int32.MaxValue;
                    }
                    oldestVisited.Add(s);
                }
            }
            // If there are multiple old/non-visited segments, randomly choose one
            Map.Segment newSetpoint = oldestVisited[Random.Range(0, oldestVisited.Count)];

            // Update the pathfinder
            pathfinder.UpdateEndpoint(newSetpoint.GetUnityPosition(), transform.position);

            if (!pathfinder.HasNextSetpoint())
            {
                visited.AddFirst(currSegment);
                currSegment = newSetpoint;
            }
        }

        // State transition
        if (Vector3.Distance(transform.position, playerPosition) < 2)
        {
            CurrentState = MonsterState.Aggressive;
        }
        else if (newSoundFlag)
        {
            float soundIntensity = GetMonsterRelativeSoundIntensity(lastSound);
            if (soundIntensity > EXPLORE_TO_AGGRESSIVE_SOUND_THRESHOLD)
            {
                CurrentState = MonsterState.Aggressive;
            }
            else
            {
                CurrentState = MonsterState.Suspicious;
            }
        }
    }

    void Investigate()
    {
        if (newSoundFlag)
        {
            pathfinder.UpdateEndpoint(lastSound.pos, transform.position);

            // State transition
            if (GetMonsterRelativeSoundIntensity(lastSound) > SUSPICIOUS_TO_AGGRESSIVE_SOUND_THRESHOLD)
            {
                CurrentState = MonsterState.Aggressive;
            }
            newSoundFlag = false;  // We have now acted upon the sound
        }

        if (Vector3.Distance(transform.position, playerPosition) < 2)
        {
            CurrentState = MonsterState.Aggressive;
        }
        else if (!pathfinder.HasNextSetpoint())
        {
            // State transition to explore
            CurrentState = MonsterState.Exploratory;
        }
    }

    void Attack()
    {
        // Acknowledge new sounds
        if (newSoundFlag) newSoundFlag = false;

        if (Vector3.Distance(transform.position, playerPosition) < MIN_ATTACK_DIST)
        {
            if (attackCooldown <= 0f)
            {
                // Monster will attack the player
                anim.SetTrigger("Attack");
                player.Attacked();

                // Set attack cooldown
                attackCooldown = 5f;
            }
        }
        else
        {
            pathfinder.UpdateEndpoint(playerPosition, transform.position);
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

    public void Stunned()
    {
        isStunned = true;
        anim.SetTrigger("Stunned");
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
            lastSound = sound;
            newSoundFlag = true;

            // If sound heard, reset timer
            if (CurrentState == MonsterState.Aggressive) aggressiveLastSoundTimeElapsed = AGGRESSIVE_TO_SUSPICIOUS_TIME_THRESHOLD;
        }
    }

    public class Pathfinder
    {
        private int[,] grid;

        private static int CAST_Y = 100;
        
        private Vector3 endpoint;

        private Stack<Vector3> setpoints;

        public float xUnit;
        public float zUnit;

        public Pathfinder(float width, float height, int xSubdivisions, int zSubdivisions)
        {
            xUnit = width / xSubdivisions;
            zUnit = height / zSubdivisions;
            grid = new int[xSubdivisions, zSubdivisions];

            // Raycast
            for (int i = 0; i < xSubdivisions; i++)
            {
                for (int j = 0; j < zSubdivisions; j++)
                {
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(
                        new Vector3(
                            xUnit * (j + 0.5f),
                            CAST_Y,
                            zUnit * (i + 0.5f)),
                        -Vector3.up);

                    if (hits.GetLength(0) == 0)
                    {
                        grid[j, i] = 1;
                    }
                    else
                    {
                        bool hitFloor = false;
                        bool hitObstacle = false;
                        foreach (RaycastHit hit in hits)
                        {
                            hitFloor |= hit.transform.gameObject.CompareTag("Floor");
                            hitObstacle |= hit.transform.gameObject.CompareTag("Obstacle");
                        }

                        grid[j, i] = hitFloor && !hitObstacle ? 0 : 1;
                    }
                }
            }

            // Instantiate
            setpoints = new Stack<Vector3>();
        }

        public void UpdateEndpoint(Vector3 newEndpoint, Vector3 position)
        {
            // IF the new endpoint is far from the old endpoint, recompute the setpoints
            if (Vector3.Distance(endpoint, newEndpoint) > 1) // TODO: Change this!
            {
                (int x, int y) src = WorldToGrid(position.x, position.z);
                (int x, int y) dst = WorldToGrid(newEndpoint.x, newEndpoint.z);

                bool[,] visited = new bool[grid.GetLength(0), grid.GetLength(1)];
                (int x, int y)[,] prev = new (int x, int y)[grid.GetLength(0), grid.GetLength(1)];
                Queue<(int x, int y)> queue = new Queue<(int x, int y)>();

                queue.Enqueue(src);
                visited[(int) src.x, (int) src.y] = true;
                
                int[] dr = {0, 0, 1, -1};
                int[] dc = {1, -1, 0, 0};
                bool foundPath = false;

                while (queue.Count > 0)
                {
                    (int x, int y) curr = queue.Dequeue();

                    if (curr == dst)
                    {
                        setpoints.Clear();
                        int count = 0;
                        while (curr != src)
                        {
                            Vector2 prevWorld = GridToWorld(curr.x, curr.y);
                            setpoints.Push(new Vector3(prevWorld.x, position.y, prevWorld.y));
                            count++;
                            curr = prev[curr.x, curr.y];
                        }
                        foundPath = true;
                        break;
                    }

                    for (int k = 0; k < 4; k++) {
                        int ni = (int) curr.x + dr[k];
                        int nj = (int) curr.y + dc[k];
                        
                        // Check if the new cell (ni, nj) is within the grid boundaries
                        if (ni >= 0 && ni < grid.GetLength(1) &&
                            nj >= 0 && nj < grid.GetLength(0) &&
                            !visited[ni, nj]) {
                            if (grid[nj, ni] == 0)
                            {
                                queue.Enqueue((ni, nj));
                                prev[ni, nj] = curr;
                                visited[ni, nj] = true;
                            }
                        }
                    }
                }

                endpoint = newEndpoint;
            }
        }
        
        public void Update(Vector3 position)
        {
            // IF the monster is close to the current setpoint, pop it and get the next one.
            while (setpoints.Count > 0 &&
                    Vector3.Distance(setpoints.Peek(), position) < 0.2)
            {
                setpoints.Pop();
            }
        }

        public bool HasNextSetpoint()
        {
            return setpoints.Count > 0;
        }

        public Vector3 GetNextSetpoint()
        {
            return setpoints.Peek();
        }

        public Vector2 GridToWorld(int i, int j) {
            return new Vector2(xUnit * (j + 0.5f),
                               zUnit * (i + 0.5f));
        }

        public (int, int) WorldToGrid(float x, float z) {
            (int x, int y) gridCoords = ((int) Mathf.Round((z / zUnit) - 0.5f),
                                         (int) Mathf.Round((x / xUnit) - 0.5f));
            
            // Get next best
            if (grid[gridCoords.y, gridCoords.x] == 1)
            {
                // Go up
                if (gridCoords.y - 1 >= 0 &&
                    grid[gridCoords.y - 1, gridCoords.x] == 0)
                {
                    return (gridCoords.x, gridCoords.y - 1);
                }

                // Go down
                if (gridCoords.y + 1 < grid.GetLength(0) &&
                    grid[gridCoords.y + 1, gridCoords.x] == 0)
                {
                    return (gridCoords.x, gridCoords.y + 1);
                }

                // Go left
                if (gridCoords.x - 1 >= 0 &&
                    grid[gridCoords.y, gridCoords.x - 1] == 0)
                {
                    return (gridCoords.x - 1, gridCoords.y);
                }

                // Go right
                if (gridCoords.y + 1 < grid.GetLength(1) &&
                    grid[gridCoords.y, gridCoords.x + 1] == 0)
                {
                    return (gridCoords.x + 1, gridCoords.y);
                }
            }
            
            return gridCoords;
        }
    }
}
