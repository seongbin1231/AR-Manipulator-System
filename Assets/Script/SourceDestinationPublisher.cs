using System;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using RosMessageTypes.Std;

public class SourceDestinationPublisher : MonoBehaviour
{
    const int k_NumRobotJoints = 6;

    public static readonly string[] LinkNames =
        { "base_link/link1", "/link2", "/link3", "/link4", "/link5", "/link6" };

    // Variables required for ROS communication
    [SerializeField]
    string m_TopicName = "/robot_joints";

    [SerializeField]
    GameObject m_Robot;
    [SerializeField]
    // GameObject m_Target;
    // [SerializeField]
    // GameObject m_TargetPlacement;
    // readonly Quaternion m_PickOrientation = Quaternion.Euler(90, 90, 0);

    // Robot Joints
    UrdfJointRevolute[] m_JointArticulationBodies;

    // ROS Connector
    ROSConnection m_Ros;

    void Start()
    {
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<Float32MultiArrayMsg>(m_TopicName);

        m_JointArticulationBodies = new UrdfJointRevolute[k_NumRobotJoints];

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
        if (MeshMapCollisionDetector.isCollision)
        {
            Publish();
        }
    }

    public void Publish()
    {
        var sourceDestinationMessage = new Float32MultiArrayMsg();
        sourceDestinationMessage.data = new float[k_NumRobotJoints];

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            if (m_JointArticulationBodies[i] == null)
            {
                Debug.LogError($"m_JointArticulationBodies[{i}] is null");
                continue;
            }
            // GetPosition()을 호출하기 전에 Joint의 이름과 위치를 출력
                string jointName = m_JointArticulationBodies[i].name;
                float position = m_JointArticulationBodies[i].GetPosition();
                Debug.Log($"Joint {i}: Name = {jointName}, Position = {position}");
            sourceDestinationMessage.data[i] = m_JointArticulationBodies[i].GetPosition();
        }

        // // Pick Pose
        // sourceDestinationMessage.pick_pose = new PoseMsg
        // {
        //     position = m_Target.transform.position.To<FLU>(),
        //     orientation = Quaternion.Euler(90, m_Target.transform.eulerAngles.y, 0).To<FLU>()
        // };

        // // Place Pose
        // sourceDestinationMessage.place_pose = new PoseMsg
        // {
        //     position = m_TargetPlacement.transform.position.To<FLU>(),
        //     orientation = m_PickOrientation.To<FLU>()
        // };

        // Finally send the message to server_endpoint.py running in ROS
        m_Ros.Publish(m_TopicName, sourceDestinationMessage);
        Debug.Log("dagfsdbf");
    }
}
