using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallHistory
{
    public string LineType;
    public string LengthType; // "Yorker", "Full", "Short", etc.
    public int runsScored;
    public bool wasWicket;
    public string shotName;


}

public class BowlingDecision : MonoBehaviour
{

    

    public List<BaseBowler> bowlers = new List<BaseBowler>(); // List of bowlers available to use
    public Queue<BaseBowler> bowlingOrder = new Queue<BaseBowler>(SinglePlayerSetup.maxOvers); // Queue of bowlers for the match
    public static int currentOver = 0; // Current over in the match 
    BaseBowler selectedBowler; // Bowler selected for the current over
    public static int maxOvers = SinglePlayerSetup.maxOvers; // Maximum overs in the match
    public BallLogic bl;

    private List<BallHistory> currentBatsmanHistory = new List<BallHistory>();
    private int currentBatsmanRuns = 0;
    private int currentBatsmanBalls = 0;
    private int runsRequired;
    private int ballsLeft;
    public int wicketsLeft = 10;
    public string ballLengthType;
    public string ballLineType;
    public Queue<string> ballLengthsToBeBowled = new Queue<string>();

    public System.Random r = new System.Random();

    public List<string> options = new List<string> { "OffStump", "WideOffStump", "MiddleStump", "LegStump" };
    //List of all vectors for the different lengths and lines
    public Dictionary<string, Vector3> Lengths = new Dictionary<string, Vector3>
    {
        {"Good", new Vector3(24.47f, 0, 0)},
        {"Short", new Vector3(21.22f, 0, 0)},
        {"Yorker", new Vector3(26.82f, 0, 0)},
        {"Full", new Vector3(25.69f, 0, 0)}
    };
    public Dictionary<string, Vector3> Lines = new Dictionary<string, Vector3>
    {
        {"OffStump", new Vector3(0, 0, 0.42f)},
        {"MiddleStump", new Vector3(0, 0, 0.22f)},
        {"LegStump", new Vector3(0, 0, 0)},
        {"WideOffStump", new Vector3(0, 0, 0.75f)}
    };

    public BatController bc;

    void Start()
    {
        OnStartOfInnings();
    }

    public void OnStartOfInnings()
    {
        bowlers.Clear();
        bowlers.Add(new PaceBowler());
        bowlers.Add(new OffSpin());
        bowlers.Add(new LegSpin());
        bowlers.Add(new InSwingBowler());
        bowlers.Add(new OutSwingBowler());
        MakeBowlingOrder();
        Debug.Log("Bowling order reset for new innings.");
    }


    // Update is called once per frame
    void Update()
    {
        bl.CheckIfOverDone();
    }

    public string ChooseBallLineType()
    {
        if (ballsLeft < 9 && runsRequired < 20)
        {
            ballLineType = options[r.Next(0, options.Count)];
            return ballLineType;
        }
        if (wicketsLeft < 2) // They're running out of batsmen so  will bowl good length but can also bowl short balls to try and take wickets
        {
            int randomChoice = r.Next(0, 3);
            if (randomChoice == 0)
            {
                ballLineType = "OffStump";

                return ballLineType;
            }
            else
            {
                ballLineType = "MiddleStump";
                return ballLineType;
            }
        }


        return StrategicLineBall();


    }

    public string StrategicLineBall()
    {
        float runsPerOver = 6f; // Default value

        if (currentBatsmanBalls > 0)
        {
            // Step 1: Convert balls to overs (6 balls = 1 over)
            float oversBowled = currentBatsmanBalls / 6f;

            // Step 2: Calculate runs per over
            runsPerOver = currentBatsmanRuns / oversBowled;
        }

        if (runsPerOver > 10)
        {
            ballLineType = "WideOffStump";
            return ballLineType; // Defensive ball to restrict scoring
        }
        else if (runsPerOver >= 6 && runsPerOver <= 10)
        {
            ballLineType = options[r.Next(0, options.Count)];
            return ballLineType; // Balanced approach
        }
        else if (runsPerOver < 6)
        {
            ballLineType = options[r.Next(0, options.Count)];
            return ballLineType; // Aggressive ball to take wickets
        }
        else
        {
           
            ballLengthType = options[UnityEngine.Random.Range(0, options.Count)];
            return ballLengthType;
        }


    }

    public string ChooseBallLengthType()
    {
        if (ballLengthsToBeBowled.Count == 0)
        {
            System.Random r = new System.Random();
            if (ballsLeft < 9 && runsRequired < 20)
            {
                ballLengthType = "Yorker";
                return ballLengthType;
            }
            if (wicketsLeft < 2) // They're running out of batsmen so  will bowl good length but can also bowl short balls to try and take wickets
            {
                int randomChoice = r.Next(0, 3);
                if (randomChoice == 0)
                {
                    ballLengthType = "Good";

                    return ballLengthType;
                }
                else
                {
                    ballLengthType = "Full";
                    return ballLengthType;
                }
            }
            if (currentBatsmanBalls >= 10)
            {
                string weakness = BatsmanLengthWeakness();
                if (weakness != null)
                {
                    ballLengthType = weakness;
                    return ballLengthType;
                }
            }


            return StrategicLengthBall();
        }
        else
        {
            return ballLengthsToBeBowled.Dequeue();
        }
    }

