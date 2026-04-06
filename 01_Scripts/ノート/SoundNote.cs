using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

public class SoundNote : MonoBehaviour
{
    /*
     サウンドエフェクト(SE)を追加する流れ

    -プール化する場合
    1.SoundListクラスのEnumnの一番下に新しいenumを追加 (e.g fireballSe)
    2.サウンドのScriptableObjectを追加,07_ScriptableObject/SoundSoフォルダ内で右クリック->Create->SoundDataを選択
    3.作成したSoundDataを選択し、Inspectorで以下を設定
        -id: enumの名前と同じにする (e.g fireballSe)
        -Volume: 音量を設定 (0.1f ~ xxf)
        -AudioClip: .wavサウンドアセットを付ける
    4.=======SoundEffectList========の内で先ほど作ったSoundDataドラッグアンドドロップでSoundLibraryで入れる,=======SoundEffectList========を一回overrideでPrefabsを更新する
    5.最後はサウンドを流したいクラス内で:        
    SoundEffect.Instance.Play(SoundList.fireballSe); で流すことができる


    -プール化しない場合(簡単、重たい)
    1.サウンドを流したいクラスのスクリプトに:
    public AudioClip newSe; //変数を追加
    2.フォルダ内の.wavサウンドアセットを付ける
    3.SoundEffectManagerのPlayOneSoundを使ってクラス内に流す:
    SoundEffect.Instance.PlayOneSound(newSe, 1f); //引数は　流したいAudioClipと音量(ここはnewSeサウンドを1fの音量で流す)

     */


}

