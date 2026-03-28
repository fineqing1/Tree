using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 主菜单：白盒 UI 可在运行时生成；开始游戏以 Additive 加载关卡并卸载 Menu，保留 Persistence 场景（如全局相机）。
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] AssetReference gameplayScene;
    [Tooltip("为 true 且场景中没有 Canvas 时，自动生成白盒界面。")]
    [SerializeField] bool buildRuntimeUi = true;

    GameObject settingsPanel;
    bool loading;

    void Awake()
    {
        if (buildRuntimeUi && FindObjectOfType<Canvas>() == null)
            BuildWhiteBoxUi();
    }

    void Update()
    {
        if (settingsPanel != null && settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            OnCloseSettings();
    }

    public void OnStartGame()
    {
        if (loading)
            return;
        if (gameplayScene == null || !gameplayScene.RuntimeKeyIsValid())
        {
            Debug.LogError("MainMenuController: 请在 Inspector 中指定已加入 Addressables 的关卡场景（gameplayScene）。");
            return;
        }

        loading = true;
        StartCoroutine(LoadGameplayRoutine());
    }

    IEnumerator LoadGameplayRoutine()
    {
        var handle = gameplayScene.LoadSceneAsync(LoadSceneMode.Additive, true);
        if (!handle.IsValid())
        {
            Debug.LogError("MainMenuController: 无法开始加载关卡。");
            loading = false;
            yield break;
        }

        yield return handle;

        Scene gameplay = handle.Result.Scene;

        Scene menu = SceneManager.GetSceneByName("Menu");
        if (menu.IsValid() && menu.isLoaded)
            yield return SceneManager.UnloadSceneAsync(menu);

        if (gameplay.IsValid() && gameplay.isLoaded)
            SceneManager.SetActiveScene(gameplay);

        loading = false;
    }

    public void OnOpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void BuildWhiteBoxUi()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject canvasGo = new GameObject("MenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasGo.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        StretchFull(bgRt);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.1f, 0.12f, 1f);

        GameObject stack = new GameObject("Stack");
        stack.transform.SetParent(canvasGo.transform, false);
        RectTransform stackRt = stack.AddComponent<RectTransform>();
        stackRt.anchorMin = new Vector2(0.5f, 0.5f);
        stackRt.anchorMax = new Vector2(0.5f, 0.5f);
        stackRt.pivot = new Vector2(0.5f, 0.5f);
        stackRt.anchoredPosition = Vector2.zero;
        stackRt.sizeDelta = new Vector2(480, 400);

        VerticalLayoutGroup vlg = stack.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 16;
        vlg.padding = new RectOffset(24, 24, 24, 24);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        AddTitle(stack.transform, font, "朽木编年");
        AddButton(stack.transform, font, "开始游戏", OnStartGame);
        AddButton(stack.transform, font, "设置", OnOpenSettings);
        AddButton(stack.transform, font, "退出", OnQuit);

        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvasGo.transform, false);
        RectTransform spRt = settingsPanel.AddComponent<RectTransform>();
        StretchFull(spRt);
        Image spBg = settingsPanel.AddComponent<Image>();
        spBg.color = new Color(0f, 0f, 0f, 0.75f);

        GameObject spStack = new GameObject("SettingsStack");
        spStack.transform.SetParent(settingsPanel.transform, false);
        RectTransform spStackRt = spStack.AddComponent<RectTransform>();
        spStackRt.anchorMin = new Vector2(0.5f, 0.5f);
        spStackRt.anchorMax = new Vector2(0.5f, 0.5f);
        spStackRt.sizeDelta = new Vector2(400, 220);
        spStackRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup spV = spStack.AddComponent<VerticalLayoutGroup>();
        spV.spacing = 12;
        spV.childAlignment = TextAnchor.MiddleCenter;

        AddLabel(spStack.transform, font, "设置（白盒占位）", 28);
        AddButton(spStack.transform, font, "返回", OnCloseSettings);
        settingsPanel.SetActive(false);
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void AddTitle(Transform parent, Font font, string label)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 80f);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = label;
        t.fontSize = 42;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80f;
    }

    static void AddLabel(Transform parent, Font font, string label, int size)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = label;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 48f;
    }

    static void AddButton(Transform parent, Font font, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 56f);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.35f, 0.28f, 1f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.48f, 0.38f);
        colors.pressedColor = new Color(0.18f, 0.24f, 0.2f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        StretchFull(trt);
        Text t = textGo.AddComponent<Text>();
        t.font = font;
        t.text = label;
        t.fontSize = 22;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }
}
