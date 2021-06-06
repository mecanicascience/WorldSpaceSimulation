using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMouseLook : MonoBehaviour {
    public float mouseSensivity = 500f;
    public Transform playerBody;

	private float currentXRotation = 0f;


	private void Start() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update() {
		float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

        currentXRotation -= mouseY;
        currentXRotation = Mathf.Clamp(currentXRotation, -90f, 90f);

		transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
		playerBody.Rotate(Vector3.up * mouseX);
	}
}
