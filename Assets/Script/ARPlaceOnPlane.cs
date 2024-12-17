using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ARPlaceOnPlane : MonoBehaviour
{
    public ARRaycastManager arRaycaster;
    public GameObject placeObject; // ¿òÁ÷ÀÏ Žë»ó ¿ÀºêÁ§Æ® (·Îº¿ º£ÀÌœº)
    private ImageTrackingHandler imageTrackingHandler; // Reference to ImageTrackingHandler
    private Pose savepose;

    [SerializeField]
    private float moveSpeed = 0.005f; // ÀÌµ¿ ŒÓµµ
    [SerializeField]
    private float rotateAngle = 0.5f; // ÈžÀü °¢µµ (µµ ŽÜÀ§)

    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isRotatingCW = false;
    private bool isRotatingCCW = false;

    void Start()
    {
        // Find ImageTrackingHandler and subscribe to OnPoseUpdated
        imageTrackingHandler = FindObjectOfType<ImageTrackingHandler>();

        if (imageTrackingHandler != null)
        {
            imageTrackingHandler.OnPoseUpdated += UpdatePoseFromImage;
        }
        else
        {
            Debug.LogWarning("ImageTrackingHandler not found. Position and rotation updates will not work.");
        }

        // placeObject가 있다면 현재 위치와 회전값으로 savepose 초기화
        if (placeObject != null)
        {
            savepose = new Pose(Vector3.zero, Quaternion.identity);
            Debug.Log($"Initial pose set: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
        else
        {
            // placeObject가 없는 경우 기본값으로 초기화
            savepose = new Pose(Vector3.zero, Quaternion.identity);
            Debug.LogWarning("placeObject not found. Initialized savepose with default values.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (imageTrackingHandler != null)
        {
            imageTrackingHandler.OnPoseUpdated -= UpdatePoseFromImage;
        }
        // TransformData.SaveTransform(placeObject.transform);
    }

    void Update()
    {
        if (placeObject != null)
        {
            // ÁöŒÓ ÀÌµ¿/ÈžÀü Ã³ž®
            if (isMovingUp)
                savepose.position += Vector3.forward * moveSpeed;

            if (isMovingDown)
                savepose.position += Vector3.back * moveSpeed;

            if (isMovingLeft)
                savepose.position += Vector3.left * moveSpeed;

            if (isMovingRight)
                savepose.position += Vector3.right * moveSpeed;

            if (isRotatingCW)
                savepose.rotation *= Quaternion.Euler(0, rotateAngle, 0); // 시계방향 회전

            if (isRotatingCCW)
                savepose.rotation *= Quaternion.Euler(0, -rotateAngle, 0); // 반시계방향 회전

            // Apply the updated pose
            ApplyPose();
            SaveObjectData();
        }
    }

    public void OnMoveUpPress() => isMovingUp = true;
    public void OnMoveUpRelease() => isMovingUp = false;

    public void OnMoveDownPress() => isMovingDown = true;
    public void OnMoveDownRelease() => isMovingDown = false;

    public void OnMoveLeftPress() => isMovingLeft = true;
    public void OnMoveLeftRelease() => isMovingLeft = false;

    public void OnMoveRightPress() => isMovingRight = true;
    public void OnMoveRightRelease() => isMovingRight = false;

    public void OnRotateCWPress() => isRotatingCW = true;
    public void OnRotateCWRelease() => isRotatingCW = false;

    public void OnRotateCCWPress() => isRotatingCCW = true;
    public void OnRotateCCWRelease() => isRotatingCCW = false;

    private void ApplyPose()
    {
        if (placeObject != null)
        {
            ArticulationBody BaseLinkArticulationBody = placeObject.GetComponent<ArticulationBody>();
            BaseLinkArticulationBody.TeleportRoot(savepose.position, savepose.rotation);

            Debug.Log($"Pose Applied: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
    }

    private void UpdatePoseFromImage(Vector3 position, Quaternion rotation)
    {
        savepose.position = position; // ImageTrackingHandler¿¡Œ­ ¹ÞÀº À§Ä¡ ÀúÀå
        savepose.rotation = rotation; // ImageTrackingHandler¿¡Œ­ ¹ÞÀº ÈžÀü ÀúÀå

        Debug.Log($"Updated Pose: Position={position}, Rotation={rotation}");
    }

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
