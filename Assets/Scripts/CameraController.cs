using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;

    float mouseX, mouseY;

    float xRotation, yRotation;


    public float mouseSensitivity = 100;
    GameManager gameManager;

    [SerializeField] PhotonView photonView;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gameManager = GameManager.sharedInstance;
    }
    private void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        if (gameManager.CurrentGameState == GameState.inGame)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerTransform.Rotate(Vector3.up * mouseX);
        }
    }
}
