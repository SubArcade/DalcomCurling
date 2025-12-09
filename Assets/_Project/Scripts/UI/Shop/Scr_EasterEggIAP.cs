using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_EasterEggIAP : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject textobj;
    [SerializeField] private int count;

    void Awake()
    {
        count = 0;
        button.onClick.AddListener(() =>
        {
            count++;
            Debug.Log($"click button : {count}");
            if (count >= 7)
            {
                GameManager.Instance.isEasterEgg = !GameManager.Instance.isEasterEgg;
                textobj.SetActive(GameManager.Instance.isEasterEgg);
                count = 0;
            }
        });
    }

    private void OnEnable()
    {
        count = 0;
    }
}
