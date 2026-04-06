using UnityEngine;

[CreateAssetMenu(fileName = "ItemEffectData", menuName = "GameScene/Item/ItemEffectData")]
public class ItemEffectData : ScriptableObject
{
    public GameObject healingEffect;        //回復エフェクト
    public GameObject powUpEffect;          //攻撃力上昇エフェクト
    public GameObject expAbsorptionEffect;  //EXP吸収エフェクト
    public GameObject spdUpEffect;          //速度上昇エフェクト
    public GameObject getCoinEffect;        //コイン入手エフェクト
    public GameObject getSwapOrbEffect;     //スワップオーブ入手エフェクト
    public GameObject getForgeOrbEffect;    //フォージオーブ入手エフェクト
    public GameObject getRerollOrbEffect;   //リロールオーブ入手エフェクト
    public GameObject getInvincbleEffect;   //無敵アイテム入手エフェクト
}
