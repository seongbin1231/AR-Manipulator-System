using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.JointControl;

/// <summary>
///     This script for making marker at 
///     end effector. During collision, color
///     of marker is red if not, color is blue
/// </summary>
public class EEMarker : MonoBehaviour
{
    // Setting marker utils
    private List<GameObject> collisionMarkers = new List<GameObject>();
    private Material collisionMarkerMaterial;
    private Material normalMarkerMaterial;
    private Matrix4x4 currentMatrix;

    // Current scene, Receving msg flag
    private bool isTrajectoryControlScene = false;
    private bool isJointControlScene = false;
    private bool isOperating = false;

    // Ros setting
    private ROSConnection ros;
    private const string topicName = "joint_control";
    private Joint_listMsg savedJointMsg;
    
    // Class made in MeshMapRenderer
    private MeshMapCollisionDetector[] meshCollisionDetectors;
    
    void Start()
    {
        // Load collision material
        collisionMarkerMaterial = Resources.Load<Material>("Material/EE_Collision_T");
        normalMarkerMaterial = Resources.Load<Material>("Material/EE_Normal_T");
        if (collisionMarkerMaterial == null || normalMarkerMaterial == null)
        {
            Debug.LogError("필요한 Material을 찾을 수 없습니다.");
        }

        // Check current scene
        isTrajectoryControlScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TrajectoryControl";
        isJointControlScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "JointControl";

        // Waiting until all JointControl initialize
        StartCoroutine(WaitForJointControlsInitialization());

        // Find all MeshMapCollisionDetector
        meshCollisionDetectors = FindObjectsOfType<MeshMapCollisionDetector>();
    }

    // Waiting until all JointControl initialize
    private IEnumerator WaitForJointControlsInitialization()
    {
        yield return new WaitForSeconds(1f);
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Joint_listMsg>(topicName, ReceiveJointControl);
        Debug.Log("Subscribed to " + topicName);
    }

    // flag setting and destroy marker when receive joint control msg
    void ReceiveJointControl(Joint_listMsg msg)
    {
        isOperating = true;
        foreach (var marker in collisionMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
    }

    void Update()
    {
        if (isOperating || isTrajectoryControlScene || isJointControlScene || JointDataManager.IsJointMessageSaved())
        {
            // Get EE position
            currentMatrix = CalculateWorldMatrix();
            Vector3 position = currentMatrix.GetColumn(3);

            // Create marker at EE position during collision
            CreateEEMarker(position, MeshMapCollisionDetector.isCollision);
        }
    }

    private Matrix4x4 CalculateWorldMatrix()
    {
        // Get current object transform
        Transform targetTransform = this.transform;

        // Get world position and rotation for current object
        // Set marker position 0.05m forward of EE position
        Vector3 worldPosition = targetTransform.position + targetTransform.forward * 0.05f;
        Quaternion worldRotation = targetTransform.rotation;
        Vector3 worldScale = targetTransform.lossyScale;

        Matrix4x4 worldMatrix = Matrix4x4.TRS(worldPosition, worldRotation, worldScale);

        return worldMatrix;
    }

    // Create marker at end effector position
    private void CreateEEMarker(Vector3 position, bool isCollision)
    {
        // Check marker material
        if (collisionMarkerMaterial == null || normalMarkerMaterial == null) return;

        // Create marker object
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;               // position of marker
        marker.transform.localScale = Vector3.one * 0.005f; // size of marker

        // Set material as collision state
        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        renderer.material = isCollision ? collisionMarkerMaterial : normalMarkerMaterial;

        // Destroy collider of marker (to avoid collision object with collision marker)
        Destroy(marker.GetComponent<Collider>());

        // Destroy marker after 20s
        Destroy(marker, 20f);

        // To handle marker
        collisionMarkers.Add(marker);
    }

    private void OnDestroy()
    {
        // Destroy all marker that created before
        foreach (var marker in collisionMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        collisionMarkers.Clear();
    }

    // To check collision state
    private bool CheckCurrentCollision()
    {
        if (meshCollisionDetectors == null) return false;

        foreach (var detector in meshCollisionDetectors)
        {
            if (detector != null)
            {
                // Use MeshMapCollisionDetector as static variable
                return MeshMapCollisionDetector.isCollision;
            }
        }
        return false;
    }
}
