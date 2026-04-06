using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class CameraFirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerBody; // Assign your Player object to this in the Inspector

    [Header("Settings")]
    [SerializeField]
    private float mouseSensitivity = 100f;

    private float xRotation = 0f; // Stores the current up/down rotation of the camera

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        playerBody = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // 1. Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 2. Rotate the Player Body (Left/Right - Yaw)
        // We rotate the entire player object around the Y-axis.
        playerBody.Rotate(Vector3.up * mouseX);

        // 3. Rotate the Camera (Up/Down - Pitch)
        // For looking up and down, we only rotate the camera, not the whole player.
        xRotation -= mouseY;
        
        // Clamp the rotation to prevent the player from flipping the camera over
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the rotation to the camera's local rotation.
        // We use localRotation because the camera is a child of the player.
        transform.localRotation = Quaternion.Euler(0f, xRotation, 0f);
    }
}

