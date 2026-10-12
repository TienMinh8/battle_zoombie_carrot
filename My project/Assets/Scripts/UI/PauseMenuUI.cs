using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Apocarrot.Audio;

namespace Apocarrot.UI
{
    /// <summary>
    /// Menu Tạm Dừng (Pause Menu) & Cài Đặt Âm Lượng trong trận đấu.
    /// Cho phép người chơi:
    /// - Nhấn nút ⏸ trên HUD hoặc phím [Esc] / [P] để tạm dừng.
    /// - Điều chỉnh âm lượng Nhạc nền (BGM) và Hiệu ứng (SFX) theo thời gian thực.
    /// - Tiếp tục trận đấu hoặc Chơi lại lượt (Restart).
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text bgmValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;

        public bool IsPaused { get; private set; } = false;

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
        }

        private void Start()
        {
            InitializeValues();
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // Phím tắt bàn phím [Escape] hoặc [P]
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                // Nếu đang mở Balo thì nhường quyền cho Balo đóng trước
                if (EquipmentBagUI.Instance != null && EquipmentBagUI.Instance.IsOpen)
                {
                    return;
                }

                TogglePause();
            }
        }

        public void InitializeValues()
        {
            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXVolume;
            }

            UpdateValueLabels();

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveAllListeners();
                bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(Resume);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartRun);
            }
        }

        private void OnBGMChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.BGMVolume = val;
            }
            UpdateValueLabels();
        }

        private void OnSFXChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SFXVolume = val;
            }
            UpdateValueLabels();
        }

        private void UpdateValueLabels()
        {
            if (bgmValueText != null && bgmSlider != null)
            {
                bgmValueText.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}%";
            }
            if (sfxValueText != null && sfxSlider != null)
            {
                sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";
            }
        }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
            InitializeValues();
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ========================== DYNAMIC UI CREATION ==========================
        public static PauseMenuUI CreatePauseMenuUI(GameObject canvasObj, Font font)
        {
            GameObject rootObj = new GameObject("PauseMenu_UI");
            rootObj.transform.SetParent(canvasObj.transform, false);

            RectTransform rootRt = rootObj.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            PauseMenuUI pauseUI = rootObj.AddComponent<PauseMenuUI>();

            // Modal Background Dim
            GameObject dimObj = new GameObject("Dim_Overlay");
            dimObj.transform.SetParent(rootObj.transform, false);
            RectTransform dimRt = dimObj.AddComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.sizeDelta = Vector2.zero;
            Image dimImg = dimObj.AddComponent<Image>();
            dimImg.color = new Color(0, 0, 0, 0.75f);

            // Modal Window Box
            GameObject boxObj = new GameObject("Pause_Box");
            boxObj.transform.SetParent(rootObj.transform, false);
            RectTransform boxRt = boxObj.AddComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.pivot = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(440, 420);

            Image boxImg = boxObj.AddComponent<Image>();
            boxImg.color = new Color(0.10f, 0.13f, 0.20f, 0.98f);
            Outline boxOutline = boxObj.AddComponent<Outline>();
            boxOutline.effectColor = new Color(0.25f, 0.5f, 0.95f, 0.8f);
            boxOutline.effectDistance = new Vector2(3f, -3f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(boxObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -25);
            titleRt.sizeDelta = new Vector2(0, 50);

            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.85f, 0.25f);
            titleTxt.text = "⏸ TẠM DỪNG TRẬN ĐẤU";

            // Subtitle
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(boxObj.transform, false);
            RectTransform subRt = subObj.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 1);
            subRt.anchorMax = new Vector2(1, 1);
            subRt.pivot = new Vector2(0.5f, 1);
            subRt.anchoredPosition = new Vector2(0, -68);
            subRt.sizeDelta = new Vector2(0, 25);

            Text subTxt = subObj.AddComponent<Text>();
            subTxt.font = font;
            subTxt.fontSize = 13;
            subTxt.alignment = TextAnchor.MiddleCenter;
            subTxt.color = new Color(0.7f, 0.8f, 0.9f);
            subTxt.text = "Cài đặt âm lượng & Trạng thái chiến đấu";

            // BGM Row
            var bgmElements = CreateSliderRow(boxObj, font, -125, "🎵 Nhạc Nền (BGM):");
            // SFX Row
            var sfxElements = CreateSliderRow(boxObj, font, -195, "🔊 Hiệu Ứng (SFX):");

            // Resume Button
            GameObject resumeBtnObj = CreateActionButton(boxObj, font, -280, "▶ TIẾP TỤC CHIẾN ĐẤU", new Color(0.18f, 0.65f, 0.35f));
            // Restart Button
            GameObject restartBtnObj = CreateActionButton(boxObj, font, -345, "🔄 CHƠI LẠI TRẬN (RESTART)", new Color(0.85f, 0.25f, 0.25f));

            // Bind References
            pauseUI.modalRoot = rootObj;
            pauseUI.bgmSlider = bgmElements.slider;
            pauseUI.bgmValueText = bgmElements.valueText;
            pauseUI.sfxSlider = sfxElements.slider;
            pauseUI.sfxValueText = sfxElements.valueText;
            pauseUI.resumeButton = resumeBtnObj.GetComponent<Button>();
            pauseUI.restartButton = restartBtnObj.GetComponent<Button>();

            pauseUI.InitializeValues();
            rootObj.SetActive(false);

            return pauseUI;
        }

        private static (Slider slider, Text valueText) CreateSliderRow(GameObject parent, Font font, float posY, string label)
        {
            // Label
            GameObject lblObj = new GameObject("Label");
            lblObj.transform.SetParent(parent.transform, false);
            RectTransform lblRt = lblObj.AddComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0, 1);
            lblRt.anchorMax = new Vector2(1, 1);
            lblRt.pivot = new Vector2(0, 1);
            lblRt.anchoredPosition = new Vector2(40, posY);
            lblRt.sizeDelta = new Vector2(-80, 24);

            Text lblTxt = lblObj.AddComponent<Text>();
            lblTxt.font = font;
            lblTxt.fontSize = 15;
            lblTxt.fontStyle = FontStyle.Bold;
            lblTxt.color = Color.white;
            lblTxt.text = label;

            // Value Text
            GameObject valObj = new GameObject("Value");
            valObj.transform.SetParent(parent.transform, false);
            RectTransform valRt = valObj.AddComponent<RectTransform>();
            valRt.anchorMin = new Vector2(1, 1);
            valRt.anchorMax = new Vector2(1, 1);
            valRt.pivot = new Vector2(1, 1);
            valRt.anchoredPosition = new Vector2(-40, posY);
            valRt.sizeDelta = new Vector2(60, 24);

            Text valTxt = valObj.AddComponent<Text>();
            valTxt.font = font;
            valTxt.fontSize = 15;
            valTxt.fontStyle = FontStyle.Bold;
            valTxt.alignment = TextAnchor.MiddleRight;
            valTxt.color = new Color(0.3f, 0.9f, 1f);
            valTxt.text = "100%";

            // Slider Background
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(parent.transform, false);
            RectTransform sliderRt = sliderObj.AddComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0, 1);
            sliderRt.anchorMax = new Vector2(1, 1);
            sliderRt.pivot = new Vector2(0.5f, 1);
            sliderRt.anchoredPosition = new Vector2(0, posY - 28);
            sliderRt.sizeDelta = new Vector2(360, 20);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Background
            GameObject bgObj = new GameObject("Bg");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.25f);
            bgRt.anchorMax = new Vector2(1, 0.75f);
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.25f);
            fillAreaRt.anchorMax = new Vector2(1, 0.75f);
            fillAreaRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.75f, 1f);

            slider.fillRect = fillRt;

            // Handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRt = handle.AddComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(24, 24);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;

            return (slider, valTxt);
        }

        private static GameObject CreateActionButton(GameObject parent, Font font, float posY, string text, Color btnColor)
        {
            GameObject btnObj = new GameObject("Button_" + text);
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, posY);
            rt.sizeDelta = new Vector2(360, 48);

            Image img = btnObj.AddComponent<Image>();
            img.color = btnColor;

            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.6f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Button btn = btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Txt");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 16;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = text;

            return btnObj;
        }
    }
}
