using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddMissingManagers
{
    [MenuItem("Window/WarGame/Fix Stage01 Missing Managers")]
    public static void Execute()
    {
        // EventTriggerManager 추가
        var managers = GameObject.Find("Managers");
        if (managers == null)
        {
            Debug.LogError("[AddMissingManagers] 'Managers' GameObject를 찾을 수 없습니다.");
            return;
        }

        var existing = managers.GetComponentInChildren<EventTriggerManager>();
        if (existing == null)
        {
            var go = new GameObject("EventTriggerManager");
            go.transform.SetParent(managers.transform, false);
            go.AddComponent<EventTriggerManager>();
            Debug.Log("[AddMissingManagers] EventTriggerManager 추가 완료");
        }
        else
        {
            Debug.Log("[AddMissingManagers] EventTriggerManager 이미 존재");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[AddMissingManagers] 씬 저장 완료");
    }
}
