using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DesktopFpsMove : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float gravity = -20f;

    private CharacterController cc;
    private float pitch;
    private float verticalVel;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main != null ? Camera.main.transform : null;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (cam == null) return;

        // Mouse Lock Switch
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
        bool show = !(Cursor.visible);
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // LOOK (mouse delta)
        Vector2 md = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        float yaw = md.x * mouseSensitivity;
        float pitchDelta = -md.y * mouseSensitivity;

        transform.Rotate(0f, yaw, 0f);
        pitch = Mathf.Clamp(pitch + pitchDelta, -80f, 80f);
        cam.localEulerAngles = new Vector3(pitch, 0f, 0f);

        // MOVE (WASD)
        Vector2 move = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) move.x -= 1;
            if (Keyboard.current.dKey.isPressed) move.x += 1;
            if (Keyboard.current.sKey.isPressed) move.y -= 1;
            if (Keyboard.current.wKey.isPressed) move.y += 1;
        }

        Vector3 wish = (transform.right * move.x + transform.forward * move.y);
        if (wish.sqrMagnitude > 1f) wish.Normalize();

        if (cc.isGrounded && verticalVel < 0f) verticalVel = -2f;
        verticalVel += gravity * Time.deltaTime;

        Vector3 vel = wish * moveSpeed;
        vel.y = verticalVel;

        cc.Move(vel * Time.deltaTime);
    }
}