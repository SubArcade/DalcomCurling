using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle effectToggle; // effectToggle
    [SerializeField] private Toggle characterToggle; // characterToggle

    private void Awake()
    {
        effectToggle = transform.Find("Mid/CharacterEffectSelector/EffectToggle")?.GetComponent<Toggle>();    
        characterToggle = transform.Find("Mid/CharacterEffectSelector/CharacterToggle")?.GetComponent<Toggle>();    
    }

    private void Start()
    {
        effectToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                characterToggle.isOn = false;
                effectToggle.isOn = true;
                EffectUI();
            }
        });

        characterToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                effectToggle.isOn = false;
                characterToggle.isOn = true;
                CharacterUI();
            }
        });
    }

    private void EffectUI()
    {
        transform.Find("Mid/CharacterEffectSelector/EffectToggle").gameObject.SetActive(true);
        transform.Find("Mid/CharacterEffectSelector/CharacterToggle").gameObject.SetActive(false);
    }

    private void CharacterUI()
    {
        transform.Find("Mid/CharacterEffectSelector/EffectToggle").gameObject.SetActive(false);
        transform.Find("Mid/CharacterEffectSelector/CharacterToggle").gameObject.SetActive(true);
    }
}
