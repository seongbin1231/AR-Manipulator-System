using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System;

/// <summary>
///     This script track image and set robot
///     pose updating step. If it can track image,
///     image returns position and orientation
/// </summary>
public class ImageTrackingHandler : MonoBehaviour
{
    // Set image used for tracking
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    // Event provide image position and orientation
    public event Action<Vector3, Quaternion> OnPoseUpdated;

    [SerializeField]
    private Button toggleButton; // Reference button (robot pose updating step)
    [SerializeField]
    private Color greenColor = Color.green; // Set green color
    [SerializeField]
    private Color redColor = Color.red; // Set red color

    private bool isButtonPressed = false; // Button status

    // Execute when image tracked
    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    // Execute when image cannot be tracked
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Execute when button down
    public void OnButtonDown()
    {
        isButtonPressed = !isButtonPressed;
        UpdateButtonColor();
    }

    // For each trackedImage, update pose
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

    // Update position and orientation set by trackedImage
    private void UpdatePose(ARTrackedImage trackedImage)
    {
        if (!isButtonPressed) return;

        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;

        OnPoseUpdated?.Invoke(position, rotation);

        Debug.Log($"이미지 위치: {position}, 이미지 회전: {rotation}");
    }

    // Update button color when button input occurs
    private void UpdateButtonColor()
    {
        Image buttonImage = toggleButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isButtonPressed ? greenColor : redColor;
        }
    }
}
