using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;

/// <summary>
/// 컬링 스톤의 움직임을 제어하는 스크립트입니다.
/// 소유권을 가진 플레이어의 입력에 따라 한 번만 움직이고, 10초 후 턴을 종료합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class Move : MonoBehaviour
{
    public float forceAmount = 10f;
    private Rigidbody rb;
    private PhotonView photonView;
    private PunTurnManager turnManager;

    private bool hasBeenLaunched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        // 씬에 있는 PunTurnManager를 찾아서 할당합니다.
        turnManager = FindObjectOfType<PunTurnManager>();
    }

    void Update()
    {
        if (!photonView.IsMine || hasBeenLaunched)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchStone();
        }
    }

    private void LaunchStone()
    {
        hasBeenLaunched = true;
        rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);

        // 10초 후에 턴을 종료하고 객체를 파괴하는 코루틴을 시작합니다.
        StartCoroutine(EndTurnAfterDelay(10f));
    }

    /// <summary>
    /// 지정된 시간 후에 턴을 종료하고 이 게임 오브젝트를 파괴합니다.
    /// </summary>
    private IEnumerator EndTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 코루틴이 실행되는 동안 씬이 바뀌거나 객체가 파괴되었을 수 있으므로 확인합니다.
        if (this == null || !photonView.IsMine)
        {
            yield break;
        }

        Debug.Log("10초 경과. 턴을 종료하고 스톤을 파괴합니다.");

        // 현재 턴을 마쳤음을 알립니다.
        if (turnManager != null)
        {
            turnManager.SendMove(null, true);
        }

        // 네트워크상의 모든 클라이언트에서 이 스톤을 파괴합니다.
        PhotonNetwork.Destroy(gameObject);
    }
}
