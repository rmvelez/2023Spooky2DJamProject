using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private GameObject target;

    [SerializeField] private float lerpFactor = 0.1f; // controls the rate at which the camera transitions to target position - value should be between 0 and 1
    [SerializeField] private float zoomFromTarget = -10;

    [SerializeField] private Camera m_Camera;
    [Tooltip("equivalent to half the height in world units")]
    [SerializeField] private float cameraZoom;

    //bounding variables
    [SerializeField] private SpriteRenderer backgroundSprite;

    private float cameraWidth;

    private float upperBound;
    private float lowerBound;
    private float leftBound;
    private float rightBound;

    // Start is called before the first frame update
    void Start()
    {
        CalculateBounds();

        transform.position = transform.position + Vector3.forward * zoomFromTarget;
    }

    //final implementation doesn't require this to be called anywhere outside of Start(). but during playtesting I wanted to be able to change and experiment with these values at runtime, so I made it a
    //seperate function to update them
    private void CalculateBounds()
    {
        cameraZoom = m_Camera.orthographicSize;
        cameraWidth = cameraZoom * m_Camera.aspect;

        //calculate the bounds of the map based on the size of the map sprite
        upperBound = backgroundSprite.sprite.bounds.center.y + backgroundSprite.sprite.bounds.extents.y;
        lowerBound = backgroundSprite.sprite.bounds.center.y - backgroundSprite.sprite.bounds.extents.y;
        rightBound = backgroundSprite.sprite.bounds.center.x + backgroundSprite.sprite.bounds.extents.x;
        leftBound = backgroundSprite.sprite.bounds.center.x - backgroundSprite.sprite.bounds.extents.x;

        upperBound -= cameraZoom;
        lowerBound += cameraZoom;
        rightBound -= cameraWidth;
        leftBound += cameraWidth;

        PlayerController pc = target.GetComponent<PlayerController>();

        upperBound += pc.distFromHitboxToSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {

        Vector3 newPosition = target.transform.position;
        newPosition.z = transform.position.z;

        if (upperBound > lowerBound)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, lowerBound, upperBound);
        }
        else
        {
            newPosition.y = 0;
        }

        if (rightBound > leftBound)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);

        }
        else
        {
            newPosition.x = 0;
        }


        //transform.position = newPosition;

        newPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, newPosition, lerpFactor);
    }
}
