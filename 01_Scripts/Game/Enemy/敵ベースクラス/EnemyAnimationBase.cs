using UnityEngine;

public class EnemyAnimationBase : MonoBehaviour
{

    bool isSingleAnime = true;

    public AnimationClip walkClip;

    void Start()
    {
        InitAnimData();

    }

    void Update()
    {
        
    }

    public void InitAnimData()
    {
        if(!isSingleAnime) return;

        var anim = GetComponent<Animation>();
        anim.wrapMode = WrapMode.Loop;
        anim.AddClip(walkClip, walkClip.name);
        anim.Play(walkClip.name);
    }

}
