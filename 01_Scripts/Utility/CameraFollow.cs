using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public Vector3 rotation = new Vector3(0f, 0f, 0f);
    public float smoothTime = 0.05f;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(rotation);
    }

    private void LateUpdate()
    { 
        if(!target) return;
        //if(ScreenShotManager.Instance.isFixCamera) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

    }


}
