using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

public class BuffManager : SingletonA<BuffManager>
{

    public float gobalPickUpRange;          // �o���l�̏E���͈�
    public float gobalMoveSpeed;            // �ړ��X�s�[�h
    public float gobalExpGain;              // �o���l�l����
    public float gobalHealthAdd = 0f;       // �̗�
    public int gobalPetSlotAdd = 0;         // �y�b�g�X���b�g
    public float gobalGoldGain = 0f;        // �R�C���l����
    public float gobalLuckAdd = 0f;         // �K�^
    public int gobalInitSkillProjNumAdd = 0;// �����̃X�L���̒e������
    public float gobalItemEffectAdd = 0f;   // �A�C�e���̌��ʗʑ���
    public float gobalCooldown; //useless

    public int gobalRerollChanceAdd = 0;    // �����[����
    public int gobalBoostChanceAdd = 0;     // �u�[�X�g��
    public int gobalSwapChanceAdd = 0;      // �X���b�v��

    public float gobalSizeAdd = 0f;         // �X�L���T�C�Y
    public float gobalDamageAdd = 0f;       // �_���[�W
    public float gobalDurationAdd = 0f;     // �p������
    public float gobalSpeedAdd = 0f;        // �X�L���X�s�[�h
    public float gobalCooldownAdd = 0f;     // �N�[���_�E��
    public float gobalCritChanceAdd = 0f;   // �N���e�B�J����
    public float gobalCritDamageAdd = 0f;   // �N���e�B�J���_���[�W

    public float gobalPlayerDefenceAdd = 0f;        //�v���C���[�̖h��� damage *(1 + gobalPlayerDefenceAdd/100)
    public float gobalPlayerReceiveDamageAdd = 0f;  //�v���C���[�̔�_���[�W�� damage *(1 - gobalPlayerReceiveDamageAdd/100)

    //public bool canPlayerDash = true;
    public bool canPlayerCastSkill = true;  // �X�L���̃��L���X�g
    public bool canPlayerMove = true;       // �v���C���[���ړ��\��

    public float canPlayerCastSkillDuration = 0f;   // �v���C���[���X�L�������L���X�g�\��
    public float canPlayerMoveDuration = 0f;        // �v���C���[���_�b�V���\��

    public bool isLimitSlot2 = false;

    [Header("�L���������ɂ��������")]
    public bool isApplyBuffPerExpEnabled = false;           // �o���l�擾�Ńo�t�l��
    public bool isApplyBuffPerCoinEnabled = false;          // �R�C���擾�Ńo�t�l��

    // �U������قǃo�t�l��
    public bool isIncreaseDamagePerAttackEnabled = false;

    // �����X�L���̐i������ʂ̔������ɒǉ��U��������
    public bool isAddAttackPerFinalSkillEnabled = false;  

    // ��莞�Ԗ��Ƀ����_���Ō��ʔ���
    public bool isIntervalRandomEffectEnabled = false;
    // �̗͂�1/3�ȉ��̍ۂɁA�󂯂�_���[�W����
    public bool isApplyPinchBuffEnabled = false;

    // �f�����Ɋ�Â��ă_���[�W�����A�N�[���_�E������
    public bool isApplyBuffSpeedScalingEnabled = false;
    // �e���Ɋ�Â��ă_���[�W����
    public bool isApplyBuffProjNumScalingEnabled = false;

    // ���݂̗̑͂̊����Ńo�t�l��
    public bool isApplyBuffHealthScalilngEnabled = false;
    // �̗͂����邲�ƂɃo�t�l��
    public bool isApplyBuffPerDamageEnabled = false;
    // �y�b�g�̘A��Ă������ƘA��Ă��Ȃ������󂫘g�̐��Ɋ�Â��ăo�t�l��
    public bool isApplyBuffPetScalingEnabled = false;

    void Start()
    {
        //ApplyBuffToGame();


    }

    void Update()
    {

        //gobalPetSlotAdd = 2;


        if (canPlayerCastSkill)
        {
            canPlayerCastSkillDuration -= Time.deltaTime;
            if (canPlayerCastSkillDuration <= 0f)
            {
                canPlayerCastSkill = true;
            }
        }

        if (!canPlayerMove)
        {
            canPlayerMoveDuration -= Time.deltaTime;
            if (canPlayerMoveDuration <= 0f)
            {
                canPlayerMove = true;
            }
        }





    }

    public void ApplyPlayerCastSkillDebuff(float _duration = 10f)
    {
        canPlayerCastSkill = false;
        canPlayerCastSkillDuration = _duration;

    }
    public void ApplyPlayerMoveDebuff(float _duration = 10f)
    {
        canPlayerMove = false;
        canPlayerMoveDuration = _duration;

    }

    //public void ApplyBuffToGame()
    //{
    //   SkillManager.Instance.luck += gobalLuckAdd;
    //}

    public void ResetAllStatus()
    {
        gobalPickUpRange = 0;
        gobalMoveSpeed = 0;
        gobalExpGain = 0;
        gobalCooldown = 0;
        gobalRerollChanceAdd = 0;
        gobalBoostChanceAdd = 0;
        gobalSwapChanceAdd = 0;
        gobalSizeAdd = gobalDamageAdd = gobalDurationAdd = gobalSpeedAdd = gobalCooldownAdd = 0;
        gobalHealthAdd = 0;
        gobalPlayerDefenceAdd = 0;
        gobalLuckAdd = 0;
        gobalItemEffectAdd = 0;
        gobalCritChanceAdd = 0;
        gobalPlayerReceiveDamageAdd = 0;
        gobalGoldGain = 0;

    }

