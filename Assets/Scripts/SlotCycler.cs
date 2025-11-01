using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SlotCycler : MonoBehaviour
{
    public Image imageUI;
    public Sprite[] symbols;
    private int index = 0;
    // Event invoked after the slot index changes 
    public UnityEvent onValueChanged = new UnityEvent();

    void Start()
    {
        // set starting image
        imageUI.sprite = symbols[index];
        imageUI.color = Color.yellow;
        // add click event
        GetComponentInChildren<Button>().onClick.AddListener(CycleImage);
    }

    void CycleImage()
    {
        index = (index + 1) % symbols.Length;
        imageUI.sprite = symbols[index];
        imageUI.color = Color.yellow;
        // Notify listeners that the value/index changed
        onValueChanged.Invoke();
    }

    public int GetIndex()
    {
        return index;
    }
}
