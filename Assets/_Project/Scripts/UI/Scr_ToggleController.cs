using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle effectToggle; // effectToggle
    [SerializeField] private Toggle characterToggle; // characterToggle

    private void Start()
    {
        //effectToggle.onValueChanged.AddListener((isOn) =>
        //{
        //    characterToggle.isOn = false;
            
        //}

        //characterToggle.onValueChanged.AddListener((isOn) =>
        //{

        //}
    }

    private void EffectUI()
    {
        transform.Find("EffectToggle").gameObject.SetActive(true);
        transform.Find("CharacterToggle").gameObject.SetActive(false);
    }

    private void CharacterUI()
    {
        transform.Find("EffectToggle").gameObject.SetActive(false);
        transform.Find("CharacterToggle").gameObject.SetActive(true);
    }
}
