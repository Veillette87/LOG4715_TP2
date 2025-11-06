using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SlotCycler : MonoBehaviour
{
    public Image imageUI;
    public Sprite[] symbols;
    private int index = 0;
    public UnityEvent onValueChanged = new UnityEvent();

    void Start()
    {
        imageUI.sprite = symbols[index];
        imageUI.color = Color.yellow;
        GetComponentInChildren<Button>().onClick.AddListener(CycleImage);
    }

    void CycleImage()
    {
        index = (index + 1) % symbols.Length;
        imageUI.sprite = symbols[index];
        imageUI.color = Color.yellow;
        onValueChanged.Invoke();
    }

    public int GetIndex()
    {
        return index;
    }
}
