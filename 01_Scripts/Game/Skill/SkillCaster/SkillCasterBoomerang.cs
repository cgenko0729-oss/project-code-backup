using UnityEngine;

public class SkillCasterBoomerang : SkillCasterBase
{
    protected override void CastSkill()
    {
        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;

        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);

        float durationMultiplier = 1f;
        if (isFinalSkill)
        {
            durationMultiplier = 2f;
        }
        else
        {
            durationMultiplier = 1f;
        }

            sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * durationMultiplier, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();

        var boomerang = sm.GetComponent<SkillBoomerangMove>();
        boomerang.moveVec = playerForwardVec;
        boomerang.speedFinal = speedFinal;
        boomerang.transform.position = spawnPos;
        boomerang.isFinalSkill = isFinalSkill;
        boomerang.Init();

        SoundEffect.Instance.Play(SoundList.BoomerangSe);


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            Invoke("EmitDoubleCast", 0.28f);
        }


    }

    void EmitDoubleCast()
    {
        TigerForge.EventManager.EmitEvent("OnPlayDoubleCastAnim");
        ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            Vector3 spawnPos2 = playerTran.position + castPosOffset - playerForwardVec * castDistance;

            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);

        float durationMultiplier = 1f;
        if (isFinalSkill)
        {
            durationMultiplier = 2f;
        }
        else
        {
            durationMultiplier = 1f;
        }

            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * durationMultiplier, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm2.SetTrait(currentTrait);
            sm2.InitStatusSizeAndPosY();

            var boomerang2 = sm2.GetComponent<SkillBoomerangMove>();
            boomerang2.moveVec = -playerForwardVec;
            boomerang2.speedFinal = speedFinal;
            boomerang2.transform.position = spawnPos2;
            boomerang2.isFinalSkill = isFinalSkill;
            boomerang2.Init();
    }

    


}
