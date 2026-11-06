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
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0)
            );

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                DrawerController drawer =
                    hit.collider.GetComponentInParent<DrawerController>();

                if (drawer != null)
                {
                    drawer.ToggleDrawer();
                }
            }
        }
    }
}