    public void ResetAll()
    {
        gobalPickUpRange = 0;
        gobalMoveSpeed = 0;
        gobalExpGain = 0;
        gobalCooldown = 0;
        gobalRerollChanceAdd = 0;
        gobalBoostChanceAdd = 0;
        gobalSwapChanceAdd = 0;
        gobalSizeAdd = gobalDamageAdd = gobalDurationAdd = gobalSpeedAdd = gobalCooldownAdd = 0;
        gobalHealthAdd = 0;
        gobalPlayerDefenceAdd = 0;
        gobalLuckAdd = 0;
        gobalPetSlotAdd = 0;
        gobalItemEffectAdd = 0;
        gobalCritChanceAdd = 0;
        gobalPlayerReceiveDamageAdd = 0;
        gobalGoldGain = 0;



        isApplyBuffPerExpEnabled = false;
        isApplyBuffPerCoinEnabled = false;  
        isIncreaseDamagePerAttackEnabled = false;
        isApplyBuffSpeedScalingEnabled = false;
        isApplyBuffProjNumScalingEnabled = false;

        isApplyBuffHealthScalilngEnabled = false;
        isApplyBuffPerDamageEnabled = false;
        isApplyBuffPetScalingEnabled = false;

        // �������̋����p�t���O
        isIntervalRandomEffectEnabled = false;
        isAddAttackPerFinalSkillEnabled = false;
        isApplyPinchBuffEnabled = false;
    }

    public void ApplyLevel(ShopItemData data, int level)
    {
        for (int i = 0; i < level; i++)
        {
            ApplyBuff(data.itemType, data.levels[i].addAmount);
        }

    }


    public void ApplyBuff(ShopItemType type, float value)
    {

        switch (type)
        {
            case ShopItemType.GobalPickUpRange: gobalPickUpRange += value; break;
            case ShopItemType.GobalMoveSpeed: gobalMoveSpeed += value; break;
            case ShopItemType.GobalExpGain: gobalExpGain += value; break;
            case ShopItemType.GobalCooldown: gobalCooldown += value; break;
            case ShopItemType.GobalRerollChance: gobalRerollChanceAdd += (int)value; break;
            case ShopItemType.GobalBoostChance: gobalBoostChanceAdd += (int)value; break;
            case ShopItemType.GobalSwapChance: gobalSwapChanceAdd += (int)value; break;
            case ShopItemType.GobalSize: gobalSizeAdd += value; break;
            case ShopItemType.GobalDamage: gobalDamageAdd += value; break;
            case ShopItemType.GobalDuration: gobalDurationAdd += value; break;
            case ShopItemType.GobalSpeed: gobalSpeedAdd += value; break;
            case ShopItemType.GobalCooldownAdd: gobalCooldownAdd += value; break;
            case ShopItemType.GobalHealth: gobalHealthAdd += value; break;
            case ShopItemType.GobalLuck: gobalLuckAdd += value; break;
            case ShopItemType.GobalPetSlot: gobalPetSlotAdd += (int)value; break;
            case ShopItemType.GobalGoldGain: gobalGoldGain += value; break;
            case ShopItemType.GobalCritChance: gobalCritChanceAdd += value; break;
            case ShopItemType.GobalDefense: gobalPlayerDefenceAdd += value; break;

        }
    }


    public void ApplyCharaBuff(BuffType type, float value)
    {
        switch (type)
        {
            case BuffType.IncreaseDamage:
                gobalDamageAdd += value; break;
            case BuffType.IncreaseDefence:
                gobalPlayerDefenceAdd += value; break;
            case BuffType.IncreaseCritChance:
                gobalCritChanceAdd += value; break;
            case BuffType.IncreaseHealth:
                gobalHealthAdd += value; break;
            case BuffType.IncreaseProjectileNum:
                gobalInitSkillProjNumAdd += (int)value; break;
            case BuffType.IncreaseExp:
                gobalExpGain += value; break;
            case BuffType.DecreaseSkillCooldown:
                gobalCooldownAdd += value; break;
            case BuffType.IncreaseOneReviveChance:
                /*�����`�����X����*/ break;
            case BuffType.IncreaseMoveSpeed:
                gobalMoveSpeed += value; break;
            case BuffType.IncreaseDamagePerAttack:
                isIncreaseDamagePerAttackEnabled = true; break;
            case BuffType.ApplyBuffPerExp:
                isApplyBuffPerExpEnabled = true; break;
            case BuffType.ApplyBuffPerCoin:
                isApplyBuffPerCoinEnabled = true; break;

            case BuffType.AddAttackPerFinalSkill:
                isAddAttackPerFinalSkillEnabled = true;  break;
            case BuffType.IntervalRandomEffect:
                isIntervalRandomEffectEnabled = true; break;
            case BuffType.ApplyPinchBuff:
                isApplyPinchBuffEnabled = true; break;

            case BuffType.IncreaseSkillSize:
                gobalSizeAdd += value; break;
            case BuffType.IncreaseItemEffectValue:
                gobalItemEffectAdd += value; break;
            case BuffType.ApplyBuffSpeedScaling:
                isApplyBuffSpeedScalingEnabled = true; break;
            case BuffType.ApplyBuffProjNumScaling:
                isApplyBuffProjNumScalingEnabled = true; break;
            case BuffType.ApplyBuffHealthScaling:
                isApplyBuffHealthScalilngEnabled = true; break;
            case BuffType.ApplyBuffPerDamage:
                isApplyBuffPerDamageEnabled = true; break;
            case BuffType.ApplyBuffPetScaling:
                isApplyBuffPetScalingEnabled = true; break;
        }
    }
}