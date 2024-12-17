using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using RosMessageTypes.JointControl;
using Unity.Robotics.UrdfImporter.Control;

public class GazeScene : MonoBehaviour
{
    private SceneController sceneController;
    private MeshFunction meshManager;
    Transform cameraTransform;

    public Image imageGaze;     // ī�޶� ������ ǥ���� �̹���
    public Image imageTime;     // �浹 �� ���� �� ������ �� �̹���
    private float collisionDuration = 0.0f;
    private float requiredDuration = 1.0f;  // 3�� ���� �浹 ���� �ʿ�
    private bool isColliding = false;

    private ARMeshManager arMeshManager;
    private MeshMapRenderer meshmaprenderer;
    private CtrlFlagPublisher ctrlflagpublisher;
    // private CardboardController cardboardController;
    private int meshLayer;  // 클래스 상단에 변수 선언 추가

    [SerializeField]
    private CardboardRenderer cardboardRenderer;

    [SerializeField]
    private Slider slider;
    public GameObject sgr532;

    private Joint_listMsg currentMessage; // 현재 실행 중인 메시지
    private float play_value = 0f;
    private float play_velocity = 0.005f;

    public static bool tag_3d = false;

    void Start()
    {
        cameraTransform = Camera.main.transform;  // Camera.main Ȯ

        // Hierarchy SceneController Ʈ ã SceneController ũƮ   ϴ.
        GameObject sceneControllerObject = GameObject.Find("SceneController");
        if (sceneControllerObject != null)
        {
            sceneController = sceneControllerObject.GetComponent<SceneController>();
            if (sceneController == null)
            {
                Debug.LogError("SceneController script is not attached to SceneController GameObject.");
            }
        }
        else
        {
            Debug.LogError("SceneController GameObject not found in the scene.");
        }

        // MeshMapRenderer 찾기
        meshmaprenderer = FindObjectOfType<MeshMapRenderer>();
        if (meshmaprenderer == null)
        {
            Debug.LogWarning("MeshMapRenderer를 찾을 수 없습니다.");
        }

        meshManager = FindObjectOfType<MeshFunction>();
            if (meshManager == null)
        {
            Debug.LogWarning("MeshManager를 찾을 수 없습니다.");
        }

        if (cardboardRenderer == null)
        {
            cardboardRenderer = FindObjectOfType<CardboardRenderer>();
            if (cardboardRenderer == null)
            {
                Debug.LogWarning("CardboardRenderer 찾을 수 없습니다.");
            }
        }

        ctrlflagpublisher = FindObjectOfType<CtrlFlagPublisher>();
        if (ctrlflagpublisher == null)
        {
            ctrlflagpublisher = FindObjectOfType<CtrlFlagPublisher>();
            if (ctrlflagpublisher == null)
            {
                Debug.LogWarning("CardboardRenderer 찾을 수 없습니다.");
            }
        }

        ResetProgress();  // Fill Amount 0 ʱȭ
        imageGaze.gameObject.SetActive(true);

        // Mesh를 위한 레이어 설정
        meshLayer = LayerMask.NameToLayer("MeshLayer");
        if (meshLayer == -1)
        {
            meshLayer = 8;  // 사용하지 않는 레이어 번호 사용
        }

        // // 씬 시작 시 tag_3d 초기화
        // tag_3d = false;
        // CardboardRenderer.Set3dTag(tag_3d);
    }

    void Update()
    {
        // MeshLayer를 제외한 모든 레이어와 충돌 검사
        int layerMask = ~(1 << meshLayer);

        // ī�޶�  ī޶ ٶ󺸴  Raycast
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            GameObject go = hit.collider.gameObject;
            if (go.CompareTag("MeshOnOff"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

                if (collisionDuration >= requiredDuration)
                {
                    if (meshmaprenderer != null)
                    {
                        meshmaprenderer.RenderMeshOnOff();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            else if (go.CompareTag("MeshTrans"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

                if (collisionDuration >= requiredDuration)
                {
                    if (meshmaprenderer != null)
                    {
                        meshmaprenderer.RenderMeshTransparent();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            else if (go.CompareTag("Publish"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

                if (collisionDuration >= requiredDuration)
                {
                    if (ctrlflagpublisher != null)
                    {
                        ctrlflagpublisher.PublishControlFlag();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            else if (go.CompareTag("RenderingScene"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);  // 浹  Progress Bar Ȱȭ
                }

                collisionDuration += Time.deltaTime;

                // 浹 Ǹ Info
                if (collisionDuration >= requiredDuration)
                {
                    TriggerSceneChange("Rendering");
                    ResetProgress();  //  ʱȭ
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;  // Ʈ
                }
            }
            else if (go.CompareTag("Trajectory"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);  // 浹  Progress Bar Ȱȭ
                }

                collisionDuration += Time.deltaTime;

                // 浹 Ǹ Info
                if (collisionDuration >= requiredDuration)
                {
                    CardboardRenderer.Set3dTag(true);
                    TriggerSceneChange("TrajectoryControl");
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;  // Ʈ
                }
            }
            else if (go.CompareTag("MotionView"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);  // 浹  Progress Bar Ȱȭ
                }

                collisionDuration += Time.deltaTime;

                // 浹 Ǹ Info
                if (collisionDuration >= requiredDuration)
                {
                    CardboardRenderer.Set3dTag(true);
                    TriggerSceneChange("MotionView");
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;  // Ʈ
                }
            }
            else if (go.CompareTag("SingleView"))  // Box collider
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

                if (collisionDuration >= requiredDuration)
                {
                    if (cardboardRenderer != null)
                    {
                        CardboardRenderer.Set3dTag(false);
                        cardboardRenderer.SetSplitView();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            else if (go.CompareTag("PlayForward"))  // Box collider
            {
                Debug.Log($"PlayForward");
                isColliding = true;
                imageTime.gameObject.SetActive(true);
                collisionDuration = requiredDuration;
                imageTime.fillAmount = collisionDuration / requiredDuration;

                play_value += play_velocity;
                if (play_value >= 1)
                {
                    play_value = 1;
                }
                slider.value = play_value;

            }
            else if (go.CompareTag("PlayBackward"))  // Box collider
            {
                Debug.Log($"PlayBackward");
                isColliding = true;
                imageTime.gameObject.SetActive(true);
                collisionDuration = requiredDuration;
                imageTime.fillAmount = collisionDuration / requiredDuration;

                play_value -= play_velocity;
                if (play_value <= 0)
                {
                    play_value = 0;
                }
                slider.value = play_value;

            }
            else
            {
                ResetProgress();
            }
        }
        else
        {
            if (isColliding)
            {
                ResetProgress();
            }
        }
    }

    public void TriggerSceneChange(string sceneName)
    {
        // SceneController GotoScene Լ ȣϿ մϴ.
        if (sceneController != null)
        {
            sceneController.GotoScene(sceneName);
        }
    }

    void ResetProgress()
    {
        collisionDuration = 0.0f;
        // imageTime.fillAmount = 0.0f;  // Fill Amount 0 ʱȭ
        // imageTime.gameObject.SetActive(false);  // Progress Bar
        if (imageTime != null)
        {
            imageTime.fillAmount = 0.0f;  // Fill Amount를 0으로 초기화
            imageTime.gameObject.SetActive(false);  // Progress Bar 비활성화
        }
        isColliding = false;

        // CardboardRenderer.Set3dTag(false);

        // tag_3d = false;
        // CardboardRenderer.Set3dTag(tag_3d);
    }
}
