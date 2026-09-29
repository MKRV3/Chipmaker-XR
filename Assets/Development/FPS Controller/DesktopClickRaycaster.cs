using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask hitMask = ~0; // лучше поставить Interactable

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || cam == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Ищем WaferSpawn на объекте кнопки или у родителей (если Collider на дочернем)
            var spawner = hit.collider.GetComponentInParent<WaferSpawn>();
            if (spawner != null)
            {
                spawner.SpawnWafer(); // имя метода должно совпадать с твоим
            }
        }
    }
}