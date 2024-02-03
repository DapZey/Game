using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerPOVManager : MonoBehaviour
{
    public float userMouseSensitivity = 0.55f;
    public Transform camFollowPos;
    private float PlayerMouseX;
    private float PlayerMouseY;
    private float currentRotationX = 0f; // Track current vertical rotation

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        PlayerMouseX = Input.GetAxisRaw("Mouse X");
        PlayerMouseY = Input.GetAxisRaw("Mouse Y");
        UpdateCameraRotation();
    }

    void UpdateCameraRotation()
    {
        float mouseXDelta = PlayerMouseX * userMouseSensitivity;
        float mouseYDelta = PlayerMouseY * userMouseSensitivity;

        // Calculate the new vertical rotation
        currentRotationX -= mouseYDelta;
        currentRotationX = Mathf.Clamp(currentRotationX, -90f, 90f); // Clamp between -90 and 90 degrees

        // Rotate the camera follow position around the local X axis (up and down)
        camFollowPos.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);

        // Rotate the player around the global Y axis (left and right)
        transform.Rotate(Vector3.up, mouseXDelta);
    }
}
