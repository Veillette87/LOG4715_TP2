using UnityEngine;

public static class CheckpointData
{
    public static bool hasCheckpoint = false;
    public static Vector3 checkpointPos;

    public static void ResetData()
    {
        hasCheckpoint = false;
        checkpointPos = Vector3.zero;
    }
}