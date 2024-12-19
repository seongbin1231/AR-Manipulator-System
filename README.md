# Five Guys Unity 로봇 제어 프로젝트

## 개요

로봇을 실제 환경에서 실험하기 전에 동작과 작업 영역을 확인하고 충돌 사고를 예방하기 위해 시뮬레이션 테스트가 필요하다. 이 프로젝트는 AR 기술을 활용하여 가상 실험 환경을 구축하고, 로봇을 실제 환경에서 실험하기 전에 증강현실로 미리 테스트하여 사고를 예방하는 시스템이다. 기존 시뮬레이터의 단점을 보완하여, 스마트폰 카메라로 3D-Depth 맵을 생성하고 사용자가 직접 맵을 구현할 필요가 없다. 사용자는 AR로 로봇을 제어하고, VR box를 통해 움직임을 확인하며, 충돌 발생 시 위치 정보를 제공받는다.

## 씬(Scene) 구성

### 1. MeshMapping Scene

- AR 기반 실시간 3D 맵 생성
  
- 메시 저장 및 가시화
  
- 메시 시각화 방식 선택 기능
  
- 3D 맵 데이터 저장
  
### 2. JointControl Scene

- 개별 관절 직접 제어
  
- 실시간 충돌 감지 및 시각화
  
- End-Effector 위치 마커 표시
  
### 3. MotionView Scene

- 로봇 궤적 반복 재생
  
- 실시간 충돌 감지 및 시각화
  
- End-Effector 위치 마커 표시
  
- ROS 토픽 기반 관절 제어
  
### 3. TrajectoryControl Scene

- 저장된 로봇 궤적 재생
  
- 슬라이더 기반 궤적 제어
  
- 3D 뷰 모드 지원
  
- 실시간 충돌 감지
  
### 4. 3D View Mode (MotionView, TrajectoryControl 씬에서 사용 가능)

- 3D 뷰 모드 렌더링
  
- 시선 기반 UI
  
  
## 주요 기능

### AR 및 이미지 트래킹(Placing Scene)

- AR Foundation을 활용한 이미지 트래킹 구현
  
- 로봇 베이스 자세 수동 설정
  
- 이미지 자세로 로봇 베이스 자세 업데이트
  
### 3D 맵 관리(Meshing Scene)

- 메싱 작업을 통한 실시간 3D 맵 생성 및 렌더링
  
- 메시 시각화 방식 변경 가능
  
- 메시 데이터 저장 및 로드
  
### 충돌 감지(JointControl, MotionView, TrajectoryControl Scene)

- 로봇과 3D 맵 간의 충돌 감지
  
- 충돌 발생 위치 및 충돌이 발생한 로봇 링크에 대한 시각적 피드백 제공
  
- 충돌 정보 저장 및 관리
  
### 로봇 제어(JointControl, MotionView, TrajectoryControl Scene)

- Forward/Inverse Kinematics 구현
  
- 관절 위치 및 속도 제어
  
- 궤적 재생 및 제어
  
### 3D 뷰 상호작용(MotionView, TrajectoryControl Scene)

- 시선 기반 UI (Gaze Interaction)
  
- 3D/2D 뷰 전환
  
  
## 프로젝트 구조

```
Assets/Script/
├── ARPlaceOnPlane.cs        # AR 로봇 배치 제어
├── ArticulationCollision.cs  # 충돌 감지
├── BoundsVisualizer.cs      # 메시 경계 시각화
├── CardboardRenderer.cs     # 3D 뷰 렌더링
├── Controller/              # 로봇 제어 관련
│   ├── Controller.cs        # 메인 제어기
│   ├── FKRobot.cs          # Forward Kinematics
│   ├── IKRobot.cs          # Inverse Kinematics
│   └── JointControl.cs      # 관절 제어
├── GazeScene.cs            # 시선 기반 상호작용
└── MeshFunction.cs         # 메시 맵 관리
```

## 설치 및 실행

1. Unity 2020.3 이상 버전 필요
   
2. AR Foundation 패키지 설치
   
3. ROS-TCP-Connector 패키지 설치
   
4. URDF-Importer 패키지 설치
   
6. Niantic Lightship ARDK 패키지 설치
 
## 의존성

- Unity 2020.3+
  
- AR Foundation
  
- ROS-TCP-Connector
  
- URDF-Importer

- Niantic Lightship ARDK
