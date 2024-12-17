using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaceOnPlane : MonoBehaviour
{
    public ARRaycastManager arRaycaster;
    public GameObject placeObject; // 배치할 오브젝트
    private ImageTrackingHandler imageTrackingHandler; // ImageTrackingHandler 참조
    private Pose savepose;

    [SerializeField]
    private float moveSpeed = 0.005f; // 이동 속도
    [SerializeField]
    private float rotateAngle = 0.5f; // 회전 각도

    private Vector3 planeNormal = Vector3.up; // 평면의 법선 벡터 (기본: y축)

    // 이동 및 회전 플래그
    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isRotatingCW = false;
    private bool isRotatingCCW = false;
    private bool isMovingNormalUp = false; // 법선 방향 위쪽 이동
    private bool isMovingNormalDown = false; // 법선 방향 아래쪽 이동

    void Start()
    {
        // ImageTrackingHandler 찾기 및 이벤트 구독
        imageTrackingHandler = FindObjectOfType<ImageTrackingHandler>();

        if (imageTrackingHandler != null)
        {
            imageTrackingHandler.OnPoseUpdated += UpdatePoseFromImage;
        }
        else
        {
            Debug.LogWarning("ImageTrackingHandler not found. Position and rotation updates will not work.");
        }

        // 초기 Pose 설정 - 원점 대신 멀리 떨어진 위치로 설정
        if (placeObject != null)
        {
            savepose = new Pose(new Vector3(1000, 0, 1000), Quaternion.identity); // 초기 위치를 멀리 설정
            Debug.Log($"Initial pose set: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
        else
        {
            savepose = new Pose(new Vector3(1000, 0, 1000), Quaternion.identity); // placeObject가 없는 경우도 동일 설정
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
        // 로컬 방향 계산 (로봇이 바라보는 방향)
        Vector3 forward = savepose.rotation * Vector3.forward; // 로봇의 전진 방향
        Vector3 right = savepose.rotation * Vector3.right;     // 로봇의 오른쪽 방향

        // 로컬 좌표계 기준 이동
        if (isMovingUp)
            savepose.position += forward * moveSpeed;

        if (isMovingDown)
            savepose.position -= forward * moveSpeed;

        if (isMovingLeft)
            savepose.position -= right * moveSpeed;

        if (isMovingRight)
            savepose.position += right * moveSpeed;

        // 회전 처리
        if (isRotatingCW)
            savepose.rotation *= Quaternion.Euler(0, rotateAngle, 0);

        if (isRotatingCCW)
            savepose.rotation *= Quaternion.Euler(0, -rotateAngle, 0);

        // 법선 방향 이동
        if (isMovingNormalUp)
            savepose.position += planeNormal * moveSpeed;

        if (isMovingNormalDown)
            savepose.position -= planeNormal * moveSpeed;

        // Pose 적용 및 데이터 저장
        ApplyPose();
        SaveObjectData();
    }
}

    // 입력 이벤트 (이동)
    public void OnMoveUpPress() => isMovingUp = true;
    public void OnMoveUpRelease() => isMovingUp = false;

    public void OnMoveDownPress() => isMovingDown = true;
    public void OnMoveDownRelease() => isMovingDown = false;

    public void OnMoveLeftPress() => isMovingLeft = true;
    public void OnMoveLeftRelease() => isMovingLeft = false;

    public void OnMoveRightPress() => isMovingRight = true;
    public void OnMoveRightRelease() => isMovingRight = false;

    // 입력 이벤트 (회전)
    public void OnRotateCWPress() => isRotatingCW = true;
    public void OnRotateCWRelease() => isRotatingCW = false;

    public void OnRotateCCWPress() => isRotatingCCW = true;
    public void OnRotateCCWRelease() => isRotatingCCW = false;

    // 입력 이벤트 (법선 방향 이동)
    public void OnMoveNormalUpPress() => isMovingNormalUp = true;
    public void OnMoveNormalUpRelease() => isMovingNormalUp = false;

    public void OnMoveNormalDownPress() => isMovingNormalDown = true;
    public void OnMoveNormalDownRelease() => isMovingNormalDown = false;

    // Pose 적용
    private void ApplyPose()
    {
        if (placeObject != null)
        {
            ArticulationBody BaseLinkArticulationBody = placeObject.GetComponent<ArticulationBody>();
            BaseLinkArticulationBody.TeleportRoot(savepose.position, savepose.rotation);

            Debug.Log($"Pose Applied: Position={savepose.position}, Rotation={savepose.rotation.eulerAngles}");
        }
    }

    // ImageTrackingHandler로부터 Pose 업데이트
    private void UpdatePoseFromImage(Vector3 position, Quaternion rotation)
    {
        savepose.position = position;
        savepose.rotation = rotation;

        Debug.Log($"Updated Pose: Position={position}, Rotation={rotation}");
    }

    // 평면 법선 벡터 설정 (선택 사항)
    public void SetPlaneNormal(Vector3 normal)
    {
        planeNormal = normal.normalized;
        Debug.Log($"Plane normal set to: {planeNormal}");
    }

    // 오브젝트 데이터 저장
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
