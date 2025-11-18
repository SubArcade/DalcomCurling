using UnityEngine;

public class SpectatorSound : MonoBehaviour
{
    void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySpectatorCheer(transform.position);
        }
    }

    void OnDisable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSpectatorCheer();
        }
    }
}
