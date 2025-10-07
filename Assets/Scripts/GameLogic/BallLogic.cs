using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;

public class BallLogic : MonoBehaviour
{
    public GameObject ballPrefab;
    public string bowlerName;
    public Transform target;
    public Transform startPos;
    private BaseBowler currentBowler;
    public BowlingDecision bowlingDecision;

    public Queue<GameObject> ballsBowledThisOver = new Queue<GameObject>();
    public Stack<Vector3> previousBallPositions = new Stack<Vector3>();
    public int overs = 0;

    public Slider timingSlider;
    public float sliderValue;
    public bool isSlidingUp = true;
    public float timingAccuracy;
    public bool overCompleted = false;

    public bool bowlingInningsDone = false;
    public bool battingInningsDone = false; 

    void Start()
    {
       

    }
    
    
    void Update()
    {
        CheckIfOverDone();

        HandleTimingSlider();

        if (SinglePlayerSetup.bowling)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                // Accuracy = 1 when slider at 0.5 (perfect timing)
                timingAccuracy = 1 - Mathf.Abs(sliderValue - 0.5f) * 2;
                Debug.Log("Timing Accuracy: " + timingAccuracy.ToString("F2"));
                Bowl();

            }
        }



        if (Input.GetKeyDown(KeyCode.R) && previousBallPositions.Count > 0)
        {
            Vector3 lastPos = previousBallPositions.Pop();
            Debug.Log("Replaying ball from position: " + lastPos);
        }


        currentBowler.Spin();
    }

    public void CheckIfOverDone()
    {
        // Only process if the over is done and the innings is not already over
        if ((ballsBowledThisOver.Count == 6 || (ballsBowledThisOver.Count <= 0 && !overCompleted)) && overs < BowlingDecision.maxOvers)
        {
            if (bowlingDecision.bowlingOrder.Count > 0)
            {
                currentBowler = bowlingDecision.bowlingOrder.Dequeue();
                bowlerName = currentBowler.GetType().Name;
                overCompleted = true;
            }

            if (ballsBowledThisOver.Count != 0)
            {
                overs++;
                ballsBowledThisOver.Clear();
                Debug.Log($"Over completed! Total overs: {overs}");
            }
        }

        // Switch innings if all overs are done
        if (overs == BowlingDecision.maxOvers && !bowlingInningsDone && !battingInningsDone)
        {
            if (SinglePlayerSetup.bowling)
            {
                SinglePlayerSetup.bowling = false;
                Debug.Log("Innings over! Switching to batting.");
                bowlingDecision.OnStartOfInnings();
                bowlingInningsDone = true;
            }
            else
            {
                SinglePlayerSetup.bowling = true;
                Debug.Log("Innings over! Switching to bowling.");
                bowlingDecision.OnStartOfInnings();
                battingInningsDone = true;
            }

            // Reset state for new innings
            overs = 0;
            overCompleted = false;
            ballsBowledThisOver.Clear();
        }
    }

    public void HandleTimingSlider()
    {
        if (isSlidingUp) sliderValue += Time.deltaTime;
        else sliderValue -= Time.deltaTime;

        if (sliderValue >= 1f) { sliderValue = 1f; isSlidingUp = false; }
        if (sliderValue <= 0f) { sliderValue = 0f; isSlidingUp = true; }

        timingSlider.value = sliderValue;
    }

    public void Bowl()
    {
        Debug.Log("Bowling with accuracy: " + timingAccuracy.ToString("F2"));
        currentBowler.Bowl(startPos, target.position, timingAccuracy, ballPrefab);

        if (currentBowler.CurrentBallInstance != null)
        {
            ballsBowledThisOver.Enqueue(currentBowler.CurrentBallInstance);
            previousBallPositions.Push(startPos.position);
        }
    }
}

public abstract class BaseBowler
{
    
    public GameObject CurrentBallInstance { get; protected set; }

    protected float minSpeed { get;  set; }
    protected float maxSpeed { get;  set; }



    public virtual void Spin()
    {
        // Override in subclasses if needed
    }
    public virtual void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        
        // Spawn ball
        CurrentBallInstance = GameObject.Instantiate(ballPrefab, bowlerTransform.position, Quaternion.identity);
        CurrentBallInstance.tag = "Ball";

        // Solve initial velocity with SUVAT maths
        Vector3 initialVelocity = SolveProjectile(bowlerTransform.position, targetPoint, timingAccuracy);

