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

public class SkillFollowMouseMove : MonoBehaviour
{

    SkillModelBase modelBase;

    public Transform playerTrans;

     public Camera mainCam;
    public float fixPosY = 3.5f;
    public float homingSpeed = 7.7f;

    // A plane to represent the ground
    private Plane groundPlane;

    public bool canRotate = true;

    [Header("Controller Settings")]
    public float controllerSpeed = 20.0f; // How fast the cursor moves with stick
    public float stickDeadzone = 0.1f;
    // Internal variable to track the target position for both Mouse and Controller
    private Vector3 targetPos; 

    public float rotSpd = 35f;


    private float orbitRadius = 9.8f; // Distance from player
    private float orbitSpeed = 149.0f;
    private float currentOrbitAngle = 0f;

    void Start()
    {
        mainCam = Camera.main;
        // Initialize the plane. The first argument is the normal to the plane (Vector3.up for a horizontal plane),
        // and the second is a point on the plane.
        // We use a point with the desired fixed Y position.
        groundPlane = new Plane(Vector3.up, new Vector3(0, fixPosY, 0));
    
        // Initialize target to current position
        targetPos = transform.position;

        playerTrans = GameObject.FindWithTag("Player").transform;

        modelBase = GetComponent<SkillModelBase>();

    }

    private void OnEnable()
    {
        targetPos = transform.position;
    }

    void Update()
    {

        if((int)CameraViewManager.Instance.currentMode == 0)
        {
            Handle3DMove();
        }
        else
        {
            bool isControllerInput = Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > stickDeadzone;

            if(isControllerInput)
            {
                HandleControllerMovement();
            }
            else
            {
                // Create a ray from the camera going through the mouse position
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

                // Variable to store the distance from the camera to the intersection point
                float distance;

                // Check if the ray intersects with the ground plane
                if (groundPlane.Raycast(ray, out distance))
                {
                        // Get the point of intersection
                        Vector3 worldPos = ray.GetPoint(distance);

                        // Homing toward the target position on the plane
                        transform.position = Vector3.Lerp(transform.position, worldPos, homingSpeed * Time.deltaTime);

                        // Ensure the Y position is fixed (though the raycast should already handle this)
                        Vector3 pos = transform.position;
                        pos.y = fixPosY;
                        transform.position = pos;

                        // Also rotate toward move direction
                        Vector3 dir = (worldPos - transform.position).normalized;
                        dir.y = 0; // Keep rotation level
                        if (dir != Vector3.zero && canRotate)
                        {
                            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
                            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 14 * Time.deltaTime);
                        }
                    }
                }
        }


            


        
    }


    void HandleControllerMovement()
    {
        Vector2 input = Gamepad.current.rightStick.ReadValue();

        // 1. Calculate Direction Relative to Camera
        // This ensures pushing "UP" on the stick moves the skill to the "TOP" of the screen
        // regardless of camera rotation.
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        // Flatten Y to ensure we only move in X and Z
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Combine inputs
        Vector3 moveDir = (camForward * input.y + camRight * input.x);

        // 2. Update the Virtual Target Position
        // We add to the existing position (creating a virtual cursor)
        targetPos += moveDir * controllerSpeed * Time.deltaTime;
        targetPos.y = fixPosY; // Enforce Height

        // 3. Clamp to Screen Bounds (Prevent going off-screen)
        // Convert world point to Viewport point (0,0 is bottom-left, 1,1 is top-right)
        Vector3 vp = mainCam.WorldToViewportPoint(targetPos);
        
        // Clamp with a small margin (0.02 to 0.98) so it doesn't clip the edge
        vp.x = Mathf.Clamp(vp.x, 0.02f, 0.98f);
        vp.y = Mathf.Clamp(vp.y, 0.02f, 0.98f);

        // Convert back to World Space using the Plane to maintain exact Y height
        Ray ray = mainCam.ViewportPointToRay(vp);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            targetPos = ray.GetPoint(distance);
        }

        // 4. Apply Movement & Rotation (Same math as your mouse logic)
        // Lerp
        transform.position = Vector3.Lerp(transform.position, targetPos, homingSpeed * Time.deltaTime);
        
        // Fix Y
        Vector3 currentPos = transform.position;
        currentPos.y = fixPosY;
        transform.position = currentPos;

        // Rotate
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero && canRotate)
        {
            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotSpd * Time.deltaTime);
        }
    }

    void Handle3DMove()
    {
       // 1. Increment the angle based on time and speed
        currentOrbitAngle += orbitSpeed * modelBase.skillSpeed * Time.deltaTime;

        // Keep angle clean (0-360)
        if (currentOrbitAngle > 360f) currentOrbitAngle -= 360f;

        // 2. Calculate the "Ideal" position on the circle (The Target)
        float rad = currentOrbitAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * orbitRadius;
        float z = Mathf.Sin(rad) * orbitRadius;

        Vector3 center = playerTrans.position;
        targetPos = new Vector3(center.x + x, fixPosY, center.z + z);

        // 3. Apply Movement (Smooth Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPos, homingSpeed * Time.deltaTime);

        // 4. Apply Rotation: Face the direction we are ACTUALLY moving
        // Instead of using the "Math Tangent", we use (Target - Current)
        // This ensures that if the player dashes, the object faces the new spot it needs to reach.
        Vector3 moveDir = (targetPos - transform.position).normalized;
        moveDir.y = 0; // Keep rotation level (no tilting up/down)

        // Only rotate if we have a valid direction
        if (moveDir != Vector3.zero && canRotate)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotSpd * Time.deltaTime);
        }
    }





}

