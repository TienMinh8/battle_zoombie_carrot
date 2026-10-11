using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Data;
using Apocarrot.Enemies;
using Apocarrot.Inventory;
using Apocarrot.Progression;

namespace Apocarrot.Loot
{
    /// <summary>
    /// Xử lý tỷ lệ rơi đồ (Loot Table) khi quái bị tiêu diệt.
    /// Hỗ trợ cơ chế bảo hiểm (Pity / Early Guarantee Drop) theo yêu cầu Gate 4:
    /// Đảm bảo người chơi luôn nhặt được ít nhất 1 vũ khí trùng loại sớm để chuẩn bị cho cơ chế ghép (Gate 5).
    /// </summary>
    public class LootDropper : MonoBehaviour
    {
        private static int totalEnemiesKilled = 0;
        private static bool earlyWeaponGuaranteeDropped = false;

        [Header("Data Config")]
        [SerializeField] private EnemyData enemyData;

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.OnDied += HandleEnemyDied;
            }
        }

        private void OnDestroy()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnDied -= HandleEnemyDied;
            }
        }

        public void Initialize(EnemyData data)
        {
            enemyData = data;
        }

        private void HandleEnemyDied(EnemyHealth deadEnemy)
        {
            Vector3 dropPosition = deadEnemy.transform.position;
            totalEnemiesKilled++;

            // 1. Kiểm tra cơ chế Pity / Early Drop Guarantee (Bảo hiểm rơi vũ khí & trang bị sớm để người chơi trải nghiệm Balo)
            if (!earlyWeaponGuaranteeDropped && totalEnemiesKilled >= 2)
            {
                earlyWeaponGuaranteeDropped = true;
                // Rơi 1 Vũ khí chất lượng Great
                EquipmentItem guaranteedWeapon = GetGuaranteedDuplicateEquipment();
                LootPickup.CreateEquipmentPickup(dropPosition + Vector3.right * 0.7f, guaranteedWeapon);

                // Rơi thêm 1 Áo Giáp Thỏ để trang bị tăng HP
                EquipmentItem guaranteedArmor = EquipmentItem.CreateRabbitVest(EquipmentRarity.Great, 1);
                LootPickup.CreateEquipmentPickup(dropPosition + Vector3.forward * 0.7f, guaranteedArmor);

                // Đồng thời thưởng luôn 1 bọc vàng khởi đầu
                int gold = Random.Range(20, 35);
                LootPickup.CreateGoldPickup(dropPosition + Vector3.left * 0.7f, gold);

                Debug.Log($"[LootDropper] Kích hoạt Early Guarantee Drop: Rơi {guaranteedWeapon.DisplayName}, {guaranteedArmor.DisplayName} và {gold} Vàng!");
                return;
            }

            // 2. Tính toán rơi đồ thông thường theo EnemyData
            if (enemyData == null)
            {
                enemyData = EnemyData.CreateBasic();
            }

            // Kiểm tra rơi Vàng
            float goldRoll = Random.value;
            if (goldRoll <= enemyData.GoldDropChance)
            {
                int goldAmount = Random.Range(enemyData.MinGold, enemyData.MaxGold + 1);
                LootPickup.CreateGoldPickup(dropPosition + GetRandomJitter(), goldAmount);
            }

            // Kiểm tra rơi Trang bị / Vũ khí (Balo)
            float eqRoll = Random.value;
            if (eqRoll <= enemyData.WeaponDropChance)
            {
                bool isElite = enemyData.VariantType == EnemyVariantType.Elite;
                EquipmentItem randomEq = GetRandomDropEquipment(isElite);
                LootPickup.CreateEquipmentPickup(dropPosition + GetRandomJitter(), randomEq);
            }

            // Kiểm tra rơi Hồi phục HP
            float healRoll = Random.value;
            if (healRoll <= enemyData.HealDropChance)
            {
                LootPickup.CreateHealthPickup(dropPosition + GetRandomJitter(), enemyData.HealAmount);
            }
        }

        private EquipmentItem GetGuaranteedDuplicateEquipment()
        {
            if (AccountProgression.Instance != null && AccountProgression.Instance.Data != null)
            {
                var curWpn = AccountProgression.Instance.Data.EquippedWeapon;
                if (curWpn != null)
                {
                    // Rơi 1 món cùng loại để chuẩn bị cho ghép đồ (Fuse)
                    return curWpn.Clone();
                }
            }
            return EquipmentItem.CreateCarrotBlade(EquipmentRarity.Great, 1);
        }

        private EquipmentItem GetRandomDropEquipment(bool isElite)
        {
            EquipmentRarity rarity = isElite ? EquipmentRarity.Rare : (Random.value < 0.25f ? EquipmentRarity.Great : EquipmentRarity.Common);
            int choice = Random.Range(0, 7);
            switch (choice)
            {
                case 0: return EquipmentItem.CreateCarrotBlade(rarity, 1);
                case 1: return EquipmentItem.CreateTomatoBlaster(rarity, 1);
                case 2: return EquipmentItem.CreateGrapeCannon(rarity, 1);
                case 3: return EquipmentItem.CreateRabbitVest(rarity, 1);
                case 4: return EquipmentItem.CreateSwiftBoots(rarity, 1);
                case 5: return EquipmentItem.CreateWarriorGloves(rarity, 1);
                default: return EquipmentItem.CreateCarrotPendant(rarity, 1);
            }
        }

        private Vector3 GetRandomJitter()
        {
            return new Vector3(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f));
        }

        public static void ResetPitySystem()
        {
            totalEnemiesKilled = 0;
            earlyWeaponGuaranteeDropped = false;
        }
    }
}
