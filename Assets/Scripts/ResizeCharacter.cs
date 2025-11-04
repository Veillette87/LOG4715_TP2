using UnityEngine;
using System.Reflection;

public class ResizeCharacter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController2D controller;
    [SerializeField] Rigidbody2D rb;

    [Header("Camera Zoom")]
    [SerializeField] Camera followCamera;
    [SerializeField] CameraFollow2D followScript;
    Vector3 normalOffset = new Vector3(0f, 2f, -10f);
    Vector3 shrunkOffset = new Vector3(0f, 0f, -10f);

    [Header("Sizing")]
    [Range(0.05f, 1f)] public float tinyScaleFactor = 0.25f;
    [Min(0f)] public float lerpTime = 0.1f;

    [Header("Safety")]
    [SerializeField] LayerMask solidMask;
    [SerializeField] float ceilingCheckDistance = 1.3f;

    public KeyCode toggleKey = KeyCode.LeftShift;
    public bool isTiny;

    Vector3 _scaleNormal;
    float _moveSpeed0, _airMax0, _desiredJumpHeight0;
    float _ortho0;
    FieldInfo _fMoveSpeed, _fAirMax, _fDesiredJumpHeight;

    void Reset()
    {
        controller = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        _scaleNormal = transform.localScale;

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        _fMoveSpeed = typeof(PlayerController2D).GetField("moveSpeed", flags);
        _fAirMax = typeof(PlayerController2D).GetField("airMax", flags);
        _fDesiredJumpHeight = typeof(PlayerController2D).GetField("desiredJumpHeight", flags);

        if (_fMoveSpeed == null || _fAirMax == null || _fDesiredJumpHeight == null)
        {
            enabled = false;
            return;
        }

        _moveSpeed0 = (float)_fMoveSpeed.GetValue(controller);
        _airMax0 = (float)_fAirMax.GetValue(controller);
        _desiredJumpHeight0 = (float)_fDesiredJumpHeight.GetValue(controller);

        if (followCamera != null)
        {
            _ortho0 = followCamera.orthographicSize;
        }
    }

    void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        if (isTiny) GrowToNormal();
        else ShrinkToTiny();
    }

    public void ShrinkToTiny()
    {
        if (isTiny) return;
        isTiny = true;

        float k = Mathf.Clamp(tinyScaleFactor, 0.05f, 1f);

        _fMoveSpeed.SetValue(controller, _moveSpeed0 * k);
        _fAirMax.SetValue(controller, _airMax0 * k);
        _fDesiredJumpHeight.SetValue(controller, _desiredJumpHeight0 * k);

        controller.RecomputeJumpParameters();

        if (rb != null)
        {
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(Mathf.Clamp(v.x, -_airMax0 * k, _airMax0 * k), v.y);
        }

        StopAllCoroutines();
        StartCoroutine(LerpScale(_scaleNormal * k, lerpTime));

        if (followCamera != null)
        {
            float targetOrtho = _ortho0 * k;
            StartCoroutine(LerpOrtho(targetOrtho, lerpTime));
        }

        if (followScript != null)
        {
            StartCoroutine(LerpOffset(shrunkOffset, lerpTime));
        }
    }

    public void GrowToNormal()
    {
        if (!isTiny) return;
        if (!CanGrow()) return;

        isTiny = false;

        _fMoveSpeed.SetValue(controller, _moveSpeed0);
        _fAirMax.SetValue(controller, _airMax0);
        _fDesiredJumpHeight.SetValue(controller, _desiredJumpHeight0);

        controller.RecomputeJumpParameters();

        StopAllCoroutines();
        StartCoroutine(LerpScale(_scaleNormal, lerpTime));

        if (followCamera != null)
        {
            StartCoroutine(LerpOrtho(_ortho0, lerpTime));
        }

        if (followScript != null)
        {
            StartCoroutine(LerpOffset(normalOffset, lerpTime));
        }
    }

    bool CanGrow()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return true;

        Bounds b = col.bounds;
        Vector2 left = new Vector2(b.min.x, b.min.y);
        Vector2 mid = new Vector2(b.center.x, b.min.y);
        Vector2 right = new Vector2(b.max.x, b.min.y);

        return
            CeilingClear(left) && CeilingClear(mid) && CeilingClear(right);
    }

    bool CeilingClear(Vector2 origin)
    {
        return Physics2D.Raycast(origin, Vector2.up, ceilingCheckDistance, solidMask).collider == null;
    }


    System.Collections.IEnumerator LerpScale(Vector3 target, float t)
    {
        Vector3 start = transform.localScale;
        float timer = 0f;
        while (timer < t)
        {
            timer += Time.deltaTime;
            float a = t <= 0f ? 1f : timer / t;
            transform.localScale = Vector3.LerpUnclamped(start, target, a);
            yield return null;
        }
        transform.localScale = target;
    }

    System.Collections.IEnumerator LerpOrtho(float target, float t)
    {
        float start = followCamera.orthographicSize;
        float timer = 0f;
        while (timer < t)
        {
            timer += Time.deltaTime;
            float a = t <= 0f ? 1f : timer / t;
            followCamera.orthographicSize = Mathf.LerpUnclamped(start, target, a);
            yield return null;
        }
        followCamera.orthographicSize = target;
    }

    System.Collections.IEnumerator LerpOffset(Vector3 target, float t)
    {
        Vector3 start = followScript.offset;
        float timer = 0f;
        while (timer < t)
        {
            timer += Time.deltaTime;
            float a = t <= 0f ? 1f : timer / t;
            followScript.offset = Vector3.LerpUnclamped(start, target, a);
            yield return null;
        }
        followScript.offset = target;
    }

    void OnDisable()
    {
        if (controller != null)
        {
            _fMoveSpeed?.SetValue(controller, _moveSpeed0);
            _fAirMax?.SetValue(controller, _airMax0);
            _fDesiredJumpHeight?.SetValue(controller, _desiredJumpHeight0);
            controller.RecomputeJumpParameters();
        }
        transform.localScale = _scaleNormal;

        if (followCamera != null)
            followCamera.orthographicSize = _ortho0;

        isTiny = false;
    }
}
