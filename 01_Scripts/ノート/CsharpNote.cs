using UnityEngine;
using UnityEngine.SceneManagement;      //シーン関係のクラスを使用するために必要
using UnityEngine.UI;                   // UI関係のクラスを使用するために必要
using Unity.Profiling;                  //Profiler関係のクラスを使用するために必要
using TMPro;                            // TextMeshPro関係のクラスを使用するために必要
using System.Collections;               //コルーチン関係のクラスを使用するために必要
using System.Collections.Generic;       //List関係のクラスを使用するために必要
using System.Linq;                    　//LINQ関係のクラスを使用するために必要


public class CsharpNote : MonoBehaviour
{

    // C#の基本

    //dictionary string int 
    Dictionary<string, int> testDictionary = new Dictionary<string, int>();

    List<int>testList = new List<int>();

    enum PlayerState //列挙型
    {
        Idle,
        Walk,
        Run,
        Jump,
        Attack,
        Dead
    }
    PlayerState playerState = PlayerState.Idle; //列挙型の変数を定義する

    static int gobalInt = 0;
    const float hpMax = 100;
    readonly float mpMax = 100;

    //Array関係
    int[] testArray = new int[10]; //int型の配列を定義する
    
    string moji;


    //Interface

    void Start()
    {
        
    }

    void Update()
    {

        //Listの操作
        testList.Add(1);                           //Listに要素を追加する
        testList.Remove(1);                        //Listから要素を削除する
        testList.Clear();                          //Listの要素をすべて削除する
        testList.Contains(1);                      //Listに要素が含まれているかどうかを判定する
        testList.IndexOf(1);                       //Listの要素のインデックスを取得する
        testList.Sort();                           //Listの要素をソートする
        testList.Reverse();                        //Listの要素を逆順にする
        testList.AddRange(new int[] { 1, 2, 3 });  //Listに配列の要素を追加する    
        testList.RemoveRange(0, 1);                //Listの要素を削除する　
        testList.Insert(0, 1);                     //Listの指定した位置に要素を追加する
        testList.InsertRange(0, new int[] {1, 2}); //Listの指定した位置に配列の要素を追加する
       
        //dictionaryの操作
        testDictionary.Add("test", 1);                      //Dictionaryに要素を追加する
        testDictionary.Remove("test");                      //Dictionaryから要素を削除する
        testDictionary.Clear();                             //Dictionaryの要素をすべて削除する
        testDictionary.ContainsKey("test");                 //Dictionaryにキーが含まれているかどうかを判定する
        testDictionary.ContainsValue(1);                    //Dictionaryに値が含まれているかどうかを判定する
        testDictionary.TryGetValue("test", out int value1); //Dictionaryから値を取得する
        testDictionary.TryGetValue("test", out int value2); //Dictionaryから値を取得する
        testDictionary.Keys.ToList();                       //Dictionaryのキーを取得する
        testDictionary.Values.ToList();                     //Dictionaryの値を取得する
        testDictionary.Select(x => x.Key).ToList();         //Dictionaryのキーを取得する
        testDictionary.Select(x => x.Value).ToList();       //Dictionaryの値を取得する
        testDictionary.Where(x => x.Value > 0).ToList();    //Dictionaryの値が0より大きい要素を取得する
        testDictionary.OrderBy(x => x.Value).ToList();      //Dictionaryの値を昇順にソートする
        testDictionary.GroupBy(x => x.Key).ToList();        //Dictionaryのキーでグループ化する
        
        foreach (var item in testDictionary) //Dictionaryの要素をすべて表示する
        {
            Debug.Log(item.Key + ":" + item.Value);
        }

        //Arrayの操作
        testArray[0] = 1;                   //Arrayの要素を設定する
        testArray[0].CompareTo(1);          //Arrayの要素を比較する
        int arrayLength = testArray.Length; //Arrayの要素数を取得する
        testArray.GetValue(0);              //Arrayの要素を取得する
        testArray.SetValue(1, 0);           //Arrayの要素を設定する
        testArray.CopyTo(testArray, 0);     //Arrayをコピーする

        Vector3.RotateTowards(Vector3.forward, Vector3.right, 1f, 1f); //Vector3の回転を取得する

        switch (moji) {
            case "a":
                Debug.Log("aです");
                break;
            case "b":
                Debug.Log("bです");
                break;
            case "c":
                Debug.Log("cです");
                break;
            default:
                Debug.Log("defaultです");
                break;
        }

       


    }




}