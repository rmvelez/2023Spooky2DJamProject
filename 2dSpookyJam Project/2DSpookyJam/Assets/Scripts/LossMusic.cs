using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LossMusic : MonoBehaviour
{
    [SerializeField] AudioSource chimes;
    [SerializeField] AudioSource clockTick;

    private bool tickInit = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(tickInit && !chimes.isPlaying)
        {
            clockTick.Play();
            tickInit = false;
        }
    }
}
