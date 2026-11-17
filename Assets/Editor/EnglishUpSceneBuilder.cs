using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem.UI;
using TMPro;

public static class EnglishUpSceneBuilder
{
    const string ScenePath = "Assets/Scenes/EnglishUp.unity";
    const string FontAssetPath = "Assets/Plugins/TextMesh Pro/Resources/Fonts & Materials/Inter SDF.asset";

    static readonly Color BackgroundColor = new Color(0.92f, 0.95f, 1f);
    static readonly Color BlueColor = new Color(0.18f, 0.48f, 0.88f);
    static readonly Color DarkBlueColor = new Color(0.09f, 0.24f, 0.48f);
    static readonly Color YellowColor = new Color(1f, 0.79f, 0.24f);
    static readonly Color GrayColor = new Color(0.42f, 0.48f, 0.56f);

    static TMP_FontAsset font;

    public static void BuildAndExit()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (font == null)
        {
            string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
            }
        }
        if (font == null)
        {
            Debug.LogError("Не найден шрифт TextMeshPro. Импортируйте TMP Essential Resources и повторите.");
            EditorApplication.Exit(1);
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = BackgroundColor;
        cameraComponent.orthographic = true;
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();

        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        CreateImage(canvasObject, "Background", BackgroundColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);

        GameObject menuScreen = CreateScreen(canvasObject, "MenuScreen");
        GameObject gameScreen = CreateScreen(canvasObject, "GameScreen");
        GameObject resultsScreen = CreateScreen(canvasObject, "ResultsScreen");

        BuildMenuScreen(menuScreen);
        BuildGameScreen(gameScreen);
        BuildResultsScreen(resultsScreen);

        gameScreen.SetActive(false);
        resultsScreen.SetActive(false);

        EnglishUpGame game = new GameObject("EnglishUpGame").AddComponent<EnglishUpGame>();
        game.MenuScreen = menuScreen;
        game.GameScreen = gameScreen;
        game.ResultsScreen = resultsScreen;
        game.StartButton = menuScreen.transform.Find("StartButton").GetComponent<Button>();
        game.ExitButton = menuScreen.transform.Find("ExitButton").GetComponent<Button>();
        game.PlayAgainButton = resultsScreen.transform.Find("PlayAgainButton").GetComponent<Button>();
        game.MenuButton = resultsScreen.transform.Find("MenuButton").GetComponent<Button>();

        Transform gamePanel = gameScreen.transform.Find("TopPanel");
        game.QuestionCounterText = gamePanel.Find("QuestionCounterText").GetComponent<TMP_Text>();
        game.ScoreText = gamePanel.Find("ScoreText").GetComponent<TMP_Text>();
        game.EnglishWordText = gameScreen.transform.Find("CardPanel/EnglishWordText").GetComponent<TMP_Text>();
        game.CardImage = gameScreen.transform.Find("CardPanel/CardImage").GetComponent<Image>();

        Button[] answerButtons = new Button[4];
        TMP_Text[] answerTexts = new TMP_Text[4];
        Transform answersGrid = gameScreen.transform.Find("AnswersGrid");
        for (int i = 0; i < 4; i++)
        {
            Transform buttonTransform = answersGrid.Find($"AnswerButton{i}");
            answerButtons[i] = buttonTransform.GetComponent<Button>();
            answerTexts[i] = buttonTransform.Find("Text").GetComponent<TMP_Text>();
        }
        game.AnswerButtons = answerButtons;
        game.AnswerTexts = answerTexts;

        game.ResultScoreText = resultsScreen.transform.Find("ResultScoreText").GetComponent<TMP_Text>();
        game.ResultCorrectText = resultsScreen.transform.Find("ResultCorrectText").GetComponent<TMP_Text>();
        game.ResultWrongText = resultsScreen.transform.Find("ResultWrongText").GetComponent<TMP_Text>();

        Sprite[] questionImages = new Sprite[EnglishUpGame.EnglishWords.Length];
        for (int i = 0; i < EnglishUpGame.EnglishWords.Length; i++)
        {
            string imagePath = $"Assets/Content/Images/{EnglishUpGame.EnglishWords[i]}.png";
            questionImages[i] = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (questionImages[i] == null)
            {
                Debug.LogWarning($"Изображение не найдено: {imagePath}");
            }
        }
        game.QuestionImages = questionImages;

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Сцена EnglishUp создана успешно.");
        EditorApplication.Exit(0);
    }

    static GameObject CreateScreen(GameObject canvasObject, string name)
    {
        GameObject screen = new GameObject(name, typeof(RectTransform));
        screen.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = screen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return screen;
    }

    static void BuildMenuScreen(GameObject menuScreen)
    {
        CreateText(menuScreen, "TitleText", "EnglishUp", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900, 130), new Vector2(0, -80), TextAlignmentOptions.Center, 92, DarkBlueColor);
        CreateText(menuScreen, "SubtitleText", "Изучай английский играя!", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900, 60), new Vector2(0, -190), TextAlignmentOptions.Center, 34, GrayColor);
        CreateButton(menuScreen, "StartButton", "Начать игру", new Vector2(0, -50), new Vector2(380, 88), BlueColor, Color.white, 36);
        CreateButton(menuScreen, "ExitButton", "Выход", new Vector2(0, -175), new Vector2(380, 88), YellowColor, DarkBlueColor, 36);
    }

    static void BuildGameScreen(GameObject gameScreen)
    {
        CreateImage(gameScreen, "TopPanel", BlueColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 96), new Vector2(0, 0));

        GameObject topPanel = gameScreen.transform.Find("TopPanel").gameObject;
        CreateText(topPanel, "GameTitleText", "EnglishUp", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(300, 0), new Vector2(30, 0), TextAlignmentOptions.Left, 40, Color.white);
        CreateText(topPanel, "QuestionCounterText", "1 / 10", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(240, 0), new Vector2(0, 0), TextAlignmentOptions.Center, 34, Color.white);
        CreateText(topPanel, "ScoreText", "Счёт: 0", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(320, 0), new Vector2(-30, 0), TextAlignmentOptions.Right, 34, Color.white);

        CreateImage(gameScreen, "CardPanel", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(460, 500), new Vector2(0, 70));

        GameObject cardPanel = gameScreen.transform.Find("CardPanel").gameObject;
        Image cardImage = CreateImage(cardPanel, "CardImage", Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(400, 330), new Vector2(0, -25)).GetComponent<Image>();
        cardImage.preserveAspect = true;
        CreateText(cardPanel, "EnglishWordText", "Apple", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420, 80), new Vector2(0, -370), TextAlignmentOptions.Center, 56, DarkBlueColor);
        CreateText(cardPanel, "PromptText", "Выбери правильный перевод", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420, 40), new Vector2(0, 25), TextAlignmentOptions.Center, 26, GrayColor);

        GameObject answersGrid = new GameObject("AnswersGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        answersGrid.transform.SetParent(gameScreen.transform, false);
        RectTransform gridRect = answersGrid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0f);
        gridRect.anchorMax = new Vector2(0.5f, 0f);
        gridRect.pivot = new Vector2(0.5f, 0f);
        gridRect.sizeDelta = new Vector2(820, 210);
        gridRect.anchoredPosition = new Vector2(0, 25);
        GridLayoutGroup gridLayout = answersGrid.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(398, 93);
        gridLayout.spacing = new Vector2(24, 24);
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < 4; i++)
        {
            CreateButton(answersGrid, $"AnswerButton{i}", "Вариант", Vector2.zero, new Vector2(398, 93), Color.white, DarkBlueColor, 34);
        }
    }

    static void BuildResultsScreen(GameObject resultsScreen)
    {
        CreateText(resultsScreen, "ResultTitleText", "Результаты", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 110), new Vector2(0, -100), TextAlignmentOptions.Center, 72, DarkBlueColor);
        CreateText(resultsScreen, "ResultScoreText", "Итоговый счёт: 0 из 100", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 60), new Vector2(0, 100), TextAlignmentOptions.Center, 40, DarkBlueColor);
        CreateText(resultsScreen, "ResultCorrectText", "Правильных ответов: 0 из 10", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 50), new Vector2(0, 20), TextAlignmentOptions.Center, 34, DarkBlueColor);
        CreateText(resultsScreen, "ResultWrongText", "Неправильных ответов: 0", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 50), new Vector2(0, -60), TextAlignmentOptions.Center, 34, DarkBlueColor);
        CreateButton(resultsScreen, "PlayAgainButton", "Играть снова", new Vector2(0, -180), new Vector2(380, 88), BlueColor, Color.white, 36);
        CreateButton(resultsScreen, "MenuButton", "Главное меню", new Vector2(0, -290), new Vector2(380, 88), YellowColor, DarkBlueColor, 36);
    }

    static GameObject CreateImage(GameObject parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    static RectTransform CreateText(GameObject parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, TextAlignmentOptions alignment, float fontSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return rect;
    }

    static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Color backgroundColor, Color textColor, float fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent.transform, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        buttonObject.GetComponent<Image>().color = backgroundColor;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = label;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return buttonObject.GetComponent<Button>();
    }
}
