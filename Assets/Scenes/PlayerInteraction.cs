using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public Camera playerCamera;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
            );

            if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance
            ))
            {
                DoorController door =
                hit.collider.GetComponentInParent<DoorController>();

                if (door != null)
                    door.Interact();
            }
        }
    }
}