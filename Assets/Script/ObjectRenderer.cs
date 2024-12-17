using UnityEngine;
using System.Collections;

public class ObjectRenderer : MonoBehaviour
{
    public GameObject objectPrefab; // Inspector에서 할당할 프리팹

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Start()
    {
        if (ObjectDataManager.IsObjectSaved())
        {
            // 저장된 데이터로 오브젝트 생성
            savedPosition = ObjectDataManager.GetSavedPosition();
            savedRotation = ObjectDataManager.GetSavedRotation();

            ArticulationBody BaseLinkArticulationBody = objectPrefab.GetComponent<ArticulationBody>();
            BaseLinkArticulationBody.TeleportRoot(savedPosition, savedRotation);
            Debug.Log($"Object가 생성되었습니다 - Position: {savedPosition}, Rotation: {savedRotation}");

            // 코루틴 시작
            // StartCoroutine(UpdateObjectTransform());
        }

        else
        {
            Debug.LogWarning("저장된 Object 데이터가 없습니다.");
        }
    }

    void Update()
    {
        // ArticulationBody BaseLinkArticulationBody = objectPrefab.GetComponent<ArticulationBody>();
        // BaseLinkArticulationBody.TeleportRoot(savedPosition, savedRotation);
    }
}
