using System.Collections.Generic;
using UnityEngine;

public class PetMummyLordAction : ActivePetActionBase
{
    private bool hasCopied = false;

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void Start()
    {
        base.Start();

        Invoke("CopyPet", 0.5f);
    }

    private void CopyPet()
    {
        if (hasCopied) return;
        hasCopied = true;

        //分身の上昇量を取得
        int copyCount = 1 + ActivePetManager.Instance.PetCloneCount;

        //全てのアクティブペットを取得
        var allActivePets = ActivePetManager.Instance.activePets;

        if (allActivePets == null || allActivePets.Count < 2)
        {
            return;
        }

        //ターゲットとなるペットを選択（ここでは2番目のペットを選択）
        GameObject targetPetObject = allActivePets[1];
        if (targetPetObject == null) return;

        //自分自身をコピーしないようにチェック
        if (targetPetObject == this.gameObject)
        {
            return;
        }

        //ダメージ情報をコピー
        CopyDamage(targetPetObject);

        for (int i = 0; i < copyCount; i++)
        {
            float offsetDistance = 1.5f;

            float angle = i * (360f / copyCount);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * (this.transform.right * offsetDistance);

            Vector3 finalOffset = targetPetObject.transform.position + offset;

            //複製ペット生成
            GameObject Copy = Instantiate(targetPetObject, finalOffset, this.transform.localRotation);

            //エフェクト再生
            ItemManager.Instance.PlayTimeParticleInObject(Copy,
                                                          ItemManager.Instance.itemKeepTimePs.EnemyRevivePs,
                                                          Vector3.zero);

            ActivePetActionBase deadStatus = Copy.GetComponent<ActivePetActionBase>();
            if (deadStatus != null)
            {
                //編成位置を設定
                deadStatus.SetFormationOffset(finalOffset);

                //君はクローンですと告げる
                deadStatus.Doppelganger();

                //強制移行
                deadStatus.ForceChangeState(PetActionStates.Walk);
            }
            ActivePetManager.Instance.RegisterActivePetList(Copy);
        }
    }

    private void CopyDamage(GameObject CopyObj)
    {
        if (CopyObj == null) return;

        ActivePetActionBase copiedPetStatus = CopyObj.GetComponent<ActivePetActionBase>();
        if (copiedPetStatus != null)
        {
            this.takeDamages = copiedPetStatus.takeDamages;
        }
    }
}
