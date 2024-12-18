using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

/// <summary>
///     This script makes mesh visual and collider,
///     handles all of the meshes.
/// </summary>

// For save mesh info
public struct MeshData
{
    public Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uvs;
    public ColliderData colliderData;
}

// For save mesh collider info
public struct ColliderData
{
    public Vector3 center;
    public Vector3 size;
    public bool isConvex;
    public bool isTrigger;
    public CollisionDetectionMode collisionDetectionMode;
}

// Handle Mesh Map Data
public class MeshFunction : MonoBehaviour
{
    // Make mesh and handle mesh
    private ARMeshManager arMeshManager;
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    // Mesh visualize variables
    private GameObject meshPrefabT;
    private GameObject meshPrefabW;
    private MeshFilter currentMeshFilter;
    private bool isTransparentPrefab = true;
    private bool isMeshVisible = true;

    // Mesh info save
    private List<MeshData> savedMeshDataList = new List<MeshData>();
    public List<MeshData> SavedMeshDataList => savedMeshDataList;
    // private bool isMeshSaved = false;

    // // Variables that used in MeshMapRenderer
    // private List<Vector3[]> savedVertices = new List<Vector3[]>();
    // public List<Vector3[]> SavedVertices => savedVertices;
    // private List<int[]> savedTriangles = new List<int[]>();
    // public List<int[]> SavedTriangles => savedTriangles;
    // private List<Vector3[]> savedNormals = new List<Vector3[]>();
    // public List<Vector3[]> SavedNormals => savedNormals;
    // private List<Vector2[]> savedUVs = new List<Vector2[]>();
    // public List<Vector2[]> SavedUVs => savedUVs;
    // // public bool IsMeshSaved => isMeshSaved;

    private void Start()
    {
        // Get ARMeshManager component
        arMeshManager = GetComponent<ARMeshManager>();

        // Load Prefab
        meshPrefabT = Resources.Load<GameObject>("Prefab/MeshPrefab_T");
        meshPrefabW = Resources.Load<GameObject>("Prefab/MeshPrefab_W");

        if (meshPrefabT == null || meshPrefabW == null)
        {
            return;
        }

        // Get MeshRenderer component in Mesh Prefab
        MeshRenderer rendererT = meshPrefabT.GetComponent<MeshRenderer>();
        if (rendererT != null && rendererT.sharedMaterial != null)
        {
            // Set colorT from shader in Prefab
            if (rendererT.sharedMaterial.shader.name != "Custom/DistanceColorShader")
            {
                Color colorT = rendererT.sharedMaterial.color;
            }
        }

        // Get MeshRenderer component in Mesh Prefab
        MeshRenderer rendererW = meshPrefabW.GetComponent<MeshRenderer>();
        if (rendererW != null && rendererW.sharedMaterial != null)
        {
            // Set colorW from Prefab
            Color colorW = rendererW.sharedMaterial.color;
        }

        // Set initial meshPrefab as meshPrefabT
        currentMeshFilter = meshPrefabT.GetComponent<MeshFilter>();
        if (currentMeshFilter == null)
        {
            return;
        }

        // Create mesh using MeshFilter that set as meshPrefabT
        arMeshManager.meshPrefab = currentMeshFilter;

        // Execute when mesh change
        if (arMeshManager != null)
        {
            arMeshManager.meshesChanged += OnMeshesChanged;
        }
    }

