using System.Collections;
using UnityEngine;
using Apocarrot.Gameplay;
using Apocarrot.Inventory;
using Apocarrot.Progression;

namespace Apocarrot.Loot
{
    public enum LootType
    {
        Gold,
        Health,
        Weapon,
        Equipment
    }

    /// <summary>
    /// Đối tượng vật phẩm rơi trên mặt đất trong đấu trường (3D Loot Pickup).
    /// Tự xoay bồng bềnh, nam châm hút về phía Thỏ khi lại gần,
    /// và cộng trực tiếp vào Balo tài khoản (AccountProgression) hoặc Máu/Vàng.
    /// </summary>
    public class LootPickup : MonoBehaviour
    {
        [Header("Loot Properties")]
        [SerializeField] private LootType lootType = LootType.Gold;
        [SerializeField] private int goldAmount = 15;
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private WeaponItem weaponItem;
        [SerializeField] private EquipmentItem equipmentItem;

        [Header("Magnet & Pickup Settings")]
        [SerializeField] private float magnetRadius = 3.8f;
        [SerializeField] private float pickupRadius = 0.85f;
        [SerializeField] private float magnetSpeed = 9.5f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobFrequency = 3.5f;

        private Transform playerTarget;
        private Vector3 startPos;
        private float randomOffset;
        private bool isAttracting = false;
        private bool isCollected = false;

        public LootType Type => lootType;
        public int GoldAmount => goldAmount;
        public float HealAmount => healAmount;
        public WeaponItem WeaponItem => weaponItem;
        public EquipmentItem EquipmentItem => equipmentItem;

        private void Start()
        {
            startPos = transform.position;
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
            FindPlayer();
        }

        private void Update()
        {
            if (isCollected) return;

            // Tự xoay tròn bồng bềnh
            transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);

            if (playerTarget == null)
            {
                FindPlayer();
                BobInPlace();
                return;
            }

            float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // Nam châm hút về phía Thỏ khi lại gần
            if (distToPlayer <= magnetRadius)
            {
                isAttracting = true;
            }

            if (isAttracting)
            {
                Vector3 targetPos = playerTarget.position + Vector3.up * 0.4f;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetSpeed * Time.deltaTime);

