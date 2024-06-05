using UnityEngine;

// This class is an imaginary sound.
public class Sound
{

    public Sound(Vector3 _pos, float _intensity)
    {

        pos = _pos;

        intensity = _intensity;
    }

    public readonly Vector3 pos;

    /// <summary>
    /// This the intensity of the sound.
    /// </summary>
    public readonly float intensity;
}
