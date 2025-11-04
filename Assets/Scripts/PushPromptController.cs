using UnityEngine;
using TMPro;

public class PushPromptController : MonoBehaviour
{
    [Header("Références")]
    public Transform player; // Assigné automatiquement si trouvé par tag
    public TextMeshPro textMesh; // Ton texte "Push"

    [Header("Paramètres d’affichage")]
    public float showDistance = 2f;  // distance à laquelle le texte apparaît
    public float offsetY = 1f;         // hauteur au-dessus du sol

    private Transform column;

    void Start()
    {
        column = transform.parent; // le parent = la colonne
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        var renderer = textMesh.GetComponent<MeshRenderer>();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 0;

        textMesh.enabled = false; // invisible par défaut
    }

    void Update()
    {
        if (column == null) return;

        // Toujours afficher si le joueur est proche
        float distance = Vector2.Distance(player.position, column.position);
        bool shouldShow = distance < showDistance;

        textMesh.enabled = shouldShow;

        if (shouldShow)
        {
            // Positionne le texte toujours au-dessus de la colonne
            transform.position = column.position + new Vector3(0, offsetY, 0);
            textMesh.transform.rotation = Quaternion.identity; // face caméra
            textMesh.alignment = TextAlignmentOptions.Center;
        }
    }

}
