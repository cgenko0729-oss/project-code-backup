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

public class CutSceneTurnipaAction : MonoBehaviour
{

    public GameObject actorA;
    public GameObject actorB;
    public GameObject actorC;
    public GameObject actorD;

    public ParticleSystem laserPsA;
    public ParticleSystem laserPsB;
    public ParticleSystem laserPsC;
    public ParticleSystem laserPsD;

    private float stepSeconds = 0.779f;

    public AudioClip teleportSe;

    // This is the method youÅfll hook from Timeline (must be public void)
    public void StartParticlesAndDeactivateActors()
    {
        RunSequence().Forget(); // fire-and-forget
    }

    // The actual async sequence
    private async UniTask RunSequence()
    {
        // If your cutscene sets Time.timeScale = 0, use ignoreTimeScale = true
        var ms = (int)(stepSeconds * 1000);

        if (!laserPsA.isPlaying) laserPsA.gameObject.SetActive(true);
        laserPsA.Play(); actorA.SetActive(false);
        SoundEffect.Instance.PlayOneSound(teleportSe);
        await UniTask.Delay(ms, ignoreTimeScale: true);

        if (!laserPsB.isPlaying) laserPsB.gameObject.SetActive(true);
        laserPsB.Play(); actorB.SetActive(false);
        SoundEffect.Instance.PlayOneSound(teleportSe);
        await UniTask.Delay(ms, ignoreTimeScale: true);

        if (!laserPsC.isPlaying) laserPsC.gameObject.SetActive(true);
        laserPsC.Play(); actorC.SetActive(false);
        SoundEffect.Instance.PlayOneSound(teleportSe);
        await UniTask.Delay(ms, ignoreTimeScale: true);

        if (!laserPsD.isPlaying) laserPsD.gameObject.SetActive(true);
        laserPsD.Play(); actorD.SetActive(false);
        SoundEffect.Instance.PlayOneSound(teleportSe);
    }

}

