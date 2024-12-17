using UnityEngine;

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

    void Start()
    {
        previousRoll = GetRoll();
        lastDirectionChangeTime = Time.time;

        MovePanelToCameraFront();
    }

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

    void MovePanelToCameraFront()
    {
        if (canvas == null)
        {
            Debug.LogWarning("Canvas가 할당되지 않았습니다.");
            return;
        }

        canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3.0f;
    }

    float GetRoll()
    {
        return Camera.main.transform.eulerAngles.y;  // z축 회전값(roll) 반환
    }
}
