using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Apocarrot.Loot
{
    /// <summary>
    /// Hiển thị thông báo bay bổng (Floating Toast / Notification) khi người chơi nhặt vật phẩm.
    /// Tối ưu hóa triệt để (Zero-Alloc / Object Pool):
    /// - Dùng 1 Canvas WorldSpace duy nhất được tạo 1 lần.
    /// - Quản lý hồ bơi (Object Pool) 8 thẻ Toast tái sử dụng, không sinh rác bộ nhớ (GC Alloc) trong trận đấu.
    /// - Phát sự kiện OnToastNotice để HUD chính đồng thời hiển thị Banner thông báo rõ ràng cho người chơi.
    /// </summary>
    public class LootToastManager : MonoBehaviour
    {
        public static LootToastManager Instance { get; private set; }
        public static event Action<string, Color> OnToastNotice;

        private const int POOL_SIZE = 8;
        private Canvas toastCanvas;
        private Camera mainCamera;
        private Font uiFont;

        private class ToastItem
        {
            public GameObject root;
            public Text text;
            public Outline outline;
            public Coroutine coroutine;
            public bool isBusy;
        }

        private List<ToastItem> toastPool = new List<ToastItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            InitCanvasAndPool();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void InitCanvasAndPool()
        {
            if (toastCanvas != null) return;

            GameObject canvasObj = new GameObject("World_LootToast_Canvas");
            canvasObj.transform.SetParent(transform, false);

            toastCanvas = canvasObj.AddComponent<Canvas>();
            toastCanvas.renderMode = RenderMode.WorldSpace;

            toastPool.Clear();
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject toastObj = new GameObject($"ToastItem_{i}");
                toastObj.transform.SetParent(canvasObj.transform, false);

                RectTransform rect = toastObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(340, 54);
                rect.localScale = Vector3.one * 0.012f;

                Text text = toastObj.AddComponent<Text>();
                text.font = uiFont;
                text.fontSize = 24;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;

                Outline outline = toastObj.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.95f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                toastObj.SetActive(false);

                toastPool.Add(new ToastItem
                {
                    root = toastObj,
                    text = text,
                    outline = outline,
                    isBusy = false
                });
            }
        }

        public static void ShowWorldToast(Vector3 worldPosition, string message, Color color)
        {
            ShowToast(message, color, worldPosition);
        }

        public static void ShowToast(string message, Color color, Vector3 worldPosition)
        {
            if (Instance == null)
            {
                GameObject toastHost = new GameObject("LootToast_Manager");
                Instance = toastHost.AddComponent<LootToastManager>();
            }

            Instance.TriggerToast(message, color, worldPosition);
            OnToastNotice?.Invoke(message, color);
        }

        private void TriggerToast(string message, Color color, Vector3 worldPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;

            ToastItem target = null;
            for (int i = 0; i < toastPool.Count; i++)
            {
                if (!toastPool[i].isBusy)
                {
                    target = toastPool[i];
                    break;
                }
            }

            // Nếu tất cả đều bận, mượn thẻ đầu tiên
            if (target == null && toastPool.Count > 0)
            {
                target = toastPool[0];
                if (target.coroutine != null) StopCoroutine(target.coroutine);
            }

            if (target != null)
            {
                target.isBusy = true;
                target.root.SetActive(true);
                target.text.text = message;
                target.text.color = color;
                target.root.transform.position = worldPos;

                if (mainCamera != null)
                {
                    target.root.transform.rotation = mainCamera.transform.rotation;
                }

                target.coroutine = StartCoroutine(AnimateToast(target, worldPos, color));
            }
        }

        private IEnumerator AnimateToast(ToastItem item, Vector3 startPos, Color baseColor)
        {
            float duration = 1.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (mainCamera != null && item.root != null)
                {
                    item.root.transform.rotation = mainCamera.transform.rotation;
                }

                if (item.root != null)
                {
                    item.root.transform.position = startPos + Vector3.up * (t * 1.5f);
                }

                Color c = baseColor;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                if (item.text != null) item.text.color = c;

                yield return null;
            }

            if (item.root != null) item.root.SetActive(false);
            item.isBusy = false;
            item.coroutine = null;
        }
    }
}
