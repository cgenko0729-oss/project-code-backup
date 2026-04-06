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

public class CutSceneNote : MonoBehaviour
{

    /*
     1.particle has to set Unscaled Time 
    2. camera shake has to set Unscaled Time
    3.Script: 
    Director hold Signal Receiver ,one Reaction = one Script Behaviour
    Add Signal Track , inside , add signal emitter
    config: Emit Signal(reaction), Drag-in the script object 
    4. TimeLine Main component
    -Director : playable Director
    -CameraGroup: CinemachineCam , for CamShake(ChannelPerlin + Cinemachine Shaker)
    -ScriptAction: atached script
    -Actor List : model with Animator + ParticleEffect
    -Audio List : AudioSource

    5.Track-Intro 
    -CamTrack
    -Animation Track: animation-playing + transform controlling
    -Activation Track: enable a particle effect / Object 
    -Audio Track: AudioSource playing
    -Signal Track: Emit Signal to trigger a reaction
   
    











     
     */


}

