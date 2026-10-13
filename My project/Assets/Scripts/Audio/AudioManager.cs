using System;
using UnityEngine;

namespace Apocarrot.Audio
{
    /// <summary>
    /// Hệ thống âm thanh trung tâm cho APOCARROT.
    /// Tạo âm thanh procedurally (AudioClip.Create) chất lượng cao, nhẹ và zero-dependency.
    /// Hỗ trợ đầy đủ:
    /// - Nhạc nền (BGM) nhịp điệu Roguelite Arcade
    /// - SFX: Chém kiếm, Bắn súng, Chém trúng quái, Quái chết, Nhặt vàng/đồ, Merge đồ thành công, Boss Slam AoE, Player Hurt.
    /// - Điều chỉnh âm lượng BGM & SFX thời gian thực.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PREF_BGM_VOL = "APO_BGM_VOL";
        private const string PREF_SFX_VOL = "APO_SFX_VOL";

        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.45f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.85f;

        private AudioSource bgmSource;
        private AudioSource[] sfxSources;
        private int sfxSourceIndex = 0;
        private const int SFX_SOURCE_POOL_SIZE = 12;

        // Cached Audio Clips
        private AudioClip clipBGM;
        private AudioClip clipMeleeSwing;
        private AudioClip clipMeleeHit;
        private AudioClip clipRangedShot;
        private AudioClip clipEnemyDeath;
        private AudioClip clipLootPickup;
        private AudioClip clipMergeSuccess;
        private AudioClip clipBossSlamTelegraph;
        private AudioClip clipBossSlamImpact;
        private AudioClip clipPlayerHurt;

