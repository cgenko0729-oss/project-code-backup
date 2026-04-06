using UnityEngine;

public class SkillModelBoomerang : SkillModelBase
{
    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        EnableCollision();
    }

    protected override void HandleSkillEndAction()
    {
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

}
