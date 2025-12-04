using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Coche ça dans l'inspecteur si tu veux que ce checkpoint soigne le joueur au contact
    public bool healPlayerOnTouch = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. On sauvegarde la position dans la mémoire statique
            CheckpointData.hasCheckpoint = true;
            CheckpointData.checkpointPos = transform.position;

            Debug.Log("Checkpoint sauvegardé en mémoire !");

            // 2. (Optionnel) On soigne le joueur quand il touche le checkpoint
            if (healPlayerOnTouch)
            {
                var hp = other.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.ResetHPToMax();
                }
            }
        }
    }
}