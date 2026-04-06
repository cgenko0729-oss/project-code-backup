using UnityEngine;

public class BGMLoopController : MonoBehaviour
{
    [Header("ループしたい曲\n※AudioSorceのループのチェックボックスは外すこと")]
    public AudioSource audioSource;

    [Header("ループの開始地点（分・秒）")]
     public int loopStartMinutes;
     public float loopStartSeconds;

    [Header("ループの終了地点（分・秒）")]
    public int loopEndMinutes;
    public float loopEndSeconds;

    private float minutesPerSeconds = 60f; 
    private int loopStartSamples;          //ループ開始地点のサンプル数
    private int loopEndSamples;            //ループ終了地点のサンプル数
    private int loopLengthSamples;         //ループ間の区画

    void Start()
    {
        //AudioClipのサンプルレートを取得
        int frequency = audioSource.clip.frequency;

        //それぞれの分+秒をfloatに変換
        float TotalStartSamples = loopStartMinutes * minutesPerSeconds + loopStartSeconds;
        float TotalEmdSamples = loopEndMinutes * minutesPerSeconds + loopEndSeconds;

        //秒 → サンプル数に変換し、
        //指定したサンプル数がAudioSource全体のサンプル数を超えないようにする
        loopStartSamples = Mathf.Clamp((int)(TotalStartSamples * frequency), 0, audioSource.clip.samples);
        loopEndSamples = Mathf.Clamp((int)(TotalEmdSamples * frequency), 0, audioSource.clip.samples);

        //ループする区間の長さを求める
        //ループ終了地点-ループ開始地点で求められる
        loopLengthSamples = loopEndSamples - loopStartSamples; 
    }

    // Update is called once per frame
    void Update()
    {
        //AudioSourceのサンプル数がloopEndSamplesを超えた時
        if (audioSource.timeSamples >= loopEndSamples)
        {
            //AudioSourceのサンプル数をloopLengthSample分巻き戻す
            audioSource.timeSamples -= loopLengthSamples;
        }
    }
}
