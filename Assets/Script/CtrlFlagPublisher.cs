using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

public class CtrlFlagPublisher : MonoBehaviour
{
    // ROS 토픽 이름 설정
    [SerializeField]
    private string m_TopicName = "/control_flag";

    // ROS 연결을 위한 변수
    private ROSConnection m_Ros;

    void Start()
    {
        // ROS 연결 인스턴스 가져오기
        m_Ros = ROSConnection.GetOrCreateInstance();
        // Bool 메시지 타입의 퍼블리셔 등록
        m_Ros.RegisterPublisher<BoolMsg>(m_TopicName);
    }

    // UI 버튼에서 호출할 함수
    public void PublishControlFlag()
    {
        var flagMessage = new BoolMsg
        {
            data = true
        };

        // ROS로 메시지 발행
        m_Ros.Publish(m_TopicName, flagMessage);
        Debug.Log("Control flag published: " + flagMessage.data);
    }
}
