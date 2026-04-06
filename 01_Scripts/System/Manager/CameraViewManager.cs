using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class CameraViewManager : Singleton<CameraViewManager>
{
    // Enum to define the possible camera modes
    public enum CameraMode
    {
        CloseView,
        TacticView,
        Transitioning
    }

    [Header("General Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private KeyCode switchKey = KeyCode.T;
    public CameraMode currentMode = CameraMode.CloseView;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.8f;

    // --- Variables from CameraAngleController (Close View) ---
    [Header("Close View Settings")]
    [SerializeField] private float gamepad_zoomSpeed = 10.0f; 
    [SerializeField] private float close_minDistance = 5.0f; // Minimum zoom (Closest)
    [SerializeField] private float close_maxDistance = 10.0f; // Maximum zoom (Farthest)
    [SerializeField] private float close_zoomSpeed = 2.0f;
    [SerializeField] private float close_distance = 10f;
    [SerializeField] private float close_smoothTime = 0.1f;
    [SerializeField] private float close_rotationSpeed = 5.0f;
    [SerializeField] private float close_minVerticalAngle = -20.0f;
    [SerializeField] private float close_maxVerticalAngle = 80.0f;

    [SerializeField] private float gamepad_rotationSpeed = 150.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    [Tooltip("The key to press to reset the camera behind the player.")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [Tooltip("How long the camera reset animation takes.")]
    public  float resetDuration = 0.4f;
    //[Tooltip("The vertical angle (pitch) the camera will have after resetting.")]
    //[SerializeField] private float reset_pitch = 15.0f;
     public bool isResetting = false; // Flag to check if we are currently in the reset coroutine
    private Coroutine resetCoroutine;

    // --- Variables from CameraController (Tactic View) ---
    [Header("Tactic View Settings")]
    [SerializeField] private Vector3 tactic_offset = new Vector3(0f, 15f, -10f);
    [SerializeField] private Vector3 tactic_rotationEuler = new Vector3(45f, 0f, 0f); // Storing as Euler for the inspector
    [SerializeField] private float tactic_smoothTime = 0.2f;

    // Shared private variables
    private Vector3 velocity = Vector3.zero;
    private Coroutine transitionCoroutine;

    [SerializeField] private Volume globalVolume;
    private DepthOfField Dof;

    public PlayerController playerController;

    public bool disableRotation = false;

    public bool isSwitchModeEnabled = true;

    public bool isPanning = false; // Flag to check if we are in the quest pan cinematic
    private Coroutine panCoroutine;

    public AudioClip changeViewSe;

    private void Start()
{
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        globalVolume.profile.TryGet(out Dof);

    if (target == null)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("CameraManager: Player target not found! Please tag your player object with 'Player'.");
            this.enabled = false;
            return;
        }
    }

    // Initialize rotation based on the camera's starting orientation
    Vector3 initialEulerAngles = transform.eulerAngles;
    yaw = initialEulerAngles.y;
    pitch = initialEulerAngles.x;

    // Instantly set the camera to its starting mode position to avoid an initial transition
    if (currentMode == CameraMode.CloseView)
    {
        HandleCloseView();
    }
    else // TacticView
    {
        HandleTacticView();
    }
}

    private void LateUpdate()
{
    if (!target || isPanning) return;

        // --- 1. Handle Input to Switch Modes ---
        if ((Input.GetKeyDown(switchKey) || (Gamepad.current != null && Gamepad.current.leftStickButton.wasPressedThisFrame)) && currentMode != CameraMode.Transitioning && !isResetting)
        {
            SwitchMode();
        }

        // --- 2. Execute Logic Based on Current Mode ---
        switch (currentMode)
    {
        case CameraMode.CloseView:
            if (Input.GetKeyDown(resetKey) && !isResetting)
            {
                // If a reset is already running, stop it before starting a new one
                if (resetCoroutine != null)
                {
                    StopCoroutine(resetCoroutine);
                }

                EventManager.EmitEvent("OnCameraResetComplete");
                resetCoroutine = StartCoroutine(ResetCameraRotationCoroutine());
            }

            // Only allow mouse rotation if we are NOT in the middle of a reset
            if (!isResetting)
            {
                HandleCloseViewMouseInput();
                HandleCloseViewZoom(); 
            }

            // This part always runs to keep the camera positioned correctly
            UpdateCloseViewCameraState();
            break;

        case CameraMode.TacticView:
            HandleTacticView();
            break;

        case CameraMode.Transitioning:
            // Do nothing here, the coroutine is handling the movement
            break;
    }
}

    private void HandleCloseViewZoom()
    {
       float zoomDelta = 0f;

        // 1. Mouse Scroll Logic
        // Scroll Up (>0) -> Decrease Distance (Zoom In)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        zoomDelta -= scrollInput * close_zoomSpeed;

        // 2. Controller Shoulder Logic
        if (Gamepad.current != null)
        {
            // Right Shoulder -> Zoom In (Decrease Distance)
            if (Gamepad.current.rightShoulder.isPressed)
            {
                // We use Time.deltaTime because buttons are held down continuously
                zoomDelta -= gamepad_zoomSpeed * Time.deltaTime;
            }

            // Left Shoulder -> Zoom Out (Increase Distance)
            if (Gamepad.current.leftShoulder.isPressed)
            {
                zoomDelta += gamepad_zoomSpeed * Time.deltaTime;
            }
        }

        // 3. Apply Zoom
        if (zoomDelta != 0f)
        {
            close_distance += zoomDelta;
            
            // Clamp ensures we stay within the 5 to 10 range
            close_distance = Mathf.Clamp(close_distance, close_minDistance, close_maxDistance);
        }
    }

    private void HandleCloseViewMouseInput()
    {
        float calculatedYaw = 0f;
        float calculatedPitch = 0f;
        bool shouldUpdateCamera = false;

        // --- 1. Gather Inputs ---
        bool isMouseRightClickHeld = Input.GetMouseButton(1);
        bool isGamepadLeftTriggerHeld = false;
        Vector2 gamepadStickInput = Vector2.zero;

        if (Gamepad.current != null)
        {
            // Check Trigger threshold
            if (Gamepad.current.leftTrigger.ReadValue() > 0.1f)
            {
                isGamepadLeftTriggerHeld = true;
            }
            // Read stick value
            gamepadStickInput = Gamepad.current.rightStick.ReadValue();
        }

        // --- 2. Logic Decision Tree ---

        // SCENARIO A: Gamepad Left Trigger is HELD
        // Effect: Player rotates with camera. Camera rotates Horizontally ONLY.
        if (isGamepadLeftTriggerHeld)
        {
            playerController.useMouseRotation = true;
            //shouldUpdateCamera = true;

            if (gamepadStickInput.magnitude > 0.1f)
            {
                // Only calculate Yaw (Horizontal). Pitch remains 0.
                calculatedYaw = gamepadStickInput.x * gamepad_rotationSpeed * Time.deltaTime;
                calculatedPitch = 0f; 
            }
        }
        // SCENARIO B: Gamepad Right Stick is MOVED (and Trigger is NOT held)
        // Effect: Player does NOT rotate. Camera rotates Freely (Horizontal + Vertical).
        else if (Gamepad.current != null && gamepadStickInput.magnitude > 0.1f)
        {
            playerController.useMouseRotation = false;
            shouldUpdateCamera = true;

            calculatedYaw = gamepadStickInput.x * gamepad_rotationSpeed * Time.deltaTime;
            calculatedPitch = gamepadStickInput.y * gamepad_rotationSpeed * Time.deltaTime;
        }
        // SCENARIO C: Mouse Right Click (Fallback)
        // Effect: Player rotates with camera. Camera rotates Freely.
        
        //else if (isMouseRightClickHeld)
        else if(isMouseRightClickHeld)
        {
            playerController.useMouseRotation = true;
            shouldUpdateCamera = true;
            calculatedYaw = Input.GetAxis("Mouse X") * close_rotationSpeed;
            calculatedPitch = Input.GetAxis("Mouse Y") * close_rotationSpeed;
        }
        // SCENARIO D: No Input
        else
        {
            playerController.useMouseRotation = false;
            shouldUpdateCamera = true;
            calculatedYaw = Input.GetAxis("Mouse X") * close_rotationSpeed;
            calculatedPitch = Input.GetAxis("Mouse Y") * close_rotationSpeed;
        }

        // --- 3. Apply Rotation to Camera ---
        if (shouldUpdateCamera)
        {
            yaw += calculatedYaw;
            
            // Subtract to look up (Standard), Add to look up (Inverted)
            pitch -= calculatedPitch; 
            
            pitch = Mathf.Clamp(pitch, close_minVerticalAngle, close_maxVerticalAngle);
        }
    }

    private void UpdateCloseViewCameraState()
{
    // Calculate desired rotation and position based on current yaw and pitch
    Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
    Vector3 desiredPosition = target.position - (desiredRotation * Vector3.forward * close_distance);

    // Smoothly move the camera to the desired position
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, close_smoothTime);

    // Always look at the target
    transform.LookAt(target);
}

    private IEnumerator ResetCameraRotationCoroutine()
{
    
    isResetting = true;

    float elapsedTime = 0f;

    // Get the starting rotation values
    float startYaw = yaw;
    //float startPitch = pitch;

    // The target yaw should match the player's forward direction
    float targetYaw = target.eulerAngles.y;

    while (elapsedTime < resetDuration)
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / resetDuration;
        t = t * t * (3f - 2f * t); // SmoothStep easing for a nicer feel

        // --- IMPORTANT: Use LerpAngle for Yaw ---
        // Mathf.LerpAngle correctly handles wrapping around 360 degrees.
        // For example, going from 350‹ to 10‹ is a 20‹ turn, not a -340‹ turn.
        yaw = Mathf.LerpAngle(startYaw, targetYaw, t);

        // Standard Lerp is fine for pitch as it doesn't wrap
        //pitch = Mathf.Lerp(startPitch, reset_pitch, t);

        yield return null; // Wait for the next frame
    }

    // Snap to the final values to ensure accuracy
    yaw = targetYaw;
    //pitch = reset_pitch;

    isResetting = false; // We're done resetting
    resetCoroutine = null;

   
}

    private void HandleCloseView()
{
    // Handle mouse rotation input
    if (Input.GetMouseButton(1)) // Right mouse button
    {
        
    }

        if (disableRotation) return;

        yaw += Input.GetAxis("Mouse X") * close_rotationSpeed;
        pitch -= Input.GetAxis("Mouse Y") * close_rotationSpeed;
        pitch = Mathf.Clamp(pitch, close_minVerticalAngle, close_maxVerticalAngle);

    // Calculate desired rotation and position
    Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
    Vector3 desiredPosition = target.position - (desiredRotation * Vector3.forward * close_distance);

    // Smoothly move the camera to the desired position
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, close_smoothTime);

    // Always look at the target
    transform.LookAt(target);
}

