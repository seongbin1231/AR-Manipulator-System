using UnityEngine;
using System.Collections;

public class BoundsVisualizer : MonoBehaviour
{
    public Color meshColliderBoundsColor = new Color(1, 0, 0, 0.5f);
    public bool showWireframe = true;
    public float boundsAlpha = 0.2f;

    private bool isRefreshing = false;
    private float refreshDelay = 0.1f;
    private LineRenderer[] boundingBoxLines;
    private GameObject boundsContainer;

    void Start()
    {
        boundsContainer = new GameObject("BoundsContainer");
        boundsContainer.transform.parent = transform;
        CreateBoundingBoxes();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isRefreshing)
        {
            StartCoroutine(RefreshCollidersCoroutine());
        }
    }

    void CreateBoundingBoxes()
    {
        // 기존 라인 제거
        if (boundingBoxLines != null)
        {
            foreach (var line in boundingBoxLines)
            {
                Destroy(line.gameObject);
            }
        }

        MeshCollider[] colliders = FindObjectsOfType<MeshCollider>();
        boundingBoxLines = new LineRenderer[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled)
            {
                GameObject lineObj = new GameObject($"BoundingBox_{i}");
                lineObj.transform.parent = boundsContainer.transform;

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                boundingBoxLines[i] = line;

                // 라인 렌더러 설정
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = line.endColor = meshColliderBoundsColor;
                line.startWidth = line.endWidth = 0.02f;
                line.positionCount = 24; // 박스를 그리기 위한 점 개수

                DrawBoundingBox(line, colliders[i].bounds);
            }
        }
    }

    void DrawBoundingBox(LineRenderer line, Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        // 박스의 모서리 점들
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(max.x, min.y, max.z);
        corners[3] = new Vector3(min.x, min.y, max.z);
        corners[4] = new Vector3(min.x, max.y, min.z);
        corners[5] = new Vector3(max.x, max.y, min.z);
        corners[6] = new Vector3(max.x, max.y, max.z);
        corners[7] = new Vector3(min.x, max.y, max.z);

        // 라인 그리기
        int index = 0;
        // 아래 사각형
        line.SetPosition(index++, corners[0]); line.SetPosition(index++, corners[1]);
        line.SetPosition(index++, corners[1]); line.SetPosition(index++, corners[2]);
        line.SetPosition(index++, corners[2]); line.SetPosition(index++, corners[3]);
        line.SetPosition(index++, corners[3]); line.SetPosition(index++, corners[0]);

        // 위 사각형
        line.SetPosition(index++, corners[4]); line.SetPosition(index++, corners[5]);
        line.SetPosition(index++, corners[5]); line.SetPosition(index++, corners[6]);
        line.SetPosition(index++, corners[6]); line.SetPosition(index++, corners[7]);
        line.SetPosition(index++, corners[7]); line.SetPosition(index++, corners[4]);

        // 수직선
        line.SetPosition(index++, corners[0]); line.SetPosition(index++, corners[4]);
        line.SetPosition(index++, corners[1]); line.SetPosition(index++, corners[5]);
        line.SetPosition(index++, corners[2]); line.SetPosition(index++, corners[6]);
        line.SetPosition(index++, corners[3]); line.SetPosition(index++, corners[7]);
    }

    IEnumerator RefreshCollidersCoroutine()
    {
        isRefreshing = true;
        yield return new WaitForSeconds(refreshDelay);
        CreateBoundingBoxes();
        isRefreshing = false;
    }
}
