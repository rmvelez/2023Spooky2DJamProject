using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [Tooltip("equivalent to half the height in world units")]
    [SerializeField] private float cameraZoom;
    private float cameraWidth;
    [SerializeField] private SpriteRenderer backgroundSprite;

    private PlayerController player;

    private float upperBound;
    private float lowerBound;
    private float leftBound;
    private float rightBound;


    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.playerController;

        CalculateBounds();
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
    }

    private void Update()
    {
        SetPosition();
    }


    //move this over to SetPosition
    // Update is called once per frame
    public void SetPosition()
    {
        CalculateBounds();

        Vector3 newPosition = player.transform.position;
        newPosition.z = transform.position.z;

        if(upperBound> lowerBound)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, lowerBound, upperBound);
        } else
        {
            newPosition.y = 0;
        }

        if(rightBound > leftBound)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
            
        } else
        {
            newPosition.x = 0;
        }


        transform.position = newPosition;
    }
}
