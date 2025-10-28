using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // if (SceneManager.GetActiveScene().name != "MainMenuScene")
        //     SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Additive);
    }
}
