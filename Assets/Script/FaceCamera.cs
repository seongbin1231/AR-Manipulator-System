using UnityEngine;

/// <summary>
///     This script for moving canvas orientation
///     in 3D View adjusting to main camera
/// </summary>
[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour
{
    Transform cam;
    Vector3 targetAngle = Vector3.zero;
    void Start()
    {
        // get main camera
        cam = Camera.main.transform;
    }

    void Update()
    {
        // get current camera transform (translation and orientation)
        transform.LookAt(cam);

        // transform the cube angle
        targetAngle = transform.localEulerAngles;
        targetAngle.x = -targetAngle.x;
        targetAngle.y += 180;
        targetAngle.z = -targetAngle.z;

        // apply to cube orientation
        transform.localEulerAngles = targetAngle;
    }
}