                if (distToPlayer <= pickupRadius)
                {
                    TryCollect();
                }
            }
            else
            {
                BobInPlace();
            }
        }

        private void BobInPlace()
        {
            float newY = startPos.y + Mathf.Sin((Time.time * bobFrequency) + randomOffset) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void FindPlayer()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.GetComponent<PlayerController>() != null || other.GetComponentInParent<PlayerController>() != null)
            {
                TryCollect();
            }
        }

        private void TryCollect()
        {
            if (isCollected) return;

            switch (lootType)
            {
                case LootType.Gold:
                    if (RunInventory.Instance != null)
                    {
                        RunInventory.Instance.AddGold(goldAmount);
                    }
                    if (AccountProgression.Instance != null && AccountProgression.Instance.Data != null)
                    {
                        AccountProgression.Instance.Data.PermanentGold += goldAmount;
                    }
                    ShowPickupToast($"+{goldAmount} Vàng 💰", new Color(1f, 0.85f, 0.2f));
                    ExecuteCollect();
                    break;

                case LootType.Health:
                    PlayerHealth health = playerTarget != null ? playerTarget.GetComponent<PlayerHealth>() : FindFirstObjectByType<PlayerHealth>();
                    if (health != null && health.IsAlive)
                    {
                        health.Heal(healAmount);
                        ShowPickupToast($"+{healAmount} HP ❤️", new Color(0.2f, 0.95f, 0.35f));
                        ExecuteCollect();
                    }
                    break;

                case LootType.Equipment:
                    if (equipmentItem != null)
                    {
                        // 1. Thêm TRỰC TIẾP vào đầu danh sách Balo tài khoản
                        AccountProgression.Instance?.AddEquipment(equipmentItem);
                        Apocarrot.UI.EquipmentBagUI.MarkRecentlyCollected(equipmentItem.Uid);

                        // 2. Nếu là vũ khí, thêm vào cả RunInventory
                        if (equipmentItem.SlotType == EquipmentSlotType.Weapon && RunInventory.Instance != null)
                        {
                            WeaponItem wItem = ConvertEquipmentToWeapon(equipmentItem);
                            RunInventory.Instance.TryAddWeapon(wItem);
                        }

                        ShowPickupToast($"🎁 ĐÃ NHẶT: {equipmentItem.DisplayName}!\n(Đã vào Balo - Bấm phím 'B' hoặc nút 🎒 để xem)", equipmentItem.GetRarityColor());
                        ExecuteCollect();
                    }
                    break;

                case LootType.Weapon:
                    if (weaponItem != null)
                    {
                        // Thêm vào RunInventory
                        RunInventory.Instance?.TryAddWeapon(weaponItem);

                        // Tự động chuyển đổi thành EquipmentItem để lưu vào Balo tài khoản vĩnh viễn!
                        EquipmentItem eq = ConvertWeaponToEquipment(weaponItem);
                        AccountProgression.Instance?.AddEquipment(eq);
                        Apocarrot.UI.EquipmentBagUI.MarkRecentlyCollected(eq.Uid);

                        ShowPickupToast($"🎁 ĐÃ NHẶT: {weaponItem.DisplayName} Lv.{weaponItem.Level}!\n(Đã vào Balo - Bấm phím 'B' hoặc nút 🎒 để xem)", weaponItem.ItemColor);
                        ExecuteCollect();
                    }
                    break;
            }
        }

        private EquipmentItem ConvertWeaponToEquipment(WeaponItem wpn)
        {
            EquipmentRarity rarity = wpn.Level >= 3 ? EquipmentRarity.Epic : wpn.Level >= 2 ? EquipmentRarity.Rare : EquipmentRarity.Great;
            if (wpn.WeaponId.Contains("blade") || wpn.WeaponId.Contains("axe"))
            {
                return EquipmentItem.CreateCarrotBlade(rarity, wpn.Level);
            }
            else if (wpn.WeaponId.Contains("tomato"))
            {
                return EquipmentItem.CreateTomatoBlaster(rarity, wpn.Level);
            }
            else
            {
                return EquipmentItem.CreateGrapeCannon(rarity, wpn.Level);
            }
        }

        private WeaponItem ConvertEquipmentToWeapon(EquipmentItem eq)
        {
            if (eq.TemplateId.Contains("tomato"))
            {
                return WeaponItem.CreateTomatoBlaster(eq.Level);
            }
            else if (eq.TemplateId.Contains("grape"))
            {
                return WeaponItem.CreateGrapeSplitter(eq.Level);
            }
            else
            {
                return WeaponItem.CreateCarrotBlade(eq.Level);
            }
        }

        private void ExecuteCollect()
        {
            isCollected = true;
            Apocarrot.Audio.AudioManager.Instance?.PlayLootPickup();
            StartCoroutine(CollectPopRoutine());
        }

        private IEnumerator CollectPopRoutine()
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 initScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(initScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void ShowPickupToast(string message, Color color)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
            LootToastManager.ShowWorldToast(spawnPos, message, color);
        }

        // ========================== STATIC SPAWNERS ==========================
        public static GameObject CreateGoldPickup(Vector3 position, int gold)
        {
            GameObject obj = new GameObject($"Pickup_Gold_{gold}");
            obj.transform.position = position + Vector3.up * 0.35f;

            LootPickup pickup = obj.AddComponent<LootPickup>();
            pickup.lootType = LootType.Gold;
            pickup.goldAmount = gold;

            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.55f;

            GameObject coinVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coinVisual.name = "Coin_Mesh";
            coinVisual.transform.SetParent(obj.transform, false);
            coinVisual.transform.localScale = new Vector3(0.4f, 0.08f, 0.4f);
            coinVisual.transform.localRotation = Quaternion.Euler(60f, 0, 0);
            Destroy(coinVisual.GetComponent<Collider>());
            coinVisual.GetComponent<Renderer>().material.color = new Color(1.0f, 0.84f, 0.1f);

            return obj;
        }

        public static GameObject CreateHealthPickup(Vector3 position, float heal)
        {
            GameObject obj = new GameObject($"Pickup_Health_{heal}");
            obj.transform.position = position + Vector3.up * 0.4f;

            LootPickup pickup = obj.AddComponent<LootPickup>();
            pickup.lootType = LootType.Health;
            pickup.healAmount = heal;

            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;

            GameObject miniCarrot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            miniCarrot.name = "Snack_Body";
            miniCarrot.transform.SetParent(obj.transform, false);
            miniCarrot.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            Destroy(miniCarrot.GetComponent<Collider>());
            miniCarrot.GetComponent<Renderer>().material.color = new Color(0.2f, 0.95f, 0.35f);

            return obj;
        }

        public static GameObject CreateWeaponPickup(Vector3 position, WeaponItem weapon)
        {
            GameObject obj = new GameObject($"Pickup_Weapon_{weapon.DisplayName}_Lv{weapon.Level}");
            obj.transform.position = position + Vector3.up * 0.45f;

            LootPickup pickup = obj.AddComponent<LootPickup>();
            pickup.lootType = LootType.Weapon;
            pickup.weaponItem = weapon;

            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.75f;

            GameObject boxVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxVisual.name = "Crate_Body";
            boxVisual.transform.SetParent(obj.transform, false);
            boxVisual.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            Destroy(boxVisual.GetComponent<Collider>());
            boxVisual.GetComponent<Renderer>().material.color = weapon.ItemColor;

            return obj;
        }

        /// <summary>
        /// Tạo Rương Rơi Trang Bị phát sáng 3D theo Phẩm chất (Survivor.io style).
        /// Khi Thỏ nhặt sẽ được đưa TRỰC TIẾP vào Túi Đồ Balo!
        /// </summary>
        public static GameObject CreateEquipmentPickup(Vector3 position, EquipmentItem equipment)
        {
            GameObject obj = new GameObject($"Pickup_Equipment_{equipment.TemplateId}");
            obj.transform.position = position + Vector3.up * 0.45f;

            LootPickup pickup = obj.AddComponent<LootPickup>();
            pickup.lootType = LootType.Equipment;
            pickup.equipmentItem = equipment;

            SphereCollider sc = obj.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.85f;

            Color rarityCol = equipment.GetRarityColor();

            // Rương trang bị chính
            GameObject crateBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crateBody.name = "Crate_Body";
            crateBody.transform.SetParent(obj.transform, false);
            crateBody.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            Destroy(crateBody.GetComponent<Collider>());
            crateBody.GetComponent<Renderer>().material.color = rarityCol;

            // Nắp rương viền trắng phát sáng
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Crate_Trim";
            trim.transform.SetParent(obj.transform, false);
            trim.transform.localPosition = new Vector3(0, 0.35f, 0);
            trim.transform.localScale = new Vector3(0.72f, 0.14f, 0.72f);
            Destroy(trim.GetComponent<Collider>());
            trim.GetComponent<Renderer>().material.color = Color.white;

            // Cột sáng/Viên ngọc biểu tượng ở trên đỉnh rương
            GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gem.name = "Top_Gem";
            gem.transform.SetParent(obj.transform, false);
            gem.transform.localPosition = new Vector3(0, 0.62f, 0);
            gem.transform.localScale = new Vector3(0.28f, 0.32f, 0.28f);
            Destroy(gem.GetComponent<Collider>());
            gem.GetComponent<Renderer>().material.color = rarityCol;

            return obj;
        }
    }
}
