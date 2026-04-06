using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class SkillModel : MonoBehaviour
{
    
    [Header("スキルプールとエフェクトオブジェクト、手動で設定する必要があります")]
    public ObjectPool effectObjPool;            // エフェクトオブジェクトのプール
    public ParticleSystem ps;                   // パーティクルシステムのキャッシュ
    public ParticleSystem finalPs;              //進化したスキルのパーティクルシステム（最終スキル用）
    public ParticleSystem subPs; // サブパーティクルシステム（必要に応じて使用）
    public GameObject skillMuzzleObject;
    public GameObject effectAfterMathObj;
    public bool isEffectAfterMath = false;
    public bool isSkillMuzzle = false; //スキルを終わる時のエフェクト
    public bool isFinalSkill = false; // 最終スキルかどうかのフラグ

    [Header("スキル情報(Casterが自動で設定するので、入力する必要はない)")]
    public SkillIdType skillIdType;             // スキルの種類
    public Vector3 skillBaseSize = Vector3.one; // スキルエフェクトの基準サイズ
	public float skillDuration;                 // スキル効果の継続時間（秒）
	public float skillSpeed;                    // スキル移動速度
	public float skillDamage;                   // スキルが与えるダメージ量
	public float skillSize;                      // スキルの拡大縮小倍率
	public int skillId;                         // スキルを識別するID
	public int skillLevel;                      // スキルのレベル
	public string skillName;                    // スキル名
   
    [Header("ほか")]
    public float collisionStartTime;            // 衝突判定を開始する時間（秒）
    public float collisionEndTime;              // 衝突判定を終了する時間（秒）
    public float posYOffset = 0.5f;             // スキル生成位置のYオフセット
    
    

    void OnEnable()
    {
        

        //ps = GetComponentInChildren<ParticleSystem>();


        switch (skillIdType)
        {
            case SkillIdType.Slash:
                HandleSlashSkill();
                break;
            case SkillIdType.CircleBall:
                HandleFireStoneSkill();
                break;
            case SkillIdType.Tornado:
                HandleTornadoSkill();
                break;
            case SkillIdType.Thunder:
                HandleThunderSkill();
                break;
            case SkillIdType.PoisonField:
                HandlePoisonFieldSkill();
                break;
            default:
                HandlePoisonFieldSkill();
                break;
        }

        
   
        UpdateSkillSize(); // サイズを現在のskillSizeに合わせて更新
        transform.position = new Vector3(transform.position.x, transform.position.y + posYOffset, transform.position.z);


    }

    private void OnDisable()
    {
        UpdateSkillSize();
    }

    private void Start()
    {
        UpdateSkillSize();
        transform.position = new Vector3(transform.position.x, transform.position.y + posYOffset, transform.position.z);

    }

    void Update()
    {
        UpdateSkilLife();

    }

    public void UpdateSkillSize() 
    {
        transform.localScale = new Vector3(skillBaseSize.x * skillSize, skillBaseSize.y * skillSize, skillBaseSize.z * skillSize);
    }

    public void UpdateSkilLife() // スキルの寿命を管理し、時間切れでエフェクトを停止してプールへ返却
    {
        skillDuration -= Time.deltaTime;
        if (skillDuration <= 0) 
        {
            if (ps != null) ps.Stop();
            if (finalPs != null) finalPs.Stop();
            if (effectObjPool != null) effectObjPool.Release(this.gameObject);
        }
    }

    public void EnableCollision()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }

    public void DisableCollision()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public void HandleSlashSkill()
    {
        if (!isFinalSkill)
        {

            if (ps != null)
            {                
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }
        else{
            if (finalPs != null)
            {
                ps.gameObject.SetActive(false); // 最終スキルなら通常のパーティクルは非表示
                finalPs.gameObject.SetActive(true);
                finalPs.Play();
            }
        }

        if(skillIdType == SkillIdType.Slash && !isFinalSkill)    // スラッシュタイプなら時間差で衝突判定を切り替え
        { 
            Invoke("EnableCollision", collisionStartTime); // 判定開始
            Invoke("DisableCollision", collisionEndTime);  // 判定終了
        }
        else
        {
            EnableCollision();
        }


    }
    public void HandleThunderSkill()
    {
        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        gameObject.GetComponent<BoxCollider>().enabled = false; // スラッシュスキルではないのでBoxColliderを無効化
        gameObject.GetComponent<SphereCollider>().enabled = false; // SphereColliderも無効化

        if (!isFinalSkill)
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;

        }
        else
        {
            gameObject.GetComponent<SphereCollider>().enabled = true; // 最終スキルならSphereColliderを有効化
            //spawn effectAfterMathObj at transform.position , rotation = -90 ,0,0
            Vector3 spawnPos = transform.position + new Vector3(0, -0.56f, 0); // Yオフセットを適用
            GameObject afterMath = Instantiate(effectAfterMathObj, spawnPos, Quaternion.Euler(-90, 0, 0));
            //afterMath.transform.localScale = new Vector3(skillBaseSize.x * skillSize, skillBaseSize.y * skillSize, skillBaseSize.z * skillSize);
            Destroy(afterMath,0.77f);
        }
    }

    public void HandleFireStoneSkill()
    {
        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        EnableCollision();

        Debug.Log("FireStone Skill Activated: " + skillIdType + ", Damage: " + skillDamage);

    }

    public void HandleTornadoSkill()
    {
        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        EnableCollision();
    }

    public void HandlePoisonFieldSkill()
    {
        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        EnableCollision();
    }


    public void SetSkill(SkillIdType type, int id, int level, string name, float duration, float speed, float damage, float size,bool _isFinalSkill) 
    {
        // スキル情報をまとめて設定するヘルパー関数
        // ItemManagerのパワーアップ効果も考慮してダメージ算出
        skillIdType = type;
        skillId       = id;
        skillLevel    = level;
        skillName     = name;
        skillDuration = duration;
        skillSpeed    = speed;
        skillDamage   = damage * ItemManager.Instance.powUpAmount;
        skillSize     = size;
        isFinalSkill = _isFinalSkill;

    }

    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        if (col.CompareTag("Enemy")) {
            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
            if (enemyStat != null) enemyStat.TakeDamage(skillDamage);   
            DpsManager.Instance.ReportDamage(skillIdType, skillDamage);
        }

        //破壊可能オブジェクトに当たったらOnHitを呼び出す
        if (col.CompareTag("DestroyObj"))
        {
            DestroyObjectController destroyObj = col.GetComponent<DestroyObjectController>();
            if (destroyObj != null) destroyObj.OnHit();
        }
    }
}

