using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This script changes view mode between
///     single view and 3D view.
/// </summary>
public class CardboardRenderer : MonoBehaviour
{
    // Render image
    public RawImage rawImageLeft;       // Image for 3D view , left half
    public RawImage rawImageRight;      // Image for 3D view , right half
    public RawImage rawImageFull;

    // Should be controlled canvas
    public Canvas buttonCanvas;
    public Canvas cubeCanvas;
    public Canvas gazeCanvas;
    public Canvas playCanvas;
    private Canvas parentCanvas;

    // Set view texture
    private RenderTexture renderTexture;
    private Camera mainCamera;

    // 3D view flag
    private bool isSplitView = false;
    private static bool tag_3d_view = false;


    void Start()
    {

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            return;
        }

        renderTexture = new RenderTexture(1440, 3200, 24);
        Camera.main.targetTexture = renderTexture;

        // set view texture
        rawImageLeft.texture = renderTexture;
        rawImageRight.texture = renderTexture;
        rawImageFull.texture = renderTexture;

        // set viewing angle
        float width = 0.8f;
        rawImageLeft.uvRect = new Rect(0, 0, width, 1);
        rawImageRight.uvRect = new Rect(1-width, 0, width, 1);
        rawImageFull.uvRect = new Rect(0, 0, 1, 1);

        SetupRectTransforms();

        // Set slitview by checking 3D view flag
        if(tag_3d_view)
        {
            isSplitView = true;
            SetSplitView();
        }
        else
        {
            isSplitView = false;
            SetSplitView();
        }
    }

    // Setting image rect
    private void SetupRectTransforms()
    {
        RectTransform leftRect = rawImageLeft.rectTransform;
        leftRect.anchorMin = new Vector2(0, 0);
        leftRect.anchorMax = new Vector2(0.5f, 1);
        leftRect.pivot = new Vector2(0.5f, 0.5f);
        leftRect.anchoredPosition = Vector2.zero;
        leftRect.sizeDelta = Vector2.zero;
        leftRect.localScale = new Vector3(0.8f, 0.8f, 1f);

        RectTransform rightRect = rawImageRight.rectTransform;
        rightRect.anchorMin = new Vector2(0.5f, 0);
        rightRect.anchorMax = new Vector2(1, 1);
        rightRect.pivot = new Vector2(0.5f, 0.5f);
        rightRect.anchoredPosition = Vector2.zero;
        rightRect.sizeDelta = Vector2.zero;
        rightRect.localScale = new Vector3(0.8f, 0.8f, 1f);

        RectTransform fullRect = rawImageFull.rectTransform;
        fullRect.anchorMin = new Vector2(0, 0);
        fullRect.anchorMax = new Vector2(1, 1);
        fullRect.pivot = new Vector2(0.5f, 0.5f);
        fullRect.anchoredPosition = Vector2.zero;
        fullRect.sizeDelta = Vector2.zero;
        fullRect.localScale = Vector3.one;
    }

    // Split view toggle (Single View <--> 3D View)
    public void SetSplitView()
    {
        // 3D View
        if (isSplitView)
        {
            // Left, right image activate
            rawImageLeft.gameObject.SetActive(true);
            rawImageRight.gameObject.SetActive(true);
            rawImageFull.gameObject.SetActive(false);

            // Canvas controll
            if (cubeCanvas != null)
            {
                cubeCanvas.gameObject.SetActive(true);
            }
            if (gazeCanvas != null)
            {
                gazeCanvas.gameObject.SetActive(true);
            }
            if (playCanvas != null)
            {
                playCanvas.gameObject.SetActive(true);
            }
            if (buttonCanvas != null)
            {
                buttonCanvas.gameObject.SetActive(false);
            }

            if (parentCanvas != null)
            {
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.worldCamera = Camera.main;
                parentCanvas.sortingOrder = 0;
            }

            // recover last setting
            rawImageLeft.transform.SetAsLastSibling();
            rawImageRight.transform.SetAsLastSibling();
        }
        // Single View
        else
        {
            // Full image activate
            rawImageLeft.gameObject.SetActive(false);
            rawImageRight.gameObject.SetActive(false);
            rawImageFull.gameObject.SetActive(true);

            // Canvas controll
            if (cubeCanvas != null)
            {
                cubeCanvas.gameObject.SetActive(false);
            }
            if (gazeCanvas != null)
            {
                gazeCanvas.gameObject.SetActive(false);
            }
            if (playCanvas != null)
            {
                playCanvas.gameObject.SetActive(false);
            }
            if (parentCanvas != null)
            {
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                parentCanvas.sortingOrder = 0;
            }

            if (buttonCanvas != null)
            {
                buttonCanvas.gameObject.SetActive(true);
                buttonCanvas.sortingOrder = 1;
                buttonCanvas.transform.SetAsLastSibling();
            }

            // recover last setting
            rawImageFull.transform.SetAsLastSibling();
        }

        Debug.Log($"View mode changed to: {(isSplitView ? "Split" : "Full")}, Parent Canvas mode: {parentCanvas?.renderMode}");
        // flag change
        isSplitView = !isSplitView;
    }

    public bool IsSplitView
    {
        get { return isSplitView; }
    }

    public static void Set3dTag(bool tag_3d)
    {
        tag_3d_view = tag_3d;
        Debug.Log($"3D view tag set to: {tag_3d_view}");
    }
}
