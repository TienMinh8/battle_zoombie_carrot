using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Apocarrot.UI
{
    /// <summary>
    /// Quản lý hiển thị số nhảy sát thương (Floating Damage Numbers) chuẩn Roguelite.
    /// Thiết kế theo mô hình Zero-Allocation Object Pool (tái sử dụng 20 Text Item trong Canvas thế giới),
    /// đảm bảo mượt mà tuyệt đối 60 FPS, không tạo Garbage Collection gây khựng giật.
    /// </summary>
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        private const int POOL_SIZE = 24;

        private class PopupItem
        {
            public GameObject RootObj;
            public RectTransform RectTr;
            public Text TextComponent;
            public Vector3 WorldPos;
            public float ElapsedTime;
            public float Lifetime;
            public bool IsActive;
            public Color BaseColor;
        }

        private Canvas worldCanvas;
        private PopupItem[] pool;
        private int nextPoolIndex = 0;
        private UnityEngine.Camera mainCam;
        private Font uiFont;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCam = UnityEngine.Camera.main;
        }

        private void InitializePool()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Tạo Canvas dạng WorldSpace hoặc Overlay cho Popup
            GameObject canvasObj = new GameObject("DamagePopup_Canvas");
            canvasObj.transform.SetParent(transform, false);
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20;

            pool = new PopupItem[POOL_SIZE];
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject pObj = new GameObject($"Popup_{i}");
                pObj.transform.SetParent(canvasObj.transform, false);

                RectTransform rt = pObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 35);
                rt.localScale = Vector3.one * 0.02f;

                Text txt = pObj.AddComponent<Text>();
                txt.font = uiFont;
                txt.fontSize = 24;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.verticalOverflow = VerticalWrapMode.Overflow;

                // Outline viền đen cho số nổi rõ nét trên mọi nền
                Outline outline = pObj.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.9f);
                outline.effectDistance = new Vector2(1.2f, -1.2f);

                pObj.SetActive(false);

                pool[i] = new PopupItem
                {
                    RootObj = pObj,
                    RectTr = rt,
                    TextComponent = txt,
                    IsActive = false
                };
            }
        }

        public static void ShowDamage(Vector3 worldPosition, float damageAmount, bool isCritical = false)
        {
            if (Instance == null)
            {
                // Tự động khởi tạo singleton nếu chưa có trong Scene
                GameObject managerObj = new GameObject("DamagePopupManager");
                Instance = managerObj.AddComponent<DamagePopupManager>();
            }

            Instance.SpawnPopup(worldPosition, damageAmount, isCritical);
        }

        private void SpawnPopup(Vector3 worldPosition, float damageAmount, bool isCritical)
        {
            if (pool == null || pool.Length == 0) return;

            PopupItem item = pool[nextPoolIndex];
            nextPoolIndex = (nextPoolIndex + 1) % POOL_SIZE;

            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-0.25f, 0.25f),
                UnityEngine.Random.Range(0.6f, 1.0f),
                UnityEngine.Random.Range(-0.25f, 0.25f)
            );

            item.WorldPos = worldPosition + randomOffset;
            item.ElapsedTime = 0f;
            item.Lifetime = isCritical ? 0.65f : 0.5f;
            item.IsActive = true;

            int intDmg = Mathf.Max(1, Mathf.RoundToInt(damageAmount));
            if (isCritical)
            {
                item.TextComponent.text = $"{intDmg}!";
                item.TextComponent.fontSize = 30;
                item.BaseColor = new Color(1f, 0.35f, 0.1f); // Cam đỏ chí mạng
            }
            else
            {
                item.TextComponent.text = intDmg.ToString();
                item.TextComponent.fontSize = 24;
                item.BaseColor = new Color(1f, 0.96f, 0.85f); // Trắng vàng ngà
            }

            item.TextComponent.color = item.BaseColor;
            item.RectTr.position = item.WorldPos;
            item.RootObj.SetActive(true);
        }

        private void LateUpdate()
        {
            if (pool == null) return;
            if (mainCam == null) mainCam = UnityEngine.Camera.main;

            float dt = Time.deltaTime;
            Quaternion camRot = mainCam != null ? mainCam.transform.rotation : Quaternion.identity;

            for (int i = 0; i < pool.Length; i++)
            {
                PopupItem item = pool[i];
                if (!item.IsActive) continue;

                item.ElapsedTime += dt;
                float progress = item.ElapsedTime / item.Lifetime;

                if (progress >= 1f)
                {
                    item.IsActive = false;
                    item.RootObj.SetActive(false);
                    continue;
                }

                // Bay nhẹ lên cao theo đường cong êm dịu
                Vector3 currentPos = item.WorldPos + Vector3.up * (progress * 0.7f);
                item.RectTr.position = currentPos;
                item.RectTr.rotation = camRot; // Luôn xoay đối diện camera

                // Mờ dần theo thời gian (Fade out)
                float alpha = 1f - Mathf.Pow(progress, 1.8f);
                Color c = item.BaseColor;
                c.a = alpha;
                item.TextComponent.color = c;
            }
        }
    }
}
