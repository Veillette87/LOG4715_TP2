using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelPuzzleController : MonoBehaviour
{
    [Header("Slots (order matters: left → right)")]
    [SerializeField] private SlotCycler slot1;
    [SerializeField] private SlotCycler slot2;
    [SerializeField] private SlotCycler slot3;
    [SerializeField] private AudioSource audioSource;
    float audioLength = 3f;

    [Header("Correct combination (indexes)")]
    // Example: 0 = glyph1, 1 = glyph2, 2 = glyph3
    [SerializeField] private int[] correct = new int[3] { 2, 0, 1 };

    private bool solved;

    private void Awake()
    {

        if (!slot1 || !slot2 || !slot3)
        {
            var slots = GetComponentsInChildren<SlotCycler>(true);
            if (slots.Length >= 3)
            {
                //order left→right
                slot1 = slots[0];
                slot2 = slots[1];
                slot3 = slots[2];
            }
        }

        HookSlot(slot1);
        HookSlot(slot2);
        HookSlot(slot3);

        Check();
    }

    private void HookSlot(SlotCycler slot)
    {
        if (!slot) return;

        slot.onValueChanged.AddListener(Check);
    }

    public void Check()
    {
        if (solved) return;
        if (!slot1 || !slot2 || !slot3 || correct.Length < 3) return;


        bool ok =
            slot1.GetIndex() == correct[0] &&
            slot2.GetIndex() == correct[1] &&
            slot3.GetIndex() == correct[2];


        if (!ok) return;

        solved = true;

        // 1. On récupère la position du joueur (plus sûr que la position du puzzle)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // On sauvegarde la position actuelle du joueur comme nouveau point de respawn
            Vector3 newPos = player.transform.position;
            newPos.x += 2f;
            
            CheckpointData.checkpointPos = newPos;
            CheckpointData.hasCheckpoint = true;
            
            // Optionnel : Si tu veux aussi soigner le joueur après l'énigme
            var hp = player.GetComponent<PlayerHealth>();
            if (hp) hp.ResetHPToMax();
        }
        else
        {
            // Si jamais on ne trouve pas le joueur, on utilise la position du puzzle par sécurité
            CheckpointData.checkpointPos = transform.position; 
            CheckpointData.hasCheckpoint = true;
        }

        LockSlots();
        var door = GameObject.FindGameObjectWithTag("Door");
        if (door)
        {
            if (audioSource)
            {
                audioSource.time = 3f;
                audioSource.Play();
            }
            var runner = GameObject.FindGameObjectWithTag("Object").GetComponent<DoorSequence>();
            if (runner)
            {
                runner.PlayFromAndClose(door, startAtSeconds: 3f, playSeconds: 2.5f);
            }

            var doorProx = door.GetComponent<DoorProximityByDistance>()
                           ?? door.GetComponentInChildren<DoorProximityByDistance>();
            if (doorProx != null)
            {
                doorProx.HideEnigmeUI();
            }
        }

    }

    private void LockSlots()
    {
        SetInteractable(slot1, false);
        SetInteractable(slot2, false);
        SetInteractable(slot3, false);
    }

    private void SetInteractable(SlotCycler slot, bool value)
    {
        if (!slot) return;
        var btn = slot.GetComponent<Button>() ?? slot.GetComponentInChildren<Button>(true);
        if (btn) btn.interactable = value;
    }

}
