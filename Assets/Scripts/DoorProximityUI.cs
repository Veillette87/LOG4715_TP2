using UnityEngine;

public class DoorProximityByDistance : MonoBehaviour
{
    [SerializeField] private GameObject proximityUI;
    [SerializeField] private Transform player;
    [SerializeField] private float showDistance = 1.5f;

    private void Awake()
    {
        if (proximityUI) proximityUI.SetActive(false);
    }

    private void Update()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            else return;
        }

        float d = Vector2.Distance(transform.position, player.position);
        bool shouldShow = d <= showDistance;

        if (proximityUI && proximityUI.activeSelf != shouldShow)
            proximityUI.SetActive(shouldShow);
    }
}
