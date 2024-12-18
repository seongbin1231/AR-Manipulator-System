using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using RosMessageTypes.JointControl;
using Unity.Robotics.UrdfImporter.Control;

/// <summary>
///     This script provide 3D View interaction
///     by gazing the boxes. Boxed are classified
///     with name of Tag.
/// </summary>
public class GazeScene : MonoBehaviour
{
    // Reference scripts
    private SceneController sceneController;
    private MeshFunction meshManager;
    private ARMeshManager arMeshManager;
    private MeshMapRenderer meshmaprenderer;
    private CtrlFlagPublisher ctrlflagpublisher;
    [SerializeField]
    private CardboardRenderer cardboardRenderer;

    // Reference objects
    Transform cameraTransform;
    public Image imageGaze;
    public Image imageTime;

    // For gaze interaction steps
    private float collisionDuration = 0.0f;
    private float requiredDuration = 1.0f;
    private bool isColliding = false;

    // Mesh layer number as avoid hit gaze with mesh map
    private int meshLayer;

    // 3D View trajectory control slider
    [SerializeField]
    private Slider slider;

    //3D View trajectory control slider controller
    private Joint_listMsg currentMessage;
    private float play_value = 0f;
    private float play_velocity = 0.005f;

    // Get current view mode
    public static bool tag_3d = false;

    void Start()
    {
        // Get main camera transform
        cameraTransform = Camera.main.transform;

        // Find SceneController
        GameObject sceneControllerObject = GameObject.Find("SceneController");
        if (sceneControllerObject != null)
        {
            sceneController = sceneControllerObject.GetComponent<SceneController>();
        }

        // Find MeshMapRenderer
        meshmaprenderer = FindObjectOfType<MeshMapRenderer>();
        if (meshmaprenderer == null)
        {
            Debug.LogWarning("MeshMapRenderer를 찾을 수 없습니다.");
        }

        // Find MeshFunction
        meshManager = FindObjectOfType<MeshFunction>();
            if (meshManager == null)
        {
            Debug.LogWarning("MeshManager를 찾을 수 없습니다.");
        }

        // Find CardboardRenderer
        if (cardboardRenderer == null)
        {
            cardboardRenderer = FindObjectOfType<CardboardRenderer>();
            if (cardboardRenderer == null)
            {
                Debug.LogWarning("CardboardRenderer 찾을 수 없습니다.");
            }
        }

        // Find CtrlFlagPublisher
        ctrlflagpublisher = FindObjectOfType<CtrlFlagPublisher>();
        if (ctrlflagpublisher == null)
        {
            ctrlflagpublisher = FindObjectOfType<CtrlFlagPublisher>();
        }

        // Initialize gaze interaction
        ResetProgress();
        imageGaze.gameObject.SetActive(true);

        // Mesh map layer
        meshLayer = LayerMask.NameToLayer("MeshLayer");
        if (meshLayer == -1)
        {
            // Number of MeshLayer
            // It can be set at editor
            meshLayer = 8;  
        }
    }

    void Update()
    {
        // Ignore hitting with MeshLayer
        int layerMask = ~(1 << meshLayer);

        // Check Raycast with user gazing (cameraTransform - position / forward)
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            // Interactino with hit.collider
            GameObject go = hit.collider.gameObject;

            // If hitting event occur, function execute by comparing with tag name

            // Mesh Map On / Off
            if (go.CompareTag("MeshOnOff"))     // if tag of box is "MeshOnOff"
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
                        // Mesh Map On / Off
                        meshmaprenderer.RenderMeshOnOff();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            // Mesh Map Transparent or Not
            else if (go.CompareTag("MeshTrans"))        // if tag of box is "MeshTrans"
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
                        // Mesh Map Transparent or Not
                        meshmaprenderer.RenderMeshTransparent();
                    }
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            // Mesh Map Transparent or Not
            else if (go.CompareTag("Publish"))      // if tag of box is "Publish"
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
            // SceneChange to Rendering
            else if (go.CompareTag("RenderingScene"))       // if tag of box is "RenderingScene"
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

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
            // SceneChange to Trajectory
            else if (go.CompareTag("Trajectory"))       // if tag of box is "Trajectory"
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
                }

                collisionDuration += Time.deltaTime;

                if (collisionDuration >= requiredDuration)
                {
                    CardboardRenderer.Set3dTag(true);
                    TriggerSceneChange("TrajectoryControl");
                    ResetProgress();
                }
                else
                {
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            // SceneChange to MotionView
            else if (go.CompareTag("MotionView"))       // if tag of box is "MotionView"
            {
                if (!isColliding)
                {
                    isColliding = true;
                    imageTime.gameObject.SetActive(true);
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
                    imageTime.fillAmount = collisionDuration / requiredDuration;
                }
            }
            // View mode change from 3D View to Single View
            else if (go.CompareTag("SingleView"))       // if tag of box is "SingleView"
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
            // In 3D View Trajectory Control, slide up is executed by PlayForward
            else if (go.CompareTag("PlayForward"))      // if tag of box is "PlayForward"
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
            // In 3D View Trajectory Control, slide down is executed by PlayBackward
            else if (go.CompareTag("PlayBackward"))     // if tag of box is "PlayBackward"
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

    // Execute to chnage scene
    public void TriggerSceneChange(string sceneName)
    {
        if (sceneController != null)
        {
            sceneController.GotoScene(sceneName);
        }
    }

    // Reset Gaze interaction
    void ResetProgress()
    {
        // Set gazing time 0s
        collisionDuration = 0.0f;
        if (imageTime != null)
        {
            // Set fill amout 0
            imageTime.fillAmount = 0.0f;
            // deactivate imageTime
            imageTime.gameObject.SetActive(false);
        }
        isColliding = false;
    }
}
