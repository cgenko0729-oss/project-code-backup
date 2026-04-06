using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;
using DamageNumbersPro;            //Object Pool

public class EffectManager : Singleton<EffectManager>
{

    public Transform playerTrans;
    public Vector3 playerpos;

    public GameObject playerPoisonEffect;
    public GameObject playerWebSlowEffect;
    public GameObject playerLevelUpEffect;

    public GameObject playerDizzyEffect;

    public GameObject bossWarningEffect;
    public TextMeshProUGUI bossWarningText;
    public bool isBossWarning = false;

    public GameObject playerDashEffect; // プレイヤーダッシュエフェクト
    public GameObject playerDashEffect2;
    public GameObject playerDashEffect3;
    public GameObject playerDashEffect4;


    //public GameObject plaeyrDashEffect;

    public bool LevelUpFlag = false;
    [SerializeField] float sphereRadius   = 14f;
    [SerializeField] float pushDistance   = 5f;
    [SerializeField] float pushDuration   = 0.5f;
    [SerializeField] LayerMask enemyMask;

    public bool isDashEffect = false;

    public GameObject gameOverEffect;   // ゲームオーバー時のエフェクト

    public GameObject playerGetTraitEffect;

    public GameObject questTraceTrailObj;

    public DamageNumber playerHealNum;
    public DamageNumber playerDamageNum;

    public float afterCastSpeedAdd = 5f;         // The percentage to add per stack // also + 5% damage
    public float afterCastSpeedAddDuration = 4f; // Duration of each stack
    public int maxAfterCastSpeedAddStack = 10; 
    private List<float> afterCastSpeedAddStacks = new List<float>();
    public GameObject afterCastSpeedAddEffectObj;

    private float getcoinGetCrit = 5f;
    private float getCoinCritAddDuration = 7.7f;
    public int maxGetCoinCritAddStack = 10;
    private List<float> getCoinCritAddStacks = new List<float>();
    public GameObject getCoinCritAddEffectObj;

    public GameObject getEnchantEffectObj;

    public void SpawnGetEnchantEffectObj(Vector3 pos)
    {
        SkillManager.Instance.renderEffectCamera.SetActive(true);
        Instantiate(getEnchantEffectObj,pos,Quaternion.identity);
        Debug.Log("spawnEnchnatEffect");

    }

    public void HandleGetCoinAddCrit()
    {
        //Debug.Log("HandleGetItemAddAttack");

        if (getCoinCritAddStacks.Count >= maxGetCoinCritAddStack) return;

        ActiveBuffManager.Instance.AddStack(TraitType.PickCoinGetCrit);

        getCoinCritAddStacks.Add(getCoinCritAddDuration);

        BuffManager.Instance.gobalCritChanceAdd += getcoinGetCrit;

        //Instantiate(getItemAttackAddEffectObj, playerTrans.position + new Vector3(0, 2.19f, 0), Quaternion.identity);
    }

    public void ManageGetCointCritBuffTimers()
    {

        if (getCoinCritAddStacks.Count == 0) return;

        for (int i = getCoinCritAddStacks.Count - 1; i >= 0; i--)
        {
            // Countdown the timer for this stack.
            getCoinCritAddStacks[i] -= Time.deltaTime;

            // Check if the timer has expired.
            if (getCoinCritAddStacks[i] <= 0)
            {
                BuffManager.Instance.gobalCritChanceAdd -= getcoinGetCrit;
                getCoinCritAddStacks.RemoveAt(i);
                ActiveBuffManager.Instance.ReduceStack(TraitType.PickCoinGetCrit);
            }

        }

    }

    public void HandleAfterCastSpeedAdd()
    {
        //Debug.Log("HandleAfterCastSpeedAdd");

        if (afterCastSpeedAddStacks.Count >= maxAfterCastSpeedAddStack) return;

        ActiveBuffManager.Instance.AddStack(TraitType.AfterCastSpeedAdd);
    
            afterCastSpeedAddStacks.Add(afterCastSpeedAddDuration);

        BuffManager.Instance.gobalMoveSpeed += afterCastSpeedAdd;

        Instantiate(afterCastSpeedAddEffectObj, playerTrans.position + new Vector3(0, 2.19f, 0), Quaternion.identity);

    }

