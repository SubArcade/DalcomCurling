using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class Src_AimLine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Transform startPoint;           // 컬링 스톤 Transform
    [SerializeField] private Camera mainCamera;         // 드래그를 감지할 카메라

    [Header("Settings")]
    [SerializeField] private float drageSensitivity = 100f; // 드래그에 따라 길어지는 라인 감도
    [SerializeField] private float maxLength = 6f;      // 드래그에 따른 최대 라인 길이
    [SerializeField] private float curveStrength = 2f;  // 좌우 휘어짐 정도
    [SerializeField] private int lineResolution = 30;   // 라인 곡선 해상도

    private LineRenderer line;
    private Vector2 dragStartPos;
    private float dragX, dragY;
    private bool isDragging;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = lineResolution;
        line.enabled = false;

        // 기본 스타일
        line.startWidth = 0.15f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = new Color(0.2f, 0.7f, 1f, 0.8f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPos = eventData.position;
        line.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - dragStartPos;

        // 선 길이 = 드래그 거리로

        float dragDistance = delta.magnitude / drageSensitivity;

        dragDistance = Mathf.Clamp(dragDistance, 0f , maxLength);

        dragX = Mathf.Clamp(delta.x / 150f, -1f, 1f);  // 좌우 커브 정도

        dragY = dragDistance / maxLength; // 0 ~ 1 사이 비율

        UpdateLine(dragDistance);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        line.enabled = false;

        // 여기서 dragY, dragX 값으로 발사 힘 / 회전값 계산 가능
        // 예: stoneShoot.Launch(force: dragY, curl: dragX);
    }

    private void UpdateLine(float currentLength)
    {
        if (!startPoint) return;

        // 시작점 (A)
        Vector3 A = startPoint.position;

        // 끝점 (B) — 드래그 길이에 따라
        Vector3 B = A + startPoint.forward * currentLength;

        // 제어점 (Control) — 드래그 세기의 절반쯤 앞에서 좌우 휘어짐 반영
        Vector3 control = A + startPoint.forward * (dragY * maxLength * 0.5f)
                            + startPoint.right * (curveStrength * dragX);

        // 베지어 곡선 계산
        for (int i = 0; i < lineResolution; i++)
        {
            float t = i / (float)(lineResolution - 1);
            Vector3 point = CalculateQuadraticBezierPoint(t, A, control, B);
            line.SetPosition(i, point);
        }
    }

    // 2차 베지어 계산식
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }
}