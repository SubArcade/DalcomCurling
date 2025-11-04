using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_Energy : MonoBehaviour
{
    public Button button;
    public TMP_Text text;

    void Awake()
    {
        button.onClick.AddListener(()=> testAdd());
        text.text = $"{DataManager.Instance.PlayerData.energy}/{DataManager.Instance.PlayerData.maxEnergy}";
    }

    void testAdd()
    {
        int energy = DataManager.Instance.PlayerData.energy + 1;
        DataManager.Instance.UpdateUserDataAsync(energy: energy);
        text.text = $"{energy}/{DataManager.Instance.PlayerData.maxEnergy}";
    }
}
