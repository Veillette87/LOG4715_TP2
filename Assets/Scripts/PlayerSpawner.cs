using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        // On lance la logique de placement au démarrage de la scène
        StartCoroutine(MovePlayerToCheckpoint());
    }

    private IEnumerator MovePlayerToCheckpoint()
    {
        // (Optionnel) Attend 1 frame pour être sûr que le Player est bien initialisé
        yield return null; 

        // 1. Vérifie si on a sauvegardé une position dans notre classe statique
        if (CheckpointData.hasCheckpoint)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                // 2. Applique la position sauvegardée
                player.transform.position = CheckpointData.checkpointPos;
                Debug.Log("Player déplacé au checkpoint static : " + CheckpointData.checkpointPos);
            }
        }
    }
}