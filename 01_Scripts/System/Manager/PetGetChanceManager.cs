using System.Collections.Generic;
using TigerForge;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PetGetChanceManager : SingletonA<PetGetChanceManager>
{
    //でもモード

    [Header("デモモードのON/OFF")]
    [SerializeField]
    private bool isDemoMode = false;

    [Header("現在のプレイヤーの持っているペットのデータベース")]
    [SerializeField]
    public PetDataBase playerPetDataBase;

    [Header("全てのペットが登録されたマスターリスト（図鑑）")]
    [SerializeField]
    public PetDataBase masterPetDataBase;

    [Header("現在仲間にしようとしてるペットのタイプ")]
    [SerializeField]
    public PetType tryingGetPetType = PetType.None;

    [Header("現在仲間にしようとしてるペットの仲間になる確率(%)")]
    [SerializeField]
    private float tryingGetPetChance = 0f;

    [Header("魂のオブジェクト")]
    [SerializeField]
    private GameObject soulObjectPrefab;

    [Header("難易度別のペット獲得確率")]
    public float normalGetChanceModifier = 1.0f;

    [Header("デモ用のペット")]
    [SerializeField]
    private List<PetData> demoPetList= new List<PetData>();

    //ゲーム実行中にプレイヤーが仲間にしたペットを保持するリスト
    public List<PetData> getPetsListInStage = new List<PetData>();


    public EasyFileSave efs;
    private const string SAVE_FILE_NAME = "PlayerPetsData_Finalversion";
    private const string SAVE_DATA_KEY = "UnlockedPetNames";

    protected override void Awake()
    {       
        base.Awake();
        efs = new EasyFileSave(SAVE_FILE_NAME);
      
        if (isDemoMode)
        {

            UnlockAllPets();
        }
        else
        {
            LoadGetPets();      
        }
    }

    private void Start()
    {
       
    }

    // ゲームが終了する直前に自動的に呼ばれるメソッド
    private void OnApplicationQuit()
    {
        SaveGetPets();
    }

    [ContextMenu("Unlock All Petsss")]
    public void UnlockAllPets()
    {    
        //プレイヤーのリストを一度クリア
        playerPetDataBase.allPetData.Clear();

        //マスターリスト（図鑑）にいる全てのペットをプレイヤーリストに追加
        foreach (PetData pet in demoPetList)
        {
            // カッパだけをリストに追加
            playerPetDataBase.allPetData.Add(pet);

            //それ以外のペットの開放状態も更新
            UpdateAllPetsLockState();
        }
  
        //デバッグで変更した状態をセーブしておく
        SaveGetPets();
    }

    [ContextMenu("Lock Deer Pet")]
    public void LockDeerPet()
    {
        
        foreach (PetData pet in demoPetList)
        {
            if(pet.petType == PetType.GoldenDeer)
            {
                playerPetDataBase.allPetData.Remove(pet);
            }
            UpdateAllPetsLockState();
        }
        SaveGetPets();

    }

    [ContextMenu("Reset Pets")]
    public void ResetPets()
    {
        //プレイヤーのリストを完全に空にする
        playerPetDataBase.allPetData.Clear();

        //マスターリストから初期ペット（カッパ）を探す
        PetData kappaData = masterPetDataBase.allPetData.Find(p => p.name == "PetData_Kappa");
        if (kappaData != null)
        {
            // カッパだけをリストに追加
            playerPetDataBase.allPetData.Add(kappaData);
        }

        //初期状態をセーブして、古いセーブデータを上書きする
        SaveGetPets();
    }

    public void GetChancePet(PetData petData,float petGetChance,Vector3 soulPos)
    {
        //nullチェックと既に仲間になっているかのチェック
        if (petData == null || playerPetDataBase == null)
        {
            return;
        }

        if (playerPetDataBase.allPetData.Contains(petData))
        {
            return; // 既に仲間になっている
        }

        //確率の抽選
        float randomValue = Random.Range(0f, 100f);

        //現在の難易度を取得
        DifficultyType currentDifficulty = StageManager.Instance.mapData.stageDifficulty;

        //難易度に応じた確率の修正値を取得
        float difficultyModifier = ChangeGetChanceDifficulty(currentDifficulty);

        //基礎倍率
        float baseModifier = 1.0f;

        //幸運ステータスによる確率の修正(%に修正)
        float luckModifier = (baseModifier + SkillManager.Instance.luck / 100);

        //最終的なペット獲得確率を計算
        float finalPetGetChance = petGetChance *luckModifier* difficultyModifier;

        //確率を抽出(デバック)
        tryingGetPetChance = finalPetGetChance;

        if (randomValue < finalPetGetChance)
        {
            //タイプを抽出
            tryingGetPetType = petData.petType;

            playerPetDataBase.allPetData.Add(petData);
            getPetsListInStage.Add(petData);

            petData.isUnlocked = true;

            //魂オブジェクト生成
            GameObject soulObj = Instantiate(soulObjectPrefab, soulPos, Quaternion.identity);
            PetSoulAction soulAction= soulObj.GetComponent<PetSoulAction>();
            soulAction.petType = petData.petType;

            SaveGetPets();      
        }
    }

    public void SaveGetPets()
    {
        List<string> petNamesToSave = new List<string>();
        //保存するのは、プレイヤーが持っているリストの中身
        foreach (var pet in playerPetDataBase.allPetData)
        {
            petNamesToSave.Add(pet.name); //アセット名を保存
        }

        efs.Add(SAVE_DATA_KEY, petNamesToSave);
        efs.Save();
    }

    public void LoadGetPets()
    {
        playerPetDataBase.allPetData.Clear();

        if (efs.Load())
        {
            List<string> loadedPetNames = efs.GetList<string>(SAVE_DATA_KEY);

            foreach (var petName in loadedPetNames)
            {
                //マスターリスト（図鑑）から、保存されていた名前のペットを探す
                PetData petData = masterPetDataBase.allPetData.Find(p => p.name == petName);
                if (petData != null)
                {
                    //見つけたペットを、プレイヤーのリストに再構築していく
                    playerPetDataBase.allPetData.Add(petData);
                }
            }
        }

        //初回起動はカッパ助を入れる
        if (playerPetDataBase.allPetData.Count == 0)
        {
            //マスターリストから「カッパ」のデータを検索
            PetData kappaData = masterPetDataBase.allPetData.Find(p => p.name == "PetData_Kappa");

            if (kappaData != null)
            {
                //プレイヤーリストに追加
                playerPetDataBase.allPetData.Add(kappaData);

                //初期ペットを追加したら、すぐにセーブしておく
                SaveGetPets();
            }
            else
            {
                return;
            }
        }

        UpdateAllPetsLockState();
    }

    //難易度に応じたペット獲得確率の修正値を取得するメソッド
    public float ChangeGetChanceDifficulty(DifficultyType difficulty)
    {
        switch (difficulty)
        {
            case DifficultyType.Normal:
                return 1.0f; //ノーマル
            case DifficultyType.Hard:
                return 1.1f; //ハード
            case DifficultyType.Nightmare:
                return 1.2f; //ナイトメア
            case DifficultyType.Hell:
                return 1.3f; //ヘル
            default:
                return 1.0f; //不明な場合は1倍
        }
    }

    private void UpdateAllPetsLockState()
    {
        //マスターリストの全ペットを一旦未開放状態にする
        foreach (var pet in masterPetDataBase.allPetData)
        {
            pet.isUnlocked = false;
        }

        //プレイヤーが持っているペットだけを開放済みに上書きする
        foreach (var ownedPet in playerPetDataBase.allPetData)
        {
            ownedPet.isUnlocked = true;
        }
    }

     public void ClearStagePetList()
    {
        getPetsListInStage.Clear();
    }

    public List<PetData> GetPetsListInNowStage()
    {
        return getPetsListInStage;
    }

    public List<PetData> GetDemoPetList()
    {
        return demoPetList;
    }
}