        public float BGMVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp01(value);
                if (bgmSource != null) bgmSource.volume = bgmVolume;
                PlayerPrefs.SetFloat(PREF_BGM_VOL, bgmVolume);
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOL, sfxVolume);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 0.45f);
            sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.85f);

            // BGM Source
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;

            // SFX Pool
            sfxSources = new AudioSource[SFX_SOURCE_POOL_SIZE];
            for (int i = 0; i < SFX_SOURCE_POOL_SIZE; i++)
            {
                sfxSources[i] = gameObject.AddComponent<AudioSource>();
                sfxSources[i].playOnAwake = false;
                sfxSources[i].spatialBlend = 0f; // 2D Sound
            }

            // Generate Procedural Audio Clips
            GenerateProceduralClips();

            // Bắt đầu phát nhạc nền
            PlayBGM();
        }

        private void GenerateProceduralClips()
        {
            clipBGM = CreateRogueliteBGMClip();
            clipMeleeSwing = CreateMeleeSwingClip();
            clipMeleeHit = CreateMeleeHitClip();
            clipRangedShot = CreateRangedShotClip();
            clipEnemyDeath = CreateEnemyDeathClip();
            clipLootPickup = CreateLootPickupClip();
            clipMergeSuccess = CreateMergeFanfareClip();
            clipBossSlamTelegraph = CreateTelegraphClip();
            clipBossSlamImpact = CreateSlamImpactClip();
            clipPlayerHurt = CreatePlayerHurtClip();
        }

        public void PlayBGM()
        {
            if (bgmSource == null || clipBGM == null) return;
            bgmSource.clip = clipBGM;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f, float pitchRandomness = 0.05f)
        {
            if (clip == null || sfxSources == null) return;

            AudioSource src = sfxSources[sfxSourceIndex];
            sfxSourceIndex = (sfxSourceIndex + 1) % SFX_SOURCE_POOL_SIZE;

            src.pitch = 1.0f + UnityEngine.Random.Range(-pitchRandomness, pitchRandomness);
            src.volume = sfxVolume * Mathf.Clamp01(volumeScale);
            src.PlayOneShot(clip);
        }

        // ========================== PUBLIC SFX TRIGGERS ==========================
        public void PlayMeleeSwing() => PlaySFX(clipMeleeSwing, 0.65f, 0.08f);
        public void PlayMeleeHit() => PlaySFX(clipMeleeHit, 0.9f, 0.12f);
        public void PlayRangedShot() => PlaySFX(clipRangedShot, 0.75f, 0.06f);
        public void PlayEnemyDeath() => PlaySFX(clipEnemyDeath, 0.85f, 0.1f);
        public void PlayLootPickup() => PlaySFX(clipLootPickup, 1.0f, 0.04f);
        public void PlayCoinPickup() => PlayLootPickup();
        public void PlayMergeSuccess() => PlaySFX(clipMergeSuccess, 1.0f, 0f);
        public void PlayBossTelegraph() => PlaySFX(clipBossSlamTelegraph, 0.95f, 0f);
        public void PlayBossSlamImpact() => PlaySFX(clipBossSlamImpact, 1.0f, 0.05f);
        public void PlayPlayerHurt() => PlaySFX(clipPlayerHurt, 0.85f, 0.08f);

        // ========================== PROCEDURAL SYNTHESIS ==========================
        private AudioClip CreateMeleeSwingClip()
        {
            int sampleRate = 44100;
            float duration = 0.18f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = Mathf.Sin(t * Mathf.PI); // Smooth envelope
                float freq = Mathf.Lerp(480f, 180f, t);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.45f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                samples[i] = (tone * 0.55f + noise) * env;
            }

            AudioClip clip = AudioClip.Create("MeleeSwing", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateMeleeHitClip()
        {
            int sampleRate = 44100;
            float duration = 0.15f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = Mathf.Pow(1f - t, 2.5f); // Sharp percussive decay
                float freq = Mathf.Lerp(220f, 60f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.6f;
                samples[i] = (tone * 0.7f + noise * 0.3f) * env;
            }

            AudioClip clip = AudioClip.Create("MeleeHit", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateRangedShotClip()
        {
            int sampleRate = 44100;
            float duration = 0.14f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = Mathf.Pow(1f - t, 2.0f);
                float freq = Mathf.Lerp(850f, 240f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                float pop = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * (float)i / sampleRate));
                samples[i] = (tone * 0.7f + pop * 0.3f) * env;
            }

            AudioClip clip = AudioClip.Create("RangedShot", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateEnemyDeathClip()
        {
            int sampleRate = 44100;
            float duration = 0.28f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = (1f - t);
                float freq = Mathf.Lerp(340f, 80f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                float crunch = (UnityEngine.Random.value * 2f - 1f) * 0.5f;
                samples[i] = (tone * 0.6f + crunch * 0.4f) * env;
            }

            AudioClip clip = AudioClip.Create("EnemyDeath", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateLootPickupClip()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = Mathf.Sin(t * Mathf.PI);
                // Hai nốt Arpeggio tươi sáng (G5 -> C6)
                float freq = t < 0.5f ? 784f : 1046.5f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                samples[i] = tone * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("LootPickup", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateMergeFanfareClip()
        {
            int sampleRate = 44100;
            float duration = 0.55f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C - E - G - C6

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t * 4), 0, 3);
                float noteT = (t * 4) - noteIndex;
                float env = (1f - noteT * 0.7f);

                float freq = notes[noteIndex];
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                float harmonic = Mathf.Sin(4f * Mathf.PI * freq * (float)i / sampleRate) * 0.35f;
                samples[i] = (tone + harmonic) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("MergeFanfare", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateTelegraphClip()
        {
            int sampleRate = 44100;
            float duration = 0.6f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = t * t; // Tăng dần độ căng thẳng
                float freq = Mathf.Lerp(120f, 380f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                samples[i] = tone * env * 0.75f;
            }

            AudioClip clip = AudioClip.Create("Telegraph", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSlamImpactClip()
        {
            int sampleRate = 44100;
            float duration = 0.5f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = Mathf.Pow(1f - t, 2.2f);
                float rumble = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 30f, t) * (float)i / sampleRate);
                float boom = (UnityEngine.Random.value * 2f - 1f) * 0.8f;
                samples[i] = (rumble * 0.65f + boom * 0.5f) * env;
            }

            AudioClip clip = AudioClip.Create("SlamImpact", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreatePlayerHurtClip()
        {
            int sampleRate = 44100;
            float duration = 0.2f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float env = (1f - t);
                float freq = Mathf.Lerp(300f, 140f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / sampleRate);
                samples[i] = tone * env * 0.8f;
            }

            AudioClip clip = AudioClip.Create("PlayerHurt", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateRogueliteBGMClip()
        {
            int sampleRate = 22050; // Tiết kiệm bộ nhớ cho vòng lặp BGM
            float duration = 16.0f; // Vòng lặp 16 giây
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Melody chiptune retro roguelite (scale Am: A, C, D, E, G)
            float[] bassNotes = new float[] { 110f, 110f, 130.81f, 146.83f, 164.81f, 130.81f, 110f, 98f };
            float[] leadNotes = new float[] { 440f, 523.25f, 587.33f, 659.25f, 523.25f, 440f, 392f, 440f, 659.25f, 587.33f, 523.25f, 440f };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // 1. Bassline (8 nhịp mỗi chu kỳ)
                int bassIdx = Mathf.FloorToInt((t * 2.5f) % bassNotes.Length);
                float bassFreq = bassNotes[bassIdx];
                float bassWave = Mathf.Sin(2f * Mathf.PI * bassFreq * t);

                // 2. Lead melody
                int leadIdx = Mathf.FloorToInt((t * 2.0f) % leadNotes.Length);
                float leadFreq = leadNotes[leadIdx];
                float leadWave = Mathf.Sin(2f * Mathf.PI * leadFreq * t) * 0.4f;

                // 3. Hi-hat / Kick nhịp điệu (120 BPM)
                float beatT = (t * 2f) % 1f;
                float kick = (beatT < 0.15f) ? Mathf.Sin(2f * Mathf.PI * 65f * beatT) * (1f - beatT / 0.15f) * 0.5f : 0f;
                float hihat = ((t * 4f) % 1f < 0.08f) ? (UnityEngine.Random.value * 2f - 1f) * 0.15f : 0f;

                samples[i] = (bassWave * 0.35f + leadWave + kick + hihat) * 0.45f;
            }

            AudioClip clip = AudioClip.Create("RogueliteBGM", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
