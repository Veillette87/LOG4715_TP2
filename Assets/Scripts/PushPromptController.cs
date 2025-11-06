using UnityEngine;
using TMPro;

public class PushPromptController : MonoBehaviour
{
    [Header("Références")]
    public Transform player;
    public TextMeshPro textMesh;

    [Header("Paramètres d’affichage")]
    public float showDistance = 2f;
    public float minDistance = 0.4f;
    public float alignTolerance = 1.2f;
    public float offsetY = 1f;

    private Transform column;

    void Start()
    {
        column = transform.parent;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        var renderer = textMesh.GetComponent<MeshRenderer>();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 0;

        textMesh.enabled = false;
    }

    void Update()
    {
        if (column == null || player == null) return;

        Vector2 diff = player.position - column.position;
        float absX = Mathf.Abs(diff.x);
        float absY = Mathf.Abs(diff.y);

        bool isSideAligned = absY < alignTolerance;
        bool inHorizontalRange = absX < showDistance && absX > minDistance;

        bool shouldShow = isSideAligned && inHorizontalRange;

        textMesh.enabled = shouldShow;

        if (shouldShow)
        {
            transform.position = column.position + new Vector3(0, offsetY, 0);
            textMesh.transform.rotation = Quaternion.identity;
            textMesh.alignment = TextAlignmentOptions.Center;
        }
    }
}
