using System.Collections;
using UnityEngine;
using TMPro;

public class Scr_EnergyTimerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnergyRegenNotifier notifier;
    [SerializeField] private TMP_Text nextEnergyText;

    [Header("업데이트 주기(초)")]
    [SerializeField] private float uiUpdateInterval = 1f;

    private void Awake()
    { 
        //notifier = FindObjectOfType<EnergyRegenNotifier>();
    }
    
    void OnDisable() => DataManager.Instance.PauseChanged -= pauseChanged;

    private void pauseChanged(bool paused)
    {
        UpdateTexts();
    }
    
    private void OnEnable()
    {
        DataManager.Instance.PauseChanged += pauseChanged;
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        var wait = new WaitForSeconds(uiUpdateInterval);

        while (true)
        {
            UpdateTexts();
            yield return wait;
        }
    }
    
    private void UpdateTexts()
    {
        if (!notifier) return;

        int secToNext = notifier.GetSecondsToNextEnergy();

        if (secToNext == 0)
        {
            nextEnergyText.text = "가득 참!";
            if (DataManager.Instance.PlayerData.energy < DataManager.Instance.PlayerData.maxEnergy)
            {
                notifier.LazyRegen();
                DataManager.Instance.EnergyChange(DataManager.Instance.PlayerData.energy);
            }
        }
        else
        {
            nextEnergyText.text = FormatTime(secToNext);
        }
        
    }

    private string FormatTime(int totalSeconds)
    {
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m:00}:{s:00}";
    }
}