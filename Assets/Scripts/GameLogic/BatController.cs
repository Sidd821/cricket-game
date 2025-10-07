using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public abstract class BatShot
{
    public abstract string ShotName { get; }
    public abstract float Strength { get; }
    public abstract float Risk { get; }
    public abstract Vector3 GetDirection(Vector3 ballDirection);
}

public class Drive : BatShot
{
    public override string ShotName => "Drive"; // Shot name
    public override float Strength => 1.1f; // Drives need power so this will have a lot of it
    public override float Risk => 0.2f; // Low risk as drives are quite safe.
    public override Vector3 GetDirection(Vector3 ballDirection)
    {
        return ballDirection * 1.2f; // This will slightly hit it away from where they want to hit it to make it realistic

    }
}

public class Aggressive : BatShot
{
    public override string ShotName => "Aggressive"; // Shot name
    public override float Strength => 1.2f; // Drives need power so this will have a lot of it
    public override float Risk => 0.7f; // Low risk as drives are quite safe.
    public override Vector3 GetDirection(Vector3 ballDirection)
    {
        return ballDirection * 2f; // This will slightly hit it away from where they want to hit it to make it realistic
    }
}
public class Defensive : BatShot
{
    public override string ShotName => "Defensive"; // Shot name
    public override float Strength => 0.2f; // Drives need power so this will have a lot of it
    public override float Risk => 0f; // Low risk as drives are quite safe.
    public override Vector3 GetDirection(Vector3 ballDirection)
    {
        return ballDirection; // This will slightly hit it away from where they want to hit it to make it realistic
    }
}

public class BatController : MonoBehaviour
{
    public BatShot selectedShot;
    public BallLogic bl;
    private Dictionary<string, BatShot> availableShots = new Dictionary<string, BatShot>();
    public  Transform aimDirection;
    private float timingAccuracy;
    public Button aggressiveButton;
    public Button defensiveButton;
    public Button driveButton;
    public Button playButton;
    private GameObject[] balls;
    private GameObject currentBall;
    private bool ballInRange = false;
    public Transform Wicket;
    public FielderMovement fm;
    public BowlingDecision bd;
    public WicketDetector wicketDetector; 






