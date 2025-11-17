using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayerProgress : MonoBehaviour
{
    public int level = 1;
    public BoardManager boardManager;
    public TextMeshProUGUI levelText;

    public void LevelUp()
    {
        level++;
        boardManager.UpdateBoardUnlock(level);
        levelText.text = $"{level}";
    }
}
