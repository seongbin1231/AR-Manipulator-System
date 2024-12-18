using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

/// <summary>
///     This script publishes a boolean robot control flag
///     to a ROS topic when triggered by UI interaction.
/// </summary>
public class CtrlFlagPublisher : MonoBehaviour
{
    // Set ROS topic name
    [SerializeField]
    private string m_TopicName = "/control_flag";

    // Variable for ROS connection
    private ROSConnection m_Ros;

    void Start()
    {
        // Get ROS connection instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        // Register publisher with Bool message type(to control real robot)
        m_Ros.RegisterPublisher<BoolMsg>(m_TopicName);
    }

    // Function to be called from UI button
    public void PublishControlFlag()
    {
        var flagMessage = new BoolMsg
        {
            data = true
        };

        // Publish message to ROS
        m_Ros.Publish(m_TopicName, flagMessage);
        Debug.Log("Control flag published: " + flagMessage.data);
    }
}
