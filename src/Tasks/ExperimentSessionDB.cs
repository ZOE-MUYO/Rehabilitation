using UnityEngine;
using System;

public class ExperimentSession : MonoBehaviour
{
    public static ExperimentSession Instance;

    public string userName;
    public string startTime;

    void Awake()
    {
        Instance = this;
    }

    public void StartSession(string name)
    {
        userName = name;

        startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        Debug.Log(
            "Session started: " +
            name +
            " time: " +
            startTime
        );
    }
}