using UnityEngine;

public class PanelMover : MonoBehaviour
{
    public GameObject canvas;
    private float nodThreshold = 60.0f;
    private bool isIncreasing = false;
    private float previousPitch;
    private float lastDirectionChangeTime; // 방향 전환 시간을 추적하기 위한 변수
    private const float MAX_NOD_DURATION = 0.5f; // 최대 허용 시간 (0.5초)

    void Start()
    {
        previousPitch = GetPitch();
        lastDirectionChangeTime = Time.time;
    }

    void Update()
    {
        float currentPitch = GetPitch();
        float pitchChange = currentPitch - previousPitch;

        if (!isIncreasing && pitchChange > nodThreshold)
        {
            isIncreasing = true;
            lastDirectionChangeTime = Time.time; // 위로 움직이기 시작한 시간 기록
        }
        else if (isIncreasing && pitchChange < -nodThreshold)
        {
            float duration = Time.time - lastDirectionChangeTime;
            if (duration <= MAX_NOD_DURATION) // 0.5초 이내에 완료된 경우에만
            {
                MovePanelToCameraFront();
            }
            isIncreasing = false;
            lastDirectionChangeTime = Time.time;
        }

        previousPitch = currentPitch;
    }

    void MovePanelToCameraFront()
    {
        if (canvas == null)
        {
            Debug.LogWarning("Canvas가 할당되지 않았습니다.");
            return;
        }

        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 6.0f;
        canvas.transform.position = newPosition;
        Debug.Log("Canvas가 앞으로 이동했습니다.");
    }

    float GetPitch()
    {
        return Camera.main.transform.eulerAngles.x;
    }
}