    public string StrategicLengthBall()
    {
        float runsPerOver = 6f; // Default value

        if (currentBatsmanBalls > 0)
        {
            // Step 1: Convert balls to overs (6 balls = 1 over)
            float oversBowled = currentBatsmanBalls / 6f;

            // Step 2: Calculate runs per over
            runsPerOver = currentBatsmanRuns / oversBowled;
        }

        if (runsPerOver > 10)
        {
            ballLengthType = "Yorker";
            return ballLengthType; // Defensive ball to restrict scoring
        }
        else if (runsPerOver >= 6 && runsPerOver <= 10)
        {
            ballLengthType = "Good";
            return ballLengthType; // Balanced approach
        }
        else if (runsPerOver < 6)
        {
            ballLengthType = "Full";
            return ballLengthType; // Aggressive ball to take wickets
        }
        else
        {
            List<string> options = new List<string> { "Good", "Full", "Short", "Yorker"};
            ballLengthType = options[UnityEngine.Random.Range(0, options.Count)];
            return ballLengthType;
        }

      
    }

    public string BatsmanLengthWeakness()
    {
        if (currentBatsmanBalls < 8)
        {
            return null; // not enough information to excecute this.
        }

        Dictionary<string, int> poorPerformances = new Dictionary<string, int>(); // this will have all of the poor performance, the ball type and how many runs scored off it

        for (int i =0; i < currentBatsmanHistory.Count; i++)
        {
            if (currentBatsmanHistory[i].runsScored <= 1 || currentBatsmanHistory[i].wasWicket)
            {
                if (poorPerformances.ContainsKey(currentBatsmanHistory[i].LengthType))
                {
                    poorPerformances[currentBatsmanHistory[i].LengthType] += currentBatsmanHistory[i].runsScored;
                }
            }
        }


        int[] sortedValues = SortDictionary(poorPerformances, 0, poorPerformances.Count - 1);
        var keys = new List<string>(poorPerformances.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            ballLengthsToBeBowled.Enqueue(keys[i]);
        }

        return ballLengthsToBeBowled.Dequeue();
    }

    public int[] SortDictionary(Dictionary<string, int> dict, int start, int end) // sort in ascending order with a merge sort
    {
        var keys = new List<string>(dict.Keys);
        int[] sortedValues = new int[end - start + 1];
        for (int i = start; i <= end; i++)
        {
            sortedValues[i - start] = dict[keys[i]];
        }

        if (start < end)
        {
            int mid = start + (end - start) / 2;
            int[] leftHalf = SortDictionary(dict, start, mid);
            int[] rightHalf = SortDictionary(dict, mid + 1, end);
            return MergeArray(leftHalf, rightHalf);
        }

        return sortedValues;
    }

    public int[] MergeArray(int[] left, int[] right)
    {
        int[] result = new int[left.Length + right.Length];
        int lIndex = 0, rIndex = 0, resIndex = 0;

        while (lIndex < left.Length && rIndex < right.Length)
        {
            if (left[lIndex] < right[rIndex])
            {
                result[resIndex++] = left[lIndex++];
            }
            else
            {
                result[resIndex++] = right[rIndex++];
            }
        }
        while (lIndex < left.Length)
        {
            result[resIndex++] = left[lIndex++];
        }
        while (rIndex < right.Length)
        {
            result[resIndex++] = right[rIndex++];
        }
        return result;
    }
    public void MakeBowlingOrder()
    {
        bowlingOrder.Clear();

        BaseBowler lastBowler = null;

        for (int i = 0; i < maxOvers; i++)
        {
            BaseBowler chosenBowler;

            // Keep picking random bowlers until we get a different one
            do
            {
                chosenBowler = bowlers[UnityEngine.Random.Range(0, bowlers.Count)];
            }
            while (chosenBowler.GetType() == lastBowler?.GetType() && bowlers.Count > 1);

            bowlingOrder.Enqueue(chosenBowler);
            lastBowler = chosenBowler;
        }
    }

    public void RecordBall(string ballLengthType, int runs, bool wicket, string ballLineType)
    {
        BallHistory ball = new BallHistory()
        {
            LengthType = ballLengthType,
            LineType = ballLineType,
            runsScored = runs,
            wasWicket = wicket,
            shotName = bc.selectedShot.ShotName
        };

        currentBatsmanHistory.Add(ball);
        if (currentBatsmanHistory.Count > 18)
        {
            currentBatsmanHistory.RemoveAt(0);
        }
        currentBatsmanRuns += runs;
        currentBatsmanBalls++;

        if (wicket)
        {
            currentBatsmanHistory.Clear();
            currentBatsmanRuns = 0;
            currentBatsmanBalls = 0;
        }

    }

    void NextOver()
    {
         if (currentOver < SinglePlayerSetup.maxOvers)
         {
            Debug.Log("All Overs complete!");
            return;
         }
        selectedBowler = bowlingOrder.Dequeue();
        currentOver++;
    }


}
