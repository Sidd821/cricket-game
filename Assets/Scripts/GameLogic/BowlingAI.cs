using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BowlingAI : MonoBehaviour
{
    public BowlingDecision bd; // Drag this in inspector
    public string chosenLengthBallType;
    public string chosenLineBallType;
    public BallLogic bl;
    public Button b;

    void Start()
    {
        bd = GetComponent<BowlingDecision>();
        b.onClick.AddListener(AIBowl);
        if (SinglePlayerSetup.bowling)
        {
            b.gameObject.SetActive(false);
        }
    }
    public void AITakeTurn()
    {
        // 1. AI chooses ball length type
        chosenLengthBallType = bd.ChooseBallLengthType();

        // 2. AI chooses ball line type
        chosenLineBallType = bd.ChooseBallLineType();


    }
    // CALL THIS when the ball outcome is known
    public void ProcessBallResult(int runs, bool wasWicket)
    {
        bd.RecordBall(chosenLengthBallType, runs, wasWicket, chosenLineBallType);
    }

    public Vector3 GetAIDeliveryPoint()
    {
        AITakeTurn(); // Let AI choose the ball types

        // Combine length and line to get final delivery point
        if (bd.Lengths.ContainsKey(chosenLengthBallType) &&
            bd.Lines.ContainsKey(chosenLineBallType))
        {
            Vector3 lengthPoint = bd.Lengths[chosenLengthBallType];
            Vector3 linePoint = bd.Lines[chosenLineBallType];

            return lengthPoint + linePoint;
        }

        return new Vector3(24.47f, 0, 0.22f); // default delivery point if something goes wrong
    }

    public async void AIBowl()
    {

        bl.CheckIfOverDone();
        await Task.Delay(750);
        Vector3 deliveryPoint = GetAIDeliveryPoint();
        bl.target.position = deliveryPoint;
        bl.timingAccuracy = 1.0f; // Assume perfect timing for AI
        bl.Bowl();
    }
}