using UnityEngine;

public class EffectController : MonoBehaviour
{
    [Header("Še²‚Ì‰ñ“]‚ğŒÅ’è‚·‚é‚©‚Ç‚¤‚©")]
    public bool freezeX = true;
    public bool freezeY = true;
    public bool freezeZ = true;

    void Update()
    {
        // Œ»İ‚Ì‰ñ“]‚ğæ“¾
        Vector3 currentRotation = transform.rotation.eulerAngles;

        // Še²‚ÌŒÅ’èˆ—
        float x = freezeX ? 0f : currentRotation.x;
        float y = freezeY ? 0f : currentRotation.y;
        float z = freezeZ ? 0f : currentRotation.z;

        // ‰ñ“]‚ğ”½‰f
        transform.rotation = Quaternion.Euler(x, y, z);
    }
}
