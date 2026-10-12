using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Apocarrot.Cameras;
using Apocarrot.Gameplay;
using Apocarrot.Combat;
using Apocarrot.Data;
using Apocarrot.Enemies;
using Apocarrot.Inventory;
using Apocarrot.Loot;
using Apocarrot.UI;
using Apocarrot.Progression;

namespace Apocarrot.Arena
{
    /// <summary>
    /// Tự động khởi tạo đấu trường Greybox, Rabbit Warrior, Camera góc 3/4,
    /// hệ thống Combat Cận Chiến, Vũ khí tầm xa tự động (Tomato & Grape), Quái Zombie Carrot và UI.
    /// </summary>
    public class ArenaSceneBootstrap : MonoBehaviour
    {
        [Header("Arena Configuration")]
        [SerializeField] private float arenaWidth = 36f;
        [SerializeField] private float arenaLength = 36f;
        [SerializeField] private float wallHeight = 2.5f;

        [Header("Gate 3 Test Enemies")]
        [SerializeField] private int initialEnemyCount = 3;

        private void Awake()
        {
            BuildArenaEnvironment();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
                {
                    if (EquipmentBagUI.Instance != null)
                    {
                        EquipmentBagUI.Instance.ToggleBag();
                    }
                }
            }
        }

        public void BuildGate2Environment() => BuildArenaEnvironment();
        public void BuildGate3Environment() => BuildArenaEnvironment();
        public void BuildGate4Environment() => BuildArenaEnvironment();
        public void BuildGate5Environment() => BuildArenaEnvironment();
        public void BuildGate6Environment() => BuildArenaEnvironment();
        public void BuildGate7Environment() => BuildArenaEnvironment();
        public void BuildGate8Environment() => BuildArenaEnvironment();

