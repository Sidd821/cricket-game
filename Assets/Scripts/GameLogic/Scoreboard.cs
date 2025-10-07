using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public Text scoreText;
    public Text oversText;
    public Text bowlersNameText;
    public Text ballsLeftText;
    public int overs = 0;
    public int wickets = 0;
    public int runs = 0;
    public string bowlerName = "";
    public int ballsLeft = 0; 
    public BallLogic bl;
    public FielderMovement fm;
    public BowlingDecision bd;
    void Start()
    {
        scoreText.text = "Score: " + runs + "/" + wickets;
        oversText.text = "Overs: " + overs;
        bowlersNameText.text = "Bowler: " + bowlerName;
        ballsLeftText.text = "Balls Left: " + ballsLeft;
    }

    // Update is called once per frame
    void Update()
    {

        ballsLeft = 6 - bl.ballsBowledThisOver.Count;
        overs = bl.overs;
        bowlerName = bl.bowlerName;
        runs = fm.runs;
        wickets = 10 - bd.wicketsLeft;

        oversText.text = "Overs: " + overs;
        scoreText.text = "Score: " + runs + "/" + wickets;
        bowlersNameText.text = "Bowler: " + bowlerName;
        ballsLeftText.text = "Balls Left: " + ballsLeft;
    }
}