    private void Start()
    {
        aggressiveButton.onClick.AddListener(() => selectedShot = availableShots["Aggressive"]); // if aggressive button clicked, set selected shot to aggressive
        defensiveButton.onClick.AddListener(() => selectedShot = availableShots["Defensive"]); // if defensive button clicked, set selected shot to defensive
        driveButton.onClick.AddListener(() => selectedShot = availableShots["Drive"]); // if drive button clicked, set selected shot to drive
        playButton.onClick.AddListener(onButtonClick); // if play button clicked, call onButtonClick function

        Rigidbody batRigidbody = GetComponent<Rigidbody>();
        if (batRigidbody != null)
        {
            batRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (SinglePlayerSetup.bowling)
        {
            aggressiveButton.gameObject.SetActive(false);
            defensiveButton.gameObject.SetActive(false);
            driveButton.gameObject.SetActive(false);
        }

        availableShots.Add("Aggressive", new Aggressive()); // Add the shots to the dictionary
        availableShots.Add("Defensive", new Defensive()); // Add the shots to the dictionary
        availableShots.Add("Drive", new Drive()); // Add the shots to the dictionary

        selectedShot = availableShots["Drive"];
        Debug.Log("Selected Shot: " + selectedShot.ShotName);
    }

    
    
    void CheckForBall()
    {
        balls = GameObject.FindGameObjectsWithTag("Ball");
        ballInRange = false;
        currentBall = null;

        foreach (GameObject ball in balls)
        {

            float distance = Vector3.Distance(transform.position, ball.transform.position);

            if (distance < 1f) 
            {
                ballInRange = true;
                currentBall = ball;
                if (timingAccuracy <0.4f)
                {
                    return; // Missed the ball due to poor timing
                }
                
                PlayShot();          
                break;
            }
        }
    }

    void onButtonClick()
    {

        timingAccuracy = 1 - Mathf.Abs(bl.sliderValue - 0.5f) * 2;
        Debug.Log("Timing: " + timingAccuracy.ToString("F2"));
        Debug.Log("Selected Shot on Play: " + selectedShot.ShotName);

    }


    void PlayShot()
    {
        if (currentBall == null)
        {
            return;
        }

        Vector3 shotDirection = (aimDirection.position - currentBall.transform.position).normalized;

        float shotPower = selectedShot.Strength * timingAccuracy * 10f;
        float unpredictability = selectedShot.Risk * (1f - timingAccuracy);

        shotDirection = Randomness(shotDirection, unpredictability);

        if (selectedShot == availableShots["Aggressive"])
        {
            AggressiveForce(shotDirection,shotPower);
            Debug.Log("Aggressive Shot Played");
        }
        else
        {
            BallForce(shotDirection, shotPower);
            Debug.Log(selectedShot.ShotName + " Shot Played");
        }

        if (fm.isBallCollected)
        {
            currentBall.tag = "Untagged"; // Prevent multiple hits on the same ball
            currentBall = null;
            ballInRange = false;

        }
        else
        {
            playButton.interactable = false; // Disable play button until ball is collected

        }
        
    }


    Vector3 Randomness(Vector3 direction, float unpredictability)
    {
        Vector3 randomVariation = new Vector3(
             Random.Range(-unpredictability, unpredictability),
             0, // Small vertical variation
             Random.Range(-unpredictability, unpredictability)
         );
        return (direction + randomVariation).normalized;
    }
    void AggressiveForce(Vector3 direction, float power)
    {
        GameObject currentBall = GameObject.FindGameObjectWithTag("Ball");


        if (currentBall != null)
        {
            Rigidbody ballRigidbody = currentBall.GetComponent<Rigidbody>(); // Get the Rigidbody component of the ball so it can be applied with force
            ballRigidbody.velocity = Vector3.zero; // Reset velocity to ensure consistent hits
            ballRigidbody.angularVelocity = Vector3.zero;
            if (ballRigidbody != null)
            {
                Vector3 upwardForce = Vector3.up * (power * 0.5f); // tweak multiplier as needed
                Vector3 combinedForce = (direction * power) + upwardForce;
                ballRigidbody.AddForce(combinedForce, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.Log("No ball found to apply force to.");

        }
    }
    void BallForce(Vector3 direction, float power)
    {
        GameObject currentBall = GameObject.FindGameObjectWithTag("Ball");
        if (currentBall != null) // Error checking to make sure the ball exists
        {
            Rigidbody ballRigidbody =  currentBall.GetComponent<Rigidbody>(); // Get the Rigidbody component of the ball so it can be applied with force
            ballRigidbody.velocity = Vector3.zero; // Reset velocity to ensure consistent hits
            ballRigidbody.angularVelocity = Vector3.zero;
            if (ballRigidbody != null)
            {
                ballRigidbody.AddForce(direction * power, ForceMode.Impulse); // Apply force to the ball
            }
        }else
        {
            Debug.Log("No ball found to apply force to.");
        }
    }




    void Update()
    {
        if (!SinglePlayerSetup.bowling)
        {
            bl.HandleTimingSlider();
            CheckForBall();

            aggressiveButton.gameObject.SetActive(true);
            defensiveButton.gameObject.SetActive(true);
            driveButton.gameObject.SetActive(true);
            playButton.gameObject.SetActive(true);
        }
        else
        {
            aggressiveButton.gameObject.SetActive(false);
            defensiveButton.gameObject.SetActive(false);
            driveButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(false);
        }

        // Re-enable play button when no ball exists
        if (GameObject.FindGameObjectWithTag("Ball") == null)
        {
            if (!playButton.interactable)
                playButton.interactable = true;
        }
    }
}

