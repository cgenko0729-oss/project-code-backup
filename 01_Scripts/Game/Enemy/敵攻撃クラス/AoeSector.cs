using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]

public class AoeSector : MonoBehaviour
{

     [Range(0,1)] public float progress;

    [Range(0,360)] public float sectorAngle = 90f;
    public float radius = 3f;
    [Range(3,64)] public int segments = 32;

    public bool isInnerSector = false;
    public float fillDuraiton = 1.5f;

    public bool isDamagePlayer = false;
    public float aoeDamage = 15f;

    void Awake() => BuildMesh();

    void OnValidate()
    {
        if (segments < 3) segments = 3;
        BuildMesh();
    }

    private void Start()
    {
        if (!isInnerSector) progress = 1;
        else progress = 0;
    }

    void Update()
    {
        // if your wedge extends along its local +Z axis:
        transform.localScale = new Vector3(progress, 1, progress);

        if (isInnerSector)
        {
            //increase progress from 0 to 1 in fillDuration seconds
            if (progress < 1)
            {
                progress += Time.deltaTime / fillDuraiton;
                if (progress > 1)
                {
                    progress = 1;

                    if(isDamagePlayer)CheckHitWithPlayer();
                }
            }

        }

    }

    public void CheckHitWithPlayer()
    {
        // Check if the player is within the sector
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                EventManager.EmitEventData("ChangePlayerHp", -aoeDamage);
            }
        }

    }

    void BuildMesh()
    {
        Mesh m = new Mesh();
        var verts = new List<Vector3>();
        var tris  = new List<int>();

        // center
        verts.Add(Vector3.zero);

        // angle step
        float half = sectorAngle * 0.5f;
        float start = -half;
        float step = sectorAngle / segments;

        // rim verts
        for (int i = 0; i <= segments; i++)
        {
            float ang = start + step * i;
            float rad = Mathf.Deg2Rad * ang;
            verts.Add(new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius);
        }

        // triangles (fan)
        for (int i = 1; i <= segments; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i+1);
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = m;
    }
}

