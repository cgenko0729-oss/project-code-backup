using UnityEngine;

[CreateAssetMenu(fileName = "ItemKeepTimePs", menuName = "GameScene/Item/ItemKeepTimePs")]

public class ParticlesData : ScriptableObject
{
    public ParticleSystem itemPowUpPs;          //攻撃力上昇を維持しているときのパーティクル
    public ParticleSystem itemSpdUpPs;          //速度上昇を維持しているときのパーティクル
    public ParticleSystem petHealPs;            //ペット回復のパーティクル
    public ParticleSystem petSheeldPs;          //ペットシールドのパーティクル
    public ParticleSystem ExplodePs;            //爆発のパーティクル
    public ParticleSystem ExplodeBeforeLightPs; //爆発する前の光のパーティクル
    public ParticleSystem RisingSmokePs;        //上昇する煙のパーティクル
    public ParticleSystem SheddingPs;           //地面から生えてくる時のパーティクル
    public ParticleSystem SoulEatPs;            //ソウルを食べるときのパーティクル
    public ParticleSystem RevengePs;            //リベンジ発動時のパーティクル
    public ParticleSystem PetGetPs;             //ペットを仲間にしたときのパーティクル
    public ParticleSystem EnemyRevivePs;        //敵が復活するときのパーティクル
}

