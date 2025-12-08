using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisabledButtonMessage : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ToastMessage toast;
    [SerializeField] private string message;
    [SerializeField] private Button targetButton;   // 실제 업그레이드 버튼

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetButton != null && targetButton.interactable) return;
        toast.Show(message);
    }
}