private void HandleTacticView()
{
    Vector3 desiredPosition = target.position + tactic_offset;
    Quaternion desiredRotation = Quaternion.Euler(tactic_rotationEuler);

    // Smoothly move position and rotation
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, tactic_smoothTime);
    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.unscaledDeltaTime / tactic_smoothTime);


}

    public void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        disableRotation = true;
        
    }

    public void ShowAndUnlockCursor()
    {
        //center the cursor in the middle of the screen so no sudden mouse movement to switch the screen 

        if (currentMode != CameraMode.CloseView) return;


            Cursor.lockState = CursorLockMode.None;
       Cursor.visible = true;

        disableRotation = false;

        //var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        //Mouse.current.WarpCursorPosition(center);
    }

    private void SwitchMode()
{

       if(!isSwitchModeEnabled) return;
       if(SkillManager.Instance.isLevelUpWindowOpen) return;

        SoundEffect.Instance.PlayOneSound(changeViewSe, 1f);

        // Determine the target mode
        CameraMode targetMode = (currentMode == CameraMode.CloseView) ? CameraMode.TacticView : CameraMode.CloseView;

    // Stop any existing transition coroutine before starting a new one
    if (transitionCoroutine != null)
    {
        StopCoroutine(transitionCoroutine);
    }

    if(currentMode == CameraMode.CloseView)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Dof.active = false;

            //var Cam = Camera.main;
            //Cam.farClipPlane = 49f;

            playerController.useMouseRotation = true;

            EventManager.EmitEventData(GameEvent.ChangeCameraMode, false);

        }
        else
        {
            playerController.useMouseRotation = false;
            //EventManager.EmitEvent("RotationModeChanged", playerController.useMouseRotation);

            //Cursor.lockState = CursorLockMode.Locked; //disabled to fix levleup bug
            Cursor.visible = false;

            Dof.active = true;

            var Cam = Camera.main;
           Cam.farClipPlane = 1400f;

            EventManager.EmitEventData(GameEvent.ChangeCameraMode, true);

        }

    // Start the new transition
    transitionCoroutine = StartCoroutine(TransitionToMode(targetMode));
}

