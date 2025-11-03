using UnityEngine;

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
