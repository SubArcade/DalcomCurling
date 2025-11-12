using FMODUnity;
using UnityEditor.Timeline;
using UnityEngine;

public class FMODSound : MonoBehaviour
{
    [SerializeField] private EventReference backgroundMusicEvent;
    private FMOD.Studio.EventInstance musicInstance;

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        musicInstance = RuntimeManager.CreateInstance(backgroundMusicEvent);

        musicInstance.start();
    }
}