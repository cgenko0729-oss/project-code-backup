using TigerForge;
using UnityEngine;
using System.Collections; // Coroutineを使うために必要

public class PetScarabroAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃力の上昇倍率(%)")]
    [SerializeField] private float attackPowerUpRate = 25f;

    [Header("攻撃力の継続時間")]
    [SerializeField] private float attackPowerDurationTime = 5f;

    //攻撃力バフのスタック数
    private int buffStacks = 0;

    //攻撃力の倍率ベース
    private float baseAttackPower = 1f;

    //元の攻撃力保存用
    private float originalAttackDamage;

    #endregion --------------------------------------------

     protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.PlayerGetDamage, AttackUp);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.PlayerGetDamage, AttackUp);
    }

    protected override void Start()
    {
        base.Start();

        //このペットの元々の攻撃力を記憶
        if (petData != null)
        {
            this.originalAttackDamage = this.petData.attackPower;
        }
        else
        {
            //保険
            this.originalAttackDamage = this.takeDamages;
        }
    }

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();

    private void AttackUp()
    {
        //エフェクト再生
        ItemManager.Instance.PlayTimeParticleInObject(this.gameObject,
                                                      ItemManager.Instance.itemKeepTimePs.RevengePs, 
                                     　　　　　　　　 ItemManager.Instance.defaultEffectPos);

        //効果音がもう再生されていたら、スルー
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        // 攻撃力バフを適用するコルーチンを開始
        StartCoroutine(AttackBuffCoroutine());
    }

    private IEnumerator AttackBuffCoroutine()
    {
        //攻撃力バフ1つ追加
        buffStacks++;
        UpdateAttackDamage();

        //スキル持続時間を設定
        float finalSkillDuration = attackPowerDurationTime * ActivePetManager.Instance.PetSkillDuration;

        //指定された継続時間待機
        yield return new WaitForSeconds(finalSkillDuration);

        //攻撃力バフを1つ減少させる
        buffStacks--;
        UpdateAttackDamage();
    }

    private void UpdateAttackDamage()
    {
        //累積による合計上昇率を計算
        float totalMultiplier = baseAttackPower + (buffStacks * (attackPowerUpRate / 100f));

        //攻撃速度にも反映
        SetAttackSpeed(totalMultiplier);

        //元々の攻撃力に、合計倍率を掛けて、現在の攻撃力を算出
        this.takeDamages = this.originalAttackDamage * totalMultiplier;
    }
}
