#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Apocarrot.Arena;

namespace Apocarrot.EditorTools
{
    public static class ArenaSceneBuilderEditor
    {
        [MenuItem("Apocarrot/Setup Arena (Gate 6: Waves & King Carrot Boss)")]
        public static void SetupGate6Scene()
        {
            ArenaSceneBootstrap bootstrap = Object.FindFirstObjectByType<ArenaSceneBootstrap>();
            if (bootstrap == null)
            {
                GameObject bootstrapObj = new GameObject("Arena_Bootstrap_Manager");
                bootstrap = bootstrapObj.AddComponent<ArenaSceneBootstrap>();
                Undo.RegisterCreatedObjectUndo(bootstrapObj, "Setup Arena");
            }

            bootstrap.BuildArenaEnvironment();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Apocarrot] Đã thiết lập hoàn tất Đấu trường Gate 6: 3 Waves, Trạm nghỉ tự động & Boss King Carrot! Bấm Play để trải nghiệm.");
        }

        [MenuItem("Apocarrot/Setup Arena (Gate 4: Enemies, Loot & Run Inventory)")]
        public static void SetupGate4Scene()
        {
            SetupGate6Scene();
        }

        [MenuItem("Apocarrot/Setup Gate 2 (Legacy)")]
        public static void SetupGate2Scene()
        {
            SetupGate4Scene();
        }
    }
}
#endif
