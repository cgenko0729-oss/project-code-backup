using UnityEngine;
using System;
using System.IO;

public class BalancingDataCollector : Singleton<BalancingDataCollector>
{
    /* --------------------------------------------------------------------- */
    /*  References                                                            */
    /* --------------------------------------------------------------------- */
    private PlaySessionData currentSession;

    private GameManager  gm;
    private EnemyManager em;
    private DpsManager   dm;

    public PlayerState playerStatus;

    /* --------------------------------------------------------------------- */
    /*  MonoBehaviour life-cycle                                             */
    /* --------------------------------------------------------------------- */
    private void Start()
    {
        StartNewSession();

        gm = GameManager.Instance;
        em = EnemyManager.Instance;
        dm = DpsManager.Instance;

        //find player status by tag player
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

    }

    // If you want an automatic dump when the editor or build closes,
    // uncomment the line below.
    /*
    private void OnApplicationQuit()
    {
        SaveSessionData();
    }
    */

    /* --------------------------------------------------------------------- */
    /*  Session control                                                      */
    /* --------------------------------------------------------------------- */
    public void StartNewSession()
    {
        currentSession = new PlaySessionData
        {
            sessionId   = Guid.NewGuid().ToString(),
            sessionDate = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") // use - not :
        };
    }

    /* --------------------------------------------------------------------- */
    /*  Manual save trigger (F10 inside the editor)                           */
    /* --------------------------------------------------------------------- */
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("<color=yellow>BalancingDataCollector ► Manual save (F10)</color>");
            SaveSessionData();
        }
    }
#endif

    /* --------------------------------------------------------------------- */
    /*  Populate & write JSON                                                */
    /* --------------------------------------------------------------------- */
    public void SaveSessionData()
    {
        PopulateDataFromManagers();

        string json = JsonUtility.ToJson(currentSession, true);

#if UNITY_EDITOR
        /* ---------- Save under Assets/-------/BalancingReports ------------ */
        string dir = Path.Combine(Application.dataPath, "BalancingReports");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string file = Path.Combine(dir, $"ゲーム実行データ_{currentSession.sessionDate}.json");
        File.WriteAllText(file, json);

        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>Balancing data saved to ASSETS ► {file}</color>");
#else
        /* ---------- Save to persistent data path (build) ------------------ */
        string dir = Path.Combine(Application.persistentDataPath, "BalancingData");
        Directory.CreateDirectory(dir);

        string file = Path.Combine(dir, $"Session_{currentSession.sessionDate}.json");
        File.WriteAllText(file, json);

        Debug.Log($"<color=lime>Balancing data saved to ► {file}</color>");
#endif
    }

    /* --------------------------------------------------------------------- */
    /*  Gather numbers from managers                                         */
    /* --------------------------------------------------------------------- */
    private void PopulateDataFromManagers()
    {
        if (currentSession == null) return;

        /* ---------- Clear previous data in case the user presses F10 twice */
        currentSession.EnemyKillData .Clear();
        currentSession.SkillDamageData.Clear();

        /* ---------- Time & player level ---------------------------------- */
        currentSession.sessionDurationSeconds = TimeManager.Instance.gameTimePassed;
        currentSession.finalPlayerLevel       = (int)playerStatus.NowLv;

        /* ---------- Enemy kills ------------------------------------------ */
        currentSession.totalKills = em.allEnemyKillNum;

        currentSession.EnemyKillData.Add(
            new EnemyKillEntry { name = "クモ撃破数",   value = em.moverKillNum });
        currentSession.EnemyKillData.Add(
            new EnemyKillEntry { name = "ドラゴン撃破数", value = em.bomberKillNum });
        currentSession.EnemyKillData.Add(
            new EnemyKillEntry { name = "コウモリ撃破数", value = em.FlyerKillNum });
        currentSession.EnemyKillData.Add(
            new EnemyKillEntry { name = "キノコ撃破数",   value = em.SurrounderKillNum });
        currentSession.EnemyKillData.Add(
            new EnemyKillEntry { name = "リザード撃破数", value = em.CasterKillNum });

        /* ---------- DPS / damage ----------------------------------------- */
        currentSession.totalDamageDealt = dm.allSkillLifeTimeTotalDamage;
        currentSession.totalLifetimeDps = dm.lifeTimeDps;

        currentSession.SkillDamageData.Add(
            new SkillDpsEntry { name = "スラッシュ", value = dm.slashLifeTimeTotalDamage });
        currentSession.SkillDamageData.Add(
            new SkillDpsEntry { name = "隕石",     value = dm.circleLifeTimeTotalDamage });
        currentSession.SkillDamageData.Add(
            new SkillDpsEntry { name = "かみなり", value = dm.thunderLifeTimeTotalDamage });
    }
}