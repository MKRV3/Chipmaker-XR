using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private LayerMask grabbableMask = ~0;

    [Header("Hold")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float followSpeed = 25f;

    [Header("Options")]
    [SerializeField] private bool holdToGrab = true;     // true: держим E; false: toggle
    [SerializeField] private bool disableCollidersWhileHeld = false; // обычно лучше false, чтобы триггеры работали

    private Rigidbody heldRb;
    private Collider[] heldColliders;

    private void Awake()
    {
        if (!holdPoint)
        {
            var hp = transform.Find("HoldPoint");
            if (hp) holdPoint = hp;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (holdToGrab)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                TryGrab();

            if (Keyboard.current.eKey.wasReleasedThisFrame)
                Drop();
        }
        else
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (heldRb) Drop();
                else TryGrab();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!heldRb || !holdPoint) return;

        Vector3 targetPos = holdPoint.position;
        Quaternion targetRot = holdPoint.rotation;

        // “Мягкая” подгонка к точке удержания
        Vector3 newPos = Vector3.Lerp(heldRb.position, targetPos, Time.fixedDeltaTime * followSpeed);
        heldRb.MovePosition(newPos);
        heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, targetRot, Time.fixedDeltaTime * followSpeed));
    }

    private void TryGrab()
    {
        if (heldRb || !holdPoint) return;

        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableMask, QueryTriggerInteraction.Ignore))
            return;

        var rb = hit.rigidbody;
        if (!rb) return;

        // (опционально) фильтр: хватать только объекты с компонентом Wafer
        // if (!rb.GetComponent<Wafer>()) return;

        heldRb = rb;
        heldColliders = heldRb.GetComponentsInChildren<Collider>();

        // Подготовка физики
        heldRb.useGravity = false;
        heldRb.linearVelocity = Vector3.zero;
        heldRb.angularVelocity = Vector3.zero;

        // Чтобы предмет не дёргался/не отскакивал
        heldRb.interpolation = RigidbodyInterpolation.Interpolate;

        if (disableCollidersWhileHeld)
        {
            foreach (var c in heldColliders)
                c.enabled = false;
        }
    }

    private void Drop()
    {
        if (!heldRb) return;

        if (disableCollidersWhileHeld && heldColliders != null)
        {
            foreach (var c in heldColliders)
                c.enabled = true;
        }

        heldRb.useGravity = true;
        heldRb = null;
        heldColliders = null;
    }
}