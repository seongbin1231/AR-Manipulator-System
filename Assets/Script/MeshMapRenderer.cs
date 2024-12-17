using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshMapRenderer : MonoBehaviour
{
    private static List<MeshData> savedMeshDataList;
    private List<GameObject> renderedMeshObjects = new List<GameObject>();

    // 추가된 변수들
    private GameObject meshPrefabT;
    private GameObject meshPrefabW;
    private bool isFirstPrefab = true;
    private bool isMeshVisible = true;

    // Gizmos 색상 설정을 위한 변수 추가
    public Color boundsColor = new Color(0, 1, 0, 0.5f); // 반투명 녹색
    private static bool showColliderBounds = false;  // 시각화 켜기/끄기
    public Color visualColliderColor = new Color(0, 1, 0, 0.5f);  // 반투명 녹색
    public Color originColliderColor = new Color(1, 0, 0, 0.5f);  // 반투명 빨간색
    // public bool showColliderWireframe = true;
    public float centerSphereRadius = 0.02f;
    private static Color meshColliderColor = new Color(0, 1, 0, 0.5f);  // 반투명 녹색

    // 시각화 제어를 위한 public 메서드
    public static void SetVisualization(bool enable)
    {
        showColliderBounds = enable;
    }

    void Start()
    {
        // 프팹 로드 추가
        meshPrefabT = Resources.Load<GameObject>("Prefab/MeshPrefab_T");
        meshPrefabW = Resources.Load<GameObject>("Prefab/MeshPrefab_W");

        if (meshPrefabT == null || meshPrefabW == null)
        {
            Debug.LogError("Prefab을 찾을 수 없습니다. 경로를 확인해주세요.");
        }

        var arMeshManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARMeshManager>();
        if (arMeshManager != null)
        {
            arMeshManager.enabled = false;
        }

        if (savedMeshDataList != null && savedMeshDataList.Count > 0)
        {
            RenderSavedMeshData();
        }
        else
        {
            Debug.LogWarning("저장된 Mesh 데이터가 없습니다.");
        }
        CheckCollisionSettings();

        // MeshMapCollisionDetector 컴포넌트 추가 확인
        foreach (var obj in renderedMeshObjects)
        {
            if (obj.GetComponent<MeshMapCollisionDetector>() == null)
            {
                obj.AddComponent<MeshMapCollisionDetector>();
            }

            // CollisionDetection 모드 통일
            ArticulationBody artBody = obj.GetComponent<ArticulationBody>();
            if (artBody != null)
            {
                artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
    }

    public static void SetMeshData(List<MeshData> meshDataList)
    {
        savedMeshDataList = meshDataList;
    }

    public void RenderSavedMeshData()
    {
        if (savedMeshDataList == null || savedMeshDataList.Count == 0)
        {
            Debug.LogWarning("저장된 Mesh 데이터가 없습니다.");
            return;
        }

        for (int i = 0; i < savedMeshDataList.Count; i++)
        {
            GameObject prefabToUse = isFirstPrefab ? meshPrefabT : meshPrefabW;
            if (prefabToUse == null)
            {
                Debug.LogError("Mesh Prefab이 없습니다.");
                return;
            }

            GameObject meshObject = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity);
            meshObject.name = $"Rendered_Mesh_{i}";

            var meshData = savedMeshDataList[i];

            // MeshFilter에 메시 할당
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = meshData.mesh;
            }

            // MeshCollider 설정
            var meshCollider = meshObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = meshData.mesh;
                meshCollider.convex = meshData.colliderData.isConvex;
                // meshCollider.isTrigger = meshData.colliderData.isTrigger;
                meshCollider.isTrigger =true;
                meshCollider.enabled = true;
            }
            else
            {
                Debug.LogError("MeshCollider 컴포넌트가 없습니다.");
            }

            // CollisionDetector 추가 및 초기화
            var collisionDetector = meshObject.AddComponent<MeshMapCollisionDetector>();
            collisionDetector.Initialize();

            // 디버그를 위한 로그 추가
            // Debug.Log($"Mesh {i} 충돌 감지기 설정 완료: {meshObject.name}");

            renderedMeshObjects.Add(meshObject);
            // Debug.Log($"Mesh {i} 렌더링 완료: vertices({meshData.vertices.Length}), triangles({meshData.triangles.Length/3})");

        }

        // Debug.Log($"메시 렌더링 완료: {renderedMeshObjects.Count}개의 메시 생성됨");
    }

    // 프리팹만 전환하는 함수로 수정
    public void RenderMeshTransparent()
    {
        // 프리팹 전환
        isFirstPrefab = !isFirstPrefab;
        GameObject selectedPrefab = isFirstPrefab ? meshPrefabT : meshPrefabW;
        Material newMaterial = selectedPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        foreach (var obj in renderedMeshObjects)
        {
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // 새로운 머티리얼 적용
                meshRenderer.material = new Material(newMaterial);
            }
        }

        Debug.Log($"메시 프리팹이 {(isFirstPrefab ? "MeshPrefab_T" : "MeshPrefab_W")}로 변경되었습니다.");
    }

    public void RenderMeshOnOff()
    {
        isMeshVisible = !isMeshVisible;

        foreach (var obj in renderedMeshObjects)
        {
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isMeshVisible;
            }
        }

        Debug.Log($"메시 가시성이 {(isMeshVisible ? "켜짐" : "꺼짐")}으로 변경되었습니다.");
    }

    private void OnDestroy()
    {
        // 생성된 모든 메시 오브젝트 정리
        foreach (var obj in renderedMeshObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    // MeshCollider 상태를 확인하는 새로운 메서드
    private void CheckAllMeshColliders()
    {
        foreach (var obj in renderedMeshObjects)
        {
            MeshCollider collider = obj.GetComponent<MeshCollider>();
            if (collider != null)
            {
                if (collider.sharedMesh != null)
                {
                    Debug.Log($"{obj.name}: MeshCollider 정상 (Convex: {collider.convex})");
                }
                else
                {
                    Debug.LogWarning($"{obj.name}: MeshCollider에 mesh가 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"{obj.name}: MeshCollider 컴포넌트가 없습니다.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showColliderBounds || renderedMeshObjects == null) return;

        Gizmos.color = meshColliderColor;

        foreach (var meshObject in renderedMeshObjects)
        {
            if (meshObject != null)
            {
                MeshCollider collider = meshObject.GetComponent<MeshCollider>();
                if (collider != null)
                {
                    // 바운딩 박스 그리기
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);

                    // 중심점 표시
                    Gizmos.DrawSphere(collider.bounds.center, 0.02f);

                    // 메시 와이어프레임 그리기
                    if (collider.sharedMesh != null)
                    {
                        Vector3[] vertices = collider.sharedMesh.vertices;
                        int[] triangles = collider.sharedMesh.triangles;

                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                            Vector3 v1 = meshObject.transform.TransformPoint(vertices[triangles[i]]);
                            Vector3 v2 = meshObject.transform.TransformPoint(vertices[triangles[i + 1]]);
                            Vector3 v3 = meshObject.transform.TransformPoint(vertices[triangles[i + 2]]);

                            Gizmos.DrawLine(v1, v2);
                            Gizmos.DrawLine(v2, v3);
                            Gizmos.DrawLine(v3, v1);
                        }
                    }
                }
            }
        }
    }

    // CheckCollisionSettings 메서드를 호출하여 확인
    public void CheckCollisionSettings()
    {
        // // Physics 설정 확인
        // Debug.Log($"\n=== Physics Settings ===");
        // Debug.Log($"Auto Simulation: {Physics.autoSimulation}");
        // Debug.Log($"Bounce Threshold: {Physics.bounceThreshold}");
        // Debug.Log($"Default Contact Offset: {Physics.defaultContactOffset}");
        // Debug.Log($"Default Solver Iterations: {Physics.defaultSolverIterations}");

        // Layer Collision Matrix 확인
        int meshMapLayer = LayerMask.NameToLayer("MeshLayer");
        int robotLayer = LayerMask.NameToLayer("RobotLink");
        bool canCollide = !Physics.GetIgnoreLayerCollision(meshMapLayer, robotLayer);
        // Debug.Log($"\n=== Layer Collision ===");
        // Debug.Log($"MeshMap Layer: {meshMapLayer}");
        // Debug.Log($"RobotLink Layer: {robotLayer}");
        // Debug.Log($"Can Collide: {canCollide}");

        // Collider 타입 및 설정 확인
        Debug.Log($"\n=== Collider Settings ===");
        foreach (var obj in renderedMeshObjects)
        {
            Debug.Log($"\nObject: {obj.name}");

            // Collider 타입 확인
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                string colliderType = "Unknown";
                bool isConvex = false;

                if (collider is MeshCollider meshCollider)
                {
                    colliderType = "Mesh Collider";
                    isConvex = meshCollider.convex;
                    Debug.Log($"- Collider Type: {colliderType}\n" +
                             $"  - Is Convex: {isConvex}\n" +
                             $"  - Is Trigger: {meshCollider.isTrigger}\n" +
                             $"  - Has Mesh: {meshCollider.sharedMesh != null}");
                }
                else if (collider is BoxCollider)
                {
                    colliderType = "Box Collider (Primitive)";
                }
                else if (collider is SphereCollider)
                {
                    colliderType = "Sphere Collider (Primitive)";
                }
                else if (collider is CapsuleCollider)
                {
                    colliderType = "Capsule Collider (Primitive)";
                }

                // Rigidbody/ArticulationBody 확인
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                ArticulationBody artBody = obj.GetComponent<ArticulationBody>();

                string bodyType = "Static Collider";
                if (rb != null)
                {
                    bodyType = rb.isKinematic ? "Kinematic Rigidbody" : "Dynamic Rigidbody";
                }
                else if (artBody != null)
                {
                    bodyType = "ArticulationBody";
                }

                if (colliderType != "Mesh Collider")
                {
                    Debug.Log($"- Collider Type: {colliderType}\n" +
                             $"  - Is Trigger: {collider.isTrigger}");
                }

                Debug.Log($"  - Body Type: {bodyType}");

                // ArticulationBody 상세 정보
                if (artBody != null)
                {
                    Debug.Log($"  - ArticulationBody Details:\n" +
                             $"    - Joint Type: {artBody.jointType}\n" +
                             $"    - Immovable: {artBody.immovable}\n" +
                             $"    - Use Gravity: {artBody.useGravity}\n" +
                             $"    - Collision Detection: {artBody.collisionDetectionMode}");
                }
                // Rigidbody 상세 정보
                else if (rb != null)
                {
                    Debug.Log($"  - Rigidbody Details:\n" +
                             $"    - Mass: {rb.mass}\n" +
                             $"    - Use Gravity: {rb.useGravity}\n" +
                             $"    - Is Kinematic: {rb.isKinematic}\n" +
                             $"    - Collision Detection: {rb.collisionDetectionMode}");
                }
            }
        }
    }
}



// CollisionDetector 클래스를 MeshMap 내부에 추가
public class MeshMapCollisionDetector : MonoBehaviour
{
    private List<GameObject> collisionMarkers = new List<GameObject>();
    private Material collisionMarkerMaterial;
    private MeshCollider meshCollider;
    public static bool isCollision = false;

    // 각 링크별 충돌 상태를 추적하기 위한 Dictionary 추가
    private static Dictionary<string, bool> linkCollisionStates = new Dictionary<string, bool>();

    public void Initialize()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogWarning($"[{gameObject.name}] MeshCollider를 찾을 수 없습니다.");
        }

        collisionMarkerMaterial = Resources.Load<Material>("Material/CollisionMarker");
        if (collisionMarkerMaterial == null)
        {
            Debug.LogError("CollisionMarker Material을 찾을 수 없습니다.");
        }

        // 초기화 시 충돌 상태 초기화
        linkCollisionStates.Clear();
        isCollision = false;
    }

    // 전체 충돌 상태 업데이트
    private void UpdateCollisionState()
    {
        // 하나라도 true가 있으면 전체 isCollision을 true로 설정
        isCollision = linkCollisionStates.Values.Any(state => state);
        Debug.Log($"충돌 상태 업데이트: {string.Join(", ", linkCollisionStates.Select(kvp => $"{kvp.Key}={kvp.Value}"))} => 전체 상태: {isCollision}");
    }

    private void CreateCollisionMarker(Vector3 position)
    {
        if (collisionMarkerMaterial == null) return;

        // 구체 프리미티브 생성
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.05f; // 5cm 크기

        // 머티리얼 적용
        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        renderer.material = collisionMarkerMaterial;

        // 콜라이더 제거 (시각적 표시만 필요)
        Destroy(marker.GetComponent<Collider>());

        // 1초 후 마커 제거
        Destroy(marker, 1f);

        // 리스트에 추가
        collisionMarkers.Add(marker);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("RobotLink") ||
            other.gameObject.tag.Contains("sgr532"))
        {
            string linkName = GetLinkName(other.gameObject);
            if (!string.IsNullOrEmpty(linkName))
            {
                // 해당 링크의 충돌 상태를 true로 설정
                linkCollisionStates[linkName] = true;
                UpdateCollisionState();

                Vector3 collisionPoint = other.bounds.center;
                CreateCollisionMarker(collisionPoint);

                // 충돌한 Collider에 대해 빨간색 Material로 Visual 업데이트
                VisualUpdate(other, "K1/Materials/rgba-1-0-0-1");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("RobotLink") ||
            other.gameObject.tag.Contains("sgr532"))
        {
            string linkName = GetLinkName(other.gameObject);
            if (!string.IsNullOrEmpty(linkName))
            {
                // 해당 링크의 충돌 상태를 false로 설정
                linkCollisionStates[linkName] = false;
                UpdateCollisionState();

                // 충돌이 해제된 Collider에 대해 흰색 Material로 Visual 업데이트
                VisualUpdate(other, "K1/Materials/rgba-1-1-1-1");
            }
        }
    }

    // 링크 이름 추출 헬퍼 메서드
    private string GetLinkName(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            // base_link 특수 처리
            if (current.name == "sagittarius_base_link")
            {
                // return "base_link";
                return string.Empty;
            }
            // 일반적인 링크 이름 처리
            if (current.name.EndsWith("_0"))
            {
                return current.parent.name;
            }
            current = current.parent;
        }
        return string.Empty;
    }

    private void OnDestroy()
    {
        // 생성된 모든 마커 정리
        foreach (var marker in collisionMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        collisionMarkers.Clear();

        // 해당 객체가 파괴될 때 충돌 상태도 초기화
        linkCollisionStates.Clear();
        isCollision = false;
    }

    private void VisualUpdate(Collider collider, string materialPath)
    {
        // Collisions 부모 객체 찾기
        Transform current = collider.transform;
        Transform collisionsParent = null;

        while (current != null)
        {
            if (current.name == "Collisions")
            {
                collisionsParent = current;
                break;
            }
            current = current.parent;
        }

        if (collisionsParent != null)
        {
            // Collisions와 같은 계층의 Visuals 찾기
            Transform visualsTransform = collisionsParent.parent.Find("Visuals");
            if (visualsTransform != null)
            {
                // link_name 찾기 (Collisions/unnamed/{link_name} 구조에서)
                Transform unnamedTransform = collisionsParent.Find("unnamed");
                if (unnamedTransform != null && unnamedTransform.childCount > 0)
                {
                    string linkName = unnamedTransform.GetChild(0).name;
                    if (linkName == "sagittarius_base_link") {
                        // Visuals/unnamed/{link_name}/{link_name}_0 경로의 MeshRenderer 찾기
                        Transform linkVisual = visualsTransform
                            .Find("unnamed")
                            ?.Find(linkName)
                            ?.Find($"{linkName}_0");

                        if (linkVisual != null)
                        {
                            MeshRenderer meshRenderer = linkVisual.GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                            {
                                // Material 로드 및 적용
                                Material material = Resources.Load<Material>("K1/Materials/rgba-1-1-1-1");
                                if (material != null)
                                {
                                    meshRenderer.material = material;
                                }
                            }
                        }
                    }
                    else{

                        // Visuals/unnamed/{link_name}/{link_name}_0 경로의 MeshRenderer 찾기
                        Transform linkVisual = visualsTransform
                            .Find("unnamed")
                            ?.Find(linkName)
                            ?.Find($"{linkName}_0");

                        if (linkVisual != null)
                        {
                            MeshRenderer meshRenderer = linkVisual.GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                            {
                                // Material 로드 및 적용
                                Material material = Resources.Load<Material>(materialPath);
                                if (material != null)
                                {
                                    meshRenderer.material = material;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
