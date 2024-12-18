using UnityEngine;

/// <summary>
///     This class move panel in 3D View
/// </summary>
public class PanelMover : MonoBehaviour
{
    public GameObject canvas;
    private float nodThreshold = 60.0f;
    private bool isIncreasing = false;
    private float previousPitch;
    private float lastDirectionChangeTime;
    private const float MAX_NOD_DURATION = 0.5f;

    // Init current state
    void Start()
    {
        previousPitch = GetPitch();
        lastDirectionChangeTime = Time.time;
    }

    // Track current state and descision move or not
    void Update()
    {
        float currentPitch = GetPitch();
        float pitchChange = currentPitch - previousPitch;

        if (!isIncreasing && pitchChange > nodThreshold)
        {
            isIncreasing = true;
            lastDirectionChangeTime = Time.time;
        }
        else if (isIncreasing && pitchChange < -nodThreshold)
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

        previousPitch = currentPitch;
    }

    // Panel position update
    void MovePanelToCameraFront()
    {
        if (canvas == null)
        {
            return;
        }

        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 6.0f;
        canvas.transform.position = newPosition;
    }

    // Get pitch from main camera (x axis)
    float GetPitch()
    {
        return Camera.main.transform.eulerAngles.x;
    }
}
