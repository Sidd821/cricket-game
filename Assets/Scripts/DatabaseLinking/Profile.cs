using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    public Text RunsScoredText;
    public Text MatchesPlayedText;
    public Text WinsText;
    public Text BallsFacedText;
    public Text DotBallsText;
    public Text FoursText;
    public Text SixesText;
    public Text AverageBattingText;
    public Text WicketsText;
    public Text RunsConcededText;
    public Text BallsBowledText;
    public Text AverageBowlingText;
    public Text StrikeRateText;
    public Button updateButton;
    public Button backButton;
    public DatabaseManager dbm;

    void Start()
    {
        backButton.onClick.AddListener(BackButton);
        updateButton.onClick.AddListener(ShowStats);   
    }

    void UpdateAndShowStats()
    {
        using (var dbConnection = dbm.GetDBConnection())
        {
            dbConnection.Open();
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT *
                    FROM PlayerStats 
                    WHERE user_id = @user_id";

                cmd.Parameters.AddWithValue("@user_id", 2);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Read stats
                        int ballsFaced = reader.GetInt32(6);
                        int matchesPlayed = reader.GetInt32(4);
                        int wins = reader.GetInt32(5);
                        int runsScored = reader.GetInt32(2);
                        int fours = reader.GetInt32(11);
                        int sixes = reader.GetInt32(10);
                        int dotBalls = reader.GetInt32(9);

                        int ballsBowled = reader.GetInt32(7);
                        int wicketsTaken = reader.GetInt32(3);
                        int runsConceded = reader.GetInt32(8);



                        // Calculate derived stats
                        float battingstrikeRate = (runsScored / (float)ballsFaced) * 100f;
                        float battingAverage = (runsConceded / (float) matchesPlayed);

                        float bowlingStrikeRate = ballsBowled / (float)wicketsTaken;
                        float bowlingAverage = (runsConceded / (float)wicketsTaken);

                        BallsFacedText.text = ballsFaced.ToString();
                        MatchesPlayedText.text = matchesPlayed.ToString();
                        WinsText.text = wins.ToString();
                        RunsScoredText.text = runsScored.ToString();
                        FoursText.text = fours.ToString();
                        SixesText.text= sixes.ToString();
                        DotBallsText.text = dotBalls.ToString();

                        BallsBowledText.text = ballsBowled.ToString();
                        WicketsText.text = wicketsTaken.ToString();
                        RunsConcededText.text = runsConceded.ToString();



                    }
                }
            }
        }
    }
    public void ShowStats()
    {
        
        UpdateAndShowStats();
        Debug.Log("Button Pressed");
    }

    public void BackButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

    }

}
