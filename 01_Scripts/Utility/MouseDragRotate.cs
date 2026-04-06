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
using UnityEngine.InputSystem;

public class MouseDragRotate : MonoBehaviour
{

    public Vector3 initRot  = Vector3.zero;
    public bool isDragModeEnabled = false;

    private void OnEnable()
    {
        EventManager.StartListening("ResetPetCameraAngle", ResetCameraAngle);
    }
     private void OnDisable()
    {
        EventManager.StopListening("ResetPetCameraAngle", ResetCameraAngle);
    }


    void Start()
    {
        transform.rotation = Quaternion.Euler(initRot);
        isDragModeEnabled = true;

    }

    void ResetCameraAngle()
    {
                transform.rotation = Quaternion.Euler(initRot);
    }

    void Update()
    {
        //if drag mouse right click to left or right , rotate the object Y axis
        if (isDragModeEnabled && Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * 7.7f;
            transform.Rotate(Vector3.up, -rotX, Space.World);
        }

        //also can use controller l and r shoulder to rotate the object Y axis

        //check if controller is connected
        if(Gamepad.current != null)
        {
              if (isDragModeEnabled && Input.GetButton("Fire1"))
        {
            float rotX = Input.GetAxis("Horizontal") * 7.7f;
            transform.Rotate(Vector3.up, -rotX, Space.World);
        }
        }

          




    }
}

