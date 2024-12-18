using UnityEngine;

/// <summary>
///     This script for moving cube orientation
///     in 3D View adjusting to main camera
/// </summary>
[ExecuteInEditMode]
public class CubeFaceCamera : MonoBehaviour
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
        targetAngle.x = 0;
        targetAngle.y += 180;
        targetAngle.z = 0;

        // apply to cube orientation
        transform.localEulerAngles = targetAngle;
    }
}
