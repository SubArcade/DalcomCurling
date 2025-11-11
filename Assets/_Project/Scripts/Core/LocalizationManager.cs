using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;


// 로컬라이제이션 josn에서 파싱할 키값
public enum LocalizationKey
{
    None = 0,
    Btn_Start,
    Btn_Exit,
    Title_Main,
    Label_Gold
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, Dictionary<string, string>> all;
    private Dictionary<string, string> current;
    public string CurrentLanguage { get; private set; } = "ko";

    public event System.Action OnLanguageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTables();
        SetLanguage(CurrentLanguage);
    }

    void LoadTables()
    {
        var asset = Resources.Load<TextAsset>("texts");
        if (asset == null)
        {
            Debug.LogError("[Loc] texts.json not found in Resources");
            return;
        }

        all = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(asset.text);
    }

    public void SetLanguage(string lang)
    {
        if (all != null && all.TryGetValue(lang, out var table))
        {
            CurrentLanguage = lang;
            current = table;
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"[Loc] Language not found: {lang}");
        }
    }
    
    public string GetText(LocalizationKey key)
    {
        if (key == LocalizationKey.None)
            return string.Empty;

        if (current == null)
            return key.ToString();

        var k = key.ToString(); // JSON key와 동일해야 함
        if (current.TryGetValue(k, out var value))
            return value;

        // 못 찾으면 키 이름 그대로 노출 (디버깅용)
        return k;
    }
}