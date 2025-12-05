using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisabledButtonMessage : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ToastMessage toast;
    [SerializeField] private string message;
    [SerializeField] private Button targetButton;   // 실제 업그레이드 버튼
    [SerializeField] private GeneratorType type;
    [SerializeField] private Scr_DonutUpgradePopUp upgradePopup;
    private Image overlayImage;

    private void Awake()
    {
        toast = FindObjectOfType<ToastMessage>();
        overlayImage = GetComponent<Image>();
    }

    public void ApplyInteractableState()
    {
        if (overlayImage == null || targetButton == null) return;
        overlayImage.raycastTarget = !targetButton.interactable;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetButton.interactable) return;

        var reasonKey = upgradePopup.GetUpgradeDisabledReasonKey(type);

        if (reasonKey != LocalizationKey.None)
        {
            string message = LocalizationManager.Instance.GetText(reasonKey);
            toast.Show(message);
        }
    }
}
