using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class ActiveBuffManager : Singleton<ActiveBuffManager>
{
    [Searchable]
    public List<Buff> allPossibleBuffs;
    private Dictionary<TraitType, int> currentStacks = new Dictionary<TraitType, int>(); // Internal dictionary to track current stacks of each buff
    private Dictionary<TraitType, Buff> buffDataLookup = new Dictionary<TraitType, Buff>(); // Fast lookup for buff data (icon, max stacks, etc.) by TraitType

    void Start()
    {
        InitializeBuffs();
    }

    void Update()
    {
        
    }

    private void InitializeBuffs()
    {
        foreach (Buff buff in allPossibleBuffs)
        {
            if (!buffDataLookup.ContainsKey(buff.buffType))
            {
                buffDataLookup.Add(buff.buffType, buff);
                currentStacks.Add(buff.buffType, 0); // Start all buffs at 0 stacks
            }
        }
    }

    public void AddStack(TraitType buffType, bool isFrameEffect = true)
    {
        if (!buffDataLookup.ContainsKey(buffType))
        {
            Debug.LogWarning($"BuffManager does not contain data for {buffType}");
            return;
        }

        Buff buffData = buffDataLookup[buffType];
        int newStackCount = currentStacks[buffType] + 1;

        currentStacks[buffType] = Mathf.Min(newStackCount, buffData.maxStacks); // Clamp the value to the max stacks defined in the ScriptableObject

        int stackCountFinal = currentStacks[buffType];
        if (buffData.isStackable == false) stackCountFinal = 1;


        ActiveBuffUiManager.Instance.UpdateBuffDisplay(buffType, stackCountFinal, buffData.buffIcon, buffData.isStackable,isTriggerFrameEffect:isFrameEffect);

    }


    public void ReduceStack(TraitType buffType)
    {
        if (!buffDataLookup.ContainsKey(buffType))
        {
            Debug.LogWarning($"BuffManager does not contain data for {buffType}");
            return;
        }

        Buff buffData = buffDataLookup[buffType];
        int newStackCount = currentStacks[buffType] - 1;

        currentStacks[buffType] = Mathf.Max(newStackCount, 0); // Ensure stacks don't go below 0
        ActiveBuffUiManager.Instance.UpdateBuffDisplay(buffType, currentStacks[buffType], buffData.buffIcon, buffData.isStackable);
    }

    public void SetStacks(TraitType buffType, int amount)
    {
        if (!buffDataLookup.ContainsKey(buffType))
        {
            Debug.LogWarning($"BuffManager does not contain data for {buffType}");
            return;
        }

        Buff buffData = buffDataLookup[buffType];
        
        // Clamp the value to the max stacks
        currentStacks[buffType] = Mathf.Clamp(amount, 0, buffData.maxStacks);

        // Update the UI
        ActiveBuffUiManager.Instance.UpdateBuffDisplay(buffType, currentStacks[buffType], buffData.buffIcon, buffData.isStackable);
    }

    public int GetStackCount(TraitType buffType)
    {
        if (currentStacks.ContainsKey(buffType))
        {
            return currentStacks[buffType];
        }
        return 0;
    }

}

