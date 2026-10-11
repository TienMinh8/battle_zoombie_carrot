using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Apocarrot.Loot
{
    /// <summary>
    /// Hiển thị thông báo bay bổng (Floating Toast / Notification) khi người chơi nhặt vật phẩm.
    /// </summary>
    public class LootToastManager : MonoBehaviour
    {
        private static LootToastManager instance;
        public static LootToastManager Instance => instance;

        public static void ShowWorldToast(Vector3 worldPosition, string message, Color color)
        {
            ShowToast(message, color, worldPosition);
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public static void ShowToast(string message, Color color, Vector3 worldPosition)
        {
            if (instance == null)
            {
                GameObject toastHost = new GameObject("LootToast_Manager");
                instance = toastHost.AddComponent<LootToastManager>();
            }

            instance.StartCoroutine(instance.SpawnFloatingToast(message, color, worldPosition));
        }

        private IEnumerator SpawnFloatingToast(string message, Color color, Vector3 worldPos)
        {
            GameObject toastObj = new GameObject("Toast_" + message);
            toastObj.transform.position = worldPos;

            // Dùng Canvas World Space hoặc Billboard Text
            Canvas canvas = toastObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rect = toastObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 50);
            rect.localScale = Vector3.one * 0.012f;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(toastObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = message;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(300, 50);

            // Shadow/Outline đơn giản
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            float duration = 1.0f;
            float elapsed = 0f;
            Vector3 startP = worldPos;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (cam != null)
                {
                    toastObj.transform.rotation = cam.transform.rotation;
                }

                toastObj.transform.position = startP + Vector3.up * (t * 1.6f);
                Color c = color;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                text.color = c;

                yield return null;
            }

            Destroy(toastObj);
        }
    }
}
