using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

/// <summary>
///     This script track image and set robot
///     pose updating step. If it can track image,
///     image returns position and orientation
/// </summary>
public struct MeshData
{
    public Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uvs;
    public ColliderData colliderData;
}

public struct ColliderData
{
    public Vector3 center;
    public Vector3 size;
    public bool isConvex;
    public bool isTrigger;
    public CollisionDetectionMode collisionDetectionMode;
}

public class MeshFunction : MonoBehaviour
{
    private ARMeshManager arMeshManager;
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private bool isFirstPrefab = true;
    private bool isMeshVisible = true;  // Mesh 가시성 상태를 추적하는 변수 추가

    // 두 개의 Prefab을 참조할 변수
    private GameObject meshPrefabT;
    private GameObject meshPrefabW;
    private MeshFilter currentMeshFilter;

    // Mesh 정보를 저장할 변수들
    private List<MeshData> savedMeshDataList = new List<MeshData>();
    public List<MeshData> SavedMeshDataList => savedMeshDataList;
    private bool isMeshSaved = false;

    // 다른 스크립트에서 접근할 수 있는 프로퍼티들
    private List<Vector3[]> savedVertices = new List<Vector3[]>();
    public List<Vector3[]> SavedVertices => savedVertices;
    private List<int[]> savedTriangles = new List<int[]>();
    public List<int[]> SavedTriangles => savedTriangles;
    private List<Vector3[]> savedNormals = new List<Vector3[]>();
    public List<Vector3[]> SavedNormals => savedNormals;
    private List<Vector2[]> savedUVs = new List<Vector2[]>();
    public List<Vector2[]> SavedUVs => savedUVs;
    public bool IsMeshSaved => isMeshSaved;

