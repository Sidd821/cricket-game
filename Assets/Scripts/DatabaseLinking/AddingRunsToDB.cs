using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class AddingRunsToDB : MonoBehaviour
{
    public DatabaseManager dbm;
    public BowlingDecision bd; 
    public RunCalculator rc;

    public void AddRuns(int runs, bool wicket)
    {
        if (SinglePlayerSetup.bowling && UserSession.HasActiveSession())
        {
            UpdateBowlingStats(wicket);
        }
        else if (!SinglePlayerSetup.bowling && UserSession.HasActiveSession())
        {
            UpdateBattingStats();
        }
    }

    void UpdateBowlingStats(bool wicket)
    {
        using (var dbConnection = dbm.GetDBConnection())
        {
            dbConnection.Open();
            
            bool userExists = false;
            string checkSQL = "SELECT user_id FROM PlayerStats WHERE user_id = @user_id";
            
            using (var checkCmd = dbConnection.CreateCommand())
            {
                checkCmd.CommandText = checkSQL;
                checkCmd.Parameters.AddWithValue("@user_id", UserSession.UserId);
                
                using (var reader = checkCmd.ExecuteReader())
                {
                    userExists = reader.Read();
                }
            }

            using (var cmd = dbConnection.CreateCommand())
            {
                if (userExists)
                {
                    cmd.CommandText = "UPDATE PlayerStats SET " +
                        "wickets_taken = wickets_taken + @wickets_taken, " +
                        "runs_conceded = runs_conceded + @runs_conceded, " +
                        "balls_bowled = balls_bowled + 1, " +
                        "fours_conceded = fours_conceded + @fours_conceded, " +
                        "sixes_conceded = sixes_conceded + @sixes_conceded, " +
                        "dot_balls = dot_balls + @dot_balls " +
                        "WHERE user_id = @user_id";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO PlayerStats " +
                        "(user_id, wickets_taken, runs_conceded, balls_bowled, fours_conceded, sixes_conceded, dot_balls) " +
                        "VALUES (@user_id, @wickets_taken, @runs_conceded, 1, @fours_conceded, @sixes_conceded, @dot_balls)";
                }

                cmd.Parameters.AddWithValue("@user_id", UserSession.UserId);
                cmd.Parameters.AddWithValue("@wickets_taken", wicket ? 1 : 0);
                cmd.Parameters.AddWithValue("@runs_conceded", rc.runScored);
                cmd.Parameters.AddWithValue("@fours_conceded", rc.runScored == 4 ? 1 : 0);
                cmd.Parameters.AddWithValue("@sixes_conceded", rc.runScored == 6 ? 1 : 0);
                cmd.Parameters.AddWithValue("@dot_balls", rc.runScored == 0 ? 1 : 0);
                
                cmd.ExecuteNonQuery();
            }
        }
    }

    void UpdateBattingStats()
    {
        using (var dbConnection = dbm.GetDBConnection())
        {
            dbConnection.Open();
            
            bool userExists = false;
            string checkSQL = "SELECT user_id FROM PlayerStats WHERE user_id = @user_id";
            
            using (var checkCmd = dbConnection.CreateCommand())
            {
                checkCmd.CommandText = checkSQL;
                checkCmd.Parameters.AddWithValue("@user_id", UserSession.UserId);
                
                using (var reader = checkCmd.ExecuteReader())
                {
                    userExists = reader.Read();
                }
            }

            using (var cmd = dbConnection.CreateCommand())
            {
                if (userExists)
                {
                    cmd.CommandText = "UPDATE PlayerStats SET " +
                        "balls_faced = balls_faced + 1, " +
                        "runs_scored = runs_scored + @runs_scored, " +
                        "fours = fours + @fours, " +
                        "sixes = sixes + @sixes, " +
                        "dot_balls = dot_balls + @dot_balls " +
                        "WHERE user_id = @user_id";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO PlayerStats " +
                        "(user_id, balls_faced, runs_scored, balls_bowled, fours, sixes, dot_balls) " +
                        "VALUES (@user_id, 1, @runs_scored, 0, @fours, @sixes, @dot_balls)";
                }

                cmd.Parameters.AddWithValue("@user_id", UserSession.UserId);
                cmd.Parameters.AddWithValue("@runs_scored", rc.runScored);
                cmd.Parameters.AddWithValue("@fours", rc.runScored == 4 ? 1 : 0);
                cmd.Parameters.AddWithValue("@sixes", rc.runScored == 6 ? 1 : 0);
                cmd.Parameters.AddWithValue("@dot_balls", rc.runScored == 0 ? 1 : 0);
                
                cmd.ExecuteNonQuery();
            }
        }
    }
}