    // Set changed mesh compare to before (added mesh, removed mesh)
    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        // Set added mesh
        foreach (var meshFilter in args.added)
        {
            // Set mesh filter for added mesh (mesh renderer, articulatino body, mesh colldier)
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer != null && !meshRenderers.Contains(meshRenderer))
            {
                // Mesh renderer
                meshRenderers.Add(meshRenderer);
                meshRenderer.enabled = isMeshVisible;

                // Articulation body
                var artBody = meshFilter.gameObject.AddComponent<ArticulationBody>();
                artBody.immovable = true;
                artBody.useGravity = false;
                artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                artBody.jointType = ArticulationJointType.FixedJoint;

                // Mesh collider
                var meshCollider = meshFilter.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    meshCollider.convex = true;
                    meshCollider.isTrigger = false;
                }
            }
        }

        // Set removed mesh
        foreach (var meshFilter in args.removed)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderers.Remove(meshRenderer);
            }
        }
    }

    // Set destroyed mesh
    private void OnDestroy()
    {
        if (arMeshManager != null)
        {
            arMeshManager.meshesChanged -= OnMeshesChanged;
        }
    }

    // private void UpdateMeshColor(Vector3 meshPosition, MeshRenderer meshRenderer)
    // {
    //     if (isTransparentPrefab) // MeshPrefab_T가 선택된 경우
    //     {
    //         // 메시의 bounds를 사용하여 중심점 계산
    //         Vector3 meshCenter = meshRenderer.bounds.center;

    //         // 카메라로부터의 거리 계산
    //         float distance = Vector3.Distance(Camera.main.transform.position, meshCenter);
    //         Debug.Log($"Mesh ID: {meshRenderer.GetInstanceID()}, 카메라로부터의 거리: {distance}m");

    //         // 거리 값을 0~10 범위로 제한
    //         distance = Mathf.Clamp(distance, 0f, 10f);
    //         Debug.Log($"Mesh ID: {meshRenderer.GetInstanceID()}, 제한된 거리: {distance}m");

    //         Color color;
    //         if (distance <= 5f)
    //         {
    //             // 0~5 범위: 빨간색(255,0,0)에서 노란색(255,255,0)으로
    //             float t = distance / 5f;
    //             color = new Color(1f, t, 0f, 0.3f);
    //         }
    //         else
    //         {
    //             // 5~10 범위: 노란색(255,255,0)에서 초록색(0,255,0)으로
    //             float t = (distance - 5f) / 5f;
    //             color = new Color(1f - t, 1f, 0f, 0.3f);
    //         }

    //         meshRenderer.sharedMaterial.color = color;
    //     }
    //     else // MeshPrefab_W��� 선택된 경우
    //     {
    //         // 색상을 고정
    //         meshRenderer.sharedMaterial.color = new Color(1f, 1f, 1f, 0.784f); // (255, 255, 255, 200)
    //     }
    // }


    // Change Mesh Prefab (Transparent or Not)
    public void ChangeMesh()
    {
        if (arMeshManager != null)
        {
            // Transparent state toggle
            isTransparentPrefab = !isTransparentPrefab;
            // Set Prefab and Mesh Renderer as status of isTransparentPrefab
            GameObject selectedPrefab = isTransparentPrefab ? meshPrefabT : meshPrefabW;
            MeshRenderer selectedPrefabRenderer = selectedPrefab.GetComponent<MeshRenderer>();

            if (selectedPrefabRenderer == null)
            {
                return;
            }

            // Get Material in selected Prefab 
            Material newMaterial = new Material(selectedPrefabRenderer.sharedMaterial);

            // Change current mesh object material
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null)
                {
                    // // meshPrefabT가 선택된 경우 색상 업데이트
                    // if (isTransparentPrefab)
                    // {
                    //     UpdateMeshColor(meshRenderer.transform.position, meshRenderer);
                    // }
                    meshRenderer.sharedMaterial = newMaterial;

                }
            }

            // Set Mesh Filter for creating new mesh properly
            currentMeshFilter = selectedPrefab.GetComponent<MeshFilter>();
            if (currentMeshFilter == null)
            {
                return;
            }

            // Apply Mesh Filter to Mesh Manager
            arMeshManager.meshPrefab = currentMeshFilter;
        }
    }

    // Mesh regenerate
    public void Remeshing()
    {
        if (arMeshManager != null)
        {
            // Destroy current meshes and set mesh manager
            arMeshManager.DestroyAllMeshes();
            meshRenderers.Clear();
            arMeshManager.enabled = false;

            // Restart mesh manager to create new mesh
            arMeshManager.enabled = true;
        }
    }

    // Change mesh visibility
    public void ToggleMeshVisibility()
    {
        // visible state toggle
        isMeshVisible = !isMeshVisible;

        // visible state change for all mesh object
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isMeshVisible;
            }
        }
    }

    // Save current mesh data
    public void SaveMesh()
    {
        // Use ARMeshManager to handle mesh data generally
        var arMeshManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARMeshManager>();
        if (arMeshManager != null)
        {
            // Initialize saved mesh data
            savedMeshDataList.Clear();
            // For all of the current mesh data
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null)
                {
                    // Class for save current mesh data
                    MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
                    ArticulationBody artBody = meshRenderer.GetComponent<ArticulationBody>();

                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        Mesh originalMesh = meshFilter.sharedMesh;

                        // Copy mesh data
                        Mesh meshCopy = Instantiate(originalMesh);

                        // Create and Set MeshData struct
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
                        // Save set mesh data
                        savedMeshDataList.Add(meshData);
                    }
                }
            }

            arMeshManager.enabled = false;
            // isMeshSaved = true;

            // Data transfer to MeshMapRenderer
            MeshMapRenderer.SetMeshData(savedMeshDataList);
        }
    }
}
