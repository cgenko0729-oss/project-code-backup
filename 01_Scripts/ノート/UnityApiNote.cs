

 






///*using関係*/
using UnityEngine;
using UnityEngine.SceneManagement;      //シーン関係のクラスを使用するために必要
using UnityEngine.UI;                   // UI関係のクラスを使用するために必要
using Unity.Profiling;                  //Profiler関係のクラスを使用するために必要
using TMPro;                            // TextMeshPro関係のクラスを使用するために必要
using System.Collections;               //コルーチン関係のクラスを使用するために必要
using System.Collections.Generic;       //List関係のクラスを使用するために必要
using System.Linq;                    　//LINQ関係のクラスを使用するために必要
using TigerForge;                         //イベント関係のクラスを使用するために必要
using QFSW.MOP2;                          //Object Pool関係のクラスを使用するために必要

//public class UnityApi : MonoBehaviour
//{
//    [Tooltip("Tipです")]　                             //Unity Inspector上での説明欄の追加する
//    [Header("プレイヤーステータス関係")]                //Unity Inspector上での説明欄の追加する 
//    [Range(0f, 10f)]　                           //Unity Inspector上でのスライダーを追加する

//    [SerializeField] private float playerHp = 100f; //SerializeField 属性を付けることで、Unity Inspector上で値を調整できるようにする

//    public float PlayerHp                           //Public変数とゲッター セッター
//        {                                           //プロパティの定義
//        get { return playerHp; }                    //ゲッター (ライベート変数(playerHp)の値を取得する)
//        set { playerHp = value; }                   //セッター (変数の値を変更する)
//    } 

//    public float PlayerHp2 => playerHp; //プロパティの定義 (ゲッターのみ、セッターなし)

//    Collider playerCollider;      
//    GameObject enemyObj;
//    Transform enemyFolderTrans;

//    GameObject effectPrefab;
//    Vector3 effectPos = new Vector3(1,1,1);　　　　　　　　　　　　　　　　　

//    /*Ui関係*/
//    CanvasGroup startMenuCanvas;    //Canvasのグループを取得する
//    RectTransform uiTransform;      //UIの位置を変更するためのRectTransform
//    Image titleImage;               //UIの画像を取得する
//    Button startButton;             //UIのボタンを取得する
//    Text message1;                  //UIのテキストを取得する
//    TextMeshPro message2;           //TextMeshProのテキストを取得する
//    TextMeshProUGUI message3;
//    //ほか　Slider, Toggle ...

//    enum PlayerState //列挙型
//    {
//        Idle,
//        Walk,
//        Run,
//        Jump,
//        Attack,
//        Dead
//    }
//    PlayerState playerState = PlayerState.Idle; //列挙型の変数を定義する



//    void Update()
//    {

//        /*オブジェクトの取得と操作*/
//         playerCollider = GetComponent<Collider>();                            //このオブジェクトのコライダを取得する
//         PlayerStatus playerStatus1 = GetComponent<PlayerStatus>();            //このゲームオブジェクト内のPlayerStatusクラスを取得する　     
//         PlayerStatus playerStatus2 = GetComponentInChildren<PlayerStatus>();　//オブジェクトの子オブジェクト内のPlayerStatusクラスを取得する
//         PlayerStatus playerStatus3 = GetComponentInParent<PlayerStatus>();　　//オブジェクトの親オブジェクト内のPlayerStatusクラスを取得する
//         if (TryGetComponent<PlayerStatus>(out PlayerStatus playerStatus4)) {　//このオブジェクトにPlayerStatusクラスを取得し、それを名前を付けて操作する　         
//             playerStatus4.currentHealth =100;
//         }
//        bool isPlayer = playerCollider.CompareTag("Player");　                //このコーライダが持っているオブジェクトが"Enemy"タグを持つかどうかを判定する式　
//        bool isEnemy = enemyObj.CompareTag("Enemy");　　                      //このオブジェクトが"Enemy"タグを持つかどうかを判定する式
//        enemyFolderTrans = GameObject.Find("enemyFolder").transform;          //enemyFolderのTransformを取得する

//        /*オブジェクトの操作*/    
//        enemyObj.GetComponent<Collider>().enabled = false;                   //このコンポーネント(コライダ)を無効にする
//        if(enemyObj.layer == LayerMask.NameToLayer("Enemy")) { }             //このオブジェクトのレイヤーが"Enemy"レイヤーかどうかを判定する式
//        this.transform.position = new Vector3(1, 1, 1);                      //このオブジェクトの位置を変更する
//        enemyObj.transform.position = new Vector3(1, 1, 1);                  //このenemyObj位置を変更する
//        this.transform.Translate(new Vector3(50,0,50));                     //このオブジェクトを50,0,50移動する
//        this.transform.Rotate(new Vector3(90,0,0)); 
//        this.transform.LookAt(effectPos);
//        enemyObj.AddComponent<EnemyColor>();                                //enemyObjにEnemyColorクラスを追加する

