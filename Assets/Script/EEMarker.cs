using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.JointControl;
public class EEMarker : MonoBehaviour
{
    private List<GameObject> collisionMarkers = new List<GameObject>();
    private Material collisionMarkerMaterial;
    private Material normalMarkerMaterial;
    private Matrix4x4 currentMatrix;
    private bool isTrajectoryControlScene = false;
    private bool isJointControlScene = false;
    private bool isOperating = false;
    private ROSConnection ros;
    private const string topicName = "joint_control";
    private Joint_listMsg savedJointMsg;
    private MeshMapCollisionDetector[] meshCollisionDetectors;
    // Start is called before the first frame update
    void Start()
    {
        // Collision Material 로드
        collisionMarkerMaterial = Resources.Load<Material>("Material/EE_Collision_T");
        // collisionMarkerMaterial.mainTextureScale = new Vector2(0.1f, 0.1f); // Material 크기 조절
        // 일반 상태 Material 로드 (�����록색 등 다른 색상)
        normalMarkerMaterial = Resources.Load<Material>("Material/EE_Normal_T");
        // normalMarkerMaterial.mainTextureScale = new Vector2(0.1f, 0.1f); // Material 크기 조절

        if (collisionMarkerMaterial == null || normalMarkerMaterial == null)
        {
            Debug.LogError("필요한 Material을 찾을 수 없습니다.");
        }
        // 현재 씬이 TrajectoryControl인지 확인
        isTrajectoryControlScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TrajectoryControl";
        Debug.Log("isTrajectoryControlScene: " + isTrajectoryControlScene);

        // 현재 씬이 TrajectoryControl인지 확인
        isJointControlScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "JointControl";
        Debug.Log("isJointControlScene: " + isJointControlScene);

        // 모든 JointControl이 초기화될 때까지 기다림
        StartCoroutine(WaitForJointControlsInitialization());

        // 씬의 모든 MeshMapCollisionDetector 찾기
        meshCollisionDetectors = FindObjectsOfType<MeshMapCollisionDetector>();
    }

    private IEnumerator WaitForJointControlsInitialization()
    {
        // 모든 JointControl의 Start 메서드가 실행될 때까지 기다림
        yield return new WaitForSeconds(1f);
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Joint_listMsg>(topicName, ReceiveJointControl);
        Debug.Log("Subscribed to " + topicName);
    }

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
    // Update is called once per frame
    void Update()
    {
        if (isOperating || isTrajectoryControlScene || isJointControlScene || JointDataManager.IsJointMessageSaved())
        {
            currentMatrix = CalculateWorldMatrix();
            Vector3 position = currentMatrix.GetColumn(3);
            CreateCollisionMarker(position, MeshMapCollisionDetector.isCollision);
        }
    }

    private Matrix4x4 CalculateWorldMatrix()
    {
        // 현재 객체의 Transform 설정
        Transform targetTransform = this.transform;

        // 현재 링크의 월드 Transform 정보로 행렬 계산
        Vector3 worldPosition = targetTransform.position + targetTransform.forward * 0.05f; // world 기준으로 orientation을 고려하여 1만큼 앞의 위치 계산
        Quaternion worldRotation = targetTransform.rotation;
        Vector3 worldScale = targetTransform.lossyScale;

        Matrix4x4 worldMatrix = Matrix4x4.TRS(worldPosition, worldRotation, worldScale);

        // Debug.Log($"[CalculateWorldMatrix] 현재 링크 '{targetTransform.name}'의 World Matrix:\n" +
        //          $"- World Position: {worldPosition}\n" +
        //          $"- World Rotation: {worldRotation.eulerAngles}\n" +
        //          $"- World Scale: {worldScale}");

        return worldMatrix;
    }

    private void CreateCollisionMarker(Vector3 position, bool isCollision)
    {
        if (collisionMarkerMaterial == null || normalMarkerMaterial == null) return;

        // 구체 프리미티브 생성
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.005f; // 5cm 크기

        // 머티리얼 적용 - 충돌 상태에 따라 다른 머티리얼 사용
        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        renderer.material = isCollision ? collisionMarkerMaterial : normalMarkerMaterial;

        // 콜라이더 제거 (시각적 표시만 필요)
        Destroy(marker.GetComponent<Collider>());

        // 1초 후 마커 제거
        Destroy(marker, 20f);

        // 리스트에 추가
        collisionMarkers.Add(marker);
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
    }

    // 현재 활성화된 충돌 상태 확인
    private bool CheckCurrentCollision()
    {
        if (meshCollisionDetectors == null) return false;

        foreach (var detector in meshCollisionDetectors)
        {
            if (detector != null)  // detector 자체가 null이 아닌지만 확인
            {
                return MeshMapCollisionDetector.isCollision;  // static 프로퍼티로 접근
            }
        }
        return false;
    }
}
