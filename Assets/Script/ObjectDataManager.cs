using UnityEngine;

public static class ObjectDataManager
{
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;
    private static bool isObjectSaved = false;

    // 데이터 저장
    public static void SetObjectData(Vector3 position, Quaternion rotation)
    {
        savedPosition = position;
        savedRotation = rotation;
        isObjectSaved = true;
        Debug.Log($"Object 데이터 저장 완료 - Position: {position}, Rotation: {rotation}");
    }

    // 저장된 위치 가져오기
    public static Vector3 GetSavedPosition()
    {
        return savedPosition;
    }

    // 저장된 회전값 가져오기
    public static Quaternion GetSavedRotation()
    {
        return savedRotation;
    }

    // 저장 여부 확인
    public static bool IsObjectSaved()
    {
        return isObjectSaved;
    }
}
