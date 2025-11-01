using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelPuzzleController : MonoBehaviour
{
    [Header("Slots (order matters: left → right)")]
    [SerializeField] private SlotCycler slot1;
    [SerializeField] private SlotCycler slot2;
    [SerializeField] private SlotCycler slot3;

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

        // Subscribe to each slot's Button click to re-check puzzle state
        HookSlot(slot1);
        HookSlot(slot2);
        HookSlot(slot3);

        Check();
    }

    private void HookSlot(SlotCycler slot)
    {
        if (!slot) return;

        // Listen to the SlotCycler's change event so Check() runs after the slot updates its index.
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
        LockSlots();
        var door = GameObject.FindGameObjectWithTag("Door");
        if (door)
        {



            var doorProx = door.GetComponent<DoorProximityByDistance>()
                           ?? door.GetComponentInChildren<DoorProximityByDistance>();
            if (doorProx != null)
            {
                doorProx.HideEnigmeUI();
            }

            //  open the door 
            door.SetActive(false);
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
