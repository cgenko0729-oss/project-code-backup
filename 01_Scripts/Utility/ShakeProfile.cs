using UnityEngine;

[CreateAssetMenu(fileName = "NewShakeProfile", menuName = "Camera/Shake Profile")]
public class ShakeProfile : ScriptableObject
{
    [Tooltip("How long the shake should last, in seconds.")]
    public float duration = 0.5f;

    [Tooltip("The speed and roughness of the shake motion.")]
    public float frequency = 15.0f;

    [Header("Positional Shake")]
    [Tooltip("The maximum distance the camera can be moved from its origin.")]
    public float positionalAmplitude = 0.25f;

    [Header("Rotational Shake")]
    [Tooltip("The maximum angle the camera can be rotated, in degrees.")]
    public float rotationalAmplitude = 1.0f;
}