using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;  // AR 관련 네임스페이스 추가

public class CardboardController : MonoBehaviour
{
    private Canvas canvas1;
    private Canvas canvas2;
    private Vector3 initialPosition1;
    private Vector3 initialPosition2;
    private RenderMode initialRenderMode1;
    private RenderMode initialRenderMode2;
    private CardboardRenderer cardboardRenderer;
    private ARCameraManager arCameraManager;  // AR 카메라 매니저 추가
    private Camera arCamera;  // AR 카메라 참조 추가

    // 현재 상태를 추적하는 flag 추가
    private bool isInitialState = true;

    void Start()
    {
        // AR 카메라 매니저�� 카메라 찾기
        arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCameraManager != null)
        {
            arCamera = arCameraManager.GetComponent<Camera>();
        }

        // "Canvas"와 "CanvasGaze" 자식 객체 찾기
        canvas1 = transform.Find("CanvasCardboard").GetComponent<Canvas>();
        canvas2 = transform.Find("CanvasGaze").GetComponent<Canvas>();

        // 초기 설정 저장
        initialPosition1 = canvas1.transform.position;
        initialPosition2 = canvas2.transform.position;
        initialRenderMode1 = canvas1.renderMode;
        initialRenderMode2 = canvas2.renderMode;

        // CardboardRenderer 컴포넌�� 찾기
        cardboardRenderer = GetComponentInChildren<CardboardRenderer>();
        if (cardboardRenderer == null)
        {
            cardboardRenderer = FindObjectOfType<CardboardRenderer>();
        }

        if (cardboardRenderer == null)
        {
            Debug.LogError("CardboardRenderer를 찾을 수 없습니다. CardboardRenderer가 씬에 존재하는지 확인해주세요.");
            return;
        }

        // 시작 시 CardboardRenderer를 비활성화
        cardboardRenderer.enabled = false;
        isInitialState = false;  // 초기 상태를 false로 설정
    }

    public void CanvasControl()
    {
        if (!isInitialState)  // 상태 체크 조건 반전
        {
            // 초기 설정으로 복원
            canvas1.renderMode = initialRenderMode1;
            canvas1.transform.position = initialPosition1;

            canvas2.renderMode = initialRenderMode2;
            canvas2.transform.position = initialPosition2;

            EnableCardboardRenderer();
            isInitialState = true;
        }
        else
        {
            // World Space로 변경
            canvas1.renderMode = RenderMode.WorldSpace;
            canvas2.renderMode = RenderMode.WorldSpace;

            // 위치를 (10, 10, 10)으로 변경
            canvas1.transform.position = new Vector3(10, 10, 10);
            canvas2.transform.position = new Vector3(10, 10, 10);

            DisableCardboardRenderer();
            isInitialState = false;
        }

        Debug.Log($"Canvas State Changed - IsInitialState: {isInitialState}");
    }

    // 현재 상태를 확인할 수 있는 public 프로퍼티 추가
    public bool IsInInitialState
    {
        get { return isInitialState; }
    }

    // public void ToggleCardboardRenderer()
    // {
    //     if (cardboardRenderer != null)
    //     {
    //         // enabled 속성으로 컴포넌트 활성화/비활성화
    //         cardboardRenderer.enabled = !cardboardRenderer.enabled;
    //         Debug.Log($"CardboardRenderer {(cardboardRenderer.enabled ? "활성화" : "비활성화")} 됨");
    //     }
    // }

    // 직접 활성화/비활성화하는 메서드
    public void EnableCardboardRenderer()
    {
        if (cardboardRenderer != null)
        {
            cardboardRenderer.enabled = true;
            Debug.Log("CardboardRenderer 활성화됨");
        }
    }

    public void DisableCardboardRenderer()
    {
        if (cardboardRenderer != null)
        {
            cardboardRenderer.enabled = false;
        }
    }
}
