using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class ItemBoxRandomSpawner : Singleton<ItemBoxRandomSpawner>
{
    [Header("Map bounds (XZ)")]
    public float mapLeftBoundX = -56;
    public float mapRightBoundX = 56;
    public float mapUpperBoundZ = 56;
    public float mapLowerBoundZ = -56;
    public float boundsPadding = 0.25f; // keep a bit away from walls

    [Header("Prefab / Pool")]
    public GameObject itemBoxObj;       // used if pool is null
    public QFSW.MOP2.ObjectPool itemBoxPool; // optional

    [Header("References")]
    public Transform playerTrans;

    [Header("Spawn distance from player")]
    public float keepDistWithPlayer = 14f;   // minimum distance
    public float withinDistWithPlayer = 21f; // maximum distance

    [Header("Spawn timing")]
    public float spawnInterval = 7f;
    public float spawnCnt;

    private const float OctantHalfAngle = 22.5f; // 45/2

    private void Start()
    {
        if (playerTrans == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTrans = player.transform;
        }
        spawnCnt = spawnInterval;
    }

    private void Update()
    {
        if (playerTrans == null) return;

        spawnCnt -= Time.deltaTime;
        if (spawnCnt <= 0f)
        {
            RandomSpawnItemBoxAroundPlayerWithinMapBound();
            spawnCnt = spawnInterval;
        }
    }

    public void RandomSpawnItemBoxAroundPlayerWithinMapBound()
    {
        if (playerTrans == null) return;
        if (withinDistWithPlayer < keepDistWithPlayer)
        {
            // swap if misconfigured
            float t = withinDistWithPlayer;
            withinDistWithPlayer = keepDistWithPlayer;
            keepDistWithPlayer = t;
        }

        Vector3 center = MapCenterXZ();
        Vector3 p = playerTrans.position;

        // Direction to the opposite area (Bottom-Left if player in Top-Right, etc.)
        // Vector from CENTER to PLAYER, then take the opposite (add 180‹)
        Vector3 vFromCenter = new Vector3(p.x - center.x, 0f, p.z - center.z);
        Vector3 baseOppDir;
        if (vFromCenter.sqrMagnitude < 1e-6f)
        {
            // Player is at center; choose any stable default (to Right) but randomize a bit
            baseOppDir = Vector3.right;
        }
        else
        {
            baseOppDir = -vFromCenter.normalized; // opposite side
        }

        // Try a few jitters within the opposite octant, and clip to bounds
        const int kMaxTries = 10;
        for (int i = 0; i < kMaxTries; i++)
        {
            float jitter = Random.Range(-OctantHalfAngle, OctantHalfAngle); // stay in the octant
            Vector3 dir = Quaternion.Euler(0f, jitter, 0f) * baseOppDir;

            float maxStep = MaxStepInsideRect(p, dir); // how far we can go before leaving bounds
            if (maxStep <= 0.05f) continue;

            float allowedMax = Mathf.Min(withinDistWithPlayer, maxStep);
            if (allowedMax < keepDistWithPlayer) continue; // this jitter would go out of bounds too soon

            float dist = Random.Range(keepDistWithPlayer, allowedMax);
            Vector3 candidate = p + dir * dist;
            candidate.y = p.y; // keep height; change if you have a fixed ground Y or NavMesh sample

            if (IsInsideMap(candidate))
            {
                SpawnBox(candidate);
                return;
            }
        }

        // Fallback: clamp along exact opposite direction without jitter
        {
            Vector3 dir = baseOppDir;
            float maxStep = MaxStepInsideRect(p, dir);
            float allowedMax = Mathf.Min(withinDistWithPlayer, maxStep);
            if (allowedMax >= keepDistWithPlayer)
            {
                float dist = Mathf.Clamp((keepDistWithPlayer + allowedMax) * 0.5f, keepDistWithPlayer, allowedMax);
                Vector3 candidate = p + dir * dist;
                candidate.y = p.y;
                SpawnBox(candidate);
                return;
            }
        }

        // If all else fails (very tiny map / player hugging corner), spawn at a safe clamped point
        Vector3 safe = new Vector3(
            Mathf.Clamp(p.x + baseOppDir.x * keepDistWithPlayer, mapLeftBoundX + boundsPadding, mapRightBoundX - boundsPadding),
            p.y,
            Mathf.Clamp(p.z + baseOppDir.z * keepDistWithPlayer, mapLowerBoundZ + boundsPadding, mapUpperBoundZ - boundsPadding)
        );
        SpawnBox(safe);
    }

    // ---------- Helpers ----------

    private Vector3 MapCenterXZ()
    {
        float cx = 0.5f * (mapLeftBoundX + mapRightBoundX);
        float cz = 0.5f * (mapLowerBoundZ + mapUpperBoundZ);
        return new Vector3(cx, 0f, cz);
    }

    private bool IsInsideMap(Vector3 xz)
    {
        return xz.x >= mapLeftBoundX + boundsPadding &&
               xz.x <= mapRightBoundX - boundsPadding &&
               xz.z >= mapLowerBoundZ + boundsPadding &&
               xz.z <= mapUpperBoundZ - boundsPadding;
    }

    /// <summary>
    /// Returns the maximum positive step t so that (start + dir * t) remains inside the rectangle.
    /// If zero, you hit a wall immediately in that direction.
    /// </summary>
    private float MaxStepInsideRect(Vector3 start, Vector3 dir)
    {
        float left  = mapLeftBoundX  + boundsPadding;
        float right = mapRightBoundX - boundsPadding;
        float low   = mapLowerBoundZ + boundsPadding;
        float up    = mapUpperBoundZ - boundsPadding;

        float tMin = 0f;
        float tMax = float.PositiveInfinity;

        // X interval
        if (Mathf.Abs(dir.x) > 1e-6f)
        {
            float tx1 = (left  - start.x) / dir.x;
            float tx2 = (right - start.x) / dir.x;
            float txMin = Mathf.Min(tx1, tx2);
            float txMax = Mathf.Max(tx1, tx2);
            tMin = Mathf.Max(tMin, txMin);
            tMax = Mathf.Min(tMax, txMax);
        }
        // Z interval
        if (Mathf.Abs(dir.z) > 1e-6f)
        {
            float tz1 = (low - start.z) / dir.z;
            float tz2 = (up  - start.z) / dir.z;
            float tzMin = Mathf.Min(tz1, tz2);
            float tzMax = Mathf.Max(tz1, tz2);
            tMin = Mathf.Max(tMin, tzMin);
            tMax = Mathf.Min(tMax, tzMax);
        }

        if (tMax < tMin) return 0f;
        if (tMax <= 0f) return 0f;
        return Mathf.Max(0f, tMax);
    }

    private void SpawnBox(Vector3 pos)
    {
        if (itemBoxPool != null)
        {
            // Using QFSW.MOP2 pool
            itemBoxPool.GetObject(pos, Quaternion.identity);
        }
        else if (itemBoxObj != null)
        {
            Instantiate(itemBoxObj, pos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[ItemBoxRandomSpawner] No pool or prefab set.");
        }
    }


}

