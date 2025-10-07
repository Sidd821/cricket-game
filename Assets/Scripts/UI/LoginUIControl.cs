using Mono.Data.Sqlite;
using System;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 // FIX THIS DO THIS

public class LoginUIControl : MonoBehaviour
{
    public InputField usernameField;
    public InputField passwordField;
    public Text feedbackText;
    public Button loginButton;
    public Button registerButton;
    private DatabaseManager dbm;
    public bool passed = false;
    public ErrorHandling ErrorHandling;
    void clearInput()
    {

        //clear the input fields
        usernameField.text = "";
        passwordField.text = "";
    }


    void Start()
    {

        dbm = FindObjectOfType<DatabaseManager>();
        if (dbm == null)
        {
            Debug.LogError("DatabaseManager not found in the scene.");
        }

        registerButton.onClick.AddListener(OnRegisterPress);
        loginButton.onClick.AddListener(OnLoginPress); // add a listener to the button so that when it is pressed, the OnButtonPress function is called    
    }

    private void OnLoginPress() // this is when the button is pressed
    {
        Debug.Log("Login button pressed.");

        loginButton.interactable = false; 
        string username;
        string password;
        username = usernameField.text; // get the text from the input field
        password = passwordField.text; // get the text from the input field

        if (username == "" || username == null || password == "" || password == null)
        {
            feedbackText.text = "Enter both username and password.";
            
            loginButton.interactable = true;
            return; // exit the function if either field is empty
        }
        
        string SQL = "SELECT user_id, password_hash FROM Users WHERE username = @username;"; // SQL query to get the user ID and password hash for the given username
        Debug.Log(dbm.GetDBConnection());
        using (var dbConnection = dbm.GetDBConnection()) // get the database connection from the DatabaseManager
        {
            dbConnection.Open();// open the connection to the database
            using (var cmd = dbConnection.CreateCommand())// create a command to execute the SQL query
            {
                cmd.CommandText = SQL;
                cmd.Parameters.AddWithValue("@username", username);// add the username parameter to the query
                try
                {
                    using (var reader = cmd.ExecuteReader())// execute the query and get a reader to read the results
                    {
                        Debug.Log("Executing SQL: " + cmd.CommandText);
                        if (reader.Read())// if there is a result, the username exists
                        {
                            int userID = reader.GetInt32(0);//Gets user ID
                            string storedHash = reader.GetString(1);//Gets stored password hash

                            string inputHash = Hashing.Hash(password);// hashes the password entered by the user
                            if (dbm.isLockedOut(username))
                            {
                               feedbackText.text =  "User " + username + " is locked out due to too many failed login attempts.";
                               loginButton.interactable = true;
                                return;

                            }
                            if (inputHash == storedHash)// compares the hashed password with the stored hash
                            {
                                feedbackText.text = "Login successful!";
                                
                                Debug.Log("User " + username + " logged in successfully.");
                                dbConnection.Close();
                                UserSession.StartSession(userID, username);
                                loginButton.interactable = true;
                                dbm.createLoginAttemptsTable(); // ensure the login attempts table exists
                                dbm.RecordLoginAttempt(userID, true,username); // record the successful login attempt
                                clearInput();
                                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

                            }
                            else if (inputHash != storedHash)
                            {
                                feedbackText.text = "Incorrect password.";
                                dbConnection.Close();

                                loginButton.interactable = true;
                                dbm.createLoginAttemptsTable(); // ensure the login attempts table exists
                                dbm.RecordLoginAttempt(userID, false,username); // record the failed login attempt

                                Debug.Log("User " + username + " failed to log in due to incorrect password.");
                                return;
                            }

                        }
                        else
                        {
                            feedbackText.text = "usename does not exist: Create account or Try again!"; // means there is no user in the database
                            Debug.Log("Login failed: Username " + username + " does not exist.");

                            loginButton.interactable = true;
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorHandling.LogError(e.ToString(), e);
                    Debug.LogWarning("Failed to record login attempt. Check error log for details.");


                }
                
            }

            dbConnection.Close(); // close the connection to the database

        }

        loginButton.interactable = true;
    }
    public bool IsPasswordStrong(string password)
    {
        // Minimum 8 chars, at least 1 uppercase, 1 lowercase, 1 number, 1 special character
        string pattern =
                            @"^(?=.*[a-z])" +     // Must have lowercase  
                            @"(?=.*[A-Z])" +      // Must have uppercase
                            @"(?=.*[0-9])" +      // Must have number
                            @"(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])" +   // Must have special char
                            @".{8,}$";       

        return System.Text.RegularExpressions.Regex.IsMatch(password, pattern); // returns true if the password matches the pattern

    }
    
        // Need to username check if it exists already
       
            
     void OnRegisterPress()
    {
        registerButton.interactable = false;
        string username = usernameField.text;
        string password = passwordField.text;
        Debug.Log("Register button pressed with username: " + username);
        if (username == "" || username == null || password == "" || password == null)
        {
            feedbackText.text = "Enter both username and password.";
            registerButton.interactable = true;

            return; // exit the function if either field is empty
        }
        string SQL = "SELECT * FROM Users WHERE username = @username;";// Gets all users with the given username showing userid, username, password_hash
        using (var dbConnection = dbm.GetDBConnection())// get the database connection from the DatabaseManager
        {
            dbConnection.Open();
            using (var cmd = dbConnection.CreateCommand())// create a command to execute the SQL query
            {
                cmd.CommandText = SQL;
                cmd.Parameters.Add(new SqliteParameter("@username", username));// add the username parameter to the query
                // ExecuteScalar returns the first column of the first row in the result set, or a null reference if the result set is empty.

                if (cmd.ExecuteScalar() != null)
                {
                    feedbackText.text = "Username already exists. Please choose a different username.";
                    clearInput();
                    registerButton.interactable = true;
                    return;
                }
                if (!IsPasswordStrong(password))
                {
                    feedbackText.text = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.";
                    registerButton.interactable = true;

                    return;
                }


                // Hash the password
                string passwordHash = Hashing.Hash(password);

                // Insert the new user into the database
                string newUserSQL = "INSERT INTO Users (username, password_hash)VALUES (@username, @password_hash);";// SQL query to insert a new user
                cmd.Parameters.Clear(); // Clear previous parameters
                cmd.CommandText = newUserSQL;
                cmd.Parameters.Add(new SqliteParameter("@username", username));// add the username parameter to the query
                cmd.Parameters.Add(new SqliteParameter("@password_hash", passwordHash));// add the hashed password parameter to the query
                try
                {
                    cmd.ExecuteNonQuery();// execute the command to insert the new user
                    feedbackText.text = "Registration successful! You can now log in.";
                    Debug.Log("User " + username + " registered successfully.");
                    clearInput();
                    passed = false; // reset the passed variable for future checks
                                    // takes user into the menu and logs them in automatically
                }
                catch (Exception e)
                {
                    ErrorHandling.LogError(e.ToString(), e);
                    Debug.LogWarning("Failed to Register. Check error log for details.");


                }
            }
            dbConnection.Close(); // close the connection to the database
        }
       

        registerButton.interactable = true;
     }

   

}
