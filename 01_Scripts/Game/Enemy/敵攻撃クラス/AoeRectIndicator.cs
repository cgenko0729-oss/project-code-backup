using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


[RequireComponent(typeof(MeshRenderer))]

public class AoeRectIndicator : MonoBehaviour
{
    [SerializeField] private float windUpSeconds = 2f;
    [SerializeField] private float lingerSeconds = 0.15f;

    [Header("Area (X = width, Z = length)")]
    [SerializeField] private Vector2 size = new(6f, 3f);

    [SerializeField] private int damage = 30;
    [SerializeField] private LayerMask targetLayers = default;

    [SerializeField] private bool leftToRight = true; //dir

    [SerializeField] private Transform innerRectTran;
    [SerializeField] private Transform outerRectTran;

    static readonly Collider[] buf = new Collider[16];
    Material innerMat;
    float timer;
    private bool isFilling = false; 

    void Awake()
    {
        innerMat = innerRectTran.GetComponent<MeshRenderer>().material;
    }

    void OnEnable()
    {
        
        //stop the timer logic from running until StartFilling() is explicitly called.
        isFilling = false;
    }

    void Update()
    {
       
        if (!isFilling) return;  

        timer -= Time.deltaTime;

        if (timer > lingerSeconds)           // Winding-up
        {
            float p = 1f - (timer - lingerSeconds) / windUpSeconds; // 0→1
            ApplyFill(p);
            SetProgress(p);
        }
        else if (timer + Time.deltaTime >= lingerSeconds && timer <= lingerSeconds) // Impact frame
        {
            Hit();
            ApplyFill(1f);
            SetProgress(1f);
        }
        else if (timer <= 0f)                // Finished
        {
            gameObject.SetActive(false);
        }
    }

    void SetProgress(float p) => innerMat.SetFloat("_Progress", p);

    /// <summary>
    ///X is width and Z is length.
    /// </summary>
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
        // This hitbox logic now needs to be correct for the final rotated object.
        // Since the prefab might be sideways, we measure its world-space bounds.
        Vector3 half = new Vector3(outerRectTran.lossyScale.x * 0.5f, 0.5f, outerRectTran.lossyScale.z * 0.5f);
        int n = Physics.OverlapBoxNonAlloc(centre, half, buf, transform.rotation, targetLayers);

        for (int i = 0; i < n; ++i)
            buf[i].GetComponent<IDamageable>()?.TakeDamage(damage);
    }

    /// <summary>
    /// PHASE 1: Called continuously during tracking.
    /// Updates the indicator's size and transform, but does NOT start the animation.
    /// </summary>
    public void UpdateTransform(Vector3 position, Quaternion rotation, Vector2 newSize)
    {
        transform.position = position;
        transform.rotation = rotation;
        size = newSize;

        float w = size.x * 1;
        outerRectTran.localScale = new Vector3(w, 1f, size.y);

        ApplyFill(0); // Keep the fill at 0% during tracking
    }

    /// <summary>
    /// PHASE 2: Called once tracking is finished.
    /// Initializes all values and starts the fill animation timer.
    /// </summary>
    public void BeginFill(float windUp, float linger, int dmg, bool ltr = true)
    {
        windUpSeconds = windUp;
        lingerSeconds = linger;
        damage = dmg;
        leftToRight = ltr;

        timer = windUpSeconds + lingerSeconds;
        isFilling = true;

        outerRectTran.localPosition = Vector3.zero;
        ApplyFill(0f);
        SetProgress(0f);
    }

}

