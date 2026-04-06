using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using Steamworks;

public class DebugKeyManager : Singleton<DebugKeyManager>
{

    GameManager gm;

    public GameObject spiderBossObj;
    public GameObject turnipaBossObj;
    public GameObject dragonBossObj;
    public GameObject anubisBossObj;

    public PlayerState player;
    public GameObject playerObj;

    public SkillCasterBase slashCaster;
    public SkillCasterBase ballCaster;
    public SkillCasterBase thunderCaster;

    public SkillCasterBase arrowCaster;
    public SkillCasterBase boomerangCaster;
    public SkillCasterBase BouncerCaster;
    public SkillCasterBase shieldCaster;

    public EnemySpawnerMover spawnerPetGetDemo;
    public EnemySpawnerMover spawnerPetGetDemo2;

    public EnemyMidBossSpawner midBossSpawnerDesert;

    public bool isInSchoolDemo = false;

    [ContextMenu("TestActiveBuffLion")]
    public void TestActiveBuffLion()
    {
        ActiveBuffManager.Instance.AddStack(TraitType.LionSkill3_LonelyLion);
            ActiveBuffManager.Instance.SetStacks(TraitType.LionSkill3_LonelyLion, 3);
    }
    [ContextMenu("TestActiveBuffLion2")]
    public void TestActiveBuffLion2()
    {
         ActiveBuffManager.Instance.AddStack(TraitType.LionSkill3_LonelyLion);
    }

    private void Start()
    {
        gm = GameManager.Instance;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
        playerObj = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        
        bool chordPressed =Input.GetKey(KeyCode.B) && Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.G);
        if (chordPressed)
        {
            gm.isDebugMode = !gm.isDebugMode;
        }

        if (!gm.isDebugMode) return;


        //if press ctrl + 1
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            TimeManager.Instance.gameTimeScale = 1f;
            TimeManager.Instance.isTimeDebug = false;
            Time.timeScale = 1f;
            

