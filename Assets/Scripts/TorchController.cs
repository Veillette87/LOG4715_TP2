using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour
{
    [SerializeField] Light2D torchLight;
    [SerializeField] KeyCode toggleKey = KeyCode.F;
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
        if (Input.GetKeyDown(toggleKey))
        {
            hasTorch = !hasTorch;
            ApplyAll();
        }
        if (hasTorch && !GlobalSettings.Instance.IsLightsOn)
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
