using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
///     This class renders saved mesh data and
///     handles all of rendered mesh data
/// </summary>
public class MeshMapRenderer : MonoBehaviour
{
    // Handle saved Data
    private static List<MeshData> savedMeshDataList;

    // Handle rendered Data
    private List<GameObject> renderedMeshObjects = new List<GameObject>();

    // Mesh visualize variables
    private GameObject meshPrefabT;
    private GameObject meshPrefabW;
    private bool isTransparentPrefab = true;
    private bool isMeshVisible = true;

    // Gizmos for visualization of collision bounds
    private static bool showColliderBounds = false;  // visualization flag
    private static Color GizmosColliderColor = new Color(0, 1, 0, 0.5f);

    // Set visualzation flag
    public static void SetVisualization(bool enable)
    {
        showColliderBounds = enable;
    }

    void Start()
    {
        // Load Prefab
        meshPrefabT = Resources.Load<GameObject>("Prefab/MeshPrefab_T");
        meshPrefabW = Resources.Load<GameObject>("Prefab/MeshPrefab_W");

        var arMeshManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARMeshManager>();
        if (arMeshManager != null)
        {
            arMeshManager.enabled = false;
        }

        if (savedMeshDataList != null && savedMeshDataList.Count > 0)
        {
            RenderSavedMeshData();
        }
        // CheckCollisionSettings();

        // Add MeshMapCollisionDetector to all of the rendered mesh objects
        foreach (var obj in renderedMeshObjects)
        {
            if (obj.GetComponent<MeshMapCollisionDetector>() == null)
            {
                obj.AddComponent<MeshMapCollisionDetector>();
            }

            // Set collisionDetection mode
            ArticulationBody artBody = obj.GetComponent<ArticulationBody>();
            if (artBody != null)
            {
                artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
    }

    // Public function that called in MeshFunction script, SaveMesh function
    public static void SetMeshData(List<MeshData> meshDataList)
    {
        savedMeshDataList = meshDataList;
    }

    // Render saved mesh data in savedMeshDataList
    public void RenderSavedMeshData()
    {
        if (savedMeshDataList == null || savedMeshDataList.Count == 0)
        {
            return;
        }

        // For all saved mesh data
        for (int i = 0; i < savedMeshDataList.Count; i++)
        {
            // Select Prefab to use
            GameObject prefabToUse = isTransparentPrefab ? meshPrefabT : meshPrefabW;
            if (prefabToUse == null)
            {
                return;
            }

            // Set mesh name, mesh filter, mesh collider for each mesh data
            GameObject meshObject = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity);
            meshObject.name = $"Rendered_Mesh_{i}";

            var meshData = savedMeshDataList[i];

            // MeshFilter
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = meshData.mesh;
            }

            // MeshCollider
            var meshCollider = meshObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = meshData.mesh;
                meshCollider.convex = meshData.colliderData.isConvex;
                meshCollider.isTrigger =true;
                meshCollider.enabled = true;
            }

            // Add and initialize CollisionDetector
            var collisionDetector = meshObject.AddComponent<MeshMapCollisionDetector>();
            collisionDetector.Initialize();

            renderedMeshObjects.Add(meshObject);
        }
    }

    // Change Mesh Prefab (Transparent or Not)
    public void RenderMeshTransparent()
    {
        // Transparent state toggle
        isTransparentPrefab = !isTransparentPrefab;
        // Set Prefab and Mesh Material as status of isTransparentPrefab
        GameObject selectedPrefab = isTransparentPrefab ? meshPrefabT : meshPrefabW;
        Material newMaterial = selectedPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        // Change MeshRenderer of rendered mesh map
        foreach (var obj in renderedMeshObjects)
        {
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = new Material(newMaterial);
            }
        }
    }

    // Change mesh visibility
    public void RenderMeshOnOff()
    {
        // visible state toggle
        isMeshVisible = !isMeshVisible;

        // visible state change for all rendered mesh object
        foreach (var obj in renderedMeshObjects)
        {
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isMeshVisible;
            }
        }
    }

    // Set destroyed mesh
    private void OnDestroy()
    {
        foreach (var obj in renderedMeshObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    // Draw mesh collider bounds and frames for debugging
    private void OnDrawGizmos()
    {
        if (!showColliderBounds || renderedMeshObjects == null) return;

        Gizmos.color = GizmosColliderColor;

        foreach (var meshObject in renderedMeshObjects)
        {
            if (meshObject != null)
            {
                MeshCollider collider = meshObject.GetComponent<MeshCollider>();
                if (collider != null)
                {
                    // Draw bounding boxes
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);

                    // Draw center of bounding boxes
                    Gizmos.DrawSphere(collider.bounds.center, 0.02f);

                    // Draw lines on the frames
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
        // Layer Collision Matrix 확인
        int meshMapLayer = LayerMask.NameToLayer("MeshLayer");
        int robotLayer = LayerMask.NameToLayer("RobotLink");
        bool canCollide = !Physics.GetIgnoreLayerCollision(meshMapLayer, robotLayer);

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



/// <summary>
///     This class is component of RenderedMeshObject.
///     This class detect collision between mesh map
///     and manipulator while visual updating
/// </summary>
public class MeshMapCollisionDetector : MonoBehaviour
{
    // Set Mesh Collider
    private MeshCollider meshCollider;

    // Check all of link collision state
    private Dictionary<string, bool> linkCollisionStates = new Dictionary<string, bool>();

    // Handle collision Marker
    private List<GameObject> collisionMarkers = new List<GameObject>();
    private Material collisionMarkerMaterial;

    // Set private static variable
    private static bool _isCollision = false;

    // Set public static variable for allowing access of EEMarker
    public static bool isCollision
    {
        get { return _isCollision; }
        private set { _isCollision = value; }
    }

    // Check collision for each robot link
    private void UpdateCollisionState()
    {
        bool anyCollision = linkCollisionStates.Values.Any(state => state);
        isCollision = anyCollision;  // static 프로퍼티 업데이트
    }

    public void Initialize()
    {
        meshCollider = GetComponent<MeshCollider>();
        collisionMarkerMaterial = Resources.Load<Material>("Material/CollisionMarker");

        linkCollisionStates.Clear();
        isCollision = false;
    }

    private void CreateCollisionMarker(Vector3 position)
    {
        if (collisionMarkerMaterial == null) return;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.05f; // Set sphere size

        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        renderer.material = collisionMarkerMaterial;

        Destroy(marker.GetComponent<Collider>()); // To use only visual
        Destroy(marker, 1f);

        collisionMarkers.Add(marker);
    }

    // Mesh collision occurs
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Contains("sgr532") && !other.gameObject.tag.Contains("base") && other.gameObject.tag != "sgr532/link1" && other.gameObject.tag != "sgr532/link2" && other.gameObject.tag != "sgr532/link3")
        {
            string linkName = GetLinkName(other.gameObject);
            if (!string.IsNullOrEmpty(linkName))
            {
                linkCollisionStates[linkName] = true;
                UpdateCollisionState();

                Vector3 collisionPoint = other.bounds.center;
                CreateCollisionMarker(collisionPoint);
                VisualUpdate(other, "K1/Materials/rgba-1-0-0-1");   // red robot link
            }
        }
    }

    // Mesh collision exit
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Contains("sgr532") && !other.gameObject.tag.Contains("base") && other.gameObject.tag != "sgr532/link1" && other.gameObject.tag != "sgr532/link2" && other.gameObject.tag != "sgr532/link3")
        {
            string linkName = GetLinkName(other.gameObject);
            if (!string.IsNullOrEmpty(linkName))
            {
                linkCollisionStates[linkName] = false;
                UpdateCollisionState();
                VisualUpdate(other, "K1/Materials/rgba-1-1-1-1");   // normal robot link
            }
        }
    }

    // Extract link name
    private string GetLinkName(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.name == "sagittarius_base_link" || current.name == "base_link" || current.name == "link1" || current.name == "link2" || current.name == "link3")
            {
                return string.Empty;
            }

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
        foreach (var marker in collisionMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        // Clear all markers and init collision state
        collisionMarkers.Clear();
        linkCollisionStates.Clear();
        isCollision = false;
    }

    // Visual update as collision state
    private void VisualUpdate(Collider collider, string materialPath)
    {
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
            // Find visual object following hierarchy
            Transform visualsTransform = collisionsParent.parent.Find("Visuals");
            if (visualsTransform != null)
            {
                Transform unnamedTransform = collisionsParent.Find("unnamed");
                if (unnamedTransform != null && unnamedTransform.childCount > 0)
                {
                    string linkName = unnamedTransform.GetChild(0).name;
                    if (linkName == "sagittarius_base_link" || linkName == "link1" || linkName == "link2" || linkName == "link3") {
                        // Find MeshRenderer at Visuals/unnamed/{link_name}/{link_name}_0
                        Transform linkVisual = visualsTransform
                            .Find("unnamed")
                            ?.Find(linkName)
                            ?.Find($"{linkName}_0");

                        if (linkVisual != null)
                        {
                            MeshRenderer meshRenderer = linkVisual.GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                            {
                                Material material = Resources.Load<Material>("K1/Materials/rgba-1-1-1-1");
                                if (material != null)
                                {
                                    meshRenderer.material = material;
                                }
                            }
                        }
                    }
                    else{
                        // Find MeshRenderer at Visuals/unnamed/{link_name}/{link_name}_0
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
