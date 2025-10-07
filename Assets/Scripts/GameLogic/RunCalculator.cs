using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunCalculator : MonoBehaviour
{
    public FielderMovement fm;
    public BowlingDecision bd;

    private bool ballActive = false;
    private bool hasBouncedAfterHit = false;
    private bool scored = false;
    private GameObject trackedBall;
    public int runScored = 0;
    public AddingRunsToDB artb;

    void Update()
    {
        // HARD BLOCK: Don't process if we already scored this ball
        if (scored && trackedBall != null)
        {
            return;
        }

        // Find and track ball
        GameObject currentBall = GameObject.FindGameObjectWithTag("Ball");

        // New ball spawned
        if (currentBall != null && currentBall != trackedBall)
        {
            trackedBall = currentBall;
            ballActive = true;
            hasBouncedAfterHit = false;
            scored = false;
        }

        // No ball exists
        if (currentBall == null)
        {
            ballActive = false;
            trackedBall = null;
            return;
        }

        // Already scored (double-check)
        if (scored)
        {
            return;
        }

        // Detect bounce AFTER hit
        if (ballActive && !hasBouncedAfterHit && currentBall.transform.position.y < 0.3f)
        {
            Rigidbody rb = currentBall.GetComponent<Rigidbody>();
            if (rb != null && rb.velocity.y < -0.5f)
            {
                hasBouncedAfterHit = true;
                Debug.Log("Ball bounced after hit");
            }
        }

        // 1. Check CAUGHT
        if (fm.isBallCollected && !hasBouncedAfterHit)
        {
            scored = true;
            Debug.Log("OUT! CAUGHT!");
            bd.wicketsLeft--;
            bd.RecordBall(bd.ballLengthType, 0, true, bd.ballLineType);
            artb.AddRuns(0, true);
            DestroyBall(currentBall);
            return;
        }

        // 2. Check BOUNDARY
        float distanceFromCenter = Vector3.Distance(currentBall.transform.position, transform.position);

        if (distanceFromCenter > 40f)
        {
            scored = true;

            if (hasBouncedAfterHit)
            {
                runScored = 4;
                fm.runs += runScored;
                Debug.Log("FOUR! Total: " + fm.runs);
                bd.RecordBall(bd.ballLengthType, 4, false, bd.ballLineType);
                artb.AddRuns(4, false);

            }
            else
            {
                runScored = 6;
                fm.runs += runScored;
                Debug.Log("SIX! Total: " + fm.runs);
                bd.RecordBall(bd.ballLengthType, 6, false, bd.ballLineType);
                artb.AddRuns(6, false);
            }

            DestroyBall(currentBall);
            return;
        }

        // 3. Fielder collected after bounce
        if (fm.isBallCollected && hasBouncedAfterHit)
        {
            scored = true;
            CalculateRunningRuns();
            DestroyBall(currentBall);
        }
    }

    void CalculateRunningRuns()
    {
        float distance = Vector3.Distance(fm.theFielderPos, fm.currentballPos);
        float time = distance / Mathf.Max(1f, fm.speed);
        runScored = Mathf.Min(4, Mathf.FloorToInt(time / 2f));

        fm.runs += runScored;

        Debug.Log($"Running: +{runScored} runs (total {fm.runs})");


            bd.RecordBall(bd.ballLengthType, runScored, false, bd.ballLineType);
            artb.AddRuns(0, true);
        

    }

    void DestroyBall(GameObject ball)
    {
        if (ball == null) return;

        // CRITICAL: Retag FIRST so FindGameObjectWithTag won't find it
        ball.tag = "Untagged";

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb && !rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Destroy(ball, 0.05f);
        fm.ResetFielder();
    }

    public void ResetForNextBall()
    {
        ballActive = false;
        hasBouncedAfterHit = false;
        scored = false;
        trackedBall = null;
    }
}