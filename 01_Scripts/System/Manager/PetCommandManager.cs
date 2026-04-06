using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

public class PetCommandManager : SingletonA<PetCommandManager>
{
    void Update()
    {
        CheckPetSkillInput();
    }

    //指定されたペットのスキルを発動する
    private void CheckPetSkillInput()
    {
        if (SkillManager.Instance.isLevelUpWindowOpen) return;

        int inputNum = -1;
        
        if (InputDeviceManager.Instance.GetLastUsedDevice() is Gamepad)
        {
            //1キーで1番目のペットのスキル発動
            if (Gamepad.current.dpad.left.wasPressedThisFrame)
            {
                inputNum = 0;
            }
            //2キーで2番目のペットのスキル発動
            if (Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                inputNum = 1;
            }
            //3キーで3番目のペットのスキル発動
            if (Gamepad.current.dpad.right.wasPressedThisFrame)
            {
                inputNum = 2;
            }
        }
        else
        {
            //1キーで1番目のペットのスキル発動
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                inputNum = 0;
            }
            //2キーで2番目のペットのスキル発動
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                inputNum = 1;
            }
            //3キーで3番目のペットのスキル発動
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                inputNum = 2;
            }
        }

        if(inputNum != -1)
        {
            ActivatePetSkill(inputNum);
        }
    }
    public void ActivatePetSkill(int petIndex)
    {
        //そもそもActivePetManagerが存在しないなら、何もできない
        if (ActivePetManager.Instance == null) return;

        //指定された番号のペットが存在しないなら、何もできない
        if (petIndex >= ActivePetManager.Instance.activePets.Count) return;

        //指定されたペットのオブジェクトが(何らかの理由で)nullなら、何もできない
        GameObject pet = ActivePetManager.Instance.activePets[petIndex];
        if (pet == null) return;

        //ペットがスキルを持っていないなら、警告を出して処理を終える
        IPetActiveSkill skill = pet.GetComponent<IPetActiveSkill>();
        if (skill == null)
        {
            return;
        }

        //スキルを発動！
        skill.PetActiveSkill();
    }
}

