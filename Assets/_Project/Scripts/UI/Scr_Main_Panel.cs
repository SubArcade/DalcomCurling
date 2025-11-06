using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_Main_Panel : MonoBehaviour
{
    [Header("휴지통 관련")]
    [SerializeField] private GameObject TrashCan;
    [SerializeField] private Transform TrashCanTransform;
    [SerializeField] private Image TrashCanImage;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // 혹시라도 로딩 지연 대비

        TrashCan = transform.Find("Bottom/ButtonGroup/basket_Button")?.gameObject;
        TrashCanTransform = TrashCan.transform;
        TrashCanImage = TrashCan.GetComponent<Image>();
        OnMouseTrashCan();
    }

    private Vector3 trashOriginalScale;

    private void OnMouseTrashCan() 
    {
        trashOriginalScale = TrashCanTransform.localScale;

        EventTrigger trigger = TrashCan.GetComponent<EventTrigger>();
        if (trigger != null) 
        {
            trigger = TrashCan.AddComponent<EventTrigger>();
        }
        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            TrashCanTransform.DOScale(trashOriginalScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
        });

        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            TrashCanTransform.DOScale(trashOriginalScale, 0.2f).SetEase(Ease.OutBack);
        });

    }
    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            Destroy(dropped);
        }
    }

}
