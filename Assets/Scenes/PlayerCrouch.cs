using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraRoot;

    public float crouchHeight = 1.1f;
    public float crouchCameraY = 1.0f;
    public float crouchSpeed = 8f;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;

    private bool isCrouching;

    void Start()
    {
        standingHeight = controller.height;
        standingCenter = controller.center;
        standingCameraPosition = cameraRoot.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        float targetHeight = isCrouching
            ? crouchHeight : standingHeight;

        float targetCenterY = isCrouching
            ? standingCenter.y - (standingHeight - crouchHeight) / 2f
            : standingCenter.y;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * crouchSpeed
        );

        Vector3 center = controller.center;
        center.y = Mathf.Lerp(
            center.y,
            targetCenterY,
            Time.deltaTime * crouchSpeed
        );
        controller.center = center;

        Vector3 cameraPosition = cameraRoot.localPosition;
        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            isCrouching
                ? crouchCameraY
                : standingCameraPosition.y,
            Time.deltaTime * crouchSpeed
        );

        cameraRoot.localPosition = cameraPosition;
    }
}