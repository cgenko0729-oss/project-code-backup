using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//選んだペットの情報を保持するマネージャー
public class PetSelectDataManager : SingletonA<PetSelectDataManager>
{
    //現在選択されているペットを保持するための変数
    public List<PetData> SelectedPets  = new List<PetData>();
 
    //{ get; private set; }

    //ペットの最大選択数
    public int MaxPets = 1;

    protected override void Awake()
    {
        if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void Update()
    {
        MaxPets = 1 + BuffManager.Instance.gobalPetSlotAdd;
    }

    //TitleSceneでペットが選択された時に呼び出す
    public bool AddPet(PetData petData)
    {
        //既に上限に達しているか、同じペットが既に追加されていたら追加しない
        if (SelectedPets.Count >= MaxPets 
           || SelectedPets.Contains(petData))
        {
            return false;
        }

        SelectedPets.Add(petData);
        Debug.Log(petData.petName + " を連れていくことにした。" +
                  "現在 " + SelectedPets.Count + " / " + MaxPets + " 体");
        return true;
    }

    //ペットを連れていくのをやめるときに呼び出す
    public void RemovePet(PetData petData)
    {
        if (SelectedPets.Contains(petData))
        {
            SelectedPets.Remove(petData);
            Debug.Log(petData.petName + " を連れて行かないことにした。" +
                      "現在 " + SelectedPets.Count + " / " + MaxPets + " 体");
        }
    }

    public void ResetPetList()
    {
        //1番目のペット以外をリムーブする
        if(SelectedPets.Count <= 1)return;

        //1番目のペットを保存しておく
        PetData leaderPet = SelectedPets[0];

        //リストをクリアしてから1番目のペットだけ追加し直す
        SelectedPets.Clear();
        SelectedPets.Add(leaderPet);
    }
}

