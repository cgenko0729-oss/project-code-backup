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

public class ResultPlayerStatusDisplay : MonoBehaviour
{
    public Image[] traitIcon = new Image[4];

    public Sprite playerDogSprite;
    public Sprite playerRabbitSprite;
    public Sprite playerLoinSprite;
    public Sprite playerBirdSprite;

    public Image playerIconImage;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerLevelText;

    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI playerMaxHpText;

    public TextMeshProUGUI playerAttackEnhanceText;
    public TextMeshProUGUI playerDamageReductionText;
    public TextMeshProUGUI playerLuckText;
    public TextMeshProUGUI playerCritText;
    public TextMeshProUGUI playerMoveSpeedText;
    public TextMeshProUGUI playerDamageReceiveText;
    public TextMeshProUGUI playerHpRecoverText;
    
    public TextMeshProUGUI playerItemGetText;

    public PlayerState player;

    private void OnEnable()
    {
        EventManager.StartListening("OpenDetailStatMenu", SetUpCharacterInfo);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OpenDetailStatMenu", SetUpCharacterInfo);
    }

    public void SetUpCharacterInfo()
    {    

        SetUpCharacteName();
        SetUpCharaTrait();

    }

    void SetUpCharaTrait()
    {
        for (int i = 0; i < traitIcon.Length; i++)
        {
            if (SkillEffectManager.Instance.playerTraitList.Count > i)
                if (SkillEffectManager.Instance.playerTraitList[i].icon)traitIcon[i].sprite = SkillEffectManager.Instance.playerTraitList[i].icon;
            //else
                //traitIcon[i].gameObject.SetActive(false);
        }
    }
    void SetUpCharacteName()
    {
        JobId job = GameManager.Instance.playerData.jobId;

        switch (job)
        {
            case JobId.DogKnight:
                playerIconImage.sprite = playerDogSprite;
                playerNameText.text = L.CharacterName(job);
                break;
            case JobId.Archer:
                playerIconImage.sprite = playerRabbitSprite;
                playerNameText.text = L.CharacterName(job);
                break;
            case JobId.Wizard:
                playerIconImage.sprite = playerBirdSprite;
                playerNameText.text = L.CharacterName(job);
                break;
            case JobId.Warrior:
                playerIconImage.sprite = playerLoinSprite;
                playerNameText.text = L.CharacterName(job);
                break;


            default:
                break;
        }

        float hp = player.NowHp;
        float maxHp = player.MaxHp;
        float lv = player.NowLv;

        playerHpText.text = hp.ToString("F0");
        playerMaxHpText.text = maxHp.ToString("F0");

    }

    void Start()
    {
        //serach player tag
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();


    }

    void Update()
    {
        
    }
}

