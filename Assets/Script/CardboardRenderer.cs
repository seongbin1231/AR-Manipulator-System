using UnityEngine;
using UnityEngine.UI;

public class CardboardRenderer : MonoBehaviour
{
    public RawImage rawImageLeft;
    public RawImage rawImageRight;
    public RawImage rawImageFull;
    public Canvas buttonCanvas;
    public Canvas cubeCanvas;
    public Canvas gazeCanvas;
    public Canvas playCanvas;
    private RenderTexture renderTexture;
    // private bool isInitialized = false;
    private bool isSplitView = false;
    private Camera mainCamera;
    private Canvas parentCanvas;
    private static bool tag_3d_view = false;


    void Start()
    {
        Debug.Log("CardboardRenderer 시작...");

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("부모 Canvas를 찾을 수 없습니다!");
            return;
        }

        renderTexture = new RenderTexture(1440, 3200, 24);
        Camera.main.targetTexture = renderTexture;
        Debug.Log("RenderTexture 설정 완료");

        rawImageLeft.texture = renderTexture;
        rawImageRight.texture = renderTexture;
        rawImageFull.texture = renderTexture;
        Debug.Log("RawImage 설정 완료");

        float width = 0.8f;
        rawImageLeft.uvRect = new Rect(0, 0, width, 1);
        rawImageRight.uvRect = new Rect(1-width, 0, width, 1);
        rawImageFull.uvRect = new Rect(0, 0, 1, 1);
        Debug.Log("UVRect 설정 완료");

        SetupRectTransforms();
        // SetSplitView();

        if(tag_3d_view)
        {
            Debug.Log("3D view mode enabled");
            isSplitView = true;
            SetSplitView(); // 즉시 split view 적용
        }
        else
        {
            Debug.Log("3D view mode disabled");
            isSplitView = false;
            SetSplitView(); // 즉시 split view 적용
        }
    }

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

        Debug.Log("RectTransform 설정 완료");
    }

    public void SetSplitView()
    {
        if (isSplitView)
        {
            rawImageLeft.gameObject.SetActive(true);
            rawImageRight.gameObject.SetActive(true);
            rawImageFull.gameObject.SetActive(false);

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

            rawImageLeft.transform.SetAsLastSibling();
            rawImageRight.transform.SetAsLastSibling();
        }
        else
        {
            rawImageLeft.gameObject.SetActive(false);
            rawImageRight.gameObject.SetActive(false);
            rawImageFull.gameObject.SetActive(true);

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

            rawImageFull.transform.SetAsLastSibling();
        }

        Debug.Log($"View mode changed to: {(isSplitView ? "Split" : "Full")}, Parent Canvas mode: {parentCanvas?.renderMode}");
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