private IEnumerator TransitionToMode(CameraMode targetMode)
{
    currentMode = CameraMode.Transitioning;

    float elapsedTime = 0f;
    Vector3 startPosition = transform.position;
    Quaternion startRotation = transform.rotation;

    // The destination position and rotation need to be calculated *each frame*
    // because the player (target) might be moving during the transition.
    while (elapsedTime < transitionDuration)
    {
        // Calculate the destination for this frame
        Vector3 targetPosition;
        Quaternion targetRotation;

        if (targetMode == CameraMode.TacticView)
        {
            targetPosition = target.position + tactic_offset;
            targetRotation = Quaternion.Euler(tactic_rotationEuler);
        }
        else // Transitioning to CloseView
        {
            // For CloseView, the target rotation depends on the current 'yaw' and 'pitch',
            // and the position depends on that rotation.
            Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
            targetPosition = target.position - (desiredRotation * Vector3.forward * close_distance);
            // The final rotation will just be looking at the target
            targetRotation = Quaternion.LookRotation(target.position - targetPosition);
        }

        // Calculate the interpolation factor (0 to 1)
        float t = elapsedTime / transitionDuration;
        // Optional: Add easing for a nicer feel (e.g., SmoothStep)
        t = t * t * (3f - 2f * t);

        // Interpolate position and rotation
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

        elapsedTime += Time.deltaTime;



        yield return null; // Wait for the next frame
    }

    // After the loop, snap to the final state to ensure accuracy and set the mode.
    // This also ensures the correct behavior (e.g., LookAt) is applied on the next frame.
    currentMode = targetMode;

    // VERY IMPORTANT: If we switched to CloseView, we must update the yaw/pitch
    // to match the final camera rotation. Otherwise, the next mouse move will snap it.
    if (targetMode == CameraMode.CloseView)
    {
        Vector3 finalEuler = transform.eulerAngles;
        // Handle potential gimbal lock issues if pitch is near 90
        pitch = (finalEuler.x > 180) ? finalEuler.x - 360 : finalEuler.x;
        yaw = finalEuler.y;
    }

    // Change the far clip plane only AFTER the transition has finished
        var Cam = Camera.main;
        if (targetMode == CameraMode.TacticView)
        {
            // Logic derived from your previous code: When entering Tactic View, set to 49
            Cam.farClipPlane = 49f;
        }
        else if (targetMode == CameraMode.CloseView)
        {
            // Logic derived from your previous code: When entering Close View, set to 1400
            Cam.farClipPlane = 1400f;
        }

}



    public void PanToQuestLocation(Vector3 questPosition)
    {
        // Stop any existing pan coroutine to avoid conflicts
        if (panCoroutine != null)
        {
            StopCoroutine(panCoroutine);
        }
        panCoroutine = StartCoroutine(PanToQuestCoroutine(questPosition));
    }

    private IEnumerator PanToQuestCoroutine(Vector3 questPosition)
    {
        isPanning = true;
        Time.timeScale = 0f;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        CameraMode originalMode = currentMode;
        currentMode = CameraMode.Transitioning; 
    
        float panToDuration = 0.7f; 
        float elapsedTime = 0f;
    
        
        Vector3 targetCamPosition = questPosition + new Vector3(0, 11.9f, -7f); // Calculate a good viewing position for the quest (e.g., slightly above and behind)
        Quaternion targetCamRotation = Quaternion.LookRotation(questPosition - targetCamPosition);
    
        while (elapsedTime < panToDuration)
        {
            float t = elapsedTime / panToDuration;
            transform.position = Vector3.Lerp(startPosition, targetCamPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetCamRotation, t);
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = targetCamPosition;
        transform.rotation = targetCamRotation;
    
        yield return new WaitForSecondsRealtime(1.4f); // Use Realtime to wait while game is paused
    
        float panBackDuration = 0.7f;
        elapsedTime = 0f;
    
        Vector3 atQuestPosition = transform.position;         // Store the "at-quest" position to smoothly lerp from it
        Quaternion atQuestRotation = transform.rotation;
    
        while (elapsedTime < panBackDuration)
        {           
            Quaternion returnRotation = Quaternion.Euler(pitch, yaw, 0); // Continuously calculate the player's camera position, as it's our moving target
            Vector3 returnPosition = target.position - (returnRotation * Vector3.forward * close_distance);
    
            float t = elapsedTime / panBackDuration;
            transform.position = Vector3.Lerp(atQuestPosition, returnPosition, t);
            transform.rotation = Quaternion.Slerp(atQuestRotation, returnRotation, t);
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        Time.timeScale = 1f; 
        currentMode = originalMode; // Restore the original camera mode
        isPanning = false; // Release the lock
        panCoroutine = null;
        
    }
    

}

