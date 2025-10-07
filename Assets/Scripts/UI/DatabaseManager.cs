using Mono.Data.Sqlite; // library that allows me to work with SQLite databases
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Windows;


public class DatabaseManager : MonoBehaviour // allow me to attach this script to a GameObject
{

    private string dbpath;// variable to hold the database path
    private string stringConnection; // string that library uses to find and connect to the database

    void Awake() // before the game starts
    {
        dbpath = Path.Combine(Application.persistentDataPath, "Howzat_db.db");
        stringConnection = "URI=file:" + dbpath + ";Version=3;";
        // format the path so it can be understood by the SQLite library
        CleanupOldLoginAttempts();// clean up old login attempts on startup to prevent the database from growing too large
        Debug.Log("DB Path: " + dbpath);

    }

    public void createLoginAttemptsTable() // public so that the login script can call this function.
                                           // Login table will only be created when required so if it is successful the first time, it won't be created.
    {

        using (var dbConnection = new SqliteConnection(stringConnection))// "using" Creates a connection to the
                                                                         // database and closes it when its done
        {
            dbConnection.Open(); // open the connection to the database

            using (var dbCommand = dbConnection.CreateCommand()) // create a command that will be sent to the database
            {
                dbCommand.CommandText = "PRAGMA foreign_keys = ON;";
                dbCommand.ExecuteNonQuery(); // execute the command

                // SQL statement to create the login attempts table if it already doesn't exist. This is so that if there is 5 attempts in 1 minute it will be blocked for suspicious activity
                dbCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS LoginAttempts (
                        attempt_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        attempt_time TEXT NOT NULL,
                        was_successful INTEGER,
                        FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
                    );";
                dbCommand.ExecuteNonQuery();
                Debug.Log("Checked/Created 'LoginAttempts' table.");

            }
            dbConnection.Close(); // close the connection to the database
        }
    }

    public void RecordLoginAttempt(int userID, bool wasSuccessful,string username)
    {
        try
        {
            using (var dbConnection = GetDBConnection())
            {
                dbConnection.Open();
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO LoginAttempts (user_id, attempt_time, was_successful) VALUES (@user_id, @attempt_time, @was_successful);";

                    cmd.Parameters.Add(new SqliteParameter("@user_id", userID));
                    cmd.Parameters.Add(new SqliteParameter("@attempt_time", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")));
                    Debug.Log("Recording login attempt for user ID: " + userID + " at " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " Was successful: " + wasSuccessful);
                    if (wasSuccessful)
                    {
                        cmd.Parameters.Add(new SqliteParameter("@was_successful", 1));// 1 for successful attempt
                        
                       
                    }
                    else if (!wasSuccessful)
                    {
                        cmd.Parameters.Add(new SqliteParameter("@was_successful", 2));// 2 for failed attempt
                    }

                    cmd.ExecuteNonQuery();


                }
                dbConnection.Close();
            }
        }
        catch(Exception e)
        {
            ErrorHandling.LogError(e.ToString(), e);
            Debug.LogWarning("Failed to record login attempt. Check error log for details.");
        }
    }
    public SqliteConnection GetDBConnection()
    {
        return new SqliteConnection(stringConnection);
    }
    private void CleanupOldLoginAttempts()
    {
        try
        {
            using (var conn = GetDBConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Delete attempts older than 1 hour
                    cmd.CommandText = "DELETE FROM LoginAttempts WHERE attempt_time < datetime('now', '-1 Hour');";
                    int rowsDeleted = cmd.ExecuteNonQuery();
                    Debug.Log($"Cleaned up {rowsDeleted} old login attempts.");
                }
                conn.Close();
            }
        }
        catch (Exception e)
        {
            ErrorHandling.LogError(e.ToString(), e);
            Debug.LogError("Error cleaning up login attempts. See the error file for more info.");


        }
        
        
    }


    public bool isLockedOut(string username)
    {
        string sql = @"
            SELECT * 
            FROM LoginAttempts 
            WHERE user_id = (SELECT user_id FROM Users WHERE username = @username)
              AND was_successful = 2
              AND attempt_time >= datetime('now', '-1 minute');";
        try
        {



            using (var dbConnection = new SqliteConnection(stringConnection))
            {
                using (var cmd = dbConnection.CreateCommand())
                {
                    dbConnection.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqliteParameter("@username", username));
                    using (var reader = cmd.ExecuteReader())
                    {
                        int failedAttempts = 0;
                        while (reader.Read())
                        {
                            failedAttempts++;
                        }
                        if (failedAttempts >= 5)
                        {
                            Debug.Log("User " + username + " is locked out due to too many failed login attempts.");
                            return true; // User is locked out
                        }
                        Debug.Log("User " + username + " is not locked out." + failedAttempts);
                    }
                    dbConnection.Close();

                    return false; // User is not locked out
                }
            }
        }
        catch (Exception e)
        {
            ErrorHandling.LogError(e.ToString(), e);
            Debug.LogError("Error checking lockout status. See the error file for more info.");
            return false; // In case of error, do not lock out the user
        }
    }

    

}

    public static class Hashing
    {
        public static string Hash(string password)
        {
            int hash = 2027; // starting with a prime number (using a really big one so it is harder to detect)

            if (string.IsNullOrEmpty(password))
            {
                return 0.ToString(); // return if the password is null or empty
            }

            byte[] data = Encoding.UTF8.GetBytes(password); // convert the password to a byte array


            for (int i = 0; i < data.Length; i++)
            {
                hash = (hash * 37) + (data[i] * 7); // multiply the hash by a prime number and add the byte value
            }
            char[] reversedChars = password.ToCharArray();// convert the password to a char array
            Array.Reverse(reversedChars);// reverse the char array
            string reversed = new string(reversedChars);// convert the reversed char array back to a string

            foreach (char c in reversed)
            {
                hash = (hash * 41); // another prime multiplier to make it more complex
                hash += (int)c; // add the ASCII value of the character
            }


            return hash.ToString("X8"); // return the hash as a hexadecimal string

        }
    }





