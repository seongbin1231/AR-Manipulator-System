using UnityEngine;
using System.Collections;

/// <summary>
///     This script handles object instantiation and positioning
///     based on previously saved position and rotation data.
///     It manages the object's transform using ArticulationBody
///     for precise placement in the scene.
/// </summary>
public class ObjectRenderer : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab to be assigned in Inspector

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Start()
    {
        if (ObjectDataManager.IsObjectSaved())
        {
            // Create object with saved data
            savedPosition = ObjectDataManager.GetSavedPosition();
            savedRotation = ObjectDataManager.GetSavedRotation();

            ArticulationBody BaseLinkArticulationBody = objectPrefab.GetComponent<ArticulationBody>();
            BaseLinkArticulationBody.TeleportRoot(savedPosition, savedRotation);
            Debug.Log($"Object has been created - Position: {savedPosition}, Rotation: {savedRotation}");
        }
        else
        {
            Debug.LogWarning("No saved object data found.");
        }
    }
}
