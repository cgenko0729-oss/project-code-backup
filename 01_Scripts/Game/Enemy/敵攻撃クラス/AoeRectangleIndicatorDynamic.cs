using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class AoeRectangleIndicatorDynamic : MonoBehaviour
{
    // ... (Your existing variables are fine) ...
    [SerializeField] private float windUpSeconds = 2f;
    [SerializeField] private float lingerSeconds = 0.15f;
    [SerializeField] private Vector2 size = new (6f, 3f);
    [SerializeField] private int damage = 30;
    [SerializeField] private LayerMask targetLayers = default;
    [SerializeField] private bool leftToRight = true;
    [SerializeField] private Transform innerRectTran;
    [SerializeField] private Transform outerRectTran;

    static readonly Collider[] buf = new Collider[16];
    Material innerMat;
    float timer;
    private bool isFilling = false; // Control when the filling starts

    void Awake()
    {
        innerMat = innerRectTran.GetComponent<MeshRenderer>().material;
        SetProgress(0f);
        Vector3 newPos = transform.position;
        newPos.y = 0.03f;
        transform.position = newPos;
    }

    void OnEnable()
    {
        isFilling = false; // Reset on enable
        outerRectTran.localScale = new Vector3(size.x, 1f, size.y);
        outerRectTran.localPosition = Vector3.zero;
        ApplyFill(0f);
        SetProgress(0f);
    }

    void Update()
    {
        // Only run the timer logic if the filling has been triggered
        if (!isFilling) return;

        timer -= Time.deltaTime;

        if (timer > lingerSeconds) // Winding-up
        {
            float p = 1f - (timer - lingerSeconds) / windUpSeconds; // 0¨1
            ApplyFill(p);
            SetProgress(p);
        }
        else if (timer + Time.deltaTime >= lingerSeconds && timer <= lingerSeconds) // Impact frame
        {
            Hit();
            ApplyFill(1f);
            SetProgress(1f);
        }
        else if (timer <= 0f) // Finished
        {
            gameObject.SetActive(false);
        }
    }
    
    // --- NEW AND MODIFIED PUBLIC METHODS ---

    /// <summary>
    /// Call this continuously during the tracking phase to update the indicator's position, direction, and length.
    /// </summary>
    public void UpdateIndicatorTransform(Vector3 bossPosition, Vector3 direction, float length)
    {
        // Set the indicator's length (size.y)
        size.y = length;
        // Position the indicator halfway between the boss and the target point
        transform.position = bossPosition + direction * (length * 0.5f);
        // Rotate the indicator to face the direction
        transform.rotation = Quaternion.LookRotation(direction);
        // Adjust the visual scale of the outer frame
        outerRectTran.localScale = new Vector3(size.x, 1f, size.y);
    }

    /// <summary>
    /// Initializes the core properties of the rectangle.
    /// </summary>
    public void InitRect(Vector2 sizeXZ, float windUpSec, float lingerSec, bool ltr = true)
    {
        size = sizeXZ;
        windUpSeconds = windUpSec;
        lingerSeconds = lingerSec;
        leftToRight = ltr;

        // Reset visuals
        outerRectTran.localScale = new Vector3(size.x, 1f, size.y);
        outerRectTran.localPosition = Vector3.zero;
        ApplyFill(0f);
        SetProgress(0f);
    }
    
    /// <summary>
    /// Sets the damage value right before the fill starts.
    /// </summary>
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    /// <summary>
    /// Kicks off the filling animation and timer.
    /// </summary>
    public void StartFilling()
    {
        isFilling = true;
        timer = windUpSeconds + lingerSeconds;
    }

    // ... (The rest of your helper methods like SetProgress, ApplyFill, Hit remain the same) ...
    void SetProgress(float p) => innerMat.SetFloat("_Progress", p);

    void ApplyFill(float p)
    {
        float w = size.x * p;
        innerRectTran.localScale = new Vector3(w, 1f, size.y);
        float offset = (size.x - w) * 0.5f;
        innerRectTran.localPosition = new Vector3(leftToRight ? -offset : offset, 0f, 0f);
    }

    void Hit()
    {
        Vector3 centre = transform.position + Vector3.up * 0.1f;
        Vector3 half = new Vector3(size.x * 0.5f, 0.5f, size.y * 0.5f);
        int n = Physics.OverlapBoxNonAlloc(centre, half, buf, transform.rotation, targetLayers);
        for (int i = 0; i < n; ++i)
            buf[i].GetComponent<IDamageable>()?.TakeDamage(damage);
    }


}

