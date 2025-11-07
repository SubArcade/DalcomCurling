using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;

/// <summary>
/// 게임 내 가상 카메라(Cinemachine)를 배열로 관리하고 제어하는 스크립트입니다.
/// SwitchCamera 메서드를 호출해 인덱스를 통해 특정 카메라를 활성화하고 타겟을 지정할 수 있다.
/// </summary>
public class Src_GameCamControl : MonoBehaviour
{
    [Header("관리할 가상 카메라 배열")]
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;

    [Header("게임 시작 타임라인")]
    [SerializeField] private PlayableDirector startTimeline;

    private const int ACTIVE_PRIORITY = 15;
    private const int INACTIVE_PRIORITY = 10;

    private void Start()
    {
        // 시작 시 첫 번째 카메라를 기본 활성 카메라로 설정
        SwitchCamera(0);
    }

    /// <summary>
    /// 게임 시작 타임라인을 재생합니다. (콜백 없음)
    /// </summary>
    public void PlayStartTimeline()
    {
        if (startTimeline != null)
        {
            startTimeline.Play();
        }
        else
        {
            Debug.LogWarning("시작 타임라인이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 지정된 인덱스의 가상 카메라를 활성화하고, 선택적으로 타겟을 설정.
    /// </summary>
    /// <param name="cameraIndex">활성화할 카메라의 배열 인덱스</param>
    /// <param name="followTarget">Follow 타겟으로 설정할 Transform</param>
    /// <param name="lookAtTarget">LookAt 타겟으로 설정할 Transform</param>
    public void SwitchCamera(int cameraIndex, Transform followTarget = null, Transform lookAtTarget = null)
    {
        if (virtualCameras == null || virtualCameras.Length == 0)
        {
            Debug.LogError("가상 카메라 배열이 비어있습니다!");
            return;
        }

        if (cameraIndex < 0 || cameraIndex >= virtualCameras.Length)
        {
            Debug.LogWarning($"카메라 인덱스 {cameraIndex}가 범위를 벗어났습니다.");
            return;
        }

        // 모든 카메라의 우선순위를 기본값으로 초기화
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] != null)
            {
                virtualCameras[i].Priority = INACTIVE_PRIORITY;
                virtualCameras[i].Follow = null;
                virtualCameras[i].LookAt = null;
            }
        }

        // 선택된 카메라에 타겟 설정 및 활성화
        var targetCamera = virtualCameras[cameraIndex];
        if (targetCamera != null)
        {
            targetCamera.Follow = followTarget;
            targetCamera.LookAt = lookAtTarget;
            targetCamera.Priority = ACTIVE_PRIORITY;
        }
    }
}