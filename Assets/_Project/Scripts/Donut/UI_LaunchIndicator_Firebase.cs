using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LaunchIndicator_Firebase : MonoBehaviour
{
    //UI의 표시(활성화) 여부를 제어하는 스크립트
    // TODO : 미리보기 궤적이 완성되면 기존에 있던 조작 UI는 삭제해야함
    
    [Header("References")]
    // 발사 로직 스크립트를 드래그 앤 드롭으로 연결
    public StoneShoot_Firebase launchScript;

    // UI Slider 컴포넌트를 드래그 앤 드롭으로 연결
    [Header("UI오브젝트들")]
    public Slider forceSlider;
    public Slider rotationSlider;
    public RectTransform arrowRectTransform;
    public CanvasGroup arrowCanvasGroup;
    public CanvasGroup rotationAmountSliderCanvasGroup;
    public GameObject tapStartPointImage;
    public TextMeshProUGUI aScoreText;
    public TextMeshProUGUI bScoreText;
    public TextMeshProUGUI roundText;
    public GameObject CountDownText;
    [Header("Debug")]
    public TextMeshProUGUI debugStateText;
    void Update()
    {
        if (launchScript == null) return;

        var currentState = launchScript.CurrentState;
        var currentDragType = launchScript.CurrentDragType;

        // 조준 상태일 때만 조작 관련 UI를 업데이트합니다.
        if (currentState == StoneShoot_Firebase.LaunchState.Aiming)
        {
            // 1. UI 가시성 제어
            // 힘/방향을 드래그할 때만 화살표 UI 그룹을 표시합니다.
            if (arrowCanvasGroup != null)
            {
                arrowCanvasGroup.alpha = currentDragType == StoneShoot_Firebase.DragType.PowerDirection ? 1f : 0f;
            }

            // 회전을 드래그할 때만 회전량 슬라이더 UI 그룹을 표시합니다.
            if (rotationAmountSliderCanvasGroup != null)
            {
                rotationAmountSliderCanvasGroup.alpha = currentDragType == StoneShoot_Firebase.DragType.Rotation ? 1f : 0f;
            }

            // 2. 슬라이더 및 화살표 값 업데이트
            // 힘 슬라이더는 항상 현재 설정된 힘의 비율을 표시합니다.
            if (forceSlider != null)
            {
                forceSlider.value = launchScript.CurrentDragRatio;
            }

            // 회전 슬라이더는 항상 현재 설정된 회전 값을 표시합니다.
            if (rotationSlider != null)
            {
                rotationSlider.value = launchScript.FinalRotationValue;
            }

            // 화살표는 항상 현재 설정된 방향과 힘을 시각적으로 표시합니다.
            if (arrowRectTransform != null)
            {
                // Z축 회전만 변경하여 화살표가 방향을 가리키도록 합니다.
                Vector3 rotation = arrowRectTransform.localEulerAngles;
                rotation.z = launchScript.CurrentLaunchAngle;
                arrowRectTransform.localEulerAngles = rotation;

                // 화살표의 크기를 드래그 비율에 따라 변경하여 피드백 강화
                float scale = Mathf.Lerp(0.5f, 1f, launchScript.CurrentDragRatio);
                arrowRectTransform.localScale = Vector3.one * scale;
            }
        }
        else // 조준 상태가 아닐 때 (예: 돌이 굴러갈 때)
        {
            // 모든 조작 관련 UI를 숨깁니다.
            if (arrowCanvasGroup != null) arrowCanvasGroup.alpha = 0f;
            if (rotationAmountSliderCanvasGroup != null) rotationAmountSliderCanvasGroup.alpha = 0f;
            if (forceSlider != null) forceSlider.value = 0;
            if (rotationSlider != null) rotationSlider.value = 0;
        }


        // 디버그용 상태 텍스트 업데이트
        if (debugStateText != null && FirebaseGameManager.Instance != null)
        {
            string localState = FirebaseGameManager.Instance.CurrentLocalState;
            string gameState = FirebaseGameManager.Instance.CurrentGameState;
            debugStateText.text = $"Local State: {localState}\nGame State: {gameState}";
        }
    }

    public void ActivateTapStartPointImage(bool activate, Vector3 pos = default(Vector3))
    {
        if (tapStartPointImage == null) return;
        tapStartPointImage.transform.position = pos;
        tapStartPointImage.SetActive(activate);
        if (activate == true)
        {
            arrowRectTransform.position = pos;
        }
    }

    public void RoundChanged(int round, int aScore, int bScore)
    {
        aScoreText.text = $"A Score : {aScore}";
        bScoreText.text = $"B Score : {bScore}";
        roundText.text = $"Round : {round}";
    }
    /// <summary>
    /// 화살표 관련 UI의 활성화 여부를 설정합니다.
    /// </summary>
    public void SetArrowUIVisibility(bool isVisible)
    {
        if (arrowCanvasGroup == null || rotationAmountSliderCanvasGroup == null) return;

        // isVisible 값에 따라 UI 그룹의 alpha 값을 변경하여 보여주거나 숨깁니다.
        arrowCanvasGroup.alpha = isVisible ? 1f : 0f;
        arrowCanvasGroup.interactable = isVisible;
        arrowCanvasGroup.blocksRaycasts = isVisible;

        rotationAmountSliderCanvasGroup.alpha = isVisible ? 1f : 0f;
        rotationAmountSliderCanvasGroup.interactable = isVisible;
        rotationAmountSliderCanvasGroup.blocksRaycasts = isVisible;
    }

    /// <summary>
    /// 카운트 다운 UI의 활성화 여부를 설정합니다.
    /// </summary>
    public void SetCountDown(bool IsturnWaitingForInput)
    {
        if (IsturnWaitingForInput)
        {
            CountDownText.SetActive(true);
        }
        else
        {
            CountDownText.SetActive(false);
        }
    }
}