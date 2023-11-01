using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetReason : MonoBehaviour
{
    [SerializeField] private TMP_Text m_Text;
    private int score;
    private int total;
    private ScoreKeeper scoreKeeper;

    // Start is called before the first frame update
    void Start()
    {
        scoreKeeper = ScoreKeeper.Instance;
        string[] strings = { scoreKeeper.getExplanation(), ".\nyou lit ", scoreKeeper.score.ToString(), " out of ", scoreKeeper.total.ToString(), " lanterns." };
        m_Text.text = string.Concat(strings);
//            scoreKeeper.explanation, "\nyou scored ", score, "out of")
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
