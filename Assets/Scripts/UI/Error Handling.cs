using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class ErrorHandling : MonoBehaviour
{
   
    private static string logPath = Path.Combine(Application.persistentDataPath, "error_log.txt");

    public static void LogError(string message, Exception e)
    {


        try
        {
            string logMessage = $"{System.DateTime.Now}: {message}\n";
            File.AppendAllText(logPath, logMessage);

        }
        catch (Exception fileEx)
        {
            Debug.LogError("Failed to write to log file: " + fileEx.Message);
        }
    }
}
