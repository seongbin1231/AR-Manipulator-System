using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System;

public class ImageTrackingHandler : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    // 위치와 회전을 전달하는 이벤트
    public event Action<Vector3, Quaternion> OnPoseUpdated;

    [SerializeField]
    private Button toggleButton; // 버튼 참조
    [SerializeField]
    private Color greenColor = Color.green; // 초록색
    [SerializeField]
    private Color redColor = Color.red; // 빨간색

    private bool isButtonPressed = false; // 버튼 상태

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    public void OnButtonDown()
    {
        isButtonPressed = !isButtonPressed;
        UpdateButtonColor(); // 버튼 색상 업데이트
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdatePose(trackedImage);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdatePose(trackedImage);
            }
        }
    }

    private void UpdatePose(ARTrackedImage trackedImage)
    {
        if (!isButtonPressed) return; // 버튼이 눌리지 않으면 리턴

        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;

        // 이벤트 호출
        OnPoseUpdated?.Invoke(position, rotation);

        Debug.Log($"이미지 위치: {position}, 이미지 회전: {rotation}");
    }

    private void UpdateButtonColor()
    {
        // 버튼의 Image 컴포넌트 색상 변경
        Image buttonImage = toggleButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isButtonPressed ? greenColor : redColor;
        }
    }
}
