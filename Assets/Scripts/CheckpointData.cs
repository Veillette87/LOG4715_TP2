using UnityEngine;
using System.Collections.Generic;

public static class CheckpointData
{
    public static bool hasCheckpoint = false;
    public static Vector3 checkpointPos;

    public static List<string> solvedPuzzles = new List<string>(); // énigmes résolues

    public static void ResetData()
    {
        hasCheckpoint = false;
        checkpointPos = Vector3.zero;
        solvedPuzzles.Clear();
    }
}