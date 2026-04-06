#if UNITY_EDITOR

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

[ExecuteAlways]                     // draw in Edit Mode and Play Mode
[RequireComponent(typeof(BoxCollider))]
public class ColliderVisuallizer : MonoBehaviour
{
    public bool onlyWhenSelected = false;
    public bool drawFill = false;
    public Color wireColor = Color.green;
    public Color fillColor = new Color(0f, 1f, 0f, 0.08f);

    BoxCollider box;

    void OnEnable() { box = GetComponent<BoxCollider>(); }
    void OnValidate() { box = GetComponent<BoxCollider>(); }

    void OnDrawGizmos() {
        if (!onlyWhenSelected) Draw();
    }

    void OnDrawGizmosSelected() {
        if (onlyWhenSelected) Draw();
    }

    void Draw()
    {
        if (box == null || box.sharedMaterial == null) { /* ok if null */ }

        // Save state
        var prevColor = Gizmos.color;
        var prevMatrix = Gizmos.matrix;

        // Match colliderÅfs pose & scale exactly
        Gizmos.matrix = transform.localToWorldMatrix;

        // Center/size are in the colliderÅfs local space
        if (drawFill) {
            Gizmos.color = fillColor;
            Gizmos.DrawCube(box.center, box.size);
        }

        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(box.center, box.size);

        // Restore state
        Gizmos.matrix = prevMatrix;
        Gizmos.color = prevColor;
    }

}

#endif
