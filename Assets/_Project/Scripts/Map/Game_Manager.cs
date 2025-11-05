
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 게임의 전체적인 흐름을 관리하는 매니저 클래스입니다.
/// PunTurnManager와 연동하여 턴제 게임 로직을 처리합니다.
/// </summary>
public class Game_Manager
{
    [Header("게임 연출")]
    [SerializeField] private PlayableDirector entryCutsceneDirector;
    [SerializeField] private Src_GameCamControl gameCamControl;

    [Header("게임 시스템")]
    [SerializeField] private GameObject[] teamStonePrefabs = new GameObject[2]; // 0: 1P, 1: 2P 스톤 프리팹
    [SerializeField] private Transform stoneSpawnPoint; // 스톤 생성 위치
    [SerializeField] private int stoneCameraIndex = 1; // 스톤 추적 카메라의 인덱스



    private void Start()
    {



    }

    private IEnumerator SetupGame() //게임이 입장한 후에 동작 부분.
    {
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

    }

    public void OnTurnBegins(int turn) // 턴이 시작될때 호출됨
    {
        Debug.Log($"{turn}번째 턴 시작");

        // 모든 클라이언트가 현재 턴의 플레이어가 누구인지 확인합니다.
        int playerIndex = (turn - 1) % 2;

        // 로컬 플레이어가 현재 턴의 플레이어와 일치하는 경우에만 스톤을 생성합니다.
        {
            if (teamStonePrefabs[playerIndex] != null && stoneSpawnPoint != null)
            {

                // StoneShoot 스크립트에 제어할 스톤을 알려줍니다.
                //GetComponent<StoneShoot>().PrepareForShot(stone.GetComponent<Rigidbody>());

                // 모든 클라이언트에게 카메라를 전환하라는 RPC를 보냅니다.

            }
            else
            {
                Debug.LogError($"{playerIndex}P의 스톤 프리팹 또는 생성 위치가 할당되지 않았습니다.");
            }
        }
    }

    public void FocusCameraOnStone(int viewID)
    {


    }

    public void OnTurnCompleted(int turn)
    {
        // 1v1 게임에서는 OnPlayerFinished에서 턴 넘김을 처리하므로 이 콜백은 비워둡니다.
    }





    private IEnumerator BeginNextTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

}