using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //THIS IS A STATIC MASTER CLASS, HOPEFULLY
    public const int MAIN_MENU_INDEX = 0;
    public const int LEVEL_1 = 1;
    public const int LEVEL_2 = 2;
    public const int LEVEL_3 = 3;
    public const int SURVIVAL_MODE = 4;

    public const int START_CUTSCENE = 5;
    public const int LEVEL_1_ED = 6;
    public const int LEVEL_2_OP = 7;
    public const int LEVEL_2_ED = 8;
    public const int LEVEL_3_OP = 9;
    public const int LEVEL_3_ED = 10;

    public static int sceneIndex;
    public static int runningScore = 0;

    //THESE METHODS ARE SO WE CAN TRACK WHERE THEY ARE BEING USED
    public static void ResetRunningScore()
    {
        Debug.Log("Reset the running score to 0!");
        runningScore = 0;
    }
    public static int GetRunningScore()
    {
        Debug.Log("using the running score...");
        return runningScore;
    }
    public static void SetRunningScore(int newScore)
    {
        Debug.Log("NEW running score: " + newScore);
        runningScore = newScore;
    }
    public static void AddRestartCounterToRunningScore()
    {
        Debug.Log("You restarted, and got marked!");
        runningScore++;
    }
}