        public void BuildArenaEnvironment()
        {
            // 1. Kiểm tra hoặc tạo Sàn Đấu Trường (Ground)
            GameObject ground = GameObject.Find("Arena_Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Arena_Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(arenaWidth / 10f, 1f, arenaLength / 10f);

                Renderer renderer = ground.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.22f, 0.28f, 0.22f); // Màu đất rêu xám tối
                }
            }

            // 2. Tạo 4 Bức tường bao quanh (Boundary Walls)
            CreateBoundaryWall("Wall_North", new Vector3(0, wallHeight / 2f, arenaLength / 2f), new Vector3(arenaWidth, wallHeight, 1f));
            CreateBoundaryWall("Wall_South", new Vector3(0, wallHeight / 2f, -arenaLength / 2f), new Vector3(arenaWidth, wallHeight, 1f));
            CreateBoundaryWall("Wall_East", new Vector3(arenaWidth / 2f, wallHeight / 2f, 0), new Vector3(1f, wallHeight, arenaLength));
            CreateBoundaryWall("Wall_West", new Vector3(-arenaWidth / 2f, wallHeight / 2f, 0), new Vector3(1f, wallHeight, arenaLength));

            // 3. Nhân vật Thỏ Rabbit Warrior
            PlayerController playerComp = FindFirstObjectByType<PlayerController>();
            GameObject player = playerComp != null ? playerComp.gameObject : GameObject.Find("Player_RabbitWarrior");

            if (player == null)
            {
                player = new GameObject("Player_RabbitWarrior");
                player.transform.position = new Vector3(0, 1f, 0);

                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0, 0.9f, 0);

                playerComp = player.AddComponent<PlayerController>();
                player.AddComponent<PlayerHealth>();

                // Tạo hình thể Thỏ (Thân trắng + Khăn xanh rêu)
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Visual_Body";
                body.transform.SetParent(player.transform, false);
                body.transform.localPosition = new Vector3(0, 0.9f, 0);
                body.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                Collider bodyCol = body.GetComponent<Collider>();
                if (bodyCol != null) Destroy(bodyCol);

                Renderer bodyRenderer = body.GetComponent<Renderer>();
                if (bodyRenderer != null)
                {
                    bodyRenderer.material.color = new Color(0.95f, 0.95f, 0.98f);
                }

                // Tai Thỏ 1
                GameObject earL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                earL.name = "Ear_Left";
                earL.transform.SetParent(body.transform, false);
                earL.transform.localPosition = new Vector3(-0.25f, 1.0f, 0);
                earL.transform.localScale = new Vector3(0.2f, 0.55f, 0.2f);
                earL.transform.localRotation = Quaternion.Euler(0, 0, 15f);
                Destroy(earL.GetComponent<Collider>());
                earL.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.98f);

                // Tai Thỏ 2
                GameObject earR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                earR.name = "Ear_Right";
                earR.transform.SetParent(body.transform, false);
                earR.transform.localPosition = new Vector3(0.25f, 1.0f, 0);
                earR.transform.localScale = new Vector3(0.2f, 0.55f, 0.2f);
                earR.transform.localRotation = Quaternion.Euler(0, 0, -15f);
                Destroy(earR.GetComponent<Collider>());
                earR.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.98f);

                // Khăn lá xanh
                GameObject scarf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                scarf.name = "Scarf_Green";
                scarf.transform.SetParent(body.transform, false);
                scarf.transform.localPosition = new Vector3(0, 0.15f, 0);
                scarf.transform.localScale = new Vector3(1.05f, 0.15f, 1.05f);
                Destroy(scarf.GetComponent<Collider>());
                scarf.GetComponent<Renderer>().material.color = new Color(0.15f, 0.65f, 0.25f);

                // Cập nhật bộ nhớ renderers để hiệu ứng chớp đỏ khi nhận sát thương hoạt động chuẩn xác
                player.GetComponent<PlayerHealth>()?.RefreshRenderers();
            }

            // Gắn hệ thống Melee Combat cho Player
            MeleeCombat meleeCombat = player.GetComponent<MeleeCombat>();
            if (meleeCombat == null)
            {
                meleeCombat = player.AddComponent<MeleeCombat>();
            }

            // Gắn hệ thống Ranged Weapon Controller cho Player
            RangedWeaponController rangedController = player.GetComponent<RangedWeaponController>();
            if (rangedController == null)
            {
                rangedController = player.AddComponent<RangedWeaponController>();
            }

            // Gắn hệ thống Run Inventory (Túi đồ & Vàng trong lượt chơi)
            RunInventory runInventory = player.GetComponent<RunInventory>();
            if (runInventory == null)
            {
                runInventory = player.AddComponent<RunInventory>();
            }

            // Gắn hệ thống Account Progression (Tiến trình tài khoản lâu dài - Gate 7)
            EnsureAccountProgression();

            // 4. Thiết lập Camera đi theo góc 3/4
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                TopDownFollowCamera followCam = mainCam.GetComponent<TopDownFollowCamera>();
                if (followCam == null)
                {
                    followCam = mainCam.gameObject.AddComponent<TopDownFollowCamera>();
                }
                followCam.Target = player.transform;
            }

            // 5. Thiết lập EventSystem và Canvas UI (HUD + Joystick + Cận Chiến + Tầm Xa + Ba Lô)
            EnsureEventSystem();
            BuildUICanvas(player, meleeCombat, rangedController, runInventory);
        }

        private void SpawnTestEnemies()
        {
            // Spawn 3 biến thể quái khác nhau có màu sắc, kích thước và tỷ lệ rơi đồ riêng biệt
            float jx = Random.Range(-2f, 2f);
            float jz = Random.Range(-2f, 2f);
            ZombieCarrotAI.CreateZombieCarrotPrefab(new Vector3(4.5f + jx, 0f, 6.5f + jz), EnemyData.CreateBasic());
            ZombieCarrotAI.CreateZombieCarrotPrefab(new Vector3(-5.5f + jx, 0f, 7.5f + jz), EnemyData.CreateFast());
            ZombieCarrotAI.CreateZombieCarrotPrefab(new Vector3(0.5f + jx, 0f, 10.5f + jz), EnemyData.CreateElite());
        }

        private void CreateBoundaryWall(string wallName, Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.Find(wallName);
            if (wall == null)
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = wallName;
                wall.transform.position = position;
                wall.transform.localScale = size;

                Renderer renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.18f, 0.22f, 0.18f);
                }
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            else
            {
                // Thay thế StandaloneInputModule cũ (nếu có) bằng InputSystemUIInputModule của New Input System
                var legacyModule = es.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Destroy(legacyModule);
                }
                if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
            }
        }

        private void BuildUICanvas(GameObject player, MeleeCombat meleeCombat, RangedWeaponController rangedController, RunInventory runInventory)
        {
            GameObject canvasObj = GameObject.Find("Gate8_Canvas") ?? GameObject.Find("Gate7_Canvas") ?? GameObject.Find("Gate6_Canvas") ?? GameObject.Find("Gate5_Canvas") ?? GameObject.Find("Gate4_Canvas") ?? GameObject.Find("Gate3_Canvas") ?? GameObject.Find("Gate2_Canvas") ?? GameObject.Find("Gate1_Canvas");
            if (canvasObj != null)
            {
                Destroy(canvasObj);
            }

            canvasObj = new GameObject("Gate8_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. HUD Top-Left (Máu & Tên Game & Tiến độ Wave)
            GameObject hudPanel = new GameObject("MinimalHUD");
            hudPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(0, 1);
            hudRect.pivot = new Vector2(0, 1);
            hudRect.anchoredPosition = new Vector2(30, -30);
            hudRect.sizeDelta = new Vector2(460, 140);

            Image hudBg = hudPanel.AddComponent<Image>();
            hudBg.color = new Color(0, 0, 0, 0.7f);

            GameObject waveTextObj = new GameObject("WaveText");
            waveTextObj.transform.SetParent(hudPanel.transform, false);
            Text waveText = waveTextObj.AddComponent<Text>();
            waveText.font = defaultFont;
            waveText.fontSize = 17;
            waveText.fontStyle = FontStyle.Bold;
            waveText.color = new Color(1f, 0.85f, 0.2f);
            waveText.text = "APOCARROT: CARROT SURVIVAL — ROGUELITE DEMO";
            RectTransform waveRect = waveTextObj.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0, 1);
            waveRect.anchorMax = new Vector2(1, 1);
            waveRect.pivot = new Vector2(0, 1);
            waveRect.anchoredPosition = new Vector2(16, -12);
            waveRect.sizeDelta = new Vector2(-32, 24);

            GameObject progressTextObj = new GameObject("Wave_Progress_Text");
            progressTextObj.transform.SetParent(hudPanel.transform, false);
            Text progressText = progressTextObj.AddComponent<Text>();
            progressText.font = defaultFont;
            progressText.fontSize = 15;
            progressText.fontStyle = FontStyle.Bold;
            progressText.color = new Color(0.25f, 0.95f, 0.55f);
            progressText.text = "⚔️ Wave: 1/4 | Quái còn lại: 6/6";
            RectTransform progressRect = progressTextObj.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0, 1);
            progressRect.anchorMax = new Vector2(1, 1);
            progressRect.pivot = new Vector2(0, 1);
            progressRect.anchoredPosition = new Vector2(16, -38);
            progressRect.sizeDelta = new Vector2(-32, 22);

            // Thanh Máu Background
            GameObject hpBgObj = new GameObject("HP_Bar_Background");
            hpBgObj.transform.SetParent(hudPanel.transform, false);
            Image hpBg = hpBgObj.AddComponent<Image>();
            hpBg.color = new Color(0.18f, 0.04f, 0.05f, 0.95f);
            RectTransform hpBgRect = hpBgObj.GetComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0, 0);
            hpBgRect.anchorMax = new Vector2(1, 0);
            hpBgRect.pivot = new Vector2(0, 0);
            hpBgRect.anchoredPosition = new Vector2(16, 14);
            hpBgRect.sizeDelta = new Vector2(-32, 30);

            // Thanh Máu Fill (Màu xanh lá tươi sáng khi đầy máu)
            GameObject hpFillObj = new GameObject("HP_Bar_Fill");
            hpFillObj.transform.SetParent(hpBgObj.transform, false);
            Image hpFill = hpFillObj.AddComponent<Image>();
            hpFill.color = new Color(0.22f, 0.88f, 0.35f);
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = 1.0f;
            RectTransform hpFillRect = hpFillObj.GetComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = Vector2.one;
            hpFillRect.sizeDelta = Vector2.zero;

            // Chữ HP rõ ràng và nổi bật
            GameObject hpTextObj = new GameObject("HP_Text");
            hpTextObj.transform.SetParent(hpBgObj.transform, false);
            Text hpText = hpTextObj.AddComponent<Text>();
            hpText.font = defaultFont;
            hpText.fontSize = 17;
            hpText.fontStyle = FontStyle.Bold;
            hpText.alignment = TextAnchor.MiddleCenter;
            hpText.color = Color.white;
            hpText.text = "❤️ HP THỎ: 100 / 100";
            RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;

            MinimalHUD hud = hudPanel.AddComponent<MinimalHUD>();
            hud.Initialize(player.GetComponent<PlayerHealth>(), hpFill, hpText, waveText);

            // 2. HUD Top-Center (Run Inventory: Vàng & Túi Đồ 6 Ô)
            GameObject invPanel = new GameObject("RunInventory_HUD");
            invPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform invRect = invPanel.AddComponent<RectTransform>();
            invRect.anchorMin = new Vector2(0.5f, 1);
            invRect.anchorMax = new Vector2(0.5f, 1);
            invRect.pivot = new Vector2(0.5f, 1);
            invRect.anchoredPosition = new Vector2(0, -25);
            invRect.sizeDelta = new Vector2(800, 110);

            Image invBg = invPanel.AddComponent<Image>();
            invBg.color = new Color(0.08f, 0.09f, 0.13f, 0.88f);

            // Chữ hiển thị Vàng
            GameObject goldTextObj = new GameObject("Gold_Text");
            goldTextObj.transform.SetParent(invPanel.transform, false);
            Text goldText = goldTextObj.AddComponent<Text>();
            goldText.font = defaultFont;
            goldText.fontSize = 20;
            goldText.fontStyle = FontStyle.Bold;
            goldText.color = new Color(1.0f, 0.85f, 0.2f);
            goldText.text = "VÀNG: 0";
            RectTransform goldRect = goldTextObj.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0, 1);
            goldRect.anchorMax = new Vector2(0, 1);
            goldRect.pivot = new Vector2(0, 1);
            goldRect.anchoredPosition = new Vector2(20, -12);
            goldRect.sizeDelta = new Vector2(180, 26);

            // Tiêu đề Túi Đồ
            GameObject invTitleObj = new GameObject("Inv_Title");
            invTitleObj.transform.SetParent(invPanel.transform, false);
            Text invTitle = invTitleObj.AddComponent<Text>();
            invTitle.font = defaultFont;
            invTitle.fontSize = 14;
            invTitle.alignment = TextAnchor.MiddleRight;
            invTitle.color = new Color(0.8f, 0.85f, 0.9f, 0.85f);
            invTitle.text = "Túi Đồ Run (2 Đang Dùng + 4 Ba Lô Dự Trữ)";
            RectTransform titleRect = invTitleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(1, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(-20, -12);
            titleRect.sizeDelta = new Vector2(400, 26);

            // 6 Ô Đồ (2 Trang bị + 4 Ba lô)
            float slotWidth = 118f;
            float slotHeight = 52f;
            float startX = 20f;
            float posY = -44f;

            // Ô Trang bị Cận Chiến
            GameObject meleeSlotObj = new GameObject("Slot_Equipped_Melee");
            meleeSlotObj.transform.SetParent(invPanel.transform, false);
            RectTransform meleeSlotRect = meleeSlotObj.AddComponent<RectTransform>();
            meleeSlotRect.anchorMin = new Vector2(0, 1);
            meleeSlotRect.anchorMax = new Vector2(0, 1);
            meleeSlotRect.pivot = new Vector2(0, 1);
            meleeSlotRect.anchoredPosition = new Vector2(startX, posY);
            meleeSlotRect.sizeDelta = new Vector2(slotWidth, slotHeight);
            Image meleeSlotBg = meleeSlotObj.AddComponent<Image>();
            meleeSlotBg.color = new Color(0.85f, 0.55f, 0.15f, 0.85f);

            GameObject meleeSlotTextObj = new GameObject("Text");
            meleeSlotTextObj.transform.SetParent(meleeSlotObj.transform, false);
            Text meleeSlotText = meleeSlotTextObj.AddComponent<Text>();
            meleeSlotText.font = defaultFont;
            meleeSlotText.fontSize = 11;
            meleeSlotText.fontStyle = FontStyle.Bold;
            meleeSlotText.alignment = TextAnchor.MiddleCenter;
            meleeSlotText.color = Color.white;
            meleeSlotText.text = "[CẬN CHIẾN]\nBlade Lv.1";
            RectTransform meleeTextRect = meleeSlotTextObj.GetComponent<RectTransform>();
            meleeTextRect.anchorMin = Vector2.zero;
            meleeTextRect.anchorMax = Vector2.one;
            meleeTextRect.sizeDelta = Vector2.zero;

            // Ô Trang bị Tầm Xa
            GameObject rangedSlotObj = new GameObject("Slot_Equipped_Ranged");
            rangedSlotObj.transform.SetParent(invPanel.transform, false);
            RectTransform rangedSlotRect = rangedSlotObj.AddComponent<RectTransform>();
            rangedSlotRect.anchorMin = new Vector2(0, 1);
            rangedSlotRect.anchorMax = new Vector2(0, 1);
            rangedSlotRect.pivot = new Vector2(0, 1);
            rangedSlotRect.anchoredPosition = new Vector2(startX + (slotWidth + 8f) * 1, posY);
            rangedSlotRect.sizeDelta = new Vector2(slotWidth, slotHeight);
            Image rangedSlotBg = rangedSlotObj.AddComponent<Image>();
            rangedSlotBg.color = new Color(0.8f, 0.2f, 0.25f, 0.85f);

            GameObject rangedSlotTextObj = new GameObject("Text");
            rangedSlotTextObj.transform.SetParent(rangedSlotObj.transform, false);
            Text rangedSlotText = rangedSlotTextObj.AddComponent<Text>();
            rangedSlotText.font = defaultFont;
            rangedSlotText.fontSize = 11;
            rangedSlotText.fontStyle = FontStyle.Bold;
            rangedSlotText.alignment = TextAnchor.MiddleCenter;
            rangedSlotText.color = Color.white;
            rangedSlotText.text = "[TẦM XA]\nTomato Lv.1";
            RectTransform rangedTextRect = rangedSlotTextObj.GetComponent<RectTransform>();
            rangedTextRect.anchorMin = Vector2.zero;
            rangedTextRect.anchorMax = Vector2.one;
            rangedTextRect.sizeDelta = Vector2.zero;

            // 4 Ô Ba Lô Dự Trữ
            Text[] backpackTexts = new Text[4];
            Image[] backpackBgs = new Image[4];

            for (int i = 0; i < 4; i++)
            {
                int slotIndex = i;
                GameObject bpSlotObj = new GameObject($"Slot_Backpack_{i + 1}");
                bpSlotObj.transform.SetParent(invPanel.transform, false);
                RectTransform bpRect = bpSlotObj.AddComponent<RectTransform>();
                bpRect.anchorMin = new Vector2(0, 1);
                bpRect.anchorMax = new Vector2(0, 1);
                bpRect.pivot = new Vector2(0, 1);
                bpRect.anchoredPosition = new Vector2(startX + (slotWidth + 8f) * (2 + i), posY);
                bpRect.sizeDelta = new Vector2(slotWidth, slotHeight);

                Image bpBg = bpSlotObj.AddComponent<Image>();
                bpBg.color = new Color(0.14f, 0.15f, 0.2f, 0.65f);
                backpackBgs[i] = bpBg;

                GameObject bpTextObj = new GameObject("Text");
                bpTextObj.transform.SetParent(bpSlotObj.transform, false);
                Text bpText = bpTextObj.AddComponent<Text>();
                bpText.font = defaultFont;
                bpText.fontSize = 11;
                bpText.alignment = TextAnchor.MiddleCenter;
                bpText.color = new Color(0.8f, 0.8f, 0.8f);
                bpText.text = $"Ô {i + 1}\n(Trống)";
                RectTransform bpTextRect = bpTextObj.GetComponent<RectTransform>();
                bpTextRect.anchorMin = Vector2.zero;
                bpTextRect.anchorMax = Vector2.one;
                bpTextRect.sizeDelta = Vector2.zero;
                backpackTexts[i] = bpText;
            }

            // Banner thông báo nhặt đồ mới nổi bật
            GameObject bannerObj = new GameObject("Loot_Notification_Banner");
            bannerObj.transform.SetParent(invPanel.transform, false);
            RectTransform bannerRect = bannerObj.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 0);
            bannerRect.anchorMax = new Vector2(1, 0);
            bannerRect.pivot = new Vector2(0.5f, 0);
            bannerRect.anchoredPosition = new Vector2(0, 4);
            bannerRect.sizeDelta = new Vector2(0, 24);

            Text bannerText = bannerObj.AddComponent<Text>();
            bannerText.font = defaultFont;
            bannerText.fontSize = 13;
            bannerText.fontStyle = FontStyle.Bold;
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.color = Color.yellow;
            bannerText.text = "";
            bannerObj.SetActive(false);

            RunInventoryHUD invHUD = invPanel.AddComponent<RunInventoryHUD>();
            invHUD.Bind(runInventory, goldText, meleeSlotText, rangedSlotText, backpackTexts, backpackBgs, bannerText);

            // 3. Hướng dẫn phím ở góc trên bên trái (dưới thanh Máu, không đè lên Hồ sơ tài khoản & nút Balo)
            GameObject hintObj = new GameObject("Controls_Hint");
            hintObj.transform.SetParent(canvasObj.transform, false);
            Text hintText = hintObj.AddComponent<Text>();
            hintText.font = defaultFont;
            hintText.fontSize = 13;
            hintText.alignment = TextAnchor.UpperLeft;
            hintText.color = new Color(1f, 1f, 1f, 0.85f);
            hintText.text = "⌨️ WASD/Joystick: Di chuyển  |  ⚔️ Space/J: Cận chiến\n🏹 Tầm xa: Tự ngắm & bắn  |  🎒 Phím [B]: Mở Balo & Ghép đồ";
            RectTransform hintRect = hintObj.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0, 1);
            hintRect.anchorMax = new Vector2(0, 1);
            hintRect.pivot = new Vector2(0, 1);
            hintRect.anchoredPosition = new Vector2(30, -150);
            hintRect.sizeDelta = new Vector2(460, 48);

            // 3. Joystick Ảo góc dưới bên trái
            GameObject joystickObj = new GameObject("VirtualJoystick");
            joystickObj.transform.SetParent(canvasObj.transform, false);
            RectTransform joyRect = joystickObj.AddComponent<RectTransform>();
            joyRect.anchorMin = new Vector2(0, 0);
            joyRect.anchorMax = new Vector2(0, 0);
            joyRect.pivot = new Vector2(0.5f, 0.5f);
            joyRect.anchoredPosition = new Vector2(180, 180);
            joyRect.sizeDelta = new Vector2(180, 180);

            Image joyRing = joystickObj.AddComponent<Image>();
            joyRing.color = new Color(1f, 1f, 1f, 0.22f);

            GameObject handleObj = new GameObject("Joystick_Handle");
            handleObj.transform.SetParent(joystickObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(70, 70);
            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.color = new Color(1f, 0.85f, 0.25f, 0.75f);

            VirtualJoystick joystick = joystickObj.AddComponent<VirtualJoystick>();
            joystick.SetPlayerController(player.GetComponent<PlayerController>());

            // 4. Nút Đánh Cận Chiến (Melee Attack Button) góc dưới bên phải
            GameObject attackBtnObj = new GameObject("MeleeAttackButton");
            attackBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform attackRect = attackBtnObj.AddComponent<RectTransform>();
            attackRect.anchorMin = new Vector2(1, 0);
            attackRect.anchorMax = new Vector2(1, 0);
            attackRect.pivot = new Vector2(0.5f, 0.5f);
            attackRect.anchoredPosition = new Vector2(-190, 180);
            attackRect.sizeDelta = new Vector2(150, 150);

            Image attackBg = attackBtnObj.AddComponent<Image>();
            attackBg.color = new Color(0.95f, 0.35f, 0.12f, 0.95f);
            Button attackBtn = attackBtnObj.AddComponent<Button>();

            // Cooldown Overlay
            GameObject cdObj = new GameObject("Cooldown_Overlay");
            cdObj.transform.SetParent(attackBtnObj.transform, false);
            Image cdImg = cdObj.AddComponent<Image>();
            cdImg.color = new Color(0, 0, 0, 0.7f);
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;
            cdImg.fillAmount = 0f;
            RectTransform cdRect = cdObj.GetComponent<RectTransform>();
            cdRect.anchorMin = Vector2.zero;
            cdRect.anchorMax = Vector2.one;
            cdRect.sizeDelta = Vector2.zero;

            GameObject attackTextObj = new GameObject("Attack_Label");
            attackTextObj.transform.SetParent(attackBtnObj.transform, false);
            Text attackLabel = attackTextObj.AddComponent<Text>();
            attackLabel.font = defaultFont;
            attackLabel.fontSize = 24;
            attackLabel.fontStyle = FontStyle.Bold;
            attackLabel.alignment = TextAnchor.MiddleCenter;
            attackLabel.color = Color.white;
            attackLabel.text = "CHÉM\n(Space)";
            RectTransform labelRect = attackTextObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            // 5. Nút Chuyển Đổi Vũ Khí Cận Chiến (Carrot Blade <-> Carrot Axe)
            GameObject switchBtnObj = new GameObject("Switch_Melee_Button");
            switchBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform switchRect = switchBtnObj.AddComponent<RectTransform>();
            switchRect.anchorMin = new Vector2(1, 0);
            switchRect.anchorMax = new Vector2(1, 0);
            switchRect.pivot = new Vector2(0.5f, 0.5f);
            switchRect.anchoredPosition = new Vector2(-190, 290);
            switchRect.sizeDelta = new Vector2(210, 46);

            Image switchBg = switchBtnObj.AddComponent<Image>();
            switchBg.color = new Color(0.2f, 0.45f, 0.85f, 0.9f);
            Button switchBtn = switchBtnObj.AddComponent<Button>();

            GameObject switchTextObj = new GameObject("Switch_Label");
            switchTextObj.transform.SetParent(switchBtnObj.transform, false);
            Text switchLabel = switchTextObj.AddComponent<Text>();
            switchLabel.font = defaultFont;
            switchLabel.fontSize = 14;
            switchLabel.fontStyle = FontStyle.Bold;
            switchLabel.alignment = TextAnchor.MiddleCenter;
            switchLabel.color = Color.white;
            switchLabel.text = "Carrot Blade (Click để đổi Rìu)";
            RectTransform switchLabelRect = switchTextObj.GetComponent<RectTransform>();
            switchLabelRect.anchorMin = Vector2.zero;
            switchLabelRect.anchorMax = Vector2.one;
            switchLabelRect.sizeDelta = Vector2.zero;

            MeleeAttackButton attackComp = attackBtnObj.AddComponent<MeleeAttackButton>();
            var meleeFields = typeof(MeleeAttackButton).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in meleeFields)
            {
                if (f.Name == "meleeCombat") f.SetValue(attackComp, meleeCombat);
                if (f.Name == "attackButton") f.SetValue(attackComp, attackBtn);
                if (f.Name == "cooldownOverlay") f.SetValue(attackComp, cdImg);
                if (f.Name == "weaponNameText") f.SetValue(attackComp, switchLabel);
                if (f.Name == "switchWeaponButton") f.SetValue(attackComp, switchBtn);
            }

            // 6. KHỐI HIỂN THỊ VŨ KHÍ TẦM XA TỰ ĐỘNG (Ranged Weapon Display)
            GameObject rangedPanel = new GameObject("RangedWeaponPanel");
            rangedPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rangedRect = rangedPanel.AddComponent<RectTransform>();
            rangedRect.anchorMin = new Vector2(1, 0);
            rangedRect.anchorMax = new Vector2(1, 0);
            rangedRect.pivot = new Vector2(0.5f, 0.5f);
            rangedRect.anchoredPosition = new Vector2(-190, 390);
            rangedRect.sizeDelta = new Vector2(210, 110);

            Image rangedBg = rangedPanel.AddComponent<Image>();
            rangedBg.color = new Color(0.12f, 0.08f, 0.18f, 0.85f);

            // Tiêu đề vũ khí tầm xa
            GameObject rangedTitleObj = new GameObject("Ranged_Title");
            rangedTitleObj.transform.SetParent(rangedPanel.transform, false);
            Text rangedTitle = rangedTitleObj.AddComponent<Text>();
            rangedTitle.font = defaultFont;
            rangedTitle.fontSize = 14;
            rangedTitle.fontStyle = FontStyle.Bold;
            rangedTitle.alignment = TextAnchor.UpperCenter;
            rangedTitle.color = new Color(1f, 0.85f, 0.3f);
            rangedTitle.text = "🏹 Tomato Blaster";
            RectTransform rangedTitleRect = rangedTitleObj.GetComponent<RectTransform>();
            rangedTitleRect.anchorMin = new Vector2(0, 1);
            rangedTitleRect.anchorMax = new Vector2(1, 1);
            rangedTitleRect.pivot = new Vector2(0.5f, 1);
            rangedTitleRect.anchoredPosition = new Vector2(0, -8);
            rangedTitleRect.sizeDelta = new Vector2(-16, 26);

            // Trạng thái khóa mục tiêu
            GameObject rangedStatusObj = new GameObject("Ranged_Status");
            rangedStatusObj.transform.SetParent(rangedPanel.transform, false);
            Text rangedStatus = rangedStatusObj.AddComponent<Text>();
            rangedStatus.font = defaultFont;
            rangedStatus.fontSize = 12;
            rangedStatus.fontStyle = FontStyle.Bold;
            rangedStatus.alignment = TextAnchor.MiddleCenter;
            rangedStatus.color = Color.white;
            rangedStatus.text = "🔍 Đang quét mục tiêu...";
            RectTransform rangedStatusRect = rangedStatusObj.GetComponent<RectTransform>();
            rangedStatusRect.anchorMin = new Vector2(0, 0.5f);
            rangedStatusRect.anchorMax = new Vector2(1, 0.5f);
            rangedStatusRect.pivot = new Vector2(0.5f, 0.5f);
            rangedStatusRect.anchoredPosition = new Vector2(0, 5);
            rangedStatusRect.sizeDelta = new Vector2(-16, 20);

            // Nút đổi vũ khí tầm xa (Tomato <-> Grape)
            GameObject switchRangedBtnObj = new GameObject("Switch_Ranged_Button");
            switchRangedBtnObj.transform.SetParent(rangedPanel.transform, false);
            RectTransform switchRangedRect = switchRangedBtnObj.AddComponent<RectTransform>();
            switchRangedRect.anchorMin = new Vector2(0, 0);
            switchRangedRect.anchorMax = new Vector2(1, 0);
            switchRangedRect.pivot = new Vector2(0.5f, 0);
            switchRangedRect.anchoredPosition = new Vector2(0, 8);
            switchRangedRect.sizeDelta = new Vector2(-16, 32);

            Image switchRangedBg = switchRangedBtnObj.AddComponent<Image>();
            switchRangedBg.color = new Color(0.6f, 0.2f, 0.8f, 0.9f);
            Button switchRangedBtn = switchRangedBtnObj.AddComponent<Button>();

            GameObject switchRangedLabelObj = new GameObject("Switch_Ranged_Label");
            switchRangedLabelObj.transform.SetParent(switchRangedBtnObj.transform, false);
            Text switchRangedLabel = switchRangedLabelObj.AddComponent<Text>();
            switchRangedLabel.font = defaultFont;
            switchRangedLabel.fontSize = 12;
            switchRangedLabel.fontStyle = FontStyle.Bold;
            switchRangedLabel.alignment = TextAnchor.MiddleCenter;
            switchRangedLabel.color = Color.white;
            switchRangedLabel.text = "Đổi sang Grape Splitter";
            RectTransform switchRangedLabelRect = switchRangedLabelObj.GetComponent<RectTransform>();
            switchRangedLabelRect.anchorMin = Vector2.zero;
            switchRangedLabelRect.anchorMax = Vector2.one;
            switchRangedLabelRect.sizeDelta = Vector2.zero;

            RangedWeaponDisplay rangedDisplay = rangedPanel.AddComponent<RangedWeaponDisplay>();
            var rangedFields = typeof(RangedWeaponDisplay).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in rangedFields)
            {
                if (f.Name == "rangedController") f.SetValue(rangedDisplay, rangedController);
                if (f.Name == "weaponNameText") f.SetValue(rangedDisplay, rangedTitle);
                if (f.Name == "statusText") f.SetValue(rangedDisplay, rangedStatus);
                if (f.Name == "switchRangedButton") f.SetValue(rangedDisplay, switchRangedBtn);
            }

            // 7. Nút Mở Nghỉ Giữa Wave (Shop & Ghép Đồ)
            GameObject intermissionBtnObj = new GameObject("Open_Intermission_Button");
            intermissionBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform intermissionBtnRect = intermissionBtnObj.AddComponent<RectTransform>();
            intermissionBtnRect.anchorMin = new Vector2(1, 0);
            intermissionBtnRect.anchorMax = new Vector2(1, 0);
            intermissionBtnRect.pivot = new Vector2(0.5f, 0.5f);
            intermissionBtnRect.anchoredPosition = new Vector2(-190, 570);
            intermissionBtnRect.sizeDelta = new Vector2(210, 42);

            Image intermissionBtnBg = intermissionBtnObj.AddComponent<Image>();
            intermissionBtnBg.color = new Color(0.95f, 0.72f, 0.1f, 0.95f);
            Button intermissionOpenBtn = intermissionBtnObj.AddComponent<Button>();
            intermissionOpenBtn.onClick.AddListener(() =>
            {
                if (EquipmentBagUI.Instance != null)
                {
                    EquipmentBagUI.Instance.OpenBag();
                }
            });

            GameObject intermissionLabelObj = new GameObject("Intermission_Label");
            intermissionLabelObj.transform.SetParent(intermissionBtnObj.transform, false);
            Text intermissionLabel = intermissionLabelObj.AddComponent<Text>();
            intermissionLabel.font = defaultFont;
            intermissionLabel.fontSize = 14;
            intermissionLabel.fontStyle = FontStyle.Bold;
            intermissionLabel.alignment = TextAnchor.MiddleCenter;
            intermissionLabel.color = new Color(0.1f, 0.1f, 0.15f);
            intermissionLabel.text = "⏸️ Shop & Ghép Đồ";
            RectTransform intermissionLabelRect = intermissionLabelObj.GetComponent<RectTransform>();
            intermissionLabelRect.anchorMin = Vector2.zero;
            intermissionLabelRect.anchorMax = Vector2.one;
            intermissionLabelRect.sizeDelta = Vector2.zero;

            // 8. Nút Respawn Quái
            GameObject respawnBtnObj = new GameObject("Respawn_Enemies_Button");
            respawnBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform respawnRect = respawnBtnObj.AddComponent<RectTransform>();
            respawnRect.anchorMin = new Vector2(1, 0);
            respawnRect.anchorMax = new Vector2(1, 0);
            respawnRect.pivot = new Vector2(0.5f, 0.5f);
            respawnRect.anchoredPosition = new Vector2(-190, 520);
            respawnRect.sizeDelta = new Vector2(210, 38);

            Image respawnBg = respawnBtnObj.AddComponent<Image>();
            respawnBg.color = new Color(0.15f, 0.65f, 0.35f, 0.9f);
            Button respawnBtn = respawnBtnObj.AddComponent<Button>();
            respawnBtn.onClick.AddListener(() =>
            {
                SpawnTestEnemies();
            });

            GameObject respawnTextObj = new GameObject("Respawn_Label");
            respawnTextObj.transform.SetParent(respawnBtnObj.transform, false);
            Text respawnLabel = respawnTextObj.AddComponent<Text>();
            respawnLabel.font = defaultFont;
            respawnLabel.fontSize = 14;
            respawnLabel.fontStyle = FontStyle.Bold;
            respawnLabel.alignment = TextAnchor.MiddleCenter;
            respawnLabel.color = Color.white;
            respawnLabel.text = "+ Thêm Zombie Carrot";
            RectTransform respawnLabelRect = respawnTextObj.GetComponent<RectTransform>();
            respawnLabelRect.anchorMin = Vector2.zero;
            respawnLabelRect.anchorMax = Vector2.one;
            respawnLabelRect.sizeDelta = Vector2.zero;

            // 8. Bảng Thông Báo Thất Bại (Defeat Screen - Luật Roguelite)
            GameObject defeatOverlay = new GameObject("Defeat_Screen_Overlay");
            defeatOverlay.transform.SetParent(canvasObj.transform, false);
            RectTransform defeatRect = defeatOverlay.AddComponent<RectTransform>();
            defeatRect.anchorMin = Vector2.zero;
            defeatRect.anchorMax = Vector2.one;
            defeatRect.sizeDelta = Vector2.zero;

            Image defeatBg = defeatOverlay.AddComponent<Image>();
            defeatBg.color = new Color(0.04f, 0.04f, 0.07f, 0.92f);

            GameObject defeatCard = new GameObject("Defeat_Card");
            defeatCard.transform.SetParent(defeatOverlay.transform, false);
            RectTransform cardRect = defeatCard.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(620, 320);

            Image cardBg = defeatCard.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

            // Tiêu đề
            GameObject titleObj = new GameObject("Defeat_Title");
            titleObj.transform.SetParent(defeatCard.transform, false);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 28;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1.0f, 0.28f, 0.28f);
            titleTxt.text = "💀 CHIẾN BINH THỎ ĐÃ TỬ TRẬN!";
            RectTransform defeatTitleRect = titleObj.GetComponent<RectTransform>();
            defeatTitleRect.anchorMin = new Vector2(0, 1);
            defeatTitleRect.anchorMax = new Vector2(1, 1);
            defeatTitleRect.pivot = new Vector2(0.5f, 1);
            defeatTitleRect.anchoredPosition = new Vector2(0, -30);
            defeatTitleRect.sizeDelta = new Vector2(-40, 45);

            // Nội dung mô tả luật roguelite
            GameObject descObj = new GameObject("Defeat_Desc");
            descObj.transform.SetParent(defeatCard.transform, false);
            Text descTxt = descObj.AddComponent<Text>();
            descTxt.font = defaultFont;
            descTxt.fontSize = 17;
            descTxt.alignment = TextAnchor.MiddleCenter;
            descTxt.color = new Color(0.85f, 0.88f, 0.95f, 0.9f);
            descTxt.text = "Bạn đã bị bầy Zombie Carrot hạ gục trong trận chiến!\n\nTheo quy tắc Roguelite: Toàn bộ Vàng và Trang bị\nnhặt được trong lượt chơi này đã bị xóa.";
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.5f);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0, 5);
            descRect.sizeDelta = new Vector2(-40, 100);

            // Nút Chơi lại (Retry / Revive)
            GameObject retryBtnObj = new GameObject("Retry_Button");
            retryBtnObj.transform.SetParent(defeatCard.transform, false);
            RectTransform retryRect = retryBtnObj.AddComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.5f, 0);
            retryRect.anchorMax = new Vector2(0.5f, 0);
            retryRect.pivot = new Vector2(0.5f, 0);
            retryRect.anchoredPosition = new Vector2(0, 30);
            retryRect.sizeDelta = new Vector2(300, 50);

            Image retryBg = retryBtnObj.AddComponent<Image>();
            retryBg.color = new Color(0.18f, 0.72f, 0.35f, 0.95f);
            Button retryBtn = retryBtnObj.AddComponent<Button>();

            GameObject retryTextObj = new GameObject("Text");
            retryTextObj.transform.SetParent(retryBtnObj.transform, false);
            Text retryLabel = retryTextObj.AddComponent<Text>();
            retryLabel.font = defaultFont;
            retryLabel.fontSize = 18;
            retryLabel.fontStyle = FontStyle.Bold;
            retryLabel.alignment = TextAnchor.MiddleCenter;
            retryLabel.color = Color.white;
            retryLabel.text = "🔄 HỒI SINH & CHƠI LẠI RUN MỚI";
            RectTransform retryLabelRect = retryTextObj.GetComponent<RectTransform>();
            retryLabelRect.anchorMin = Vector2.zero;
            retryLabelRect.anchorMax = Vector2.one;
            retryLabelRect.sizeDelta = Vector2.zero;

            // Mặc định ẩn Defeat Screen
            defeatOverlay.SetActive(false);

            PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.OnPlayerDied += () =>
                {
                    if (ResultsScreenUI.Instance == null)
                    {
                        defeatOverlay.SetActive(true);
                    }
                };
            }

            retryBtn.onClick.AddListener(() =>
            {
                defeatOverlay.SetActive(false);
                if (WaveDirector.Instance != null)
                {
                    WaveDirector.Instance.RestartRun();
                }
                else
                {
                    if (pHealth != null)
                    {
                        pHealth.Revive(100f);
                    }
                    player.transform.position = new Vector3(0, 1f, 0);

                    if (runInventory != null)
                    {
                        runInventory.ClearRunData();
                    }

                    foreach (var z in FindObjectsByType<ZombieCarrotAI>(FindObjectsSortMode.None))
                    {
                        Destroy(z.gameObject);
                    }
                    foreach (var l in FindObjectsByType<LootPickup>(FindObjectsSortMode.None))
                    {
                        Destroy(l.gameObject);
                    }

                    LootDropper.ResetPitySystem();
                    SpawnTestEnemies();
                }
            });

            // 9. Màn Hình Nghỉ Giữa Wave & Ghép Vũ Khí đã được HỢP NHẤT vào Balo & Trang Bị Duy Nhất (BuildEquipmentBagUI)

            // 10. Xây dựng Thanh Máu Boss King Carrot (Gate 6)
            BuildBossHealthBarUI(canvasObj, defaultFont);

            // 11. Xây dựng Màn hình Chiến Thắng (Victory Screen - Gate 6)
            BuildVictoryScreenUI(canvasObj, defaultFont);

            // 12. Xây dựng Banner Thông Báo Wave (Gate 6)
            BuildWaveBannerUI(canvasObj, defaultFont, out GameObject bannerRoot, out Text bannerTitle, out Text bannerSubtitle);

            // 13. Khởi tạo & Liên kết WaveDirector (Gate 6)
            WaveDirector director = WaveDirector.Instance ?? FindFirstObjectByType<WaveDirector>();
            if (director == null)
            {
                GameObject directorObj = new GameObject("WaveDirector");
                director = directorObj.AddComponent<WaveDirector>();
            }
            director.BindUI(progressText, bannerRoot, bannerTitle, bannerSubtitle);

            // 14. Xây dựng HUD Hồ Sơ Tài Khoản & Năng Lượng (Gate 7)
            BuildAccountProfileHUD(canvasObj, defaultFont);

            // 15. Xây dựng Màn hình Kết Quả Roguelite (Gate 7)
            BuildResultsScreenUI(canvasObj, defaultFont);

            // 16. Xây dựng Màn hình Balo & Trang Bị Tổng (Survivor/Archero Meta Equipment Bag)
            BuildEquipmentBagUI(canvasObj, defaultFont);
        }

        private void BuildIntermissionUI(GameObject canvasObj, Font defaultFont, RunInventory runInventory)
        {
            GameObject overlay = new GameObject("Intermission_Screen_Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            Image overlayBg = overlay.AddComponent<Image>();
            overlayBg.color = new Color(0.04f, 0.04f, 0.08f, 0.95f);

            GameObject card = new GameObject("Intermission_Card");
            card.transform.SetParent(overlay.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(1180, 640);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.09f, 0.13f, 0.98f);

            // Tiêu đề Header
            GameObject headerTitleObj = new GameObject("Header_Title");
            headerTitleObj.transform.SetParent(card.transform, false);
            Text headerTitle = headerTitleObj.AddComponent<Text>();
            headerTitle.font = defaultFont;
            headerTitle.fontSize = 22;
            headerTitle.fontStyle = FontStyle.Bold;
            headerTitle.alignment = TextAnchor.MiddleLeft;
            headerTitle.color = new Color(1.0f, 0.85f, 0.25f);
            headerTitle.text = "GIỜ NGHỈ GIỮA WAVE — GHÉP VŨ KHÍ & CỬA HÀNG";
            RectTransform headerTitleRect = headerTitleObj.GetComponent<RectTransform>();
            headerTitleRect.anchorMin = new Vector2(0, 1);
            headerTitleRect.anchorMax = new Vector2(0, 1);
            headerTitleRect.pivot = new Vector2(0, 1);
            headerTitleRect.anchoredPosition = new Vector2(30, -20);
            headerTitleRect.sizeDelta = new Vector2(600, 35);

            // Vàng Header
            GameObject headerGoldObj = new GameObject("Header_Gold");
            headerGoldObj.transform.SetParent(card.transform, false);
            Text headerGold = headerGoldObj.AddComponent<Text>();
            headerGold.font = defaultFont;
            headerGold.fontSize = 20;
            headerGold.fontStyle = FontStyle.Bold;
            headerGold.alignment = TextAnchor.MiddleRight;
            headerGold.color = new Color(1.0f, 0.85f, 0.25f);
            headerGold.text = "💰 VÀNG HIỆN CÓ: 0";
            RectTransform headerGoldRect = headerGoldObj.GetComponent<RectTransform>();
            headerGoldRect.anchorMin = new Vector2(1, 1);
            headerGoldRect.anchorMax = new Vector2(1, 1);
            headerGoldRect.pivot = new Vector2(1, 1);
            headerGoldRect.anchoredPosition = new Vector2(-30, -20);
            headerGoldRect.sizeDelta = new Vector2(350, 35);

            // ===== CỘT 1: TRANG BỊ ĐANG DÙNG (Trái) =====
            GameObject colEquipped = new GameObject("Col_Equipped");
            colEquipped.transform.SetParent(card.transform, false);
            RectTransform colEqRect = colEquipped.AddComponent<RectTransform>();
            colEqRect.anchorMin = new Vector2(0, 1);
            colEqRect.anchorMax = new Vector2(0, 1);
            colEqRect.pivot = new Vector2(0, 1);
            colEqRect.anchoredPosition = new Vector2(30, -70);
            colEqRect.sizeDelta = new Vector2(350, 470);

            Image colEqBg = colEquipped.AddComponent<Image>();
            colEqBg.color = new Color(0.12f, 0.13f, 0.18f, 0.85f);

            GameObject eqHeaderObj = new GameObject("Title");
            eqHeaderObj.transform.SetParent(colEquipped.transform, false);
            Text eqHeader = eqHeaderObj.AddComponent<Text>();
            eqHeader.font = defaultFont;
            eqHeader.fontSize = 15;
            eqHeader.fontStyle = FontStyle.Bold;
            eqHeader.alignment = TextAnchor.MiddleCenter;
            eqHeader.color = Color.white;
            eqHeader.text = "⚔️ TRANG BỊ ĐANG HOẠT ĐỘNG";
            RectTransform eqHeaderRect = eqHeaderObj.GetComponent<RectTransform>();
            eqHeaderRect.anchorMin = new Vector2(0, 1);
            eqHeaderRect.anchorMax = new Vector2(1, 1);
            eqHeaderRect.pivot = new Vector2(0.5f, 1);
            eqHeaderRect.anchoredPosition = new Vector2(0, -10);
            eqHeaderRect.sizeDelta = new Vector2(0, 30);

            // Card Cận Chiến
            GameObject meleeCardObj = new GameObject("Melee_Card");
            meleeCardObj.transform.SetParent(colEquipped.transform, false);
            RectTransform meleeCardRect = meleeCardObj.AddComponent<RectTransform>();
            meleeCardRect.anchorMin = new Vector2(0.5f, 1);
            meleeCardRect.anchorMax = new Vector2(0.5f, 1);
            meleeCardRect.pivot = new Vector2(0.5f, 1);
            meleeCardRect.anchoredPosition = new Vector2(0, -50);
            meleeCardRect.sizeDelta = new Vector2(320, 180);
            Image meleeBg = meleeCardObj.AddComponent<Image>();
            meleeBg.color = new Color(0.85f, 0.55f, 0.15f, 0.9f);

            GameObject meleeTitleObj = new GameObject("Title");
            meleeTitleObj.transform.SetParent(meleeCardObj.transform, false);
            Text meleeTitle = meleeTitleObj.AddComponent<Text>();
            meleeTitle.font = defaultFont;
            meleeTitle.fontSize = 17;
            meleeTitle.fontStyle = FontStyle.Bold;
            meleeTitle.color = Color.white;
            meleeTitle.text = "Carrot Blade (Lv.1)";
            RectTransform meleeTitleRect = meleeTitleObj.GetComponent<RectTransform>();
            meleeTitleRect.anchorMin = new Vector2(0, 1);
            meleeTitleRect.anchorMax = new Vector2(1, 1);
            meleeTitleRect.pivot = new Vector2(0, 1);
            meleeTitleRect.anchoredPosition = new Vector2(16, -14);
            meleeTitleRect.sizeDelta = new Vector2(-32, 30);

            GameObject meleeStatsObj = new GameObject("Stats");
            meleeStatsObj.transform.SetParent(meleeCardObj.transform, false);
            Text meleeStats = meleeStatsObj.AddComponent<Text>();
            meleeStats.font = defaultFont;
            meleeStats.fontSize = 13;
            meleeStats.color = Color.white;
            meleeStats.text = "Sát thương: 25 | Cooldown: 0.38s";
            RectTransform meleeStatsRect = meleeStatsObj.GetComponent<RectTransform>();
            meleeStatsRect.anchorMin = new Vector2(0, 0);
            meleeStatsRect.anchorMax = new Vector2(1, 1);
            meleeStatsRect.offsetMin = new Vector2(16, 12);
            meleeStatsRect.offsetMax = new Vector2(-16, -50);

            // Card Tầm Xa
            GameObject rangedCardObj = new GameObject("Ranged_Card");
            rangedCardObj.transform.SetParent(colEquipped.transform, false);
            RectTransform rangedCardRect = rangedCardObj.AddComponent<RectTransform>();
            rangedCardRect.anchorMin = new Vector2(0.5f, 1);
            rangedCardRect.anchorMax = new Vector2(0.5f, 1);
            rangedCardRect.pivot = new Vector2(0.5f, 1);
            rangedCardRect.anchoredPosition = new Vector2(0, -245);
            rangedCardRect.sizeDelta = new Vector2(320, 180);
            Image rangedBg = rangedCardObj.AddComponent<Image>();
            rangedBg.color = new Color(0.8f, 0.2f, 0.25f, 0.9f);

            GameObject rangedTitleObj = new GameObject("Title");
            rangedTitleObj.transform.SetParent(rangedCardObj.transform, false);
            Text rangedTitle = rangedTitleObj.AddComponent<Text>();
            rangedTitle.font = defaultFont;
            rangedTitle.fontSize = 17;
            rangedTitle.fontStyle = FontStyle.Bold;
            rangedTitle.color = Color.white;
            rangedTitle.text = "Tomato Blaster (Lv.1)";
            RectTransform rangedTitleRect = rangedTitleObj.GetComponent<RectTransform>();
            rangedTitleRect.anchorMin = new Vector2(0, 1);
            rangedTitleRect.anchorMax = new Vector2(1, 1);
            rangedTitleRect.pivot = new Vector2(0, 1);
            rangedTitleRect.anchoredPosition = new Vector2(16, -14);
            rangedTitleRect.sizeDelta = new Vector2(-32, 30);

            GameObject rangedStatsObj = new GameObject("Stats");
            rangedStatsObj.transform.SetParent(rangedCardObj.transform, false);
            Text rangedStats = rangedStatsObj.AddComponent<Text>();
            rangedStats.font = defaultFont;
            rangedStats.fontSize = 13;
            rangedStats.color = Color.white;
            rangedStats.text = "Sát thương: 22 | Tốc độ: 16 | Đạn: 1";
            RectTransform rangedStatsRect = rangedStatsObj.GetComponent<RectTransform>();
            rangedStatsRect.anchorMin = new Vector2(0, 0);
            rangedStatsRect.anchorMax = new Vector2(1, 1);
            rangedStatsRect.offsetMin = new Vector2(16, 12);
            rangedStatsRect.offsetMax = new Vector2(-16, -50);

            // ===== CỘT 2: BA LÔ & GHÉP ĐỒ (Giữa) =====
            GameObject colBackpack = new GameObject("Col_Backpack");
            colBackpack.transform.SetParent(card.transform, false);
            RectTransform colBagRect = colBackpack.AddComponent<RectTransform>();
            colBagRect.anchorMin = new Vector2(0, 1);
            colBagRect.anchorMax = new Vector2(0, 1);
            colBagRect.pivot = new Vector2(0, 1);
            colBagRect.anchoredPosition = new Vector2(400, -70);
            colBagRect.sizeDelta = new Vector2(370, 470);

            Image colBagBg = colBackpack.AddComponent<Image>();
            colBagBg.color = new Color(0.12f, 0.13f, 0.18f, 0.85f);

            GameObject bagHeaderObj = new GameObject("Title");
            bagHeaderObj.transform.SetParent(colBackpack.transform, false);
            Text bagHeader = bagHeaderObj.AddComponent<Text>();
            bagHeader.font = defaultFont;
            bagHeader.fontSize = 15;
            bagHeader.fontStyle = FontStyle.Bold;
            bagHeader.alignment = TextAnchor.MiddleCenter;
            bagHeader.color = Color.white;
            bagHeader.text = "🎒 BA LÔ DỰ TRỮ (4 Ô)";
            RectTransform bagHeaderRect = bagHeaderObj.GetComponent<RectTransform>();
            bagHeaderRect.anchorMin = new Vector2(0, 1);
            bagHeaderRect.anchorMax = new Vector2(1, 1);
            bagHeaderRect.pivot = new Vector2(0.5f, 1);
            bagHeaderRect.anchoredPosition = new Vector2(0, -10);
            bagHeaderRect.sizeDelta = new Vector2(0, 30);

            GameObject bagContainerObj = new GameObject("Bag_Container");
            bagContainerObj.transform.SetParent(colBackpack.transform, false);
            RectTransform bagContainerRect = bagContainerObj.AddComponent<RectTransform>();
            bagContainerRect.anchorMin = new Vector2(0.5f, 1);
            bagContainerRect.anchorMax = new Vector2(0.5f, 1);
            bagContainerRect.pivot = new Vector2(0.5f, 1);
            bagContainerRect.anchoredPosition = new Vector2(0, -48);
            bagContainerRect.sizeDelta = new Vector2(350, 330);
            VerticalLayoutGroup bagLayout = bagContainerObj.AddComponent<VerticalLayoutGroup>();
            bagLayout.spacing = 10;
            bagLayout.childControlHeight = false;
            bagLayout.childControlWidth = true;

            // Nút Ghép Tự Động (Auto Merge)
            GameObject mergeBtnObj = new GameObject("Auto_Merge_Button");
            mergeBtnObj.transform.SetParent(colBackpack.transform, false);
            RectTransform mergeBtnRect = mergeBtnObj.AddComponent<RectTransform>();
            mergeBtnRect.anchorMin = new Vector2(0.5f, 0);
            mergeBtnRect.anchorMax = new Vector2(0.5f, 0);
            mergeBtnRect.pivot = new Vector2(0.5f, 0);
            mergeBtnRect.anchoredPosition = new Vector2(0, 16);
            mergeBtnRect.sizeDelta = new Vector2(340, 48);

            Image mergeBtnBg = mergeBtnObj.AddComponent<Image>();
            mergeBtnBg.color = new Color(0.95f, 0.72f, 0.1f);
            Button autoMergeBtn = mergeBtnObj.AddComponent<Button>();

            GameObject mergeTextObj = new GameObject("Text");
            mergeTextObj.transform.SetParent(mergeBtnObj.transform, false);
            Text mergeText = mergeTextObj.AddComponent<Text>();
            mergeText.font = defaultFont;
            mergeText.fontSize = 14;
            mergeText.fontStyle = FontStyle.Bold;
            mergeText.alignment = TextAnchor.MiddleCenter;
            mergeText.color = new Color(0.1f, 0.1f, 0.15f);
            mergeText.text = "⚡ GHÉP VŨ KHÍ (AUTO MERGE)";
            RectTransform mergeTextRect = mergeTextObj.GetComponent<RectTransform>();
            mergeTextRect.anchorMin = Vector2.zero;
            mergeTextRect.anchorMax = Vector2.one;
            mergeTextRect.sizeDelta = Vector2.zero;

            // ===== CỘT 3: CỬA HÀNG RUN SHOP (Phải) =====
            GameObject colShop = new GameObject("Col_Shop");
            colShop.transform.SetParent(card.transform, false);
            RectTransform colShopRect = colShop.AddComponent<RectTransform>();
            colShopRect.anchorMin = new Vector2(0, 1);
            colShopRect.anchorMax = new Vector2(0, 1);
            colShopRect.pivot = new Vector2(0, 1);
            colShopRect.anchoredPosition = new Vector2(790, -70);
            colShopRect.sizeDelta = new Vector2(360, 470);

            Image colShopBg = colShop.AddComponent<Image>();
            colShopBg.color = new Color(0.12f, 0.13f, 0.18f, 0.85f);

            GameObject shopHeaderObj = new GameObject("Title");
            shopHeaderObj.transform.SetParent(colShop.transform, false);
            Text shopHeader = shopHeaderObj.AddComponent<Text>();
            shopHeader.font = defaultFont;
            shopHeader.fontSize = 15;
            shopHeader.fontStyle = FontStyle.Bold;
            shopHeader.alignment = TextAnchor.MiddleCenter;
            shopHeader.color = Color.white;
            shopHeader.text = "🏪 CỬA HÀNG RUN SHOP";
            RectTransform shopHeaderRect = shopHeaderObj.GetComponent<RectTransform>();
            shopHeaderRect.anchorMin = new Vector2(0, 1);
            shopHeaderRect.anchorMax = new Vector2(1, 1);
            shopHeaderRect.pivot = new Vector2(0.5f, 1);
            shopHeaderRect.anchoredPosition = new Vector2(0, -10);
            shopHeaderRect.sizeDelta = new Vector2(0, 30);

            GameObject shopContainerObj = new GameObject("Shop_Container");
            shopContainerObj.transform.SetParent(colShop.transform, false);
            RectTransform shopContainerRect = shopContainerObj.AddComponent<RectTransform>();
            shopContainerRect.anchorMin = new Vector2(0.5f, 1);
            shopContainerRect.anchorMax = new Vector2(0.5f, 1);
            shopContainerRect.pivot = new Vector2(0.5f, 1);
            shopContainerRect.anchoredPosition = new Vector2(0, -48);
            shopContainerRect.sizeDelta = new Vector2(340, 400);
            VerticalLayoutGroup shopLayout = shopContainerObj.AddComponent<VerticalLayoutGroup>();
            shopLayout.spacing = 8;
            shopLayout.childControlHeight = false;
            shopLayout.childControlWidth = true;

            // ===== FOOTER CARD: NOTICE & NÚT TIẾP TỤC =====
            GameObject noticeObj = new GameObject("Status_Notice");
            noticeObj.transform.SetParent(card.transform, false);
            Text noticeTxt = noticeObj.AddComponent<Text>();
            noticeTxt.font = defaultFont;
            noticeTxt.fontSize = 14;
            noticeTxt.alignment = TextAnchor.MiddleLeft;
            noticeTxt.color = Color.white;
            noticeTxt.text = "Trạm nghỉ giữa wave.";
            RectTransform noticeRect = noticeObj.GetComponent<RectTransform>();
            noticeRect.anchorMin = new Vector2(0, 0);
            noticeRect.anchorMax = new Vector2(0, 0);
            noticeRect.pivot = new Vector2(0, 0);
            noticeRect.anchoredPosition = new Vector2(30, 24);
            noticeRect.sizeDelta = new Vector2(650, 40);

            GameObject continueBtnObj = new GameObject("Continue_Button");
            continueBtnObj.transform.SetParent(card.transform, false);
            RectTransform continueRect = continueBtnObj.AddComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(1, 0);
            continueRect.anchorMax = new Vector2(1, 0);
            continueRect.pivot = new Vector2(1, 0);
            continueRect.anchoredPosition = new Vector2(-30, 18);
            continueRect.sizeDelta = new Vector2(320, 50);

            Image continueBg = continueBtnObj.AddComponent<Image>();
            continueBg.color = new Color(0.18f, 0.75f, 0.35f);
            Button continueBtn = continueBtnObj.AddComponent<Button>();

            GameObject continueTxtObj = new GameObject("Text");
            continueTxtObj.transform.SetParent(continueBtnObj.transform, false);
            Text continueTxt = continueTxtObj.AddComponent<Text>();
            continueTxt.font = defaultFont;
            continueTxt.fontSize = 17;
            continueTxt.fontStyle = FontStyle.Bold;
            continueTxt.alignment = TextAnchor.MiddleCenter;
            continueTxt.color = Color.white;
            continueTxt.text = "TIẾP TỤC CHIẾN ĐẤU ▶";
            RectTransform continueTxtRect = continueTxtObj.GetComponent<RectTransform>();
            continueTxtRect.anchorMin = Vector2.zero;
            continueTxtRect.anchorMax = Vector2.one;
            continueTxtRect.sizeDelta = Vector2.zero;

            // Khởi tạo component IntermissionUI
            IntermissionUI intermissionUI = overlay.AddComponent<IntermissionUI>();
            intermissionUI.Bind(
                overlay,
                headerGold,
                meleeTitle, meleeStats, meleeBg,
                rangedTitle, rangedStats, rangedBg,
                bagContainerObj.transform,
                autoMergeBtn, mergeText,
                shopContainerObj.transform,
                continueBtn,
                noticeTxt,
                defaultFont
            );

            overlay.SetActive(false);
        }

        private void BuildBossHealthBarUI(GameObject canvasObj, Font defaultFont)
        {
            GameObject bossBarObj = new GameObject("Boss_Health_Bar_Overlay");
            bossBarObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bossBarRect = bossBarObj.AddComponent<RectTransform>();
            bossBarRect.anchorMin = new Vector2(0.5f, 1);
            bossBarRect.anchorMax = new Vector2(0.5f, 1);
            bossBarRect.pivot = new Vector2(0.5f, 1);
            bossBarRect.anchoredPosition = new Vector2(0, -145);
            bossBarRect.sizeDelta = new Vector2(580, 54);

            Image bossBarBg = bossBarObj.AddComponent<Image>();
            bossBarBg.color = new Color(0.08f, 0.08f, 0.12f, 0.94f);

            // Title
            GameObject bossTitleObj = new GameObject("Title");
            bossTitleObj.transform.SetParent(bossBarObj.transform, false);
            Text bossTitle = bossTitleObj.AddComponent<Text>();
            bossTitle.font = defaultFont;
            bossTitle.fontSize = 15;
            bossTitle.fontStyle = FontStyle.Bold;
            bossTitle.alignment = TextAnchor.MiddleCenter;
            bossTitle.color = new Color(1f, 0.85f, 0.2f);
            bossTitle.text = "👑 CHÚA TỂ CÀ RỐT — KING CARROT";
            RectTransform bossTitleRect = bossTitleObj.GetComponent<RectTransform>();
            bossTitleRect.anchorMin = new Vector2(0, 1);
            bossTitleRect.anchorMax = new Vector2(1, 1);
            bossTitleRect.pivot = new Vector2(0.5f, 1);
            bossTitleRect.anchoredPosition = new Vector2(0, -5);
            bossTitleRect.sizeDelta = new Vector2(0, 20);

            // Health bar container
            GameObject bossHpBgObj = new GameObject("Bar_Bg");
            bossHpBgObj.transform.SetParent(bossBarObj.transform, false);
            RectTransform bossHpBgRect = bossHpBgObj.AddComponent<RectTransform>();
            bossHpBgRect.anchorMin = new Vector2(0, 0);
            bossHpBgRect.anchorMax = new Vector2(1, 0);
            bossHpBgRect.pivot = new Vector2(0.5f, 0);
            bossHpBgRect.anchoredPosition = new Vector2(0, 8);
            bossHpBgRect.sizeDelta = new Vector2(-28, 18);
            Image bossHpBg = bossHpBgObj.AddComponent<Image>();
            bossHpBg.color = new Color(0.2f, 0.05f, 0.05f, 0.9f);

            // Ghost fill
            GameObject bossGhostObj = new GameObject("Bar_Ghost");
            bossGhostObj.transform.SetParent(bossHpBgObj.transform, false);
            RectTransform bossGhostRect = bossGhostObj.AddComponent<RectTransform>();
            bossGhostRect.anchorMin = Vector2.zero;
            bossGhostRect.anchorMax = Vector2.one;
            bossGhostRect.sizeDelta = Vector2.zero;
            Image bossGhost = bossGhostObj.AddComponent<Image>();
            bossGhost.color = new Color(1f, 0.75f, 0.2f, 0.8f);
            bossGhost.type = Image.Type.Filled;
            bossGhost.fillMethod = Image.FillMethod.Horizontal;

            // Fill
            GameObject bossFillObj = new GameObject("Bar_Fill");
            bossFillObj.transform.SetParent(bossHpBgObj.transform, false);
            RectTransform bossFillRect = bossFillObj.AddComponent<RectTransform>();
            bossFillRect.anchorMin = Vector2.zero;
            bossFillRect.anchorMax = Vector2.one;
            bossFillRect.sizeDelta = Vector2.zero;
            Image bossFill = bossFillObj.AddComponent<Image>();
            bossFill.color = new Color(0.95f, 0.2f, 0.15f);
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;

            // HP Text
            GameObject bossHpTxtObj = new GameObject("HP_Text");
            bossHpTxtObj.transform.SetParent(bossHpBgObj.transform, false);
            Text bossHpTxt = bossHpTxtObj.AddComponent<Text>();
            bossHpTxt.font = defaultFont;
            bossHpTxt.fontSize = 13;
            bossHpTxt.fontStyle = FontStyle.Bold;
            bossHpTxt.alignment = TextAnchor.MiddleCenter;
            bossHpTxt.color = Color.white;
            bossHpTxt.text = "480 / 480 HP";
            RectTransform bossHpTxtRect = bossHpTxtObj.GetComponent<RectTransform>();
            bossHpTxtRect.anchorMin = Vector2.zero;
            bossHpTxtRect.anchorMax = Vector2.one;
            bossHpTxtRect.sizeDelta = Vector2.zero;

            BossHealthBarUI bossBarUI = bossBarObj.AddComponent<BossHealthBarUI>();
            bossBarUI.Bind(bossBarObj, bossTitle, bossHpTxt, bossFill, bossGhost);
        }

        private void BuildVictoryScreenUI(GameObject canvasObj, Font defaultFont)
        {
            GameObject victoryOverlay = new GameObject("Victory_Screen_Overlay");
            victoryOverlay.transform.SetParent(canvasObj.transform, false);
            RectTransform vicOverlayRect = victoryOverlay.AddComponent<RectTransform>();
            vicOverlayRect.anchorMin = Vector2.zero;
            vicOverlayRect.anchorMax = Vector2.one;
            vicOverlayRect.sizeDelta = Vector2.zero;
            Image vicOverlayBg = victoryOverlay.AddComponent<Image>();
            vicOverlayBg.color = new Color(0.04f, 0.04f, 0.08f, 0.94f);

            GameObject vicCard = new GameObject("Victory_Card");
            vicCard.transform.SetParent(victoryOverlay.transform, false);
            RectTransform vicCardRect = vicCard.AddComponent<RectTransform>();
            vicCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            vicCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            vicCardRect.pivot = new Vector2(0.5f, 0.5f);
            vicCardRect.sizeDelta = new Vector2(620, 520);
            Image vicCardBg = vicCard.AddComponent<Image>();
            vicCardBg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

            GameObject vicTitleObj = new GameObject("Title");
            vicTitleObj.transform.SetParent(vicCard.transform, false);
            Text vicTitle = vicTitleObj.AddComponent<Text>();
            vicTitle.font = defaultFont;
            vicTitle.fontSize = 26;
            vicTitle.fontStyle = FontStyle.Bold;
            vicTitle.alignment = TextAnchor.MiddleCenter;
            vicTitle.color = new Color(1f, 0.85f, 0.2f);
            vicTitle.text = "🏆 CHIẾN THẮNG HUY HOÀNG! 🏆";
            RectTransform vicTitleRect = vicTitleObj.GetComponent<RectTransform>();
            vicTitleRect.anchorMin = new Vector2(0, 1);
            vicTitleRect.anchorMax = new Vector2(1, 1);
            vicTitleRect.pivot = new Vector2(0.5f, 1);
            vicTitleRect.anchoredPosition = new Vector2(0, -25);
            vicTitleRect.sizeDelta = new Vector2(0, 40);

            GameObject vicSubObj = new GameObject("Subtitle");
            vicSubObj.transform.SetParent(vicCard.transform, false);
            Text vicSub = vicSubObj.AddComponent<Text>();
            vicSub.font = defaultFont;
            vicSub.fontSize = 15;
            vicSub.alignment = TextAnchor.MiddleCenter;
            vicSub.color = new Color(0.85f, 0.9f, 0.95f);
            vicSub.text = "Bạn đã tiêu diệt Chúa Tể Cà Rốt và hoàn thành bản Demo!";
            RectTransform vicSubRect = vicSubObj.GetComponent<RectTransform>();
            vicSubRect.anchorMin = new Vector2(0, 1);
            vicSubRect.anchorMax = new Vector2(1, 1);
            vicSubRect.pivot = new Vector2(0.5f, 1);
            vicSubRect.anchoredPosition = new Vector2(0, -68);
            vicSubRect.sizeDelta = new Vector2(0, 25);

            GameObject vicStatsObj = new GameObject("Stats");
            vicStatsObj.transform.SetParent(vicCard.transform, false);
            Text vicStats = vicStatsObj.AddComponent<Text>();
            vicStats.font = defaultFont;
            vicStats.fontSize = 16;
            vicStats.alignment = TextAnchor.MiddleLeft;
            vicStats.color = Color.white;
            vicStats.text = "Tổng kết chiến tích...";
            RectTransform vicStatsRect = vicStatsObj.GetComponent<RectTransform>();
            vicStatsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vicStatsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vicStatsRect.pivot = new Vector2(0.5f, 0.5f);
            vicStatsRect.anchoredPosition = new Vector2(0, -5);
            vicStatsRect.sizeDelta = new Vector2(520, 240);

            GameObject vicBtnObj = new GameObject("Restart_Button");
            vicBtnObj.transform.SetParent(vicCard.transform, false);
            RectTransform vicBtnRect = vicBtnObj.AddComponent<RectTransform>();
            vicBtnRect.anchorMin = new Vector2(0.5f, 0);
            vicBtnRect.anchorMax = new Vector2(0.5f, 0);
            vicBtnRect.pivot = new Vector2(0.5f, 0);
            vicBtnRect.anchoredPosition = new Vector2(0, 25);
            vicBtnRect.sizeDelta = new Vector2(380, 52);
            Image vicBtnBg = vicBtnObj.AddComponent<Image>();
            vicBtnBg.color = new Color(0.18f, 0.75f, 0.35f);
            Button vicBtn = vicBtnObj.AddComponent<Button>();

            GameObject vicBtnTxtObj = new GameObject("Text");
            vicBtnTxtObj.transform.SetParent(vicBtnObj.transform, false);
            Text vicBtnTxt = vicBtnTxtObj.AddComponent<Text>();
            vicBtnTxt.font = defaultFont;
            vicBtnTxt.fontSize = 18;
            vicBtnTxt.fontStyle = FontStyle.Bold;
            vicBtnTxt.alignment = TextAnchor.MiddleCenter;
            vicBtnTxt.color = Color.white;
            vicBtnTxt.text = "CHƠI LẠI RUN MỚI (RESTART) 🔄";
            RectTransform vicBtnTxtRect = vicBtnTxtObj.GetComponent<RectTransform>();
            vicBtnTxtRect.anchorMin = Vector2.zero;
            vicBtnTxtRect.anchorMax = Vector2.one;
            vicBtnTxtRect.sizeDelta = Vector2.zero;

            VictoryScreenUI victoryUI = victoryOverlay.AddComponent<VictoryScreenUI>();
            victoryUI.Bind(victoryOverlay, vicTitle, vicStats, vicBtn);
        }

        private void BuildWaveBannerUI(GameObject canvasObj, Font defaultFont, out GameObject bannerRoot, out Text bannerTitle, out Text bannerSubtitle)
        {
            bannerRoot = new GameObject("Wave_Banner_Center");
            bannerRoot.transform.SetParent(canvasObj.transform, false);
            RectTransform bannerRect = bannerRoot.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.72f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.72f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta = new Vector2(680, 75);

            Image bannerBg = bannerRoot.AddComponent<Image>();
            bannerBg.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);

            GameObject bTitleObj = new GameObject("Banner_Title");
            bTitleObj.transform.SetParent(bannerRoot.transform, false);
            bannerTitle = bTitleObj.AddComponent<Text>();
            bannerTitle.font = defaultFont;
            bannerTitle.fontSize = 22;
            bannerTitle.fontStyle = FontStyle.Bold;
            bannerTitle.alignment = TextAnchor.MiddleCenter;
            bannerTitle.color = new Color(1f, 0.85f, 0.2f);
            bannerTitle.text = "WAVE 1 BẮT ĐẦU";
            RectTransform bTitleRect = bTitleObj.GetComponent<RectTransform>();
            bTitleRect.anchorMin = new Vector2(0, 0.5f);
            bTitleRect.anchorMax = new Vector2(1, 1);
            bTitleRect.pivot = new Vector2(0.5f, 0.5f);
            bTitleRect.anchoredPosition = new Vector2(0, 0);
            bTitleRect.sizeDelta = Vector2.zero;

            GameObject bSubObj = new GameObject("Banner_Subtitle");
            bSubObj.transform.SetParent(bannerRoot.transform, false);
            bannerSubtitle = bSubObj.AddComponent<Text>();
            bannerSubtitle.font = defaultFont;
            bannerSubtitle.fontSize = 14;
            bannerSubtitle.alignment = TextAnchor.MiddleCenter;
            bannerSubtitle.color = new Color(0.9f, 0.9f, 0.95f);
            bannerSubtitle.text = "Tiêu diệt bầy Zombie Carrot!";
            RectTransform bSubRect = bSubObj.GetComponent<RectTransform>();
            bSubRect.anchorMin = new Vector2(0, 0);
            bSubRect.anchorMax = new Vector2(1, 0.5f);
            bSubRect.pivot = new Vector2(0.5f, 0.5f);
            bSubRect.anchoredPosition = new Vector2(0, 0);
            bSubRect.sizeDelta = Vector2.zero;

            bannerRoot.SetActive(false);
        }

        private void EnsureAccountProgression()
        {
            if (AccountProgression.Instance == null)
            {
                GameObject progObj = new GameObject("AccountProgression_Manager");
                progObj.AddComponent<AccountProgression>();
            }
        }

        private void BuildAccountProfileHUD(GameObject canvasObj, Font defaultFont)
        {
            GameObject profilePanel = new GameObject("AccountProfile_HUD");
            profilePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform profileRect = profilePanel.AddComponent<RectTransform>();
            profileRect.anchorMin = new Vector2(1, 1);
            profileRect.anchorMax = new Vector2(1, 1);
            profileRect.pivot = new Vector2(1, 1);
            profileRect.anchoredPosition = new Vector2(-30, -30);
            profileRect.sizeDelta = new Vector2(340, 75);

            Image bg = profilePanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.14f, 0.85f);

            GameObject txtObj = new GameObject("Profile_Text");
            txtObj.transform.SetParent(profilePanel.transform, false);
            Text profileTxt = txtObj.AddComponent<Text>();
            profileTxt.font = defaultFont;
            profileTxt.fontSize = 14;
            profileTxt.fontStyle = FontStyle.Bold;
            profileTxt.alignment = TextAnchor.MiddleLeft;
            profileTxt.color = Color.white;
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(16, 8);
            txtRect.offsetMax = new Vector2(-16, -8);

            void RefreshProfileText()
            {
                var acc = AccountProgression.Instance?.Data;
                int lvl = acc != null ? acc.RabbitLevel : 1;
                int pGold = acc != null ? acc.PermanentGold : 0;
                int energy = acc != null ? acc.CurrentEnergy : 5;
                bool unlimited = acc != null && acc.UnlimitedEnergyForTesting;
                string energyStr = unlimited ? "⚡ Vô Hạn (Test)" : $"⚡ {energy}/5";

                profileTxt.text = $"🐰 Thỏ Cấp: {lvl}  |  Vàng TK: {pGold} 💰\n" +
                                  $"Năng lượng: {energyStr}  |  Lưu: JSON OK";
            }

            RefreshProfileText();

            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged += RefreshProfileText;
            }

            // Nút Mở Balo & Trang Bị (Survivor.io / Archero Bag)
            GameObject bagBtnObj = new GameObject("Btn_Open_EquipmentBag");
            bagBtnObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bagBtnRect = bagBtnObj.AddComponent<RectTransform>();
            bagBtnRect.anchorMin = new Vector2(1, 1);
            bagBtnRect.anchorMax = new Vector2(1, 1);
            bagBtnRect.pivot = new Vector2(1, 1);
            bagBtnRect.anchoredPosition = new Vector2(-30, -112);
            bagBtnRect.sizeDelta = new Vector2(340, 46);

            Image bagBtnBg = bagBtnObj.AddComponent<Image>();
            bagBtnBg.color = new Color(0.12f, 0.45f, 0.85f, 0.95f); // Xanh dương tươi sáng nổi bật

            GameObject bagBtnTxtObj = new GameObject("Text");
            bagBtnTxtObj.transform.SetParent(bagBtnObj.transform, false);
            RectTransform btnTxtRect = bagBtnTxtObj.AddComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.sizeDelta = Vector2.zero;

            Text bagBtnTxt = bagBtnTxtObj.AddComponent<Text>();
            bagBtnTxt.font = defaultFont;
            bagBtnTxt.fontSize = 16;
            bagBtnTxt.fontStyle = FontStyle.Bold;
            bagBtnTxt.alignment = TextAnchor.MiddleCenter;
            bagBtnTxt.color = Color.white;
            bagBtnTxt.text = "🎒 BALO & TRANG BỊ [B]";

            Button bagBtn = bagBtnObj.AddComponent<Button>();
            bagBtn.onClick.AddListener(() => {
                if (EquipmentBagUI.Instance != null)
                {
                    EquipmentBagUI.Instance.ToggleBag();
                }
            });

            void UpdateBagBtnState()
            {
                if (bagBtnTxt == null) return;
                int count = EquipmentBagUI.UnviewedNewItemsCount;
                if (count > 0)
                {
                    bagBtnTxt.text = $"🎒 BALO ({count} MỚI 🔴) [B]";
                    bagBtnBg.color = new Color(0.95f, 0.25f, 0.2f, 0.98f);
                }
                else
                {
                    bagBtnTxt.text = "🎒 BALO & TRANG BỊ [B]";
                    bagBtnBg.color = new Color(0.12f, 0.45f, 0.85f, 0.95f);
                }
            }

            EquipmentBagUI.OnRecentlyCollectedChanged += UpdateBagBtnState;
            UpdateBagBtnState();
        }

        private void BuildEquipmentBagUI(GameObject canvasObj, Font defaultFont)
        {
            EquipmentBagUI.CreateEquipmentBagUI(canvasObj, defaultFont);
        }

        private void BuildResultsScreenUI(GameObject canvasObj, Font defaultFont)
        {
            GameObject overlay = new GameObject("Results_Screen_Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            Image overlayBg = overlay.AddComponent<Image>();
            overlayBg.color = new Color(0.04f, 0.04f, 0.08f, 0.95f);

            GameObject card = new GameObject("Results_Card");
            card.transform.SetParent(overlay.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760, 580);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.09f, 0.11f, 0.15f, 0.98f);

            // Banner trên cùng
            GameObject bannerObj = new GameObject("Header_Banner");
            bannerObj.transform.SetParent(card.transform, false);
            RectTransform bannerRect = bannerObj.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 1);
            bannerRect.anchorMax = new Vector2(1, 1);
            bannerRect.pivot = new Vector2(0.5f, 1);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta = new Vector2(0, 80);
            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.12f, 0.45f, 0.2f, 0.95f);

            GameObject titleObj = new GameObject("Outcome_Title");
            titleObj.transform.SetParent(bannerObj.transform, false);
            Text outcomeTitle = titleObj.AddComponent<Text>();
            outcomeTitle.font = defaultFont;
            outcomeTitle.fontSize = 24;
            outcomeTitle.fontStyle = FontStyle.Bold;
            outcomeTitle.alignment = TextAnchor.MiddleCenter;
            outcomeTitle.color = new Color(1f, 0.85f, 0.2f);
            outcomeTitle.text = "🏆 CHIẾN THẮNG HUY HOÀNG!";
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.4f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;

            GameObject subObj = new GameObject("Outcome_Subtitle");
            subObj.transform.SetParent(bannerObj.transform, false);
            Text outcomeSub = subObj.AddComponent<Text>();
            outcomeSub.font = defaultFont;
            outcomeSub.fontSize = 14;
            outcomeSub.alignment = TextAnchor.MiddleCenter;
            outcomeSub.color = new Color(0.9f, 0.95f, 1f);
            outcomeSub.text = "Bạn đã hoàn thành toàn bộ đợt quái!";
            RectTransform subRect = subObj.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0);
            subRect.anchorMax = new Vector2(1, 0.45f);
            subRect.sizeDelta = Vector2.zero;

            // Cột Trái: Chi tiết chiến tích lượt chơi
            GameObject statsObj = new GameObject("Run_Stats_Panel");
            statsObj.transform.SetParent(card.transform, false);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 1);
            statsRect.pivot = new Vector2(0, 1);
            statsRect.anchoredPosition = new Vector2(30, -95);
            statsRect.sizeDelta = new Vector2(330, 260);

            Text statsText = statsObj.AddComponent<Text>();
            statsText.font = defaultFont;
            statsText.fontSize = 15;
            statsText.lineSpacing = 1.35f;
            statsText.color = Color.white;
            statsText.text = "Chi tiết chiến tích...";

            // Cột Phải: Tiến trình tài khoản lâu dài
            GameObject progPanel = new GameObject("Account_Prog_Panel");
            progPanel.transform.SetParent(card.transform, false);
            RectTransform progPanelRect = progPanel.AddComponent<RectTransform>();
            progPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            progPanelRect.anchorMax = new Vector2(1, 1);
            progPanelRect.pivot = new Vector2(0, 1);
            progPanelRect.anchoredPosition = new Vector2(20, -95);
            progPanelRect.sizeDelta = new Vector2(330, 260);

            Image progBg = progPanel.AddComponent<Image>();
            progBg.color = new Color(0.13f, 0.15f, 0.22f, 0.85f);

            GameObject lvlObj = new GameObject("Level_Text");
            lvlObj.transform.SetParent(progPanel.transform, false);
            Text lvlText = lvlObj.AddComponent<Text>();
            lvlText.font = defaultFont;
            lvlText.fontSize = 16;
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.color = new Color(1f, 0.85f, 0.25f);
            lvlText.text = "CẤP THỎ: 1  |  VÀNG TK: 0 💰";
            RectTransform lvlRect = lvlObj.GetComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(0, 1);
            lvlRect.anchorMax = new Vector2(1, 1);
            lvlRect.pivot = new Vector2(0.5f, 1);
            lvlRect.anchoredPosition = new Vector2(16, -14);
            lvlRect.sizeDelta = new Vector2(-32, 26);

            // XP Bar BG
            GameObject xpBgObj = new GameObject("XP_Bar_Bg");
            xpBgObj.transform.SetParent(progPanel.transform, false);
            RectTransform xpBgRect = xpBgObj.AddComponent<RectTransform>();
            xpBgRect.anchorMin = new Vector2(0, 1);
            xpBgRect.anchorMax = new Vector2(1, 1);
            xpBgRect.pivot = new Vector2(0.5f, 1);
            xpBgRect.anchoredPosition = new Vector2(16, -45);
            xpBgRect.sizeDelta = new Vector2(-32, 20);
            Image xpBg = xpBgObj.AddComponent<Image>();
            xpBg.color = new Color(0.06f, 0.08f, 0.12f, 0.9f);

            // XP Bar Fill
            GameObject xpFillObj = new GameObject("XP_Bar_Fill");
            xpFillObj.transform.SetParent(xpBgObj.transform, false);
            RectTransform xpFillRect = xpFillObj.AddComponent<RectTransform>();
            xpFillRect.anchorMin = Vector2.zero;
            xpFillRect.anchorMax = Vector2.one;
            xpFillRect.sizeDelta = Vector2.zero;
            Image xpFill = xpFillObj.AddComponent<Image>();
            xpFill.color = new Color(0.2f, 0.8f, 0.95f);
            xpFill.type = Image.Type.Filled;
            xpFill.fillMethod = Image.FillMethod.Horizontal;
            xpFill.fillAmount = 0.5f;

            // XP Text
            GameObject xpTxtObj = new GameObject("XP_Text");
            xpTxtObj.transform.SetParent(progPanel.transform, false);
            Text xpTxt = xpTxtObj.AddComponent<Text>();
            xpTxt.font = defaultFont;
            xpTxt.fontSize = 13;
            xpTxt.alignment = TextAnchor.MiddleCenter;
            xpTxt.color = Color.white;
            xpTxt.text = "XP: 50 / 100";
            RectTransform xpTxtRect = xpTxtObj.GetComponent<RectTransform>();
            xpTxtRect.anchorMin = new Vector2(0, 1);
            xpTxtRect.anchorMax = new Vector2(1, 1);
            xpTxtRect.pivot = new Vector2(0.5f, 1);
            xpTxtRect.anchoredPosition = new Vector2(16, -72);
            xpTxtRect.sizeDelta = new Vector2(-32, 22);

            // Phần thưởng nhận được
            GameObject rwdObj = new GameObject("Reward_Text");
            rwdObj.transform.SetParent(progPanel.transform, false);
            Text rwdTxt = rwdObj.AddComponent<Text>();
            rwdTxt.font = defaultFont;
            rwdTxt.fontSize = 14;
            rwdTxt.color = new Color(0.25f, 0.95f, 0.5f);
            rwdTxt.text = "🎁 Phần thưởng trận...";
            RectTransform rwdRect = rwdObj.GetComponent<RectTransform>();
            rwdRect.anchorMin = new Vector2(0, 0);
            rwdRect.anchorMax = new Vector2(1, 1);
            rwdRect.offsetMin = new Vector2(16, 12);
            rwdRect.offsetMax = new Vector2(-16, -100);

            // Thông báo Roguelite Banner
            GameObject noticeObj = new GameObject("Roguelite_Notice");
            noticeObj.transform.SetParent(card.transform, false);
            RectTransform noticeRect = noticeObj.AddComponent<RectTransform>();
            noticeRect.anchorMin = new Vector2(0, 0);
            noticeRect.anchorMax = new Vector2(1, 0);
            noticeRect.pivot = new Vector2(0.5f, 0);
            noticeRect.anchoredPosition = new Vector2(0, 80);
            noticeRect.sizeDelta = new Vector2(-60, 50);

            Text noticeTxt = noticeObj.AddComponent<Text>();
            noticeTxt.font = defaultFont;
            noticeTxt.fontSize = 13;
            noticeTxt.alignment = TextAnchor.MiddleCenter;
            noticeTxt.color = new Color(0.85f, 0.9f, 0.95f, 0.9f);
            noticeTxt.text = "🛡️ QUY TẮC ROGUELITE: Đồ nhặt trong run đã bị xóa; Cấp thỏ & thành tựu được lưu vĩnh viễn.";

            // Nút Chơi Lại (Play Again)
            GameObject playAgainBtnObj = new GameObject("Play_Again_Button");
            playAgainBtnObj.transform.SetParent(card.transform, false);
            RectTransform playAgainRect = playAgainBtnObj.AddComponent<RectTransform>();
            playAgainRect.anchorMin = new Vector2(0.5f, 0);
            playAgainRect.anchorMax = new Vector2(0.5f, 0);
            playAgainRect.pivot = new Vector2(0.5f, 0);
            playAgainRect.anchoredPosition = new Vector2(-130, 20);
            playAgainRect.sizeDelta = new Vector2(300, 48);

            Image playAgainBg = playAgainBtnObj.AddComponent<Image>();
            playAgainBg.color = new Color(0.18f, 0.75f, 0.35f);
            Button playAgainBtn = playAgainBtnObj.AddComponent<Button>();

            GameObject paTxtObj = new GameObject("Text");
            paTxtObj.transform.SetParent(playAgainBtnObj.transform, false);
            Text paTxt = paTxtObj.AddComponent<Text>();
            paTxt.font = defaultFont;
            paTxt.fontSize = 16;
            paTxt.fontStyle = FontStyle.Bold;
            paTxt.alignment = TextAnchor.MiddleCenter;
            paTxt.color = Color.white;
            paTxt.text = "CHƠI LẠI RUN MỚI 🔄";
            RectTransform paTxtRect = paTxtObj.GetComponent<RectTransform>();
            paTxtRect.anchorMin = Vector2.zero;
            paTxtRect.anchorMax = Vector2.one;
            paTxtRect.sizeDelta = Vector2.zero;

            // Nút Reset Tiến Trình (Dev QA Test)
            GameObject resetBtnObj = new GameObject("Reset_Prog_Button");
            resetBtnObj.transform.SetParent(card.transform, false);
            RectTransform resetRect = resetBtnObj.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.5f, 0);
            resetRect.anchorMax = new Vector2(0.5f, 0);
            resetRect.pivot = new Vector2(0.5f, 0);
            resetRect.anchoredPosition = new Vector2(170, 20);
            resetRect.sizeDelta = new Vector2(240, 48);

            Image resetBg = resetBtnObj.AddComponent<Image>();
            resetBg.color = new Color(0.35f, 0.38f, 0.45f);
            Button resetBtn = resetBtnObj.AddComponent<Button>();

            GameObject resTxtObj = new GameObject("Text");
            resTxtObj.transform.SetParent(resetBtnObj.transform, false);
            Text resTxt = resTxtObj.AddComponent<Text>();
            resTxt.font = defaultFont;
            resTxt.fontSize = 14;
            resTxt.fontStyle = FontStyle.Bold;
            resTxt.alignment = TextAnchor.MiddleCenter;
            resTxt.color = Color.white;
            resTxt.text = "XÓA SAVE (DEV RESET)";
            RectTransform resTxtRect = resTxtObj.GetComponent<RectTransform>();
            resTxtRect.anchorMin = Vector2.zero;
            resTxtRect.anchorMax = Vector2.one;
            resTxtRect.sizeDelta = Vector2.zero;

            ResultsScreenUI resultsUI = overlay.AddComponent<ResultsScreenUI>();
            resultsUI.Bind(
                overlay,
                outcomeTitle, outcomeSub, bannerBg,
                statsText,
                lvlText, xpTxt, xpFill, rwdTxt,
                noticeTxt,
                playAgainBtn,
                resetBtn
            );
        }
    }
}