            //left this Update function to prevent multiple key press in this frame : 
            return;


        }
        //if press ctrl + 2
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            TimeManager.Instance.isTimeDebug = true;
            TimeManager.Instance.gameTimeScale = 2f;
            return;
        }
        //if press ctrl + 3
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            TimeManager.Instance.isTimeDebug = true;
            TimeManager.Instance.gameTimeScale = 3f;
            return;
        }
        //if press ctrl + 4
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            TimeManager.Instance.isTimeDebug = !TimeManager.Instance.isTimeDebug;
            if(TimeManager.Instance.isTimeDebug)
            {
                TimeManager.Instance.gameTimeScale = 0.25f;
            }
            else
            {
                Time.timeScale = 1f;
                TimeManager.Instance.gameTimeScale = 1f;
            }
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha5))
        {
            TimeManager.Instance.isTimeDebug = true;
            TimeManager.Instance.gameTimeScale = 5f;
            return;
        }

        //if press R
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameQuestManager.Instance.questTriggerTimer = 3f;

            // (debugSpawnObj) Instantiate(debugSpawnObj, playerObj.transform.position, Quaternion.identity);

            //Camera.main.transform.position = Vector3.zero;
            //Camera.main.transform.rotation = Quaternion.identity;

            //CameraShake.Instance.StartPhrase2GroundShake();
        }

        //if press p
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (VideoShotManager.Instance.isVideoShotMode)
            {
                VideoShotManager.Instance.ResetEveryThing();
            }
            else
            {
                VideoShotManager.Instance.ChangeToScreenShotMode();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            EventManager.EmitEventData("AddQuestProgress", 101f);

           
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            spawnerPetGetDemo.gameObject.SetActive(true);
            spawnerPetGetDemo2.gameObject.SetActive(true);

            spawnerPetGetDemo.spawnCoolDown = 1f;
                spawnerPetGetDemo2.spawnCoolDown = 1f;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SkillManager.Instance.playerStatus.AddExp((int)SkillManager.Instance.playerStatus.NextLvExp +1);
               
        }

        if (Gamepad.current != null)
        {
            //if press left shoulder:
            if (Gamepad.current.leftTrigger.wasPressedThisFrame)
            {
                SkillManager.Instance.playerStatus.AddExp((int)SkillManager.Instance.playerStatus.NextLvExp + 1);
            }

        }

        //if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    EventManager.EmitEventData(GameEvent.ChangePlayerHp, 35f);
        //    SoundEffect.Instance.Play(SoundList.DebugkeySe);
        //}

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, -35f);
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && gm.isDebugMode)
        {
             EventManager.EmitEventData(GameEvent.ChangePlayerHp, 35f);
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            EnemyStatusBase bossStatus = null;
            if (spiderBossObj && spiderBossObj.activeInHierarchy) bossStatus = spiderBossObj.GetComponent<EnemyStatusBase>();
            else if (turnipaBossObj && turnipaBossObj.activeInHierarchy) bossStatus = turnipaBossObj.GetComponent<EnemyStatusBase>();
            else if (dragonBossObj && dragonBossObj.activeInHierarchy) bossStatus = dragonBossObj.GetComponent<EnemyStatusBase>();
            else if(anubisBossObj && anubisBossObj.activeInHierarchy) bossStatus = anubisBossObj.GetComponent<EnemyStatusBase>();    

            if (bossStatus)bossStatus.enemyHp = (bossStatus.enemyMaxHp / 2 - 70); 
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            EnemyStatusBase bossStatus = null;
            if (spiderBossObj && spiderBossObj.activeInHierarchy) bossStatus = spiderBossObj.GetComponent<EnemyStatusBase>();
            else if (turnipaBossObj && turnipaBossObj.activeInHierarchy) bossStatus = turnipaBossObj.GetComponent<EnemyStatusBase>();
            else if (dragonBossObj && dragonBossObj.activeInHierarchy) bossStatus = dragonBossObj.GetComponent<EnemyStatusBase>();
            else if (anubisBossObj && anubisBossObj.activeInHierarchy) bossStatus = anubisBossObj.GetComponent<EnemyStatusBase>();

            if (bossStatus)bossStatus.enemyHp = 100; 
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TimeManager.Instance.gameTimeLeft = 280;
            TimeManager.Instance.gameTimePassed = 60* 5f; // 5•ª
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            
            TimeManager.Instance.gameTimeLeft = 120;
            TimeManager.Instance.gameTimePassed = 60 * 8.1f; // 2•ª
            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("EnemySpawner");
            foreach (GameObject obj in gameObjects)
            {
                EnemySpawnerBase spawner = obj.GetComponent<EnemySpawnerBase>();
                if (spawner != null && spawner.gameObject.activeInHierarchy)
                {
                    spawner.spawnCoolDown = 1f;
                }
            }

            midBossSpawnerDesert.spawnCnt = 2f;

        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TimeManager.Instance.gameTimeLeft = 90;
            TimeManager.Instance.gameTimePassed = 60 * 10.9f; // 10•ª
            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("EnemySpawner");
            foreach (GameObject obj in gameObjects)
            {
                EnemySpawnerBase spawner = obj.GetComponent<EnemySpawnerBase>();
                if (spawner != null && spawner.gameObject.activeInHierarchy)
                {
                    spawner.spawnCoolDown = 1f;
                }
            }

            midBossSpawnerDesert.spawnCnt = 2f;
        }

         if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            
            float timeLeft = 7f;
            QuickRunModeManager.Instance.quickModeTimer = timeLeft;
            TimeManager.Instance.gameTimeLeft = timeLeft;
            TimeManager.Instance.gameTimePassed = TimeManager.Instance.GameTotalTime -timeLeft; // 10•ª
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            //ballCaster.isActivated = true;
            //thunderCaster.isActivated= true;

            //slashCaster.casterLevel = 9;
            //ballCaster.casterLevel = 9;
            //thunderCaster.casterLevel = 9;

            //slashCaster.damageScaler = 50f;
            //ballCaster.damageScaler = 50f;
            //thunderCaster.damageScaler = 50f;

            //slashCaster.coolDownFactor = 0.42f;
            //thunderCaster.coolDownFactor = 0.35f;

            //ballCaster.projectileNumScaler = 4;
            //thunderCaster.projectileNumScaler = 2;

            //ballCaster.durationScaler = 50f;
            //slashCaster.sizeScaler = 50f;

            boomerangCaster.isActivated = true;
            BouncerCaster.isActivated = true;
            shieldCaster.isActivated = true;

            arrowCaster.casterLevel = 9;
            boomerangCaster.casterLevel = 9;
            BouncerCaster.casterLevel = 9;
            shieldCaster.casterLevel = 9;

            arrowCaster.damageScaler = 10f;
            boomerangCaster.damageScaler = 10f;
            BouncerCaster.damageScaler = 10f;
            shieldCaster.damageScaler = 10f;

            arrowCaster.coolDownFactor = 0.7f;
            boomerangCaster.coolDownFactor = 1f;
            //BouncerCaster.coolDownFactor = 0.7f;
            shieldCaster.coolDownFactor = 1f;

            boomerangCaster.sizeScaler = 21f;
            BouncerCaster.sizeScaler = 21f;
            shieldCaster.sizeScaler = 35f;

            arrowCaster.projectileNumScaler = 3;
            BouncerCaster.projectileNumScaler = 5;
            shieldCaster.projectileNumScaler = 3;

            player.LevelMax();
            SoundEffect.Instance.Play(SoundList.DebugkeySe);      

        }






    }

}

