using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class PSXCameraEffect : MonoBehaviour
{
    [Header("PS2 Resolution")]
    [Range(180, 720)]
    public int verticalResolution = 360;

    [Header("Screen Material")]
    public Material screenMaterial;

    private Camera mainCam;
    private RenderTexture renderTexture;
    private RawImage screenImage;
    private GameObject canvasObject;

    void Start()
    {
        mainCam = GetComponent<Camera>();

        // Create screen canvas
        canvasObject = new GameObject(
        "PSX Screen Canvas",
        typeof(RectTransform),
        typeof(Canvas)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -100;

        // Create screen image
        GameObject imageObject = new GameObject(
        "PSX Screen",
        typeof(RectTransform),
        typeof(RawImage)
        );

        imageObject.transform.SetParent(
        canvasObject.transform, false
        );

        RectTransform rect =
        imageObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        screenImage = imageObject.GetComponent<RawImage>();
        screenImage.raycastTarget = false;
        screenImage.material = screenMaterial;

        UpdateResolution();
    }

    void LateUpdate()
    {
        int targetWidth = Mathf.Max(
        1,
        Mathf.RoundToInt(
        verticalResolution *
        (float)Screen.width /
        Mathf.Max(1, Screen.height)
        )
        );

        if (renderTexture == null ||
        renderTexture.width != targetWidth ||
        renderTexture.height != verticalResolution)
        {
            UpdateResolution();
        }
    }

    void UpdateResolution()
    {
        if (renderTexture != null)
        {
            mainCam.targetTexture = null;
            screenImage.texture = null;

            renderTexture.Release();
            Destroy(renderTexture);
        }

        int width = Mathf.Max(
        1,
        Mathf.RoundToInt(
        verticalResolution *
        (float)Screen.width /
        Mathf.Max(1, Screen.height)
        )
        );

        renderTexture = new RenderTexture(
        width,
        verticalResolution,
        24,
        RenderTextureFormat.ARGB32
        );

        renderTexture.filterMode = FilterMode.Point;
        renderTexture.antiAliasing = 1;
        renderTexture.useMipMap = false;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.Create();

        mainCam.targetTexture = renderTexture;
        screenImage.texture = renderTexture;
    }

    void OnDestroy()
    {
        if (mainCam != null &&
        mainCam.targetTexture == renderTexture)
        {
            mainCam.targetTexture = null;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (canvasObject != null)
        {
            Destroy(canvasObject);
        }
    }
}