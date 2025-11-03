using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    // Additive 쓰기 위한 값
    private const byte EV_LOAD_ADD = 101;
    private string bootstrapSceneName = "";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        
        if(string.IsNullOrEmpty(bootstrapSceneName))
            bootstrapSceneName = SceneManager.GetActiveScene().name;
    }
    
    // 로컬
    public void LoadLocal(string sceneName, bool wait = false, Action onFinish = null)
    {
        print("Loading local scene: " + sceneName);
        UIManager.Instance?.ShowLoading("Loading...");
        StartCoroutine(LoadRoutine(sceneName, wait, onFinish));
    }

    private IEnumerator LoadRoutine(string sceneName, bool wait = false, Action onFinish = null)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;
        
        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid()) SceneManager.SetActiveScene(newScene);

        yield return null;
        yield return null;
        
        // 3) 부트스트랩/타겟 외 모든 씬 언로드
        //print($"{sceneName} loadedaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        yield return UnloadOthersExcept(bootstrapSceneName, sceneName);
        //print("UnloadOthersExcept가 끝남");

        //yield return null;
        //yield return new WaitForSeconds(10f);

        if (!wait)
        {
            //Debug.Log("wait가 false");
            UIManager.Instance?.HideLoading();
        }
        
        //Debug.Log($"Loading scene: {sceneName}");
        onFinish?.Invoke();
    } 
    
    
    // 기존 포톤 이벤트
    public void LoadAdditiveForAll(string sceneName)
    {
        /*if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
        {
            //Debug.LogWarning("[SceneLoader] Only MasterClient can call LoadForAll inside a room.");
            return;
        }

        UIManager.Instance?.ShowLoading("Sync loading...");
        
        bool prev = PhotonNetwork.AutomaticallySyncScene;
        PhotonNetwork.AutomaticallySyncScene = false;
        
        var content = new object[] { sceneName };
        var options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EV_LOAD_ADD, content, options, SendOptions.SendReliable);
        
        PhotonNetwork.AutomaticallySyncScene = prev;*/
    }
    
    // 방장이외의 다른사람들 이벤트
    /*private void OnPhotonEvent(EventData ev)
    {
        if (ev.Code != EV_LOAD_ADD) return;
    
        //Debug.Log("OnPhotonEvent 실행");
        string sceneName = (string)((object[])ev.CustomData)[0];
        //Debug.Log($"OnPhotonEvent에서 실행하는 sceneName : {sceneName}");
        
        UIManager.Instance?.ShowLoading("loading...");
        StartCoroutine(LoadRoutine(sceneName));
    }*/
    
    private IEnumerator UnloadOthersExcept(string keepA, string keepB)
    {
        var keep = new HashSet<string> { keepA, keepB };
        
        int count = SceneManager.sceneCount;
        var scenes = new List<Scene>(count);
        for (int i = 0; i < count; i++) scenes.Add(SceneManager.GetSceneAt(i));

        //print($"SceneCount: {scenes.Count}");
        foreach (var sc in scenes)
        {
            if (!sc.IsValid() || !sc.isLoaded)
            { 
                //Debug.Log($"[SceneLoader] Unload: {sc.name}");
                continue;
            }

            if (keep.Contains(sc.name))
            {
                //Debug.Log($"[SceneLoader] Unload: {sc.name}");
                continue;
            }
            
            //Debug.Log($"[SceneLoader] Unload: {sc.name}");
            var op = SceneManager.UnloadSceneAsync(sc);
            while (op != null && !op.isDone) yield return null;
        }
        yield return null;
        yield return null;
    }

}
