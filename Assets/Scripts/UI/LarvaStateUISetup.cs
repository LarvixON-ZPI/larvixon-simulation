using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LarvaStateUISetup : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;

        [SerializeField] private Vector2 buttonSize = new(120, 30);
        [SerializeField] private float buttonSpacing = 10f;

        private Canvas _canvas;
        private LarvaStateUIManager _uiManager;

        private void Start()
        {
            if (autoSetupOnStart) SetupUI();
        }

        [ContextMenu("Setup UI")]
        public void SetupUI()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("Canvas component not found! Please attach this script to a Canvas.");
                return;
            }

            CreateUIElements();
        }

        private void CreateUIElements()
        {
            var mainContainer = CreateUIObject("LarvaStateUI", _canvas.transform);
            var mainRect = mainContainer.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0, 1);
            mainRect.anchorMax = new Vector2(0, 1);
            mainRect.pivot = new Vector2(0, 1);
            mainRect.anchoredPosition = new Vector2(20, -20);
            mainRect.sizeDelta = new Vector2(200, 200);

            var backgroundImage = mainContainer.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

            var titleObj = CreateTextObject("Title", mainContainer.transform, "Larva States");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(-20, 25);

            var titleText = titleObj.GetComponent<Text>();
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;

            var currentStateObj = CreateTextObject("CurrentState", mainContainer.transform, "Current State: None");
            var currentStateRect = currentStateObj.GetComponent<RectTransform>();
            currentStateRect.anchorMin = new Vector2(0, 1);
            currentStateRect.anchorMax = new Vector2(1, 1);
            currentStateRect.pivot = new Vector2(0.5f, 1);
            currentStateRect.anchoredPosition = new Vector2(0, -40);
            currentStateRect.sizeDelta = new Vector2(-20, 20);

            var currentStateText = currentStateObj.GetComponent<Text>();
            currentStateText.alignment = TextAnchor.MiddleCenter;
            currentStateText.fontSize = 12;

            var buttonContainer = CreateUIObject("ButtonContainer", mainContainer.transform);
            var buttonContainerRect = buttonContainer.GetComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0, 0);
            buttonContainerRect.anchorMax = new Vector2(1, 1);
            buttonContainerRect.pivot = new Vector2(0.5f, 1);
            buttonContainerRect.anchoredPosition = new Vector2(0, -70);
            buttonContainerRect.sizeDelta = new Vector2(-20, -80);

            var layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = buttonSpacing;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            if (_uiManager == null) _uiManager = gameObject.AddComponent<LarvaStateUIManager>();

            _uiManager.currentStateText = currentStateText;

            Debug.Log("Larva State UI setup complete!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreateTextObject(string name, Transform parent, string text)
        {
            var textObj = CreateUIObject(name, parent);
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            return textObj;
        }

        private GameObject CreateButtonPrefab(Transform parent)
        {
            var buttonObj = CreateUIObject("ButtonPrefab", parent);

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            buttonImage.type = Image.Type.Sliced;

            var buttonComponent = buttonObj.AddComponent<Button>();

            var colors = buttonComponent.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            buttonComponent.colors = colors;

            var textObj = CreateTextObject("Text", buttonObj.transform, "Button");
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var textComponent = textObj.GetComponent<Text>();
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;

            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;

            return buttonObj;
        }
    }
}