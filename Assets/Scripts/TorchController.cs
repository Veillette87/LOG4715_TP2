using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour
{
    [SerializeField] Light2D torchLight;
    [SerializeField] bool hasTorch = true;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        ApplyAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(ControlsManager.GetKey(PlayerAction.Torch)))
        {
            hasTorch = !hasTorch;
            ApplyAll();
        }
        if (hasTorch)
        {
            torchLight.enabled = true;
        }
        else
        {
            torchLight.enabled = false;
        }
    }

    void ApplyAll()
    {
        if (anim) anim.SetBool("HasTorch", hasTorch);
    }
}
