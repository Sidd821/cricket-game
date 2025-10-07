using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserSession
{
    // Public properties to store user data
    public static int UserId { get; private set; }// every method needs to know the user id
    public static string Username { get; private set; }// every method needs to know the username
    public static bool IsLoggedIn { get; private set; }// 

    // Method to start a new session when login is complete
    public static void StartSession(int userId, string username)
    {
        UserId = userId;
        Username = username;
        IsLoggedIn = true;

        Debug.Log("Session started for user: " +  username + " ID: + " + userId.ToString());
    }// letting the game know that the user is logged in and will stay logged in until they log out

    // Method to end the session when user logs out
    public static void EndSession()
    {
        UserId = 0;
        Username = null;
        IsLoggedIn = false;

        Debug.Log("User session ended.");
    }// logs the user out and then makes it so they have to log in again after.

    // Method to check if a user is logged in (simple example)
    public static bool HasActiveSession()
    {
        return IsLoggedIn;// this shows the login status (true for logged in and false for not logged in)
    }
}
