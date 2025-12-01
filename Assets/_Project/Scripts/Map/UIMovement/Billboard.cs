using UnityEngine;

/// <summary>
/// 항상 메인 카메라를 바라보게 하는 빌보드 효과를 적용하는 스크립트
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 카메라가 할당되었는지 확인
        if (mainCamera != null)
        {
            // 이 게임 오브젝트의 트랜스폼이 카메라의 트랜스폼을 마주보게 함
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
}
