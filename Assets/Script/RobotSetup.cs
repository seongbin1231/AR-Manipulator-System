using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter;

public class RobotSetup : MonoBehaviour
{
    // public bool showRobotColliders = true;  // Inspector에서 토글 가능
    // public Color robotColliderColor = new Color(1, 0, 0, 0.5f);  // 반투명 빨간색

    private static bool showRobotColliders = false;  // 시각화 켜기/끄기
    private static Color robotColliderColor = new Color(1, 0, 0, 0.5f);  // 반투명 빨간색

    void Start()
    {
        SetupAllRobotLinks();
        // CheckRobotColliderSettings();
    }

    void SetupAllRobotLinks()
    {
        GameObject robotRoot = GameObject.Find("sgr532");
        if (robotRoot == null)
        {
            Debug.LogError("로봇 루트 오브젝트(sgr532)를 찾을 수 없습니다.");
            return;
        }

        // base_link 설정
        Transform baseLink = robotRoot.transform.Find("base_link");
        if (baseLink != null)
        {
            SetupCollisionDetection(baseLink.gameObject, "sgr532/base_link");

            // link1부터 link6 순차적으로 찾기
            Transform currentLink = baseLink;
            for (int i = 1; i <= 6; i++)
            {
                Transform nextLink = currentLink.Find($"link{i}");
                if (nextLink != null)
                {
                    SetupCollisionDetection(nextLink.gameObject, $"sgr532/link{i}");
                    currentLink = nextLink;
                }
                else
                {
                    Debug.LogError($"link{i}를 찾을 수 없습니다.");
                }
            }

            // gripper 링크 설정 (link6 하위에서 찾기)
            if (currentLink != null) // currentLink는 마지막으로 찾은 link6
            {
                Transform gripperLeft = currentLink.Find("link_gripper_left");
                Transform gripperRight = currentLink.Find("link_gripper_right");

                if (gripperLeft != null)
                {
                    SetupCollisionDetection(gripperLeft.gameObject, "sgr532/link_gripper_left");
                }
                else
                {
                    Debug.LogError("link_gripper_left를 찾을 수 없습니다.");
                }

                if (gripperRight != null)
                {
                    SetupCollisionDetection(gripperRight.gameObject, "sgr532/link_gripper_right");
                }
                else
                {
                    Debug.LogError("link_gripper_right를 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("base_link를 찾을 수 없습니다.");
        }
    }

    void SetupCollisionDetection(GameObject linkObject, string tagName)
    {
        Debug.Log($"SetupCollisionDetection : {linkObject.name}");
        int robotLinkLayer = LayerMask.NameToLayer("RobotLink");

        // RobotLink Layer 할당 (base_link 제외)
        if (tagName != "sgr532/base_link")
        {
            linkObject.layer = robotLinkLayer;
        }

        // ArticulationBody 설정
        ArticulationBody artBody = linkObject.GetComponent<ArticulationBody>();
        if (artBody == null)
        {
            artBody = linkObject.AddComponent<ArticulationBody>();
        }

        // ArticulationBody 기본 설정
        artBody.useGravity = false;
        // artBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        artBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        // artBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        // artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

        artBody.matchAnchors = true;
        artBody.jointFriction = 0.01f;
        artBody.angularDamping = 0.01f;
        artBody.linearDamping = 0.01f;

        var drive = artBody.xDrive;
        drive.stiffness = 10000;
        drive.damping = 100;
        drive.forceLimit = 1000;
        artBody.xDrive = drive;

        // Collisions 객체 찾기
        Transform collisionsTransform = linkObject.transform.Find("Collisions/unnamed");
        if (collisionsTransform != null)
        {
            Transform meshObjectTransform;
            if (linkObject.name == "base_link")
            {
                meshObjectTransform = collisionsTransform.Find($"sagittarius_{linkObject.name}/sagittarius_{linkObject.name}_0");
            }
            else
            {
                meshObjectTransform = collisionsTransform.Find($"{linkObject.name}/{linkObject.name}_0");
            }

            if (meshObjectTransform != null)
            {
                GameObject meshObject = meshObjectTransform.gameObject;

                // Layer 및 Tag 설정
                meshObject.layer = robotLinkLayer;
                meshObject.tag = tagName;

                // 기존 MeshCollider 확인 및 설정
                MeshCollider[] existingColliders = meshObject.GetComponents<MeshCollider>();
                if (existingColliders.Length == 0)
                {
                    // MeshFilter에서 메시 가져오기
                    MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        MeshCollider collider = meshObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = meshFilter.sharedMesh;
                        collider.convex = true;
                        // collider.isTrigger = false;
                        collider.isTrigger = true;
                        collider.enabled = true;
                    }
                }

                // // RobotLinkColliderUpdater 설정
                // RobotLinkColliderUpdater updater = meshObject.GetComponent<RobotLinkColliderUpdater>();
                // if (updater == null)
                // {
                //     updater = meshObject.AddComponent<RobotLinkColliderUpdater>();
                //     Debug.Log($"RobotLinkColliderUpdater 추가됨: {meshObject.name}");
                // }
                // updater.Initialize();

                // Debug.Log($"[RobotSetup] 링크 설정 완료: {linkObject.name}\n" +
                //          $"- Layer: {LayerMask.LayerToName(meshObject.layer)}\n" +
                //          $"- Tag: {meshObject.tag}\n" +
                //          $"- CollisionDetection: {artBody.collisionDetectionMode}\n" +
                //          $"- MeshCollider Count: {meshObject.GetComponents<MeshCollider>().Length}\n" +
                //          $"- Has RobotLinkColliderUpdater: {updater != null}");
            }
            else
            {
                Debug.LogError($"메시 오브젝트를 찾을 수 없음: {linkObject.name}_0");
            }
        }
        else
        {
            Debug.LogError($"Collisions/unnamed를 찾을 수 없음: {linkObject.name}");
        }
        Debug.Log($"SetupCollisionDetection : {linkObject.name} finished");
    }

    // private void CheckRobotColliderSettings()
    // {
    //     GameObject robotRoot = GameObject.Find("sgr532");
    //     if (robotRoot == null) return;

    //     Debug.Log("\n=== 로봇 Collider 설정 확인 ===");

    //     // base_link 찾기
    //     Transform baseLink = robotRoot.transform.Find("base_link");
    //     if (baseLink != null)
    //     {
    //         // base_link 확인
    //         CheckLinkColliderSettings(baseLink.gameObject);

    //         // link1부터 link6까지 순차적으로 확인
    //         Transform currentLink = baseLink;
    //         for (int i = 1; i <= 6; i++)
    //         {
    //             Transform nextLink = currentLink.Find($"link{i}");
    //             if (nextLink != null)
    //             {
    //                 CheckLinkColliderSettings(nextLink.gameObject);
    //                 currentLink = nextLink;
    //             }
    //         }

    //         // gripper 링크 확인 (link6 하위)
    //         if (currentLink != null)
    //         {
    //             Transform gripperLeft = currentLink.Find("link_gripper_left");
    //             Transform gripperRight = currentLink.Find("link_gripper_right");

    //             if (gripperLeft != null)
    //             {
    //                 CheckLinkColliderSettings(gripperLeft.gameObject);
    //             }

    //             if (gripperRight != null)
    //             {
    //                 CheckLinkColliderSettings(gripperRight.gameObject);
    //             }
    //         }
    //     }
    // }

    // private void CheckLinkColliderSettings(GameObject linkObject)
    // {
    //     Transform collisionsTransform = linkObject.transform.Find("Collisions/unnamed");
    //     if (collisionsTransform != null)
    //     {
    //         Transform meshObjectTransform;
    //         if (linkObject.name == "base_link")
    //         {
    //             meshObjectTransform = collisionsTransform.Find($"sagittarius_{linkObject.name}/sagittarius_{linkObject.name}_0");
    //         }
    //         else
    //         {
    //             meshObjectTransform = collisionsTransform.Find($"{linkObject.name}/{linkObject.name}_0");
    //         }

    //         if (meshObjectTransform != null)
    //         {
    //             GameObject meshObject = meshObjectTransform.gameObject;

    //             // 모든 Collider 정보 출력
    //             MeshCollider[] meshColliders = meshObject.GetComponents<MeshCollider>();
    //             Debug.Log($"\n오브젝트: {meshObject.name}");

    //             for (int i = 0; i < meshColliders.Length; i++)
    //             {
    //                 var meshCollider = meshColliders[i];
    //                 Debug.Log($"- Mesh Collider #{i + 1}\n" +
    //                          $"  - Convex: {meshCollider.convex}\n" +
    //                          $"  - Is Trigger: {meshCollider.isTrigger}\n" +
    //                          $"  - Has Mesh: {meshCollider.sharedMesh != null}");
    //             }

    //             // ArticulationBody 정보 출력
    //             ArticulationBody artBody = linkObject.GetComponent<ArticulationBody>();
    //             if (artBody != null)
    //             {
    //                 Debug.Log($"  - Body 타입: ArticulationBody\n" +
    //                          $"    - Joint Type: {artBody.jointType}\n" +
    //                          $"    - Immovable: {artBody.immovable}\n" +
    //                          $"    - Use Gravity: {artBody.useGravity}\n" +
    //                          $"    - Collision Detection: {artBody.collisionDetectionMode}");
    //             }
    //         }
    //     }
    // }

//     private void OnDrawGizmos()
//     {
//         if (!showRobotColliders) return;

//         // 에디터에서도 실시간 업데이트를 위해 현재 선택된 오브젝트 사용
//         GameObject robotRoot = null;

// #if UNITY_EDITOR
//         if (Selection.activeGameObject != null)
//         {
//             robotRoot = Selection.activeGameObject.transform.root.gameObject;
//             if (!robotRoot.name.Equals("sgr532"))
//             {
//                 robotRoot = GameObject.Find("sgr532");
//             }
//         }
//         else
// #endif
//         {
//             robotRoot = GameObject.Find("sgr532");
//         }

//         if (robotRoot == null) return;

//         // 매 프레임마다 현재 Transform 정보로 Gizmos 그리기
//         Gizmos.color = robotColliderColor;
//         DrawLinkColliders(robotRoot.transform.Find("base_link"));
//     }

//     private void DrawLinkColliders(Transform linkTransform)
//     {
//         if (linkTransform == null) return;

//         Transform collisionsTransform = linkTransform.Find("Collisions/unnamed");
//         if (collisionsTransform != null)
//         {
//             Transform meshObjectTransform;
//             if (linkTransform.name == "base_link")
//             {
//                 meshObjectTransform = collisionsTransform.Find($"sagittarius_{linkTransform.name}/sagittarius_{linkTransform.name}_0");
//             }
//             else
//             {
//                 meshObjectTransform = collisionsTransform.Find($"{linkTransform.name}/{linkTransform.name}_0");
//             }

//             if (meshObjectTransform != null)
//             {
//                 GameObject meshObject = meshObjectTransform.gameObject;
//                 MeshCollider[] meshColliders = meshObject.GetComponents<MeshCollider>();

//                 foreach (var collider in meshColliders)
//                 {
//                     if (collider != null && collider.sharedMesh != null)
//                     {
//                         // 메시의 실제 월드 공간 바운드 계산
//                         Bounds worldBounds = new Bounds();
//                         Vector3[] vertices = collider.sharedMesh.vertices;

//                         if (vertices.Length > 0)
//                         {
//                             // 첫 번째 정점으로 초기화
//                             Vector3 firstVertex = meshObject.transform.TransformPoint(vertices[0]);
//                             worldBounds.center = firstVertex;

//                             // 나머지 정점으로 바운드 확장
//                             for (int i = 1; i < vertices.Length; i++)
//                             {
//                                 worldBounds.Encapsulate(meshObject.transform.TransformPoint(vertices[i]));
//                             }

//                             // 실시간으 계산된 바운드로 시각화
//                             Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
//                             Gizmos.DrawSphere(worldBounds.center, 0.02f);

//                             // 와이어프레 그리기
//                             int[] triangles = collider.sharedMesh.triangles;
//                             for (int i = 0; i < triangles.Length; i += 3)
//                             {
//                                 Vector3 v1 = meshObject.transform.TransformPoint(vertices[triangles[i]]);
//                                 Vector3 v2 = meshObject.transform.TransformPoint(vertices[triangles[i + 1]]);
//                                 Vector3 v3 = meshObject.transform.TransformPoint(vertices[triangles[i + 2]]);

//                                 Gizmos.DrawLine(v1, v2);
//                                 Gizmos.DrawLine(v2, v3);
//                                 Gizmos.DrawLine(v3, v1);
//                             }

//                             // Debug.Log($"콜라이더 찾음: {meshObject.name}\n" +
//                             //         $"계산된 중심점: {worldBounds.center}\n" +
//                             //         $"계산된 크기: {worldBounds.size}");
//                         }
//                     }
//                 }
//             }
//         }

//         // 자식 링크들에 대해 재귀적으로 시각화
//         for (int i = 0; i < linkTransform.childCount; i++)
//         {
//             DrawLinkColliders(linkTransform.GetChild(i));
//         }
//     }

//     // 시각화 제어를 위 public 메서드
//     public static void SetVisualization(bool enable)
//     {
//         showRobotColliders = enable;
//     }

    //void Update()
    //{
    //    // "Urdf Robot" 스크립트 컴포넌트 가져오기
    //    var urdfRobot = GetComponent<UrdfRobot>();
    //    if (urdfRobot != null)
    //    {
    //        // All Colliders의 Convex 속성을 Enable로 설정
    //        urdfRobot.collidersConvex = true;
    //    }
    //}
}

// // 새로운 컴포넌트 추가
// public class RobotLinkColliderUpdater : MonoBehaviour
// {
//     private class ColliderData
//     {
//         public MeshCollider collider;
//         public Vector3[] originalVertices;
//         public Mesh originalMesh;
//         public Bounds initialBounds;
//         public Vector3 relativeCenter;
//     }

//     private class LinkData
//     {
//         public string linkName;
//         public int linkIndex; // base_link: 0, link1: 1, ..., gripper_left: 7, gripper_right: 8
//         public Transform linkTransform;
//         public Transform parentLink;
//     }

//     private List<ColliderData> colliderDataList = new List<ColliderData>();
//     private LinkData currentLinkData;
//     private Vector3 lastBaseLinkPosition;
//     private Quaternion lastBaseLinkRotation;
//     private bool needsUpdate = true;
//     private Matrix4x4 initialLinkTransform; // 초기 변환 행렬 저장

//     // 모든 링�의 업데이트 상태를 추적하기 위한 정적 변수들
//     private static HashSet<string> updatedLinks = new HashSet<string>();
//     private static readonly string[] allLinks = new string[]
//     {
//         "base_link",
//         "link1",
//         "link2",
//         "link3",
//         "link4",
//         "link5",
//         "link6",
//         "link_gripper_left",
//         "link_gripper_right"
//     };

//     public void Initialize()
//     {
//         MeshCollider[] meshColliders = GetComponents<MeshCollider>();

//         foreach (var collider in meshColliders)
//         {
//             if (collider != null && collider.sharedMesh != null)
//             {
//                 // 초기 bounds.center와 각 link position의 상대적 차이 계산
//                 Vector3 relativeCenter = collider.bounds.center - transform.position;

//                 ColliderData data = new ColliderData
//                 {
//                     collider = collider,
//                     originalMesh = collider.sharedMesh,
//                     originalVertices = collider.sharedMesh.vertices,
//                     initialBounds = collider.bounds,
//                     relativeCenter = relativeCenter
//                 };

//                 collider.enabled = true;
//                 // collider.isTrigger = false;
//                 collider.isTrigger = true;
//                 collider.convex = true;

//                 colliderDataList.Add(data);

//                 Debug.Log($"[Initialize] MeshCollider 이름: {collider.name}\n" +
//                          $"Collider {colliderDataList.Count}: {collider.name}\n" +
//                          $"Initial Bounds Center: {data.initialBounds.center}\n" +
//                          $"Relative Center: {relativeCenter}");
//             }
//         }

//         // 현재 링크 정보 초기화
//         InitializeLinkData();

//         // 초기 변환 행렬 계산 및 저장
//         CalculateInitialTransform();

//         lastBaseLinkPosition = ObjectDataManager.GetSavedPosition();
//         lastBaseLinkRotation = ObjectDataManager.GetSavedRotation();
//     }

//     private void InitializeLinkData()
//     {
//         currentLinkData = new LinkData();
//         string objectName = gameObject.name.Replace("_0", "");

//         if (objectName.Contains("base_link"))
//         {
//             currentLinkData.linkIndex = 0;
//             currentLinkData.linkName = "base_link";
//         }
//         else if (objectName.Contains("link"))
//         {
//             if (objectName.Contains("gripper"))
//             {
//                 currentLinkData.linkIndex = objectName.Contains("left") ? 7 : 8;
//                 currentLinkData.linkName = objectName;
//             }
//             else
//             {
//                 currentLinkData.linkIndex = int.Parse(objectName.Replace("link", ""));
//                 currentLinkData.linkName = objectName;
//             }
//         }

//         // 부모 링크 ���조 저장
//         Transform current = transform;
//         while (current != null)
//         {
//             if (current.parent != null && current.parent.GetComponent<ArticulationBody>() != null)
//             {
//                 currentLinkData.parentLink = current.parent;
//                 break;
//             }
//             current = current.parent;
//         }

//         Debug.Log($"링크 초기화 완료: {currentLinkData.linkName} (인덱스: {currentLinkData.linkIndex})");
//     }

//     private Matrix4x4 CalculateWorldMatrix()
//     {
//         Debug.Log($"[CalculateWorldMatrix] 시작 - 현재 링크: {currentLinkData.linkName} (인덱스: {currentLinkData.linkIndex})");

//         // 현재 링크의 Transform 가져오기
//         Transform targetTransform = transform;
//         while (targetTransform != null && !targetTransform.name.Contains("_0"))
//         {
//             targetTransform = targetTransform.parent;
//         }

//         if (targetTransform == null)
//         {
//             Debug.LogError($"[CalculateWorldMatrix] 현재 링크의 Transform을 찾을 수 없음: {currentLinkData.linkName}");
//             return Matrix4x4.identity;
//         }

//         // 현재 링크의 월드 Transform 정보로 행렬 계산
//         Vector3 worldPosition = targetTransform.position;
//         Quaternion worldRotation = targetTransform.rotation;
//         Vector3 worldScale = targetTransform.lossyScale;

//         Matrix4x4 worldMatrix = Matrix4x4.TRS(worldPosition, worldRotation, worldScale);

//         Debug.Log($"[CalculateWorldMatrix] 현재 링크 '{targetTransform.name}'의 World Matrix:\n" +
//                  $"- World Position: {worldPosition}\n" +
//                  $"- World Rotation: {worldRotation.eulerAngles}\n" +
//                  $"- World Scale: {worldScale}");

//         Debug.Log($"[CalculateWorldMatrix] 완료 - 링크: {currentLinkData.linkName} 변환행렬 : \n{worldMatrix}");
//         return worldMatrix;
//     }

//     private Matrix4x4 CalculateBaseMatrix()
//     {
//         Debug.Log($"[CalculateBaseMatrix] 시작 - 현재 링크: {currentLinkData.linkName} (인덱스: {currentLinkData.linkIndex})");

//         // base_link의 Transform 찾기
//         Transform baseLink = GameObject.Find("sgr532/base_link").transform;
//         if (baseLink == null)
//         {
//             Debug.LogError("[CalculateBaseMatrix] base_link를 찾을 수 없음");
//             return Matrix4x4.identity;
//         }

//         // base_link의 월드 Transform 정보로 행렬 계산
//         Vector3 worldPosition = baseLink.position;
//         // Quaternion worldRotation = baseLink.rotation;
//         Quaternion worldRotation = Quaternion.identity;
//         Vector3 worldScale = baseLink.lossyScale;

//         Matrix4x4 worldMatrix = Matrix4x4.TRS(worldPosition, worldRotation, worldScale);

//         Debug.Log($"[CalculateBaseMatrix] base_link의 World Matrix:\n" +
//                 $"- World Position: {worldPosition}\n" +
//                 $"- World Rotation: {worldRotation.eulerAngles}\n" +
//                 $"- World Scale: {worldScale}");

//         Debug.Log($"[CalculateWorldMatrix] 완료 - base_link 변환행렬 : \n{worldMatrix}");
//         return worldMatrix;
//     }

//     private Matrix4x4 InverseHomogeneousMatrix(Matrix4x4 matrix)
//     {
//         // 3x3 회전 행렬과 translation 벡터 추출
//         Vector3 translation = matrix.GetColumn(3);
//         Matrix4x4 rotationMatrix = matrix;
//         rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

//         // 회전 행렬의 전치 계산 (inverse of rotation = transpose)
//         Matrix4x4 rotationTranspose = Matrix4x4.zero;
//         for (int i = 0; i < 3; i++)
//         {
//             for (int j = 0; j < 3; j++)
//             {
//                 rotationTranspose[i, j] = matrix[j, i];
//             }
//         }
//         rotationTranspose[3, 3] = 1;

//         // 새로운 translation 계산: -(R^T * t)
//         Vector3 newTranslation = Vector3.zero;
//         for (int i = 0; i < 3; i++)
//         {
//             for (int j = 0; j < 3; j++)
//             {
//                 newTranslation[i] -= rotationTranspose[i, j] * translation[j];
//             }
//         }

//         // 최종 역행렬 생성
//         Matrix4x4 inverseMatrix = rotationTranspose;
//         inverseMatrix.SetColumn(3, new Vector4(newTranslation.x, newTranslation.y, newTranslation.z, 1));

//         return inverseMatrix;
//     }

//     private void CalculateInitialTransform()
//     {
//         Debug.Log($"[CalculateInitialTransform] 시작 - 현재 링크: {currentLinkData.linkName} (인덱스: {currentLinkData.linkIndex})");

//         // 현재 링크부터 base_link까지의 변환 행렬 계산
//         Matrix4x4 transform = Matrix4x4.identity;
//         Transform current = this.transform;

//         while (current != null)
//         {
//             // 현재 Transform의 로컬 행렬 계산
//             Matrix4x4 localMatrix = Matrix4x4.TRS(
//                 current.localPosition,
//                 current.localRotation,
//                 current.localScale);

//             Debug.Log($"[CalculateInitialTransform] 링크 '{current.name}'의 Local Matrix:\n" +
//                      $"- Position: {current.localPosition}\n" +
//                      $"- Rotation: {current.localRotation.eulerAngles}\n" +
//                      $"- Scale: {current.localScale}");

//             // 로컬 행렬의 역행렬 계산
//             Matrix4x4 inverseLocalMatrix = InverseHomogeneousMatrix(localMatrix);
//             Debug.Log($"[CalculateInitialTransform] 링크 '{current.name}'의 Inverse Local Matrix:\n{inverseLocalMatrix}");

//             // 전체 변환 행렬에 현재 로컬 행렬 곱하기
//             transform = transform * inverseLocalMatrix;

//             // base_link에 도달하면 중단
//             if (current.name.Contains("base_link"))
//             {
//                 break;
//             }

//             current = current.parent;
//         }

//         initialLinkTransform = transform;

//         Debug.Log($"[CalculateInitialTransform] 완료 - 링크: {currentLinkData.linkName}\n" +
//                  $"Initial Transform Matrix:\n{initialLinkTransform}");
//     }

//     private void FixedUpdate()
//     {
//         // if (Unity.Robotics.UrdfImporter.Control.Controller.control_flag)
//         if (true)
//         {
//             needsUpdate = true;
//             lastBaseLinkPosition = ObjectDataManager.GetSavedPosition();
//             lastBaseLinkRotation = ObjectDataManager.GetSavedRotation();

//             Debug.Log($"[ColliderUpdate] 링크: {currentLinkData.linkName} (인덱스: {currentLinkData.linkIndex})\n" +
//                      $"Position: {lastBaseLinkPosition}, Rotation: {lastBaseLinkRotation.eulerAngles}");
//         }

//         if (needsUpdate)
//         {
//             // Quaternion이 유효한지 확인
//             if (Mathf.Approximately(lastBaseLinkRotation.x * lastBaseLinkRotation.x +
//                                   lastBaseLinkRotation.y * lastBaseLinkRotation.y +
//                                   lastBaseLinkRotation.z * lastBaseLinkRotation.z +
//                                   lastBaseLinkRotation.w * lastBaseLinkRotation.w, 0f))
//             {
//                 lastBaseLinkRotation = Quaternion.identity;
//             }

//             // Matrix4x4 worldMatrix = CalculateWorldMatrix();
//             Matrix4x4 baseMatrix = CalculateBaseMatrix();
//             Vector3 basePosition = baseMatrix.GetColumn(3);
//             // Matrix4x4 transformMatrix = Matrix4x4.TRS(lastBaseLinkPosition, lastBaseLinkRotation, Vector3.one);

//             foreach (var data in colliderDataList)
//             {
//                 if (data.collider != null)
//                 {
//                     // // 변환 행렬 적용
//                     // data.collider.transform.position = transformMatrix.GetColumn(3);
//                     // data.collider.transform.rotation = transformMatrix.rotation;

//                     // 메시 업데이트
//                     Mesh updatedMesh = new Mesh();
//                     // Matrix4x4 finalMatrix = initialLinkTransform * transformMatrix * worldMatrix;

//                     Vector3[] worldVertices = new Vector3[data.originalVertices.Length];
//                     for (int i = 0; i < data.originalVertices.Length; i++)
//                     {
//                         // worldVertices[i] = baseMatrix.MultiplyPoint3x4(data.originalVertices[i]);
//                         // worldVertices[i] = data.originalVertices[i] + basePosition;
//                         worldVertices[i] = data.originalVertices[i];
//                     }
//                     // Debug.Log($"worldVertices[0] - worldVertices[1]: {worldVertices[0] - worldVertices[1]}");
//                     // Debug.Log($"data.originalVertices[0] - data.originalVertices[1]: {data.originalVertices[0] - data.originalVertices[1]}");

//                     updatedMesh.vertices = worldVertices;
//                     updatedMesh.triangles = data.originalMesh.triangles;
//                     updatedMesh.normals = data.originalMesh.normals;
//                     updatedMesh.uv = data.originalMesh.uv;
//                     updatedMesh.tangents = data.originalMesh.tangents;

//                     if (data.collider.sharedMesh != data.originalMesh)
//                     {
//                         Destroy(data.collider.sharedMesh);
//                     }

//                     data.collider.sharedMesh = updatedMesh;
//                     data.collider.convex = true;
//                     data.collider.isTrigger = true;
//                 }
//             }

//             needsUpdate = false;

//             // 현재 링크를 업데이트된 링크 목록에 추가
//             updatedLinks.Add(currentLinkData.linkName);

//             // 모든 링크가 업데이트되었는지 확인
//             bool allLinksUpdated = allLinks.All(link => updatedLinks.Contains(link));

//             if (allLinksUpdated)
//             {
//                 // Unity.Robotics.UrdfImporter.Control.Controller.control_flag = false;
//                 updatedLinks.Clear(); // 다음 업데이트를 위해 초기화
//                 Debug.Log($"[ColliderUpdate] 모든 링크 업데이트 완료 - control_flag 해제\n" +
//                          $"업데이트된 링크들: {string.Join(", ", updatedLinks)}");
//             }
//         }
//     }

//     private void OnDestroy()
//     {
//         // 컴포넌트가 파괴될 때 업데이트된 링크 목록에서 제거
//         updatedLinks.Remove(currentLinkData.linkName);

//         foreach (var data in colliderDataList)
//         {
//             if (data.collider != null && data.collider.sharedMesh != data.originalMesh)
//             {
//                 Destroy(data.collider.sharedMesh);
//             }
//         }
//         colliderDataList.Clear();
//     }
// }
