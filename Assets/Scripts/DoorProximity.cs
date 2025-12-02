using UnityEngine;

public class DoorProximityByDistance : MonoBehaviour
{
    [SerializeField] private GameObject proximityUI;
    [SerializeField] private GameObject enigmeUI;
    [SerializeField] private Transform player;
    [SerializeField] private float showDistance = 1.5f;

    private bool isInRange = false;

    private void Awake()
    {
        if (proximityUI) proximityUI.SetActive(false);
        if (enigmeUI) enigmeUI.SetActive(false);
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
        isInRange = d <= showDistance;

        if (proximityUI && proximityUI.activeSelf != isInRange)
            proximityUI.SetActive(isInRange);

        if (isInRange && Input.GetKeyDown(ControlsManager.GetKey(PlayerAction.Interact)))
        {
            ShowEnigmeUI();
        }

        if (enigmeUI && enigmeUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideEnigmeUI();
        }
    }

    private void ShowEnigmeUI()
    {
        if (enigmeUI)
        {
            enigmeUI.SetActive(true);
            GlobalSettings.Instance.SetUIVisibility(true);
            SetPlayerMovementEnabled(false);

        }
    }


    public void HideEnigmeUI()
    {
        if (enigmeUI)
        {
            enigmeUI.SetActive(false);
            GlobalSettings.Instance.SetUIVisibility(false);
            SetPlayerMovementEnabled(true);
        }
    }


    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (!player) return;

        var pm2 = player.GetComponent<PlayerController2D>() ?? player.GetComponentInChildren<PlayerController2D>();
        if (pm2 != null)
        {
            pm2.SetMovementEnabled(enabled);
            return;
        }
    }
}
