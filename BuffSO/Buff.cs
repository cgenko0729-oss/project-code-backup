using UnityEngine;

[CreateAssetMenu(fileName = "New Buff",order = -759)]                        

public class Buff : ScriptableObject
{
    public TraitType buffType;
    public string buffName;
    public Sprite buffIcon;
    public bool isStackable;
    public int maxStacks;
}