    void ManageSpeedBuffTimers()
    {
      
        if (afterCastSpeedAddStacks.Count == 0) return;

        for (int i = afterCastSpeedAddStacks.Count - 1; i >= 0; i--)
        {
            // Countdown the timer for this stack.
            afterCastSpeedAddStacks[i] -= Time.deltaTime;

            // Check if the timer has expired.
            if (afterCastSpeedAddStacks[i] <= 0)
            {
                BuffManager.Instance.gobalMoveSpeed -= afterCastSpeedAdd;
                afterCastSpeedAddStacks.RemoveAt(i);
                ActiveBuffManager.Instance.ReduceStack(TraitType.AfterCastSpeedAdd);
            }

        }

    }

    public void OnEnable()
    {
        //EventManager.StartListening(GameEvent.ChangePlayerHp, CreatePlayerWebSlowEffect);
        EventManager.StartListening(GameEvent.PlayerDash, PlayPlayerDashEffect);
        EventManager.StartListening(GameEvent.PlayerLevelUp, PushBackAllEnemyUponLevelUp);
        EventManager.StartListening("isGameOver", PlayGameOverEffect);

        EventManager.StartListening("QuestStarted", SpawnQuestTraceTrailEffect);

        EventManager.StartListening(GameEvent.ChangePlayerHp, SpawnHealPlayerMoji);

        EventManager.StartListening("AfterCastSpeedAdd",HandleAfterCastSpeedAdd);

        

    }

    public void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerDash, PlayPlayerDashEffect);
        EventManager.StopListening(GameEvent.PlayerLevelUp, PushBackAllEnemyUponLevelUp);
        EventManager.StopListening("isGameOver", PlayGameOverEffect);

        EventManager.StopListening("QuestStarted", SpawnQuestTraceTrailEffect);

        EventManager.StopListening(GameEvent.ChangePlayerHp, SpawnHealPlayerMoji);
        EventManager.StopListening("AfterCastSpeedAdd", HandleAfterCastSpeedAdd);
    }

    public void SpawnHealPlayerMoji()
    {
        float amount = EventManager.GetFloat("ChangePlayerHp");

        if(amount <= 0) return; // 回復量が0以下の場合は何もしない

        if (SkillEffectManager.Instance.universalTrait.isHealDouble) amount *= 2; // 回復量が2倍になるスキルが有効な場合、回復量を2倍にする

        playerHealNum.Spawn(playerTrans.position + new Vector3(0, 0.7f, 0), amount, playerTrans);

    }

    public void SpawnDamagePlayerMoji(float damageAmount)
    {
        playerDamageNum.Spawn(playerTrans.position + new Vector3(0, 0.7f, 0), damageAmount, playerTrans);
    }

    public void SpawnQuestTraceTrailEffect()
    {
            
        // Vector3 targetPos = EventManager.GetGameObject("QuestStarted").transform.position;

        //QuestTraceTrailController obj = Instantiate(questTraceTrailObj, playerTrans.position + new Vector3(0, 1.5f, 0), Quaternion.identity).GetComponent<QuestTraceTrailController>();
        //obj.questTargetPos = targetPos;

    }

    public void SpawnQuestTraceTrailEffectDelay(Vector3 pos)
    {
         QuestTraceTrailController obj = Instantiate(questTraceTrailObj, playerTrans.position + new Vector3(0, 1.5f, 0), Quaternion.identity).GetComponent<QuestTraceTrailController>();
        obj.questTargetPos = pos;
    }

    public void SpawnPlayerGetTraitEffect()
    {
        GameObject obj = Instantiate(playerGetTraitEffect,playerTrans.position,Quaternion.identity,playerTrans);
        
    }

    public void PlayPlayerDashEffect()
    {
        if(!isDashEffect) return; // ダッシュエフェクトが無効な場合は何もしない
        playerDashEffect.gameObject.SetActive(true);
        playerDashEffect2.gameObject.SetActive(true);
        playerDashEffect3.gameObject.SetActive(true);
        playerDashEffect4.gameObject.SetActive(true);
        Invoke("UnActiveDashEffect", 0.5f); // 0.5秒後にエフェクトを非アクティブにする

    }

    public void UnActiveDashEffect()
    {
        playerDashEffect.gameObject.SetActive(false);
        playerDashEffect2.gameObject.SetActive(false);
        playerDashEffect3.gameObject.SetActive(false);
        playerDashEffect4.gameObject.SetActive(false);
    }

    public void PushBackAllEnemyUponLevelUp()
    {       
        Vector3 centre = playerTrans.position;

        Collider[] hits = Physics.OverlapSphere(centre, sphereRadius, enemyMask, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            Transform enemyTf = hit.transform;
            Vector3 dir = (enemyTf.position - centre).normalized;
            Vector3 targetPos = enemyTf.position + dir * pushDistance;
            targetPos.y = enemyTf.position.y;
            enemyTf.DOMove(targetPos, pushDuration).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Fixed); 
        }

    }

    public void ShowDamamgeScreen()
    {

    }

    void Start()
    {
       
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        if(!isBossWarning && TimeManager.Instance.gameTimeLeft <= 5)
        {
            isBossWarning = true;
            //bossWarningEffect.SetActive(true);
            //bossWarningText.gameObject.SetActive(true);          
            //Destroy(bossWarningEffect, 5.0f);
            //SoundEffect.Instance.Play(SoundList.CountDownwarningSe);

            DisplayMidBossWarning();

        }
        
        playerpos = playerTrans.position;



        ManageSpeedBuffTimers();
        ManageGetCointCritBuffTimers();

    }

    public void DisplayMidBossWarning()
    {
        bossWarningEffect.SetActive(true);
        bossWarningEffect.GetComponent<ParticleSystem>().Stop();
        bossWarningEffect.GetComponent<ParticleSystem>().Play();
        SoundEffect.Instance.Play(SoundList.CountDownwarningSe);
        bossWarningText.gameObject.SetActive(true);
        bossWarningText.DOFade(1f, 0.35f).SetUpdate(true);
        //Invoke("DisableMidBossWarning", 2.8f);
        DOVirtual.DelayedCall(2.8f, DisableMidBossWarning).SetUpdate(true);

    }

    void DisableMidBossWarning()
    {
        bossWarningEffect.SetActive(false);
        bossWarningText.DOFade(0f, 0.35f).SetUpdate(true).OnComplete(() => {
            bossWarningText.gameObject.SetActive(false);
        });
    }

    public void CreateDashEffect()
    {

       

    }

    public void CreatePlayerPoisonEffect()
    {
        var effect = Instantiate(playerPoisonEffect, playerTrans.position, Quaternion.identity, playerTrans);

        //エフェクトの親をプレイヤーに設定
        effect.transform.SetParent(playerTrans);
        Destroy(effect, 3.0f);

    }

    public void CreatePlayerDizzyEffect()
    {
        var effect = Instantiate(playerDizzyEffect, playerTrans.position + new Vector3(0,1.4f,0), Quaternion.identity, playerTrans);

        //エフェクトの親をプレイヤーに設定
        effect.transform.SetParent(playerTrans);
        Destroy(effect, 4.9f);

    }


    public  void CreatePlayerWebSlowEffect()
    {
        var effect = Instantiate(playerWebSlowEffect, playerTrans.position, Quaternion.identity,playerTrans);

        //エフェクトの親をプレイヤーに設定
        effect.transform.SetParent(playerTrans);

        Destroy(effect, 3.0f);
    }

    public void CreatePlayerLevelUpEffect()
    {
        var effect = Instantiate(playerLevelUpEffect, playerTrans.position + new Vector3(0f,0.14f,0f), Quaternion.identity);
        //effect.transform.SetParent(playerTrans);
        Destroy(effect, 3.0f);

    }

    public void PlayGameOverEffect()
    {
        gameOverEffect.gameObject.SetActive(true);
    }
}

