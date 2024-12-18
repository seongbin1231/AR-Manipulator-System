using System;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using RosMessageTypes.Std;

/// <summary>
///     This script reads the robot joint positions and
///     publishes them to ROS topic as Float32MultiArray message.
///     It monitors joint states and publishes when collision is detected.
/// </summary>
public class SourceDestinationPublisher : MonoBehaviour
{
    // Constants for the number of robot joints
    const int k_NumRobotJoints = 6;

    // Array of link names for the robot joints
    public static readonly string[] LinkNames =
        { "base_link/link1", "/link2", "/link3", "/link4", "/link5", "/link6" };

    // ROS topic name for publishing joint states
    [SerializeField]
    string m_TopicName = "/robot_joints";

    // Reference to the robot GameObject
    [SerializeField]
    GameObject m_Robot;
    [SerializeField]
    // GameObject m_Target;
    // [SerializeField]
    // GameObject m_TargetPlacement;
    // readonly Quaternion m_PickOrientation = Quaternion.Euler(90, 90, 0);

    // Array to store joint components
    UrdfJointRevolute[] m_JointArticulationBodies;

    // ROS connection instance
    ROSConnection m_Ros;

    void Start()
    {
        // Initialize ROS connection and register publisher
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<Float32MultiArrayMsg>(m_TopicName);

        // Initialize joint array
        m_JointArticulationBodies = new UrdfJointRevolute[k_NumRobotJoints];

        // Find and store all joint components
        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += LinkNames[i];
            m_JointArticulationBodies[i] = m_Robot.transform.Find(linkName).GetComponent<UrdfJointRevolute>();
            Debug.Log("dagfsdbf");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Publish message when collision is detected
        if (MeshMapCollisionDetector.isCollision)
        {
            Publish();
        }
    }

    public void Publish()
    {
        // Create new message for joint states
        var sourceDestinationMessage = new Float32MultiArrayMsg();
        sourceDestinationMessage.data = new float[k_NumRobotJoints];

        // Get position of each joint and store in message
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            if (m_JointArticulationBodies[i] == null)
            {
                Debug.LogError($"m_JointArticulationBodies[{i}] is null");
                continue;
            }
            // Log joint name and position for debugging
            string jointName = m_JointArticulationBodies[i].name;
            float position = m_JointArticulationBodies[i].GetPosition();
            Debug.Log($"Joint {i}: Name = {jointName}, Position = {position}");
            sourceDestinationMessage.data[i] = position;
        }

        // Finally send the message to server_endpoint.py running in ROS
        m_Ros.Publish(m_TopicName, sourceDestinationMessage);
        Debug.Log("dagfsdbf");
    }
}
