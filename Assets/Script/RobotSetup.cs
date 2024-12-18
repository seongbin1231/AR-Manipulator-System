using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter;

/// <summary>
///     This class setup the robot settings
/// </summary>
public class RobotSetup : MonoBehaviour
{
    // Init settings of robot links
    void Start()
    {
        SetupAllRobotLinks();
    }

    void SetupAllRobotLinks()
    {
        // Find robot object
        GameObject robotRoot = GameObject.Find("sgr532");
        if (robotRoot == null)
        {
            return;
        }

        // Set base link
        Transform baseLink = robotRoot.transform.Find("base_link");
        if (baseLink != null)
        {
            SetupCollisionDetection(baseLink.gameObject, "sgr532/base_link");

            // Find link1 to link6 and set
            Transform currentLink = baseLink;
            for (int i = 1; i <= 6; i++)
            {
                Transform nextLink = currentLink.Find($"link{i}");
                if (nextLink != null)
                {
                    SetupCollisionDetection(nextLink.gameObject, $"sgr532/link{i}");
                    currentLink = nextLink;
                }
            }

            // Set gripper link
            if (currentLink != null)
            {
                Transform gripperLeft = currentLink.Find("link_gripper_left");
                Transform gripperRight = currentLink.Find("link_gripper_right");

                // gripperLeft and gripperRight is children of link6
                if (gripperLeft != null)
                {
                    SetupCollisionDetection(gripperLeft.gameObject, "sgr532/link_gripper_left");
                }

                if (gripperRight != null)
                {
                    SetupCollisionDetection(gripperRight.gameObject, "sgr532/link_gripper_right");
                }
            }
        }
    }

    // Set for collision detection each link
    void SetupCollisionDetection(GameObject linkObject, string tagName)
    {
        // Robot link layer
        int robotLinkLayer = LayerMask.NameToLayer("RobotLink");

        // Assign RobotLink Layer except for base link
        if (tagName != "sgr532/base_link")
        {
            linkObject.layer = robotLinkLayer;
        }

        // Set ArticulationBody
        ArticulationBody artBody = linkObject.GetComponent<ArticulationBody>();
        if (artBody == null)
        {
            artBody = linkObject.AddComponent<ArticulationBody>();
        }

        // Set ArticulationBody collsion mode 
        artBody.useGravity = false;
        // artBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        artBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        // artBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        // artBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // Set ArticulationoBody
        artBody.matchAnchors = true;
        artBody.jointFriction = 0.01f;
        artBody.angularDamping = 0.01f;
        artBody.linearDamping = 0.01f;

        // Set ArticulationoBody xDrive
        var drive = artBody.xDrive;
        drive.stiffness = 10000;
        drive.damping = 100;
        drive.forceLimit = 1000;
        artBody.xDrive = drive;

        // Find Collisions
        Transform collisionsTransform = linkObject.transform.Find("Collisions/unnamed");
        if (collisionsTransform != null)
        {
            Transform meshObjectTransform;
            if (linkObject.name == "base_link")
            {
                meshObjectTransform = collisionsTransform.Find($"sagittarius_{linkObject.name}/sagittarius_{linkObject.name}_0");
            }
            else
            {
                meshObjectTransform = collisionsTransform.Find($"{linkObject.name}/{linkObject.name}_0");
            }

            if (meshObjectTransform != null)
            {
                GameObject meshObject = meshObjectTransform.gameObject;

                // Set layer and tag
                meshObject.layer = robotLinkLayer;
                meshObject.tag = tagName;

                // Set MeshCollider
                MeshCollider[] existingColliders = meshObject.GetComponents<MeshCollider>();
                if (existingColliders.Length == 0)
                {
                    MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        MeshCollider collider = meshObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = meshFilter.sharedMesh;
                        collider.convex = true;
                        collider.isTrigger = true;
                        collider.enabled = true;
                    }
                }
            }
        }
    }
}