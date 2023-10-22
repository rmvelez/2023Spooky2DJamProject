using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private GameObject target;

    [SerializeField] private float lerpFactor = 0.1f; // controls the rate at which the camera transitions to target position - value should be between 0 and 1
    [SerializeField] private float zoomFromTarget = -10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.transform.position + Vector3.forward * zoomFromTarget, lerpFactor);
    }
}
