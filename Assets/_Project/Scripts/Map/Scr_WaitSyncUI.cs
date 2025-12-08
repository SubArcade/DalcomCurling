using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_WaitSyncUI : MonoBehaviour
{
    [SerializeField] private Scr_TweenHandDragGuide waitingImg;
    private void OnEnable()
    {
        waitingImg.PlayTouchMove();
    }

}