//        /*オブジェクトの生成と削除　有効化と無効化*/
//        Instantiate(effectPrefab,effectPos,Quaternion.identity);                //enemyObjを生成、親なし
//        Instantiate(effectPrefab,effectPos,Quaternion.identity,enemyFolderTrans); //enemyObjを生成し、　親オブジェクトとしてenemyFolderPosに持たせる　(inspectorのenemyフォルダーの中)
//        Destroy(enemyObj,3f);                                                   //3秒後にobjを削除する
//        enemyObj.SetActive(false);                                           　　//このオブジェクトを無効にする


//        /*キー入力検査 */
//        if(Input.GetKeyDown(KeyCode.Space)) { };

//        /*物理判定　と　レイキャスト*/
//        Vector3 startPos = transform.position;
//        Vector3 direction   = transform.forward;
//        float rayDist = 50f;
//        int layerMask = enemyObj.layer;

//        bool isHit = Physics.Raycast(startPos,direction,out RaycastHit hit, rayDist, layerMask);  // レーを飛ばす(出発点、方向、RaycastHitの情報、距離 、レイヤーマスク)
//        //Physics.SphereCast();
//        //Physics.OverlapSphereNonAlloc(this.transform.position, radius,Collider[] results, int layerMask);　

//        /*シーンの遷移*/
//        SceneManager.LoadScene("StartScene");
//        SceneManager.LoadSceneAsync("GameScene");

//        /*数学関係 UnityEngine.Mathf */
//        //Mathf.Max Min Floor Ceil Abs Atan2 Acos Asin Clamp Sqrt
//        //float num4 = 0f - Mathf.Acos((Mathf.Pow(num3, 2f) + Mathf.Pow(num, 2f) - Mathf.Pow(num2, 2f)) / (2f * num3 * num));
//       // float num2 = Mathf.Abs(0.5f * Physics.gravity.y * m_arrivalTime * m_arrivalTime);

//        Vector3 v;
//        //v.magnitude;

//        /*アセットの読み込み*/
//        Resources.Load("effectFolder/effect01");
//        //Addressables.LoadAssetAsync<T>

//        /*デバッグとコンソールのログ*/ 
//        Debug.Log("こんにち");
//        Debug.LogError("エラーです");
//        Debug.LogWarning("警告です");
//        Debug.Log($"PlayerHp:{playerHp}"); //$で文字列を囲むことで、変数を埋め込むことができる
//        Debug.Log($"effectPos:{effectPos.x},{effectPos.y},{effectPos.z}");

//        /*アニメーションの再生*/
//        Animator animator = GetComponent<Animator>();
//        animator.SetTrigger("GetHit");     //トリガーをセット
//        animator.SetBool("isMoving", true);//boolをセット
//        animator.SetFloat("Speed", 1.0f);  //floatをセット
//        animator.Play("PlayerWalk");       //指定したアニメーションを再生

//        //Coroutine: IEumerator StartCoroutine(MyRoutine());, yield return new WaitForSeconds(2).
//        //Unity Profiler  (パフォーマンス測定)
//        //ScriptableObject (オブジェクトデータの保存と追加)
//        //Event Action &Observer Pattern (ゲーム内のイベント)
//        //Object Pool   (オブジェクトの再利用)
//        //State Machine (敵の行動パタン)
//        //Sound        (音声の管理)
//        //Particle System　(エフェクト)
//        //Cinemachine Timeline (カメラの制御、カードシーン) 
//        //await async Task UniTask (非同期処理)
//        //NavMesh  (いらない)

//        /*乱数生成*/
//        int randomNum = Random.Range(0, 10); //0から10の間の整数を生成する

//        /*時間制御*/
//        Time.timeScale = 1;               //ゲームの時間のスケールを変更する
//        Time.timeScale = 0;               //ゲームの時間を停止する

//        /*カメラ関係*/
//        Vector3 camForward = Camera.main.transform.forward; //メインカメラの前方ベクトルを取得する
//        Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
//	    //Ray ray = m_mainCamera.ScreenPointToRay(vector);

//        /*Ui操作*/
//        startMenuCanvas.alpha = 0;              //グループ内の全てのUIの透明度を変更する
//        startMenuCanvas.blocksRaycasts = false; //UIのクリックを無効にする、

//        startButton.image.color = Color.red;    //ボタンの色を変更する
//        startButton.interactable = false;       //ボタンを無効にする

//        message2.text = "Hello World";          //テキストを変更する
//        message2.fontSize = 20;                 //テキストのフォントサイズを変更する

//        uiTransform.anchoredPosition = new Vector2(100, 200); //UIの位置を変更する

//        message1.text = playerHp.ToString();

//        //OnGUI
//        //OnDrawGizmos

//        //OnParticleCollision
//        //OnParticleTrigger

//        /*DOtween*/


//        //delay event 
//        Destroy(gameObject,10f);
//        Invoke("メソッド名", 3f); //3秒後にメソッドを実行する
//        StopCoroutine("メソッド名"); //コルーチンを停止する

