using Fusion;
using System;
using UnityEngine;

public class MyCam : NetworkBehaviour
{
    [Header("Camera Settings")]
    public Transform Body;
    public Transform Head;
    public Camera playerCamera;
    public float mouseSensitivity = 100f;

    [Header("Movement Settings")]
    public float minLookAngle = -90f;
    public float maxLookAngle = 90f;

    // Input variables
    private float mouseX;
    private float mouseY;
    private float verticalRotation;

    // State
    private bool isLocalPlayer;
    private AudioListener playerAudioListener;

    [Networked, OnChangedRender(nameof(OyunBasladýmý))] public bool oyundaMý { get; set; } = true;

    private void OyunBasladýmý()
    {
        Debug.Log("oyun baþlandý");
        if(isLocalPlayer)
        {
            EnablePlayerCamera();
            SetMouseLocked(true);
        }
        else
        {
            DisablePlayerCamera();
        }
    }

    public override void Spawned()
    {
        isLocalPlayer = Object.HasInputAuthority;

        // Audio Listener referansýný al
        if (playerCamera != null)
        {
            playerAudioListener = playerCamera.GetComponent<AudioListener>();
        }

        Debug.Log($"[MyCam] Player spawned - IsLocal: {isLocalPlayer}");

        // Kamera durumunu ayarla
        SetupCamera();
    }

    private void SetupCamera()
    {
        //if (isLocalPlayer)
        //{
        //    // Local player - kamerayý aç
        //    EnablePlayerCamera();
        //    SetMouseLocked(true);
        //    Debug.Log("[MyCam] Local player camera enabled");
        //}
        //else
        //{
        //    // Remote player - kamerayý kapat
        //    DisablePlayerCamera();
        //    Debug.Log("[MyCam] Remote player camera disabled");
        //}
    }

    private void EnablePlayerCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;

            if (playerAudioListener != null)
                playerAudioListener.enabled = true;
        }
    }

    public void DisablePlayerCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = false;

            if (playerAudioListener != null)
                playerAudioListener.enabled = false;
        }
    }

    private void SetMouseLocked(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        // Sadece local player için input handling
        if (!isLocalPlayer) return;

        HandleDebugInput();
    }

    void LateUpdate()
    {
        // Sadece local player için mouse look
        if (!isLocalPlayer) return;

        // Mouse locked deðilse mouse look yapma
        if (Cursor.lockState != CursorLockMode.Locked) return;

        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        // Mouse input al
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Horizontal rotation (Body)
        Body.Rotate(Vector3.up * mouseX);

        // Vertical rotation (Head)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
        Head.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleDebugInput()
    {
        // ESC tuþu ile mouse lock toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMouseLock();
        }

        // F1 debug bilgi
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"[MyCam Debug] IsLocal: {isLocalPlayer}, Camera Enabled: {playerCamera?.enabled}, Mouse Locked: {Cursor.lockState}");
        }
    }

    private void ToggleMouseLock()
    {
        bool isLocked = Cursor.lockState == CursorLockMode.Locked;
        SetMouseLocked(!isLocked);
        Debug.Log($"[MyCam] Mouse {(isLocked ? "unlocked" : "locked")}");
    }

    // Public methods
    public bool IsLocalPlayerCamera()
    {
        return isLocalPlayer;
    }

    public bool IsCameraEnabled()
    {
        return playerCamera != null && playerCamera.enabled;
    }

    public void ForceEnableCamera()
    {
        if (isLocalPlayer)
        {
            EnablePlayerCamera();
            SetMouseLocked(true);
        }
    }

    public void ForceDisableCamera()
    {
        DisablePlayerCamera();
        SetMouseLocked(false);
    }

    // Cleanup
    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            DisablePlayerCamera();
            SetMouseLocked(false);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (isLocalPlayer)
        {
            DisablePlayerCamera();
            SetMouseLocked(false);
        }
    }
}