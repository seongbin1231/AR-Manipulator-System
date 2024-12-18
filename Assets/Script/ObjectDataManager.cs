using UnityEngine;

/// <summary>
///     This script manages object transform data by storing and retrieving
///     position and rotation information. It provides static methods
///     to save and access object transform data.
/// </summary>

public static class ObjectDataManager
{
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;
    private static bool isObjectSaved = false;

    // Save data
    public static void SetObjectData(Vector3 position, Quaternion rotation)
    {
        savedPosition = position;
        savedRotation = rotation;
        isObjectSaved = true;
        Debug.Log($"Object data saved - Position: {position}, Rotation: {rotation}");
    }

    // Get saved position
    public static Vector3 GetSavedPosition()
    {
        return savedPosition;
    }

    // Get saved rotation
    public static Quaternion GetSavedRotation()
    {
        return savedRotation;
    }

    // Check if object is saved
    public static bool IsObjectSaved()
    {
        return isObjectSaved;
    }
}
