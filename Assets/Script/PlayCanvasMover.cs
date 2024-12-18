using UnityEngine;

/// <summary>
///     This class move play canvas in 3D View
/// </summary>
public class PlayCanvasMover : MonoBehaviour
{
    public GameObject canvas;
    public GameObject objectPrefab;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private float nodThreshold = 3.0f;
    private bool isIncreasing = false;
    private float previousRoll;
    private float lastDirectionChangeTime;
    private const float MAX_NOD_DURATION = 0.5f;

    // Init current state
    void Start()
    {
        previousRoll = GetRoll();
        lastDirectionChangeTime = Time.time;
        MovePanelToCameraFront();
    }

    // Track current state and descision move or not
    void Update()
    {
        float currentRoll = GetRoll();
        float rollChange = currentRoll - previousRoll;

        if (!isIncreasing && rollChange > nodThreshold)
        {
            isIncreasing = true;
            lastDirectionChangeTime = Time.time;
        }
        else if (isIncreasing && rollChange < -nodThreshold)
        {
            // Check rotation change occur
            float duration = Time.time - lastDirectionChangeTime;
            if (duration <= MAX_NOD_DURATION)
            {
                MovePanelToCameraFront();
            }
            isIncreasing = false;
            lastDirectionChangeTime = Time.time;
        }

        previousRoll = currentRoll;
    }

    // Panel position update
    void MovePanelToCameraFront()
    {
        if (canvas == null)
        {
            Debug.LogWarning("Canvas가 할당되지 않았습니다.");
            return;
        }

        canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3.0f;
    }

    // Get pitch from main camera (z axis)
    float GetRoll()
    {
        return Camera.main.transform.eulerAngles.y;
    }
}
