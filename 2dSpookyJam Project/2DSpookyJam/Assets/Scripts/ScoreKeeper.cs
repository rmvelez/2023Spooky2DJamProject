using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{

    public static ScoreKeeper _instance;
    public static ScoreKeeper Instance { get { return _instance; } }

    public float score;
    public enum LossReason { Ghost, Lantern, Win}
    public LossReason reason;
    public string explanation;

    public void setExplanation()
    {
        if( reason == LossReason.Ghost)
        {
            explanation = "A ghost caught you";
        } else if (reason == LossReason.Lantern)
        {
            explanation = "You ran out of oil";
        }
    }

    public void loadEnding(float score,  LossReason reason)
    {
        this.reason = reason; 
        this.score = score;
    }

    public void loadEnding(float score)
    {
        this.score = score;
    }

    // Start is called before the first frame update
    void Start()
    {

        //SINGLETON PATTERN - ensures that there only ever exists a single gamemanager

        //is this the first time we've created this singleton
        if (_instance == null)
        {
            //we're the first gameManager, so assign ourselves to this instance
            _instance = this;
            DontDestroyOnLoad(this);

            // don't keep ourselves between levels
        }
        else
        {
            //if there's another one, then destroy this one
            Destroy(this.gameObject);
        }

    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
