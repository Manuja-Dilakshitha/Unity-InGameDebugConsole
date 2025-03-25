using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq.Expressions;
using IGDC; //import inGameDebugConsole

public class Tests : MonoBehaviour
{
    [DebugVariable]
    string sampleString = "this is a sample string";

    [DebugVariable]
    [SerializeField]
    int _sampleInt = 1;

    void Start()
    {
        StartCoroutine(simulateErrorsIE());
        StartCoroutine(simulateWarningsIE());
    }

    IEnumerator simulateErrorsIE()
    {
        yield return new WaitForSeconds(6);

        //this will be used to simulate errors withing certain time intervals
        //Debug.Log("This is a sample log message");
        Debug.LogError("This is a test error!");

        // KeyNotFoundException: Trying to access a non-existing key in a dictionary
        Dictionary<string, int> scores = new Dictionary<string, int>();
        scores.Add("Player1", 100);

        // This key doesn’t exist, triggering a KeyNotFoundException
        int nonExistentScore = scores["Player2"];

        yield return null;
    }

    IEnumerator simulateWarningsIE()
    {
        yield return new WaitForSeconds(8);

        Debug.LogWarning("Rigidbody component is missing! This may cause issues.");
    }
}
