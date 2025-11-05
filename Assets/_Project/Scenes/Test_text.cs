using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_text : MonoBehaviour
{
    public Button button;
    public TMP_Text text;
    public int count = 0;
    
    void Awake()
    {
        count = 0;
        button.onClick.AddListener(addcount);
    }

    void addcount()
    {
        count++;
        text.text = $"click {count}";
    }
}
