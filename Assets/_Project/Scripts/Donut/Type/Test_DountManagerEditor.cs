#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DonutManager))]
public class DonutManagerEditor : Editor
{
    private DonutParticleSystem.AuraType particleTestType = DonutParticleSystem.AuraType.Fire;
    private DonutParticleSystem.TrailType trailTestType = DonutParticleSystem.TrailType.FireTail;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 
        DrawDefaultInspector();

        DonutManager manager = (DonutManager)target;

        GUILayout.Space(10);
        GUILayout.Label("도넛 컨트롤", EditorStyles.boldLabel);

        // 도넛 생성, 업데이트, 개별 업데이트, 제거버튼
        if (GUILayout.Button("도넛 생성", GUILayout.Height(30)))
        {
            manager.SpawnDonut(DonutType.Hard, manager.level);
        }

        if (GUILayout.Button("모든 도넛 업데이트", GUILayout.Height(30)))
        {
            manager.UpdateDonut();
        }

        if (GUILayout.Button("마지막 도넛 제거", GUILayout.Height(30)))
        {
            manager.RemoveDonut();
        }

        if (GUILayout.Button("모든 도넛 제거", GUILayout.Height(30)))
        {
            manager.RemoveAllDonuts();
        }

        GUILayout.Space(10);
        GUILayout.Label("파티클 테스트", EditorStyles.boldLabel);

        particleTestType = (DonutParticleSystem.AuraType)EditorGUILayout.EnumPopup("파티클 타입", particleTestType);

        if (GUILayout.Button("선택 도넛에 파티클 적용", GUILayout.Height(30))) 
        {
            ApplyParticleToSelectedDonut(manager, particleTestType, true);
        }

        if (GUILayout.Button("선택 도넛에 파티클 해제", GUILayout.Height(30)))
        {
            ApplyParticleToSelectedDonut(manager, particleTestType, false);
        }

        // 꼬리 파티클만 추가
        trailTestType = (DonutParticleSystem.TrailType)EditorGUILayout.EnumPopup("꼬리 파티클 타입", trailTestType);

        if (GUILayout.Button("선택 도넛에 꼬리파티클 적용", GUILayout.Height(30)))
        {
            ApplyTrailToSelectedDonut(manager, trailTestType, true);
        }

        if (GUILayout.Button("선택 도넛에 꼬리파티클 해제", GUILayout.Height(30)))
        {
            ApplyTrailToSelectedDonut(manager, trailTestType, false);
        }
    }

    // 파티클 적용 메서드
    private void ApplyParticleToSelectedDonut(DonutManager manager, DonutParticleSystem.AuraType auraType, bool enabled)
    {
        if (manager.GetSpawnedDonutCount() > manager.selectDonutIndex)
        {
            GameObject selectedDonut = manager.GetSpawnedDonuts()[manager.selectDonutIndex];
            var particleSystem = selectedDonut.GetComponent<DonutParticleSystem>();

            if (particleSystem != null)
            {
                if (auraType != DonutParticleSystem.AuraType.None)
                {
                    particleSystem.ChangeAuraType(auraType);
                }
                particleSystem.SetAuraEnabled(enabled);

                Debug.Log($"{manager.selectDonutIndex}번 도넛에 {auraType} 파티클 {(enabled ? "ON" : "OFF")}");
            }
        }
        else
        {
            Debug.LogWarning("선택된 도넛이 없습니다. 먼저 도넛을 생성해주세요.");
        }
    }

    // 꼬리 파티클 적용 메서드
    private void ApplyTrailToSelectedDonut(DonutManager manager, DonutParticleSystem.TrailType trailType, bool enabled)
    {
        if (manager.GetSpawnedDonutCount() > manager.selectDonutIndex)
        {
            GameObject selectedDonut = manager.GetSpawnedDonuts()[manager.selectDonutIndex];
            var particleSystem = selectedDonut.GetComponent<DonutParticleSystem>();

            if (particleSystem != null)
            {
                if (trailType != DonutParticleSystem.TrailType.None)
                {
                    particleSystem.ChangeTrailType(trailType);
                }
                particleSystem.SetTrailEnabled(enabled);

                Debug.Log($"{manager.selectDonutIndex}번 도넛에 {trailType} 꼬리파티클 {(enabled ? "ON" : "OFF")}");
            }
        }
        else
        {
            Debug.LogWarning("선택된 도넛이 없습니다. 먼저 도넛을 생성해주세요.");
        }
    }
}
#endif