    private void Start()
    {
        // 같은 게임오브젝트에서 ARMeshManager 컴포넌트 가져오기
        arMeshManager = GetComponent<ARMeshManager>();

        // Prefab 로드
        meshPrefabT = Resources.Load<GameObject>("Prefab/MeshPrefab_T");
        meshPrefabW = Resources.Load<GameObject>("Prefab/MeshPrefab_W");

        if (meshPrefabT == null || meshPrefabW == null)
        {
            Debug.LogError("Prefab을 찾을 수 없습니다. 경로를 확인해주세요.");
            return;
        }

        // MeshPrefab_T의 Material 색상 정보 출력
        MeshRenderer rendererT = meshPrefabT.GetComponent<MeshRenderer>();
        if (rendererT != null && rendererT.sharedMaterial != null)
        {
            // Custom 셰이더를 사�하는 경우 색상 정보 출력 건너뛰기
            if (rendererT.sharedMaterial.shader.name != "Custom/DistanceColorShader")
            {
                Color colorT = rendererT.sharedMaterial.color;
                // Debug.Log($"MeshPrefab_T 색상 - R: {colorT.r}, G: {colorT.g}, B: {colorT.b}, A: {colorT.a}");
            }
        }

        // MeshPrefab_W의 Material 색상 정보 출력
        MeshRenderer rendererW = meshPrefabW.GetComponent<MeshRenderer>();
        if (rendererW != null && rendererW.sharedMaterial != null)
        {
            Color colorW = rendererW.sharedMaterial.color;
            // Debug.Log($"MeshPrefab_W 색상 - R: {colorW.r}, G: {colorW.g}, B: {colorW.b}, A: {colorW.a}");
        }

        // 초기 MeshFilter 설정
        currentMeshFilter = meshPrefabT.GetComponent<MeshFilter>();
        if (currentMeshFilter == null)
        {
            Debug.LogError("MeshFilter 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        arMeshManager.meshPrefab = currentMeshFilter;

        // meshesChanged 이벤트 구독
        if (arMeshManager != null)
        {
            arMeshManager.meshesChanged += OnMeshesChanged;
        }
    }

    // OnMeshesChanged 메서드 수정 - 새로 생성되는 메시의 가시성도 현재 상태에 맞게 설정
    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        // 새로 추가된 메시 처리
        foreach (var meshFilter in args.added)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer != null && !meshRenderers.Contains(meshRenderer))
            {
                meshRenderers.Add(meshRenderer);
                meshRenderer.enabled = isMeshVisible;

                // ArticulationBody 추가 및 설정
                var artBody = meshFilter.gameObject.AddComponent<ArticulationBody>();
                artBody.immovable = true;  // 고정된 환경으로 설정
                artBody.useGravity = false;
                artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                artBody.jointType = ArticulationJointType.FixedJoint;

                // MeshCollider 설정 확인
                var meshCollider = meshFilter.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    meshCollider.convex = true;
                    meshCollider.isTrigger = false;
                }
            }
        }

        // 제거된 메시 처리
        foreach (var meshFilter in args.removed)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderers.Remove(meshRenderer);
            }
        }
    }


    private void OnDestroy()
    {
        // ARMeshManager에서 이벤트 구독 해제
        if (arMeshManager != null)
        {
            arMeshManager.meshesChanged -= OnMeshesChanged;
        }
    }

    private void UpdateMeshColor(Vector3 meshPosition, MeshRenderer meshRenderer)
    {
        if (isFirstPrefab) // MeshPrefab_T가 선택된 경우
        {
            // 메시의 bounds를 사용하여 중심점 계산
            Vector3 meshCenter = meshRenderer.bounds.center;

            // 카메라로부터의 거리 계산
            float distance = Vector3.Distance(Camera.main.transform.position, meshCenter);
            Debug.Log($"Mesh ID: {meshRenderer.GetInstanceID()}, 카메라로부터의 거리: {distance}m");

            // 거리 값을 0~10 범위로 제한
            distance = Mathf.Clamp(distance, 0f, 10f);
            Debug.Log($"Mesh ID: {meshRenderer.GetInstanceID()}, 제한된 거리: {distance}m");

            Color color;
            if (distance <= 5f)
            {
                // 0~5 범위: 빨간색(255,0,0)에서 노란색(255,255,0)으로
                float t = distance / 5f;
                color = new Color(1f, t, 0f, 0.3f);
            }
            else
            {
                // 5~10 범위: 노란색(255,255,0)에서 초록색(0,255,0)으로
                float t = (distance - 5f) / 5f;
                color = new Color(1f - t, 1f, 0f, 0.3f);
            }

            meshRenderer.sharedMaterial.color = color;
        }
        else // MeshPrefab_W��� 선택된 경우
        {
            // 색상을 고정
            meshRenderer.sharedMaterial.color = new Color(1f, 1f, 1f, 0.784f); // (255, 255, 255, 200)
        }
    }

    public void ChangeMesh()
    {
        if (arMeshManager != null)
        {
            // Prefab 전환 및 MeshFilter, MeshRenderer 가져오기
            isFirstPrefab = !isFirstPrefab;
            GameObject selectedPrefab = isFirstPrefab ? meshPrefabT : meshPrefabW;
            MeshRenderer selectedPrefabRenderer = selectedPrefab.GetComponent<MeshRenderer>();

            if (selectedPrefabRenderer == null)
            {
                Debug.LogError("선택된 Prefab에서 MeshRenderer를 찾을 수 없습니다.");
                return;
            }

            // 새로운 Prefab의 material 가져오기
            Material newMaterial = new Material(selectedPrefabRenderer.sharedMaterial);

            // 기존의 모든 mesh의 material 변경
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null)
                {
                    // meshPrefabT가 선택된 경우 색상 업데이트
                    if (isFirstPrefab)
                    {
                        UpdateMeshColor(meshRenderer.transform.position, meshRenderer);
                    }
                    meshRenderer.sharedMaterial = newMaterial;

                }
            }

            // 새로운 mesh 생성을 위한 MeshFilter 설정
            currentMeshFilter = selectedPrefab.GetComponent<MeshFilter>();
            if (currentMeshFilter == null)
            {
                Debug.LogError("선택된 Prefab에서 MeshFilter를 찾을 수 없습니다.");
                return;
            }

            // 새로운 MeshFilter 설정 (이후 생성될 mesh에 적용)
            arMeshManager.meshPrefab = currentMeshFilter;

            Debug.Log($"Changed to {(isFirstPrefab ? "MeshPrefab_T" : "MeshPrefab_W")}");
        }
        else
        {
            Debug.LogWarning("ARMeshManager가 없습니다.");
        }
    }

    public void Remeshing()
    {
        if (arMeshManager != null)
        {
            // 기존의 모든 Mesh 제거
            arMeshManager.DestroyAllMeshes();
            meshRenderers.Clear();
            Debug.Log("모든 기존 Mesh 데이터가 제거되었습니다.");

            // 새로운 Mesh 생성을 위해 AR Mesh Manager를 재시작
            arMeshManager.enabled = false;
            arMeshManager.enabled = true;

            Debug.Log("새로운 MeshMap 생성을 시작합니다.");
        }
        else
        {
            Debug.LogWarning("ARMeshManager가 없어서 Remeshing을 실행할 수 없습니다.");
        }
    }

    public void ToggleMeshVisibility()
    {
        isMeshVisible = !isMeshVisible;  // 상태 토글

        // 모든 MeshRenderer의 가시성 변경
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isMeshVisible;
            }
        }

        // // ARMeshManager의 Mesh 생성 기능도 토글
        // if (arMeshManager != null)
        // {
        //     arMeshManager.enabled = isMeshVisible;
        // }

        // Debug.Log($"Mesh visibility turned {(isMeshVisible ? "ON" : "OFF")}");
    }

    public void SaveMesh()
    {
        var arMeshManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARMeshManager>();
        if (arMeshManager != null)
        {
            savedMeshDataList.Clear(); // 기존 데이터 초기화

            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null)
                {
                    MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
                    ArticulationBody artBody = meshRenderer.GetComponent<ArticulationBody>();

                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        Mesh originalMesh = meshFilter.sharedMesh;

                        // 메시 복제
                        Mesh meshCopy = Instantiate(originalMesh);

                        // MeshData 구조체 생성 및 설정
                        MeshData meshData = new MeshData
                        {
                            mesh = meshCopy,
                            vertices = originalMesh.vertices,
                            triangles = originalMesh.triangles,
                            normals = originalMesh.normals,
                            uvs = originalMesh.uv,
                            colliderData = new ColliderData
                            {
                                center = meshCollider != null ? meshCollider.bounds.center : Vector3.zero,
                                size = meshCollider != null ? meshCollider.bounds.size : Vector3.one,
                                isConvex = meshCollider != null ? meshCollider.convex : true,
                                isTrigger = meshCollider != null ? meshCollider.isTrigger : false,
                                collisionDetectionMode = artBody != null ?
                                    artBody.collisionDetectionMode :
                                    CollisionDetectionMode.Discrete
                            }
                        };

                        savedMeshDataList.Add(meshData);
                    }
                }
            }

            arMeshManager.enabled = false;
            isMeshSaved = true;

            // 수정된 데이터 전달 메서드 호출
            MeshMapRenderer.SetMeshData(savedMeshDataList);
            Debug.Log($"Mesh 데이터 저장 완료: {savedMeshDataList.Count}개의 메시 저장됨");
        }
        else
        {
            Debug.LogWarning("ARMeshManager가 없어서 Mesh를 저장할 수 없습니다.");
        }
    }

    // // Mesh 생성 재개를 위한 함수 추가
    // public void ResumeMeshGeneration()
    // {
    //     if (arMeshManager != null)
    //     {
    //         arMeshManager.enabled = true;
    //         isMeshSaved = false;
    //         Debug.Log("Mesh 생성이 재개되었습니다.");
    //     }
    // }
}
