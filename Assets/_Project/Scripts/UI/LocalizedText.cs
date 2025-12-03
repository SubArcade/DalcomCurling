using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private LocalizationKey key = LocalizationKey.None;
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;

        UpdateText();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (LocalizationManager.Instance == null)
            return;

        text.text = LocalizationManager.Instance.GetText(key);
    }
}