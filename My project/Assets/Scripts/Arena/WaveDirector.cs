using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Data;
using Apocarrot.Combat;
using Apocarrot.Gameplay;
using Apocarrot.Enemies;
using Apocarrot.Inventory;
using Apocarrot.Loot;
using Apocarrot.UI;

namespace Apocarrot.Arena
{
    public enum RunState
    {
        NotStarted,
        WaveActive,
        WaveCompleted,
        Intermission,
        Victory,
        Defeat
    }

    /// <summary>
    /// Điều phối viên các đợt quái (Wave Director) - Gate 6.
    /// Quản lý 3 wave thường + 1 boss wave King Carrot, đếm số quái còn lại,
    /// tự động mở trạm nghỉ Intermission giữa các wave và kích hoạt Chiến Thắng.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {
        public static WaveDirector Instance { get; private set; }

        [Header("Arena Spawn Area")]
        [SerializeField] private float arenaSpawnRadius = 14f;

        [Header("UI Elements")]
        [SerializeField] private Text waveProgressText;
        [SerializeField] private GameObject waveBannerRoot;
        [SerializeField] private Text waveBannerTitle;
        [SerializeField] private Text waveBannerSubtitle;

        private List<WaveData> waves;
        private int currentWaveIndex = 0; // 0-indexed (0 to 3)
        private RunState currentState = RunState.NotStarted;

        private int remainingEnemiesInWave = 0;
        private int totalEnemiesInCurrentWave = 0;
        private int totalKillsInRun = 0;
        private float runStartTime = 0f;
        private bool isSpawningInProgress = false;

        private PlayerHealth playerHealth;
        private PlayerController playerController;
        private Coroutine waveSpawnCoroutine;
        private Coroutine bannerCoroutine;

