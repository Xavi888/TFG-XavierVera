using System;
using System.IO;
using UnityEngine;

public class Logger
{
    private string logFilePath;

    public Logger(string fileName)
    {
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("Creating log file at" + logFilePath);

        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "Log file created at " + DateTime.Now + "\n");
        }
    }

    public void Log(string message, int mode)
    {
        string logMessage;
        if (mode == 0) {
            logMessage = "[CHEF] [" + DateTime.Now + "]: " + message + "\n";
        } else if (mode == 1) {
            logMessage = "[WAITER] [" + DateTime.Now + "]: " + message + "\n";
        } else {
            logMessage = DateTime.Now + ": " + message + "\n";
        }
        File.AppendAllText(logFilePath, logMessage);
    }
}
