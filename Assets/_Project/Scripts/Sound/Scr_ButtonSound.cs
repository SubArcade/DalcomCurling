using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(delegate {OnButtonClicked();});
        }
    }

    private void OnButtonClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.buttonClick();
        }
    }
}
