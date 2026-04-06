using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(MeshRenderer))]
public class AoeRectIndicatorLine : MonoBehaviour
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
        float w = size.y * p;
        innerRectTran.localScale = new Vector3(w, size.x, 1);

        float offset = (size.y - w) * 0.5f;
        innerRectTran.localPosition = new Vector3(leftToRight ? -offset : offset, 0f, 0f);
    }

    public void UpdateTransform(Vector3 position, Quaternion rotation, Vector2 newSize)
    {
        transform.position = position;
        transform.rotation = rotation;
        size = newSize;

        float w = size.x * 1;
        outerRectTran.localScale = new Vector3(size.y, size.x, 1);

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

    void Hit()
    {
        Vector3 centre = transform.position + Vector3.up * 0.1f;
        // This hitbox logic now needs to be correct for the final rotated object.
        // Since the prefab might be sideways, we measure its world-space bounds.
        Vector3 half = new Vector3(outerRectTran.lossyScale.x * 0.5f, 0.5f, outerRectTran.lossyScale.y * 0.5f);
        int n = Physics.OverlapBoxNonAlloc(centre, half, buf, transform.rotation, targetLayers);

        for (int i = 0; i < n; ++i)buf[i].GetComponent<PlayerState>()?.ApplyHpChange(-20);

        //DrawWireBox(centre, half, transform.rotation, Color.yellow, 0.25f);

    }

    //static void DrawWireBox(Vector3 centre, Vector3 half, Quaternion rot,Color colour, float duration) //Enable Editor's Gizmos to see this in the Scene view
    //{
    //    Vector3[] corners = new Vector3[8];
    //    Vector3 ext = half;
    
    //    // Local-space corners
    //    corners[0] = new Vector3( ext.x,  ext.y,  ext.z);
    //    corners[1] = new Vector3(-ext.x,  ext.y,  ext.z);
    //    corners[2] = new Vector3(-ext.x,  ext.y, -ext.z);
    //    corners[3] = new Vector3( ext.x,  ext.y, -ext.z);
    
    //    corners[4] = new Vector3( ext.x, -ext.y,  ext.z);
    //    corners[5] = new Vector3(-ext.x, -ext.y,  ext.z);
    //    corners[6] = new Vector3(-ext.x, -ext.y, -ext.z);
    //    corners[7] = new Vector3( ext.x, -ext.y, -ext.z);
    
    //    // Transform to world space once, then draw
    //    for (int i = 0; i < corners.Length; i++)
    //        corners[i] = centre + rot * corners[i];
    
    //    // 12 edges (top square, bottom square, 4 sides)
    //    int[,] edges = {
    //        {0,1},{1,2},{2,3},{3,0},
    //        {4,5},{5,6},{6,7},{7,4},
    //        {0,4},{1,5},{2,6},{3,7}
    //    };
    
    //    for (int e = 0; e < edges.GetLength(0); e++)
    //    {
    //        Debug.DrawLine(
    //            corners[edges[e,0]],
    //            corners[edges[e,1]],
    //            colour, duration);
    //    }
    //}

    //void OnDrawGizmos()
    //{
    //    // Same maths you use in Hit()
    //    Vector3 centre = transform.position + Vector3.up * 0.1f;
    //    Vector3 half   = new Vector3(
    //        outerRectTran.lossyScale.x * 0.5f,
    //        0.5f,
    //        outerRectTran.lossyScale.z * 0.5f);

    //    // Save current matrix so we can restore it afterwards
    //    Matrix4x4 old = Gizmos.matrix;

    //    // Make the cube obey the AOE's rotation
    //    Gizmos.matrix = Matrix4x4.TRS(centre, transform.rotation, Vector3.one);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(Vector3.zero, half * 2f);   // half*2 == full size

    //    Gizmos.matrix = old;
    //}

    /// <summary>
    /// PHASE 1: Called continuously during tracking.
    /// Updates the indicator's size and transform, but does NOT start the animation.
    /// </summary>
    

    

}