        // Apply to Rigidbody ONCE
        Rigidbody rb = CurrentBallInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = initialVelocity;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        Debug.Log($"Initial Velocity: {initialVelocity}");
    }

    public Vector3 SolveProjectile(Vector3 start, Vector3 target, float accuracy)
    {
        Vector3 displacement = target - start;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

        float dx = displacementXZ.magnitude;
        float dy = displacement.y;
        float g = Mathf.Abs(Physics.gravity.y);

        float speed = Random.Range(minSpeed, maxSpeed) * accuracy;

        float speedSqr = speed * speed;
        float underRoot = (speedSqr * speedSqr) - g * (g * dx * dx + 2 * dy * speedSqr);

        if (underRoot <= 0)
        {
            return displacementXZ.normalized * speed + Vector3.up * 2f;
        }

        float root = Mathf.Sqrt(underRoot);

        float angle1 = Mathf.Atan((speedSqr + root) / (g * dx));
        float angle2 = Mathf.Atan((speedSqr - root) / (g * dx));

        float theta = Mathf.Min(angle1, angle2);

        float cos = Mathf.Cos(theta);
        float sin = Mathf.Sin(theta);

        Vector3 dirXZ = displacementXZ.normalized;
        Vector3 velocity = dirXZ * (speed * cos) + Vector3.up * (speed * sin);

        return velocity;
    }

}


public class PaceBowler : BaseBowler
{
    public PaceBowler()
    {
        minSpeed = 40f; // ~55mph
        maxSpeed = 45f; // ~70mph
        
    }
    public override void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        base.Bowl(bowlerTransform, targetPoint, timingAccuracy, ballPrefab);
    }
}
public class InSwingBowler: BaseBowler
{
    public InSwingBowler()
    {
        minSpeed = 20f; // ~45mph
        maxSpeed = 35f; // ~62mph
    }

    public override void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        Debug.Log("Applying inswing effect");
        // Add sideways force for inswing
        if (CurrentBallInstance != null)
        {
            Rigidbody rb = CurrentBallInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("Applying inswing effect");
                float swingForce = 2.5f * timingAccuracy; // More accurate = more swing
                targetPoint = targetPoint + new Vector3(0, 0, Random.Range(-1.4f,0));// Adjust target for inswing
                rb.AddForce(-rb.velocity.normalized.z * swingForce, 0, rb.velocity.normalized.x * swingForce, ForceMode.VelocityChange);
            }
        }

        base.Bowl(bowlerTransform, targetPoint, timingAccuracy, ballPrefab);
    }
}
public class OutSwingBowler : BaseBowler
{
    public OutSwingBowler()
    {
        minSpeed = 20f; // ~45mph
        maxSpeed = 35f; // ~62mph
    }

    public override void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        if (CurrentBallInstance != null)
        {
            Rigidbody rb = CurrentBallInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("Applying outswing effect");
                float swingForce = 2.5f * timingAccuracy; // More accurate = more swing
                targetPoint = targetPoint + new Vector3(0, 0, Random.Range(0,1.3f));// Adjust target for inswing
                rb.AddForce(rb.velocity.normalized.z * swingForce, 0, rb.velocity.normalized.x * swingForce, ForceMode.VelocityChange);
            }
        }

        base.Bowl(bowlerTransform, targetPoint, timingAccuracy, ballPrefab);
    }
    
}
public class LegSpin : BaseBowler
{


    public LegSpin()
    {
        minSpeed = 15f;  // 20mph
        maxSpeed = 25f;  // 40mph
    }

    public override void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        // Spawn the ball first
        base.Bowl(bowlerTransform, targetPoint, timingAccuracy, ballPrefab);

        if (CurrentBallInstance != null)
        {
            Rigidbody rb = CurrentBallInstance.GetComponent<Rigidbody>();
            if (rb != null)
            { 
               
                BallSpin spinComp = CurrentBallInstance.AddComponent<BallSpin>();
                spinComp.spinDirection = -1; // Leg spin
            }
        }
    }

    
}
public class OffSpin : BaseBowler
{


    public OffSpin()
    {
        minSpeed = 15f;  // 20mph
        maxSpeed = 25f;  // 40mph
    }

    public override void Bowl(Transform bowlerTransform, Vector3 targetPoint, float timingAccuracy, GameObject ballPrefab)
    {
        // Spawn the ball first
        base.Bowl(bowlerTransform, targetPoint, timingAccuracy, ballPrefab);

        if (CurrentBallInstance != null)
        {
            Rigidbody rb = CurrentBallInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {

                BallSpin spinComp = CurrentBallInstance.AddComponent<BallSpin>();
                spinComp.spinDirection = 1; // Leg spin
            }
        }
    }


}

public class BallSpin : MonoBehaviour
{
    public int spinDirection = -1;
    public float bounceSideImpulse = 1.9f;  // lateral impulse on first bounce
    public string pitchTag = "Pitch";

    Rigidbody rb;
    bool hasBounced = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

   
    void OnCollisionEnter(Collision col)
    {
        if (hasBounced) return;

        // if ball hits the pitch, give a lateral "turn" impulse
        if (col.collider.CompareTag(pitchTag))
        {
            hasBounced = true;

            // compute lateral direction perpendicular to travel
            Vector3 lateral = Vector3.Cross(Vector3.up, rb.velocity).normalized;
            Vector3 impulse = lateral * bounceSideImpulse * spinDirection;

            rb.AddForce(impulse, ForceMode.Impulse);

            Debug.Log($"BallSpin: bounce impulse applied {impulse}");
        }
    }
}