//        /*Optimization*/
//        //GpuInstaner pro ,with animation, compute shader
//        //mesh animator 
//        //optimizer 
//        //camera culling 
//        //mesh combine (LOD)


//        //particle collison detection

//        //scriptable object utility creation

//        //short cut key : create folder = Ctrl + Shift + N , create new mono script = Ctrl + Shift + M

//        //damage number pro

//        //
//    }





//    void OnEnable() //このオブジェクトが有効になったときに実行される
//    {
//        Debug.Log("OnEnableです");
//    }

//    void OnDisable() //このオブジェクトが無効になったときに実行される
//    {
//        Debug.Log("OnDisableです");
//    }

//    void OnDestroy() //このオブジェクトが削除されたときに実行される
//    {
//        Debug.Log("OnDestroyです");
//    }

//    void Awake() //AwakeはStartよりも早く実行される
//    {
//        Debug.Log("Awakeです"); 
//    }

//    void Start() //StartはAwakeの後に実行される
//    {
//        Debug.Log("Startです");        
//    }

//     void FixedUpdate() //物理演算の更新
//    {
//        Debug.Log("FixedUpdateです"); 
//    }

//    void LateUpdate() //LateUpdateはUpdateの後に実行される
//    {
//        Debug.Log("LateUpdateです"); 
//    }



//    void OnTriggerEnter(Collider other) //トリガーに入ったときに実行される
//    {
//        Debug.Log("OnTriggerEnter");
//    }

//    void OnTriggerStay(Collider other) //トリガーにいるときに実行される
//    {
//        Debug.Log("OnTriggerStay");
//    }

//    void OnTriggerExit(Collider other) //トリガーから出たときに実行される
//    {
//        Debug.Log("OnTriggerExit");
//    }

//    void OnCollisionEnter(Collision collision) //衝突したときに実行される
//    {
//        Debug.Log("OnCollisionEnter");
//    }

//    void OnCollisionStay(Collision collision) //衝突しているときに実行される
//    {
//        Debug.Log("OnCollisionStay");
//    }

//    void OnCollisionExit(Collision collision) //衝突から離れたときに実行される
//    {
//        Debug.Log("OnCollisionExit");
//    }


//    void OnMouseDown() //マウスがこのオブジェクトに当たったときに実行される
//    {
//        Debug.Log("OnMouseDown");
//    }

//    void OnMouseUp() //マウスがこのオブジェクトから離れたときに実行される
//    {
//        Debug.Log("OnMouseUp");
//    }
//    void OnMouseDrag() //マウスがこのオブジェクトをドラッグしているときに実行される
//    {
//        Debug.Log("OnMouseDrag");
//    }

//    void OnMouseEnter() //マウスがこのオブジェクトに入ったときに実行される
//    {
//        Debug.Log("OnMouseEnter");
//    }

//    void OnMouseExit() //マウスがこのオブジェクトから出たときに実行される
//    {
//        Debug.Log("OnMouseExit");
//    }

//    void OnMouseOver() //マウスがこのオブジェクトの上にあるときに実行される
//    {
//        Debug.Log("OnMouseOver");
//    }



//    //参考サイト
//    //Zen: https://zenn.dev/topics/unity

//}

//PrefabをHierarchyに値や設定を変更した後、上のOverride->Applyを押すことでPrefabに変更を保存することを忘れなくことが大事です。



/*
 スキル追加する時の流れ・やり方
0.必要なクラス: SkillModel, SkillCaster , スキルの移動方を決めるクラス(例: SkillForwardMove, SkillCircleMove...)
0.5.SkillType Enum でスキルの種類を定義する, 例: Slash, CircleBall, Tornado, Thunder, PoisonField　の後で　Ice,Arrow...etc
1.HierarchyのCasterフォルダ内で新GameObjectを作成する, 名前は何々Caster (ArrowCaster, IceMagicCaster ...),CasterオブジェクトにSkillCasterスクリプトをアタッチする
2.Casterから発動するスキルの本体を作成する,作成したスキル本体はPrefab化してEffectPrefabsのスキルエフェクトフォルダ内に置く、そしてCasterのスクリプトのSkillPrefabにアタッチする
3.スキル本体が必要なComponent(見た目、ステータス、当たり判定、行為(移動など)): 当たり判定用のRigidbody,Box/Sphere Collider(isTriggerをtrueに)、エフェクトオブジェクト(Particle System)、必要なスクリプト(3.5に)
3.5スキル本体に必要なスクリプトをアタッチする, 例: スキルのステータスと種類を決めるSkillModel と  スキルの移動方法を決めるスクリプト(例: SkillForwardMove, SkillCircleMove...), 
5.大体のスキル本体はParticleエフェクトがあるので、Particleエフェクトを子供に追加する
6.CasterのCooldownが0の時に、スキル本体をInstantiateして発動する、ParticleSystemのPlay()を呼び出す、


 * */










