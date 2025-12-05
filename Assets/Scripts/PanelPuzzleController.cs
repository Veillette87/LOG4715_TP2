using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelPuzzleController : MonoBehaviour
{
    [Header("Identifiant Unique (IMPORTANT)")]
    [Tooltip("Donne un nom différent à chaque puzzle (ex: Puzzle_Niveau1, Puzzle_Foret, etc.)")]
    public string puzzleID = "Puzzle_Default"; // <-- NOUVEAU

    [Header("Slots (order matters: left → right)")]
    [SerializeField] private SlotCycler slot1;
    [SerializeField] private SlotCycler slot2;
    [SerializeField] private SlotCycler slot3;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DoorSequence doorSequence;
    [SerializeField] private GameObject door;
    float audioLength = 3f;

    [Header("Correct combination (indexes)")]
    [SerializeField] private int[] correct = new int[3] { 2, 0, 1 };

    private bool solved;

    private void Awake()
    {
        // ... (Ton code d'initialisation des slots reste identique) ...
        if (!slot1 || !slot2 || !slot3)
        {
            var slots = GetComponentsInChildren<SlotCycler>(true);
            if (slots.Length >= 3)
            {
                slot1 = slots[0];
                slot2 = slots[1];
                slot3 = slots[2];
            }
        }

        HookSlot(slot1);
        HookSlot(slot2);
        HookSlot(slot3);
    }

    private void Start()
    {
        // NOUVEAU : Vérification au démarrage
        // Si cet ID est déjà dans la mémoire statique, on met l'état "Résolu" directement
        if (CheckpointData.solvedPuzzles.Contains(puzzleID))
        {
            SetAlreadySolvedState();
        }
        else
        {
            Check(); // Vérification normale
        }
    }

    private void HookSlot(SlotCycler slot)
    {
        if (!slot) return;
        slot.onValueChanged.AddListener(Check);
    }

    // Cette fonction sert à remettre le puzzle en état "fini" sans donner les bonus (vie/checkpoint)
    private void SetAlreadySolvedState()
    {
        solved = true;
        LockSlots();
        
        // On ouvre la porte instantanément ou on joue l'anim de fin
        if (door)
        {
            if (audioSource)
            {
                audioSource.time = 3f;
                audioSource.Play();
            }
            if (doorSequence)
            {
                doorSequence.PlayFromAndClose(door, startAtSeconds: 3f, playSeconds: 2.5f);
            }

            var doorProx = door.GetComponent<DoorProximityByDistance>()
                            ?? door.GetComponentInChildren<DoorProximityByDistance>();
            if (doorProx != null)
            {
                doorProx.HideEnigmeUI();
            }
        }
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

        // --- SUCCÈS : C'est ici que le joueur résout l'énigme pour la PREMIÈRE fois ---
        
        solved = true;
        
        // 1. On l'ajoute à la mémoire pour ne plus jamais le refaire
        if (!CheckpointData.solvedPuzzles.Contains(puzzleID))
        {
            CheckpointData.solvedPuzzles.Add(puzzleID);
        }

        // 2. Checkpoint logic
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 newPos = player.transform.position;
            newPos.x += 2f;
            CheckpointData.checkpointPos = newPos;
            CheckpointData.hasCheckpoint = true;

            // 3. LE SOIN : Se produit uniquement ici, lors de la résolution active
            var hp = player.GetComponent<PlayerHealth>();
            if (hp) hp.ResetHPToMax();
        }
        else
        {
            CheckpointData.checkpointPos = transform.position;
            CheckpointData.hasCheckpoint = true;
        }

        // 4. Animation et Son
        LockSlots();
        if (door)
        {
            if (audioSource)
            {
                audioSource.time = 3f;
                audioSource.Play();
            }
            if (doorSequence)
            {
                doorSequence.PlayFromAndClose(door, startAtSeconds: 3f, playSeconds: 2.5f);
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