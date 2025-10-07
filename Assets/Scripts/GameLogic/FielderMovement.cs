using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FielderMovement : MonoBehaviour
{

    public List<Node> path = new List<Node>();
    public Queue<Node> pathQ = new Queue<Node>();

    private Node currentNodeTarget;
    private bool isMoving = false;
    public DijkrasAlgorithm dka;
    public Transform[] fielders;
    public Transform fielder1;
    public Transform fielder2;
    public Transform fielder3;
    public Transform fielder4;
    public Transform fielder5;
    public Transform fielder6;
    public Transform fielder7;
    public Transform fielder8;
    public Transform fielder9;
    public Transform fielder10;
    private Transform Fielder;

    public bool chased = false;
    public float totalDistance = 0;
    public float speed = 20f;
    public int runs = 0;
    public bool isBallCollected = false;
    public Vector3 currentballPos;
    private bool hasInitialised = false;
    Vector3[] startFielderPos;
    public Vector3 theFielderPos;
    public RunCalculator rc;






    private void Start()
    {
        fielders = new Transform[] { fielder1, fielder2, fielder3, fielder4, fielder5, fielder6, fielder7, fielder8, fielder9, fielder10 };
        runs = 0;
        startFielderPos = new Vector3[fielders.Length];
        for (int i = 0; i < fielders.Length; i++)
        {
            startFielderPos[i] = fielders[i].position;
        }

    }

    void FielderInitialiser()
    {
        if (hasInitialised || isBallCollected) return; // Only init once and if not collected
        GameObject currentBall = GameObject.FindGameObjectWithTag("Ball"); // Find the ball in the scene

        if (currentBall != null && Fielder != null)// Ensure ball and fielder are valid
        {
            path = dka.PathFinding(Fielder.position, currentBall.transform.position);// Finds the path from fielder to ball
            currentballPos = currentBall.transform.position;// Store current ball position

            if (path != null && path.Count > 0)
            {
                // Clear queue and add all path nodes
                pathQ.Clear();

                for (int i = 0; i < path.Count; i++)
                {
                    Node node = path[i];
                    //Debug.Log(node.distance);
                    pathQ.Enqueue(node);
                    //Debug.Log("Path node: " + node.id + " at position " + node.position);

                }
            }

            hasInitialised = true;
            // Start moving
            isMoving = true;
            GetNextTarget();
        }
    }
    public void ResetFielder()
    {
        // Move fielders back to start
        for (int i = 0; i < fielders.Length; i++)
        {
            fielders[i].position = startFielderPos[i];
        }

        // Reset movement state
        isBallCollected = false;
        hasInitialised = false;
        chased = false;
        currentNodeTarget = null;
        path.Clear();
        pathQ.Clear();
        isMoving = false;
        Fielder = null;

        // Reset scoring (let RunCalculator handle its own reset)
        if (rc != null)
            rc.ResetForNextBall();
    }

    public void Update()
    {
        GameObject currentBall = GameObject.FindGameObjectWithTag("Ball");

        // Find closest fielder
        if (currentBall != null)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestFielder = null;

            for (int i = 0; i < fielders.Length; i++)
            {
                if (fielders[i] == null) continue;

                float currentDistance = Vector3.Distance(fielders[i].position, currentBall.transform.position);

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestFielder = fielders[i];
                    theFielderPos = startFielderPos[i];
                }
            }
            if (closestFielder != null && closestFielder != Fielder)
            {
                Fielder = closestFielder;
                hasInitialised = false;
            }
        }

        // Initialize path once
        if (!hasInitialised && currentBall != null && Fielder != null && !isBallCollected)
        {
            path = dka.PathFinding(Fielder.position, currentBall.transform.position);
            currentballPos = currentBall.transform.position;

            if (path != null && path.Count > 0)
            {
                pathQ.Clear();
                for (int i = 0; i < path.Count; i++)
                    pathQ.Enqueue(path[i]);

                isMoving = true;
                GetNextTarget();
            }
            hasInitialised = true;
        }

        // Move towards target
        if (isMoving && currentNodeTarget != null && currentBall != null && !isBallCollected)
        {
            Fielder.position = Vector3.MoveTowards(Fielder.position, currentNodeTarget.position, speed * Time.deltaTime);

            if (Vector3.Distance(Fielder.position, currentNodeTarget.position) < 0.1f)
            {
                GetNextTarget();
            }

            // Check if collected
            if (Vector3.Distance(currentBall.transform.position, Fielder.position) < 2f)
            {
                isBallCollected = true;
                isMoving = false;
                pathQ.Clear();
                Debug.Log("Fielder collected ball!");
            }
        }

        // Replan if ball moved
        if (!isBallCollected && currentBall != null && hasInitialised)
        {
            if (Vector3.Distance(currentBall.transform.position, currentballPos) > 2f)
            {
                currentballPos = currentBall.transform.position;
                hasInitialised = false;
            }
        }
    }


    void MoveFielder(Vector3 ballPos)
    {
        // Move towards current target node
        Fielder.position = Vector3.MoveTowards(Fielder.position,currentNodeTarget.position,speed * Time.deltaTime); 

            GetNextTarget();
        

    }

    void GetNextTarget()
    {
        Transform currentball = GameObject.FindGameObjectWithTag("Ball").transform;
        if (pathQ.Count > 0)
        {
            currentNodeTarget = pathQ.Dequeue();
            //Debug.Log("Moving to next node: " + currentNodeTarget.id);
            if (Vector3.Distance(currentball.position, currentballPos) > 0.5f)
            {
                currentballPos = currentball.position;
            }
        }
        else
        {
            
            // Reached destination
            isMoving = false;
            currentNodeTarget = null;
            //Debug.Log("Fielder reached destination!");

            if (Vector3.Distance(currentball.position, Fielder.position) < 2.5f)
            {
                Debug.Log("Ball Collected by Fielder");
                isBallCollected = true;
            }



        }
    }

    void ChaseBallAgain()
    {
        if (!chased && !isBallCollected)
        {
            GameObject currentBall = GameObject.FindGameObjectWithTag("Ball");
            if (currentBall != null)
            {
                path = dka.PathFinding(Fielder.position, currentBall.transform.position);
                if (path != null && path.Count > 0)
                {
                    pathQ.Clear();
                    int startIdx = 0;
                    if (Vector3.Distance(Fielder.position, path[0].position) < 0.2f)
                    {
                        startIdx = 1;
                    }
                    for (int i = startIdx; i < path.Count; i++)
                    {
                        pathQ.Enqueue(path[i]);
                    }
                    isMoving = true;
                    GetNextTarget();
                    chased = true;
                }
            }
        }
    }

   
}
