using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables; 
using Photon.Pun.UtilityScripts; // IPunTurnManagerCallbacks를 사용하기 위해 추가

/// <summary>
/// 게임의 전체적인 흐름을 관리하는 매니저 클래스입니다.
/// PunTurnManager와 연동하여 턴제 게임 로직을 처리합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))] // RPC 사용을 위해 PhotonView 추가
public class Game_Manager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    [Header("게임 연출")]
    [SerializeField] private PlayableDirector entryCutsceneDirector;
    [SerializeField] private Src_GameCamControl gameCamControl;

    [Header("게임 시스템")]
    [SerializeField] private PunTurnManager turnManager;
    [SerializeField] private GameObject[] teamStonePrefabs = new GameObject[2]; // 0: 1P, 1: 2P 스톤 프리팹
    [SerializeField] private Transform stoneSpawnPoint; // 스톤 생성 위치
    [SerializeField] private int stoneCameraIndex = 1; // 스톤 추적 카메라의 인덱스



    private void Start()
    {


        if (turnManager != null)
        {
            turnManager.enabled = false;
            turnManager.TurnManagerListener = this; // 턴 매니저 리스너로 등록
        }

        StartCoroutine(SetupGame());
    }

    private IEnumerator SetupGame() //게임이 입장한 후에 동작 부분.
    {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount == 2);
        Debug.Log("모든 플레이어가 입장했습니다. 입장 연출을 시작합니다.");

        if (entryCutsceneDirector != null)
        {
            entryCutsceneDirector.Play();
            yield return new WaitUntil(() => entryCutsceneDirector.state != PlayState.Playing);
        }
        else
        {
            Debug.LogWarning("입장 컷씬이 설정되지 않았습니다. 2초 후 게임을 시작합니다.");
            yield return new WaitForSeconds(2f);
        }

        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("게임을 시작합니다! 턴 매니저를 활성화하고 첫 턴을 시작합니다.");
        if (turnManager != null)
        {
            turnManager.enabled = true;
            //마스터 클라이언트부터 턴 시작하도록 ( 시작 플레이어를 정하는 로직 추가 해야함 )
            if (PhotonNetwork.IsMasterClient)
            {
                turnManager.BeginTurn();
            }
        }
        else
        {
            Debug.LogError("PunTurnManager가 할당되지 않았습니다!");
        }
    }

    #region IPunTurnManagerCallbacks

    public void OnTurnBegins(int turn) // 턴이 시작될때 호출됨
    {
        Debug.Log($"{turn}번째 턴 시작");

        // 모든 클라이언트가 현재 턴의 플레이어가 누구인지 확인합니다.
        int playerIndex = (turn - 1) % 2; 
        Player activePlayer = PhotonNetwork.PlayerList[playerIndex];

        // 로컬 플레이어가 현재 턴의 플레이어와 일치하는 경우에만 스톤을 생성합니다.
        if (activePlayer == PhotonNetwork.LocalPlayer)
        {
            if (teamStonePrefabs[playerIndex] != null && stoneSpawnPoint != null)
            {
                // 자기 턴인 플레이어가 직접 스톤을 생성하여 자동으로 소유권을 가집니다.
                GameObject stone = PhotonNetwork.Instantiate(teamStonePrefabs[playerIndex].name, stoneSpawnPoint.position, stoneSpawnPoint.rotation);
                PhotonView stonePv = stone.GetComponent<PhotonView>();

                // StoneShoot 스크립트에 제어할 스톤을 알려줍니다.
                //GetComponent<StoneShoot>().PrepareForShot(stone.GetComponent<Rigidbody>());

                // 모든 클라이언트에게 카메라를 전환하라는 RPC를 보냅니다.
                photonView.RPC("FocusCameraOnStone", RpcTarget.All, stonePv.ViewID);
            }
            else
            {
                Debug.LogError($"{playerIndex}P의 스톤 프리팹 또는 생성 위치가 할당되지 않았습니다.");
            }
        }
    }

    [PunRPC] // 모든 클라이언트에서 실행될 카메라 포커싱 메서드
    public void FocusCameraOnStone(int viewID)
    {
        PhotonView stoneView = PhotonView.Find(viewID);
        if (stoneView != null && gameCamControl != null)
        {
            gameCamControl.SwitchCamera(stoneCameraIndex, stoneView.transform, stoneView.transform);
        }
        else
        {
            Debug.LogError($"ViewID {viewID}를 가진 스톤을 찾을 수 없거나 카메라 컨트롤러가 없습니다.");
        }
    }

    public void OnTurnCompleted(int turn)
    {
        // 1v1 게임에서는 OnPlayerFinished에서 턴 넘김을 처리하므로 이 콜백은 비워둡니다.
    }

    public void OnPlayerMove(Player player, int turn, object move) { }

    public void OnPlayerFinished(Player player, int turn, object move)
    {
        Debug.Log($"{player.NickName}이(가) {turn}번째 턴을 마쳤습니다.");

        // 마스터 클라이언트가 다음 턴을 시작합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(BeginNextTurnAfterDelay(0.5f)); //0.5f 딜레이를 주어 오류가생길 문제를 예방
        }
    }

    private IEnumerator BeginNextTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        turnManager.BeginTurn();
    }

    public void OnTurnTimeEnds(int turn) { }

    #endregion

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("플레이어 {0}가 입장했습니다.", newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("플레이어 {0}가 퇴장했습니다.", otherPlayer.NickName);
    }

    #endregion
}
