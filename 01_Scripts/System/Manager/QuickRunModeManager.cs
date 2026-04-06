using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class QuickRunModeManager : Singleton<QuickRunModeManager>
{

    public bool isQuickRunMode = false;

    public EnemyMidBossSpawner es1;
    public EnemyMidBossSpawner es2;

    public EnemyMidBossSpawner es0;
    public EnemyMidBossSpawner es3;

    public EnemyMidBossSpawner olds1;
    public EnemyMidBossSpawner olds2;

    public EnemyMidBossSpawner olds0;
    public EnemyMidBossSpawner olds3;

    public EnemySpawnerMover bomberSpawner;
    public EnemySpawnerMover bomberSpawner2;

    public EnemySpawnerMover bomberSpawner0;
    public EnemySpawnerMover bomberSpawner3;

    public EnemySpawnerMover casterSpawner;
    public EnemySpawnerMover casterSpawner2;

    public EnemySpawnerMover casterSpawner0;
    public EnemySpawnerMover casterSpawner3;

    public EnemySpawnerBase surrounderSpawner;
    public EnemySpawnerBase surrounderSpawner2;

    public EnemySpawnerBase surrounderSpawner0;
    public EnemySpawnerBase surrounderSpawner3;

    public EnemySpawnerMover runnerSpawner;
    public EnemySpawnerMover runnerSpawner2;

    public EnemySpawnerMover runnerSpawner0;
    public EnemySpawnerMover runnerSpawner3;

    public SkillCasterArrow arrow;
    public SkillCasterSlash slash;
    public SkillCasterHammer hammer;
    public SkillCasterStar star;

    public PlayerState playerstat;

    public float quickModeTimer = 300;
    public float quickModeProgressTimer = 0;

    public bool isInit = false;

    public bool isQuickGameOver = false;

    public TimeManager tm;

    //change time 

    //default weapon 

    //change time phase 

    //when reach 7 min kill player end game 

    //

    void Start()
    {
        tm = TimeManager.Instance;

        if (!isQuickRunMode) return;

        tm.gamePhrase = 2;
        tm.DisplayerWaveMessage();

        if (bomberSpawner0 != null)
        {
            bomberSpawner0.spawnCoolDown = 35f;
        }
        if (bomberSpawner3 != null)
        {
            bomberSpawner3.spawnCoolDown = 35f;
        }

        if (bomberSpawner != null)
        {
            bomberSpawner.spawnCoolDown = 35f;
        }

        if(bomberSpawner2 != null)
        {
            bomberSpawner2.spawnCoolDown = 35f;
        }

        if(casterSpawner0 != null)
        {
            casterSpawner0.spawnCoolDown = 140f;
        }
        if (casterSpawner3 != null)
        {
            casterSpawner3.spawnCoolDown = 140f;
        }

        if (casterSpawner != null)
        {
            casterSpawner.spawnCoolDown = 140f;
        }
        if (casterSpawner2 != null)
        {
            casterSpawner2.spawnCoolDown = 140f;
        }

        if (surrounderSpawner0 != null)
        {
            surrounderSpawner0.spawnCoolDown = 77f;
        }

        if (surrounderSpawner3 != null)
        {
            surrounderSpawner3.spawnCoolDown = 77f;
        }

        if (surrounderSpawner != null)
        {
            surrounderSpawner.spawnCoolDown = 77f;
        }
        if (surrounderSpawner2 != null)
        {
            surrounderSpawner2.spawnCoolDown = 77f;
        }

        if (runnerSpawner0 != null)
        {
            runnerSpawner0.spawnCoolDown = 100f;
            runnerSpawner0.spawnCooldownMax = 100f;
        }
        if (runnerSpawner3 != null)
        {
            runnerSpawner3.spawnCoolDown = 140f;
            runnerSpawner3.spawnCooldownMax = 100f;

            //runnerSpawner3.spawnCoolDown = 7000;
            //runnerSpawner3.spawnCooldownMax = 7000;

        }

        if (runnerSpawner != null)
        {
            runnerSpawner.spawnCoolDown = 100f;
            runnerSpawner.spawnCooldownMax = 100f;
        }

        if (runnerSpawner2 != null)
        {
            runnerSpawner2.spawnCoolDown = 140f;
            runnerSpawner2.spawnCooldownMax = 100f;

            //runnerSpawner2.spawnCoolDown = 7000;
            //runnerSpawner2.spawnCooldownMax = 7000;

        }

        if(olds1 != null)
        {
            olds1.spawnCnt = 120f;
        }
        if (olds2 != null)
        {
            olds2.spawnCnt = 120f;
        }

        if (olds0 != null)
        {
            olds0.spawnCnt = 120f;
        }
        if (olds3 != null)
        {
            olds3.spawnCnt = 120f;
        }

    }

    public bool isBossAppear = false;

    void UpdateQuickModeTimePhase()
    {
        quickModeProgressTimer += Time.deltaTime;

        int targetPhrase = 3;

        if(quickModeProgressTimer > 60 * 3.1)
        {
            targetPhrase = 7;
        }
        else if (quickModeProgressTimer > 60 * 2.5)
        {
            targetPhrase = 6;
        }
        else if (quickModeProgressTimer > 60 * 1.77)
        {
            targetPhrase = 5;
        }
        else if (quickModeProgressTimer > 60 * 1)
        {
            targetPhrase = 4;
        }
       

        if (targetPhrase != tm.gamePhrase)
        {
            tm.gamePhrase = targetPhrase;
            tm.DisplayerWaveMessage();
            Debug.Log("Quick Mode Time Phase Changed to: " + targetPhrase);
        }


    }

    void Update()
    {
        if (!isQuickRunMode) return;

        UpdateQuickModeTimePhase();


        if(!GameManager.Instance.isGameClear || !GameManager.Instance.isGameOver)quickModeTimer -= Time.deltaTime;

        if(quickModeTimer <= 0 && !isQuickGameOver)
        {
            isQuickGameOver = true;


            //EventManager.EmitEvent("isGameClear");
            //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            //foreach (GameObject enemy in enemies)
            //{
            //    EnemyStatusBase es = enemy.GetComponent<EnemyStatusBase>();
            //    //LayerMask enemySpiderLayer = LayerMask.GetMask("EnemySpider");
            //    //if(enemy.layer != enemySpiderLayer) continue; 
            //    es.DeadNoExp();
            //}

            CutSceneManager.Instance.ClearAllGameObjectForCutScene();

        }

        if (!isInit)
        {
            isInit = true;
            TimeManager.Instance.gameTimePassed = 320f;
            TimeManager.Instance.gameTimeLeft = 280f;

            arrow.casterLevel = 5;
            arrow.damageScaler = 28f;
            arrow.coolDownFactor = 0.7f;
            arrow.projectileNumScaler = 2;

            slash.casterLevel = 5;
            slash.damageScaler = 28f;
            slash.coolDownFactor = 0.7f;
            slash.sizeScaler = 49f;

            hammer.casterLevel = 5;
            hammer.damageScaler = 28f;
            hammer.coolDownFactor = 0.7f;
            hammer.sizeScaler = 49f;

            star.casterLevel = 5;
            star.damageScaler = 28f;
            star.coolDownFactor = 0.7f;
            star.sizeScaler = 49f;

            playerstat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

            BuffManager.Instance.gobalExpGain = 70f;
            BuffManager.Instance.gobalPickUpRange = 100f;
        }


        if (quickModeProgressTimer >= 60*3f && !isBossAppear)
        {
            isBossAppear = true;
            es1.spawnCnt = 1;
            es0.spawnCnt = 1;
            es3.spawnCnt = 1;

            es2.spawnCnt = 1;

        }

        
    }
}

