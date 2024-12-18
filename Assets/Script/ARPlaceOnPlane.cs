using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
///     This script locate the robot base position,
///     orientation from trakced image and provide
///     controller for moving robot base by arrow UI.
/// </summary>
public class ARPlaceOnPlane : MonoBehaviour
{
    public ARRaycastManager arRaycaster;
    public GameObject placeObject;
    private ImageTrackingHandler imageTrackingHandler;
    private Pose savepose;

    [SerializeField]
    private float moveSpeed = 0.005f;
    [SerializeField]
    private float rotateAngle = 0.5f;

    private Vector3 planeNormal = Vector3.up;

    // translation and orientation flag
    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isRotatingCW = false;
    private bool isRotatingCCW = false;
    private bool isMovingNormalUp = false;
    private bool isMovingNormalDown = false;

    void Start()
    {
        // Find imageTrackingHandler
        imageTrackingHandler = FindObjectOfType<ImageTrackingHandler>();

        if (imageTrackingHandler != null)
        {
            imageTrackingHandler.OnPoseUpdated += UpdatePoseFromImage;
        }
        else
        {
            Debug.LogWarning("ImageTrackingHandler not found. Position and rotation updates will not work.");
        }

        // Init pose
        if (placeObject != null)
        {
            savepose = new Pose(new Vector3(1000, 0, 1000), Quaternion.identity);
            Debug.Log($"Initial pose set: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
        else
        {
            savepose = new Pose(new Vector3(1000, 0, 1000), Quaternion.identity);
            Debug.LogWarning("placeObject not found. Initialized savepose with far-away default values.");
        }
    }

    void OnDestroy()
    {
        if (imageTrackingHandler != null)
        {
            imageTrackingHandler.OnPoseUpdated -= UpdatePoseFromImage;
        }
    }

void Update()
{
    if (placeObject != null)
    {
        // Calculate local orientation
        Vector3 forward = savepose.rotation * Vector3.forward;
        Vector3 right = savepose.rotation * Vector3.right;

        // Move at local coordinate
        if (isMovingUp)
            savepose.position += forward * moveSpeed;

        if (isMovingDown)
            savepose.position -= forward * moveSpeed;

        if (isMovingLeft)
            savepose.position -= right * moveSpeed;

        if (isMovingRight)
            savepose.position += right * moveSpeed;

        // orientation
        if (isRotatingCW)
            savepose.rotation *= Quaternion.Euler(0, rotateAngle, 0);

        if (isRotatingCCW)
            savepose.rotation *= Quaternion.Euler(0, -rotateAngle, 0);

        // normal vector
        if (isMovingNormalUp)
            savepose.position += planeNormal * moveSpeed;

        if (isMovingNormalDown)
            savepose.position -= planeNormal * moveSpeed;

        // Apply pose and save pose
        ApplyPose();
        SaveObjectData();
    }
}

    // input event (translation)
    public void OnMoveUpPress() => isMovingUp = true;
    public void OnMoveUpRelease() => isMovingUp = false;

    public void OnMoveDownPress() => isMovingDown = true;
    public void OnMoveDownRelease() => isMovingDown = false;

    public void OnMoveLeftPress() => isMovingLeft = true;
    public void OnMoveLeftRelease() => isMovingLeft = false;

    public void OnMoveRightPress() => isMovingRight = true;
    public void OnMoveRightRelease() => isMovingRight = false;

    // input event (rotation)
    public void OnRotateCWPress() => isRotatingCW = true;
    public void OnRotateCWRelease() => isRotatingCW = false;

    public void OnRotateCCWPress() => isRotatingCCW = true;
    public void OnRotateCCWRelease() => isRotatingCCW = false;

    // input event (normal)
    public void OnMoveNormalUpPress() => isMovingNormalUp = true;
    public void OnMoveNormalUpRelease() => isMovingNormalUp = false;

    public void OnMoveNormalDownPress() => isMovingNormalDown = true;
    public void OnMoveNormalDownRelease() => isMovingNormalDown = false;

    // Apply pose
    private void ApplyPose()
    {
        if (placeObject != null)
        {
            ArticulationBody BaseLinkArticulationBody = placeObject.GetComponent<ArticulationBody>();
            BaseLinkArticulationBody.TeleportRoot(savepose.position, savepose.rotation);

            Debug.Log($"Pose Applied: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
    }

    // Pose update from imageTrackingHandler
    private void UpdatePoseFromImage(Vector3 position, Quaternion rotation)
    {
        savepose.position = position;
        savepose.rotation = rotation;

        Debug.Log($"Updated Pose: Position={position}, Rotation={rotation}");
    }

    // set normal vector for plane
    public void SetPlaneNormal(Vector3 normal)
    {
        planeNormal = normal.normalized;
        Debug.Log($"Plane normal set to: {planeNormal}");
    }

    // save object data
    public void SaveObjectData()
    {
        if (placeObject != null)
        {
            ObjectDataManager.SetObjectData(savepose.position, savepose.rotation);
            Debug.Log("Object 위치와 회전값이 저장되었습니다.");
        }
        else
        {
            Debug.LogWarning("저장할 Object가 없습니다.");
        }
    }
}
