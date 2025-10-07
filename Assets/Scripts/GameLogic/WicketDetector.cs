using UnityEngine;

public class WicketDetector : MonoBehaviour
{
    public BowlingDecision bd;
    public FielderMovement fm;
    public RunCalculator rc;

    private bool wicketFallen = false;
    private GameObject trackedBall;
    public float detectionRadius = 0.35f;

    void Update()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");

        // New ball
        if (ball != null && ball != trackedBall)
        {
            trackedBall = ball;
            wicketFallen = false;
        }

        // No ball or wicket already taken
        if (ball == null || wicketFallen) return;

        // Manual distance check (XZ plane)
        Vector2 ballPosXZ = new Vector2(ball.transform.position.x, ball.transform.position.z);
        Vector2 wicketPosXZ = new Vector2(transform.position.x, transform.position.z);
        float distance = Vector2.Distance(ballPosXZ, wicketPosXZ);

        // Ball hit wickets (within radius and at stump height)
        if (distance < detectionRadius && ball.transform.position.y > 0.05f && ball.transform.position.y < 1.2f)
        {
            Debug.Log("WICKET! Bowled/Hit Wicket!");
            wicketFallen = true;

            bd.wicketsLeft--;
            bd.RecordBall(bd.ballLengthType, 0, true, bd.ballLineType);

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
            }

            Destroy(ball, 0.3f);
            fm.ResetFielder();
            rc.ResetForNextBall();
        }
    }
}