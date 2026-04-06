using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class UnScaleTimeUpdateer : MonoBehaviour
{
    // We create a shader property ID for "_UnscaledTime" for efficiency.
    private static readonly int UnscaledTimeProperty = Shader.PropertyToID("_UnscaledTime");

    void Update()
    {
        // This line sends the current unscaled time to ALL shaders
        // that have a "_UnscaledTime" property.
        Shader.SetGlobalFloat(UnscaledTimeProperty, Time.unscaledTime);
    }
}

