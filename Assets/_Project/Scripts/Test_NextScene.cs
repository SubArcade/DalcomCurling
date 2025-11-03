using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_NextScene : MonoBehaviour
{
    public Button button;
    
    void Awake()
    {
        button.onClick.AddListener(() => SceneLoader.Instance.LoadLocal(GameManager.Instance.gameSceneName));
    }


}