        public RunState CurrentState => currentState;
        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves != null ? waves.Count : 4;
        public int RemainingEnemies => remainingEnemiesInWave;
        public int TotalKills => totalKillsInRun;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            waves = WaveScalingService.GenerateBalancedWaves();
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            playerController = FindFirstObjectByType<PlayerController>();

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }

            if (EquipmentBagUI.Instance != null)
            {
                EquipmentBagUI.Instance.OnContinueWaveRequested += HandleIntermissionContinued;
            }

            if (VictoryScreenUI.Instance != null)
            {
                VictoryScreenUI.Instance.OnRestartRequested += RestartRun;
            }

            if (ResultsScreenUI.Instance != null)
            {
                ResultsScreenUI.Instance.OnPlayAgainRequested += RestartRun;
                ResultsScreenUI.Instance.OnReturnToHomeRequested += ReturnToHome;
            }

            if (HomeHubUI.Instance != null)
            {
                HomeHubUI.Instance.OnStartRunRequested += StartRun;
            }

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            currentState = RunState.NotStarted;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }

            if (EquipmentBagUI.Instance != null)
            {
                EquipmentBagUI.Instance.OnContinueWaveRequested -= HandleIntermissionContinued;
            }

            if (VictoryScreenUI.Instance != null)
            {
                VictoryScreenUI.Instance.OnRestartRequested -= RestartRun;
            }

            if (ResultsScreenUI.Instance != null)
            {
                ResultsScreenUI.Instance.OnPlayAgainRequested -= RestartRun;
                ResultsScreenUI.Instance.OnReturnToHomeRequested -= ReturnToHome;
            }

            if (HomeHubUI.Instance != null)
            {
                HomeHubUI.Instance.OnStartRunRequested -= StartRun;
            }
        }

        public void BindUI(Text progressTxt, GameObject bannerRoot, Text bannerTitle, Text bannerSubtitle)
        {
            waveProgressText = progressTxt;
            waveBannerRoot = bannerRoot;
            waveBannerTitle = bannerTitle;
            waveBannerSubtitle = bannerSubtitle;

            if (waveBannerRoot != null)
            {
                waveBannerRoot.SetActive(false);
            }
        }

        public void StartWave(int waveIndex)
        {
            if (waveIndex >= waves.Count)
            {
                HandleRunVictory();
                return;
            }

            currentWaveIndex = waveIndex;
            currentState = RunState.WaveActive;
            WaveData wave = waves[currentWaveIndex];

            remainingEnemiesInWave = wave.TotalEnemies;
            totalEnemiesInCurrentWave = wave.TotalEnemies;

            UpdateHUD();
            ShowWaveBanner(wave.WaveTitle, wave.WaveDescription);

            if (waveSpawnCoroutine != null) StopCoroutine(waveSpawnCoroutine);
            waveSpawnCoroutine = StartCoroutine(SpawnWaveRoutine(wave));
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            isSpawningInProgress = true;

            // Chờ banner hiển thị 1 giây rồi bắt đầu spawn
            yield return new WaitForSeconds(1.0f);

            if (wave.IsBossWave)
            {
                // Spawn Boss King Carrot ở phía Bắc võ đài
                Vector3 bossSpawnPos = new Vector3(0, 0, 11f);
                GameObject bossObj = KingCarrotBossAI.CreateKingCarrotPrefab(bossSpawnPos);

                EnemyHealth bossHealth = bossObj.GetComponent<EnemyHealth>();
                KingCarrotBossAI bossAI = bossObj.GetComponent<KingCarrotBossAI>();

                if (bossHealth != null)
                {
                    bossHealth.OnDied += HandleEnemyDied;
                }

                if (BossHealthBarUI.Instance != null && bossHealth != null && bossAI != null)
                {
                    BossHealthBarUI.Instance.ShowBoss(bossHealth, bossAI);
                }
            }

            // Spawn các nhóm quái theo config
            foreach (var group in wave.SpawnGroups)
            {
                if (group.DelayBeforeSpawn > 0f)
                {
                    yield return new WaitForSeconds(group.DelayBeforeSpawn);
                }

                if (currentState != RunState.WaveActive) yield break;

                for (int i = 0; i < group.Count; i++)
                {
                    if (currentState != RunState.WaveActive) yield break;

                    SpawnSingleEnemy(group.Type);
                    yield return new WaitForSeconds(0.35f);
                }
            }

            isSpawningInProgress = false;
        }

        private void SpawnSingleEnemy(EnemyVariantType type)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            // Tự động scale lượng máu và sát thương của quái theo cấp độ Wave hiện tại
            EnemyData data = WaveScalingService.CreateScaledEnemyData(type, CurrentWaveNumber);

            GameObject enemy = ZombieCarrotAI.CreateZombieCarrotPrefab(spawnPos, data);
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.OnDied += HandleEnemyDied;
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 playerPos = playerController != null ? playerController.transform.position : Vector3.zero;
            Vector3 spawnPos;
            int attempts = 0;

            do
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float dist = UnityEngine.Random.Range(7.5f, arenaSpawnRadius);
                spawnPos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
                attempts++;
            }
            while (Vector3.Distance(spawnPos, playerPos) < 6f && attempts < 10);

            // Giới hạn trong sàn đấu
            spawnPos.x = Mathf.Clamp(spawnPos.x, -arenaSpawnRadius, arenaSpawnRadius);
            spawnPos.z = Mathf.Clamp(spawnPos.z, -arenaSpawnRadius, arenaSpawnRadius);
            spawnPos.y = 0f;

            return spawnPos;
        }

        private void HandleEnemyDied(EnemyHealth deadEnemy)
        {
            if (currentState != RunState.WaveActive) return;

            if (deadEnemy != null)
            {
                var ai = deadEnemy.GetComponent<ZombieCarrotAI>();
                if (ai != null)
                {
                    Progression.AccountProgression.Instance?.RecordEnemyKilled(ai.VariantType);
                }
                else
                {
                    Progression.AccountProgression.Instance?.RecordEnemyKilled(EnemyVariantType.Basic);
                }
            }

            totalKillsInRun++;
            remainingEnemiesInWave = Mathf.Max(0, remainingEnemiesInWave - 1);
            UpdateHUD();

            // Kiểm tra điều kiện hoàn thành Wave: hết quái và đã spawn xong hết
            if (remainingEnemiesInWave <= 0 && !isSpawningInProgress)
            {
                HandleWaveCompleted();
            }
        }

        private void HandleWaveCompleted()
        {
            currentState = RunState.WaveCompleted;
            WaveData currentWave = waves[currentWaveIndex];
            Progression.AccountProgression.Instance?.RecordWaveCleared(CurrentWaveNumber);

            if (currentWave.IsBossWave)
            {
                // Thắng run khi hạ gục King Carrot!
                Progression.AccountProgression.Instance?.RecordBossDefeated();
                StartCoroutine(DelayedVictoryRoutine());
            }
            else
            {
                // Mở trạm nghỉ Intermission giữa các Wave thường
                StartCoroutine(TransitionToIntermissionRoutine());
            }
        }

        private IEnumerator TransitionToIntermissionRoutine()
        {
            ShowWaveBanner($"🎉 HOÀN THÀNH WAVE {CurrentWaveNumber}!", "Chuẩn bị vào Trạm Nghỉ để nâng cấp...");
            LootToastManager.ShowWorldToast(Vector3.up * 2f, $"✨ VƯỢT QUA WAVE {CurrentWaveNumber}! ✨", new Color(0.2f, 0.9f, 0.4f));

            yield return new WaitForSeconds(1.6f);

            currentState = RunState.Intermission;
            if (EquipmentBagUI.Instance != null)
            {
                EquipmentBagUI.Instance.OpenForIntermission(CurrentWaveNumber);
            }
        }

        private void HandleIntermissionContinued()
        {
            if (currentState == RunState.Intermission)
            {
                // Chuyển sang Wave kế tiếp
                StartWave(currentWaveIndex + 1);
            }
        }

        private IEnumerator DelayedVictoryRoutine()
        {
            ShowWaveBanner("🏆 HOÀN THÀNH DEMO!", "Chúa Tể King Carrot đã bị tiêu diệt!");
            yield return new WaitForSeconds(1.8f);
            HandleRunVictory();
        }

        private void HandleRunVictory()
        {
            currentState = RunState.Victory;
            float duration = Time.time - runStartTime;
            int gold = RunInventory.Instance != null ? RunInventory.Instance.RunGold : 0;

            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.Hide();
            }

            if (ResultsScreenUI.Instance != null)
            {
                ResultsScreenUI.Instance.ShowResults(true, 4, totalKillsInRun, gold, duration);
            }
            else if (VictoryScreenUI.Instance != null)
            {
                VictoryScreenUI.Instance.ShowVictory(duration, totalKillsInRun, gold);
            }
        }

        private void HandlePlayerDied()
        {
            // Khi player chết, dừng các coroutine wave để tránh kích hoạt thắng/chuyển wave
            currentState = RunState.Defeat;
            if (waveSpawnCoroutine != null) StopCoroutine(waveSpawnCoroutine);
            if (bannerCoroutine != null) StopCoroutine(bannerCoroutine);

            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.Hide();
            }

            float duration = Time.time - runStartTime;
            int gold = RunInventory.Instance != null ? RunInventory.Instance.RunGold : 0;

            if (ResultsScreenUI.Instance != null)
            {
                ResultsScreenUI.Instance.ShowResults(false, CurrentWaveNumber, totalKillsInRun, gold, duration);
            }
        }

        public void CleanUpArena()
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e != null) Destroy(e.gameObject);
            }

            var pickups = FindObjectsByType<LootPickup>(FindObjectsSortMode.None);
            foreach (var p in pickups)
            {
                if (p != null) Destroy(p.gameObject);
            }

            var projectiles = FindObjectsByType<Combat.Projectile>(FindObjectsSortMode.None);
            foreach (var proj in projectiles)
            {
                if (proj != null) Destroy(proj.gameObject);
            }

            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.Hide();
            }

            if (EquipmentBagUI.Instance != null && EquipmentBagUI.Instance.IsOpen)
            {
                EquipmentBagUI.Instance.CloseBag();
            }
        }

        public void StartRun()
        {
            RestartRun();
        }

        public void RestartRun()
        {
            if (waveSpawnCoroutine != null) StopCoroutine(waveSpawnCoroutine);
            CleanUpArena();

            // Reset kho đồ và máu Thỏ
            if (RunInventory.Instance != null)
            {
                RunInventory.Instance.ResetInventory();
            }

            if (playerHealth != null)
            {
                playerHealth.Revive(100f);
            }

            if (playerController != null)
            {
                var cc = playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerController.transform.position = Vector3.zero;
                if (cc != null) cc.enabled = true;
                playerController.enabled = true;
            }

            totalKillsInRun = 0;
            runStartTime = Time.time;
            Progression.AccountProgression.Instance?.StartNewRun();

            FindFirstObjectByType<ArenaSceneBootstrap>()?.SetCombatHUDActive(true);
            HomeHubUI.Instance?.HideHub();

            StartWave(0);
        }

        public void ReturnToHome()
        {
            if (waveSpawnCoroutine != null) StopCoroutine(waveSpawnCoroutine);
            CleanUpArena();

            if (playerHealth != null)
            {
                playerHealth.Revive(100f);
            }

            if (playerController != null)
            {
                var cc = playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerController.transform.position = Vector3.zero;
                if (cc != null) cc.enabled = true;
                playerController.enabled = false;
            }

            currentState = RunState.NotStarted;
            Time.timeScale = 1f;

            FindFirstObjectByType<ArenaSceneBootstrap>()?.SetCombatHUDActive(false);
            HomeHubUI.Instance?.ShowHub();
        }

        private void UpdateHUD()
        {
            if (waveProgressText != null)
            {
                string bossTag = waves[currentWaveIndex].IsBossWave ? " [BOSS]" : "";
                waveProgressText.text = $"⚔️ Wave: {CurrentWaveNumber}/{TotalWaves}{bossTag} | Quái còn lại: {remainingEnemiesInWave}/{totalEnemiesInCurrentWave}";
            }
        }

        private void ShowWaveBanner(string title, string subtitle)
        {
            if (waveBannerRoot == null) return;

            if (bannerCoroutine != null) StopCoroutine(bannerCoroutine);
            bannerCoroutine = StartCoroutine(WaveBannerRoutine(title, subtitle));
        }

        private IEnumerator WaveBannerRoutine(string title, string subtitle)
        {
            if (waveBannerTitle != null) waveBannerTitle.text = title;
            if (waveBannerSubtitle != null) waveBannerSubtitle.text = subtitle;

            waveBannerRoot.SetActive(true);
            yield return new WaitForSeconds(2.6f);
            waveBannerRoot.SetActive(false);
        }
    }
}
