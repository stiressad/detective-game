using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public float slideDistance = 0.35f;
    public float slideSpeed = 3f;

    // Направление выдвижения в мировых координатах
    public Vector3 moveDirection = Vector3.right;

    private Vector3 closedPosition;
    private bool isOpen;
    private Coroutine moveCoroutine;

    void Start()
    {
        closedPosition = transform.position;
    }

    public void ToggleDrawer()
    {
        isOpen = !isOpen;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveDrawer());
    }

    IEnumerator MoveDrawer()
    {
        Vector3 target = isOpen
            ? closedPosition + moveDirection.normalized * slideDistance
            : closedPosition;

        while (Vector3.Distance(transform.position, target) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                slideSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }
}
