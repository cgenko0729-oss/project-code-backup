using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class ResultMenuManager : Singleton<ResultMenuManager>
{
    EnemyManager em;
    DpsManager dm;
    PlayerState playerStatus;
    public List<SkillCasterBase> skillCasterBases = new();


    public CanvasGroup menuBackgorund;
    public GameObject uiBackground;

    public TextMeshProUGUI clearTimeText;
    public TextMeshProUGUI playerLvText;
    public TextMeshProUGUI enemyKillText;
    public TextMeshProUGUI TotalDamageText;
    public TextMeshProUGUI rankMojiText;
    public TextMeshProUGUI rankValText;
    public TextMeshProUGUI cointToAddText;

    public TextMeshProUGUI caster1DmgText;
    public TextMeshProUGUI caster2DmgText;
    public TextMeshProUGUI caster3DmgText;
    public TextMeshProUGUI caster4DmgText;

    public float clearTime;
    public int playerLv;
    public int playerHp;
    public int enemyKillCount;
    public int totalDamage;
    public int coinToAdd;

    public float clearTimeTextAnimator = 0;
    public float playerLvTextAnimator = 0;
    public float enemyKillTextAnimator = 0;
    public float TotalDamageTextAnimator = 0;

    public bool isOpen = false;
    public bool hasPlayedTextAnimation = false;
    public float startFade = 0.5f;
    public Vector3 backgroundScale = new Vector3(9f, 10f, 7f);

    public void AddInSkillCasterBase(SkillCasterBase casterToAdd)
    {      
        skillCasterBases.Add(casterToAdd);
    }

    void Start()
    {
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
        em = EnemyManager.Instance;
        dm = DpsManager.Instance;

        //rankValText.transform.localScale = Vector3.zero;       

    }

    private void Update()
    {
        UpdateResultInfo();
        
    }

    public void UpdateResultInfo()
    {
        //skillCasterBases = SkillManager's activeSkillHOlder list
        skillCasterBases = SkillManager.Instance.activeSkillCastersHolder;


        clearTime = TimeManager.Instance.gameTimePassed;
        playerHp = (int)playerStatus.NowHp;
        playerLv = (int)playerStatus.NowLv;
        enemyKillCount = EnemyManager.Instance.allEnemyKillNum;
        totalDamage = (int)DpsManager.Instance.allSkillLifeTimeTotalDamage;
    }


}

