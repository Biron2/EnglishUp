using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EnglishUpRuntimeCheck
{
    public static void RunCheckAndExit()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/EnglishUp.unity");
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        GameObject runnerObject = new GameObject("RuntimeCheckRunner");
        runnerObject.AddComponent<RuntimeCheckRunner>();
    }
}

public class RuntimeCheckRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        EnglishUpGame game = Object.FindObjectOfType<EnglishUpGame>();
        if (game == null)
        {
            Fail("Компонент EnglishUpGame не найден в сцене.");
            yield break;
        }

        if (game.MenuScreen == null || game.GameScreen == null || game.ResultsScreen == null ||
            game.StartButton == null || game.ExitButton == null || game.PlayAgainButton == null ||
            game.MenuButton == null || game.QuestionCounterText == null || game.ScoreText == null ||
            game.EnglishWordText == null || game.CardImage == null ||
            game.AnswerButtons == null || game.AnswerButtons.Length != 4 ||
            game.AnswerTexts == null || game.AnswerTexts.Length != 4 ||
            game.ResultScoreText == null || game.ResultCorrectText == null || game.ResultWrongText == null)
        {
            Fail("Не все ссылки компонента EnglishUpGame назначены.");
            yield break;
        }

        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(255, 200, 60, 255);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        Sprite testSprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 100);
        for (int i = 0; i < game.QuestionImages.Length; i++)
        {
            game.QuestionImages[i] = testSprite;
        }
        game.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

        if (!game.MenuScreen.activeSelf || game.GameScreen.activeSelf || game.ResultsScreen.activeSelf)
        {
            Fail("После запуска должен быть активен экран меню.");
            yield break;
        }

        game.StartGame();
        yield return new WaitForSeconds(0.3f);

        int expectedScore = 0;
        int expectedCorrect = 0;
        for (int questionNumber = 1; questionNumber <= 10; questionNumber++)
        {
            if (!game.GameScreen.activeSelf)
            {
                Fail($"Вопрос {questionNumber}: экран игры неактивен.");
                yield break;
            }
            if (game.QuestionCounterText.text != $"{questionNumber} / 10")
            {
                Fail($"Вопрос {questionNumber}: счётчик вопросов '{game.QuestionCounterText.text}' вместо '{questionNumber} / 10'.");
                yield break;
            }

            string correctTranslation = FindTranslation(game, game.EnglishWordText.text);
            if (correctTranslation == null)
            {
                Fail($"Вопрос {questionNumber}: неизвестное слово '{game.EnglishWordText.text}'.");
                yield break;
            }
            if (game.CardImage.sprite == null || !game.CardImage.enabled)
            {
                Fail($"Вопрос {questionNumber}: изображение не назначено.");
                yield break;
            }

            int correctButtonIndex = -1;
            int wrongButtonIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (game.AnswerTexts[i].text == correctTranslation)
                {
                    correctButtonIndex = i;
                }
                else if (wrongButtonIndex < 0)
                {
                    wrongButtonIndex = i;
                }
            }
            if (correctButtonIndex < 0 || wrongButtonIndex < 0)
            {
                Fail($"Вопрос {questionNumber}: вариант правильного перевода отсутствует среди кнопок.");
                yield break;
            }

            bool clickCorrect = questionNumber % 2 == 1;
            if (clickCorrect)
            {
                expectedScore += 10;
                expectedCorrect++;
            }
            game.AnswerButtons[clickCorrect ? correctButtonIndex : wrongButtonIndex].onClick.Invoke();

            Color chosenColor = game.AnswerButtons[clickCorrect ? correctButtonIndex : wrongButtonIndex].image.color;
            Color correctColor = game.AnswerButtons[correctButtonIndex].image.color;
            bool chosenIsGreen = IsClose(chosenColor, new Color32(76, 175, 80, 255));
            bool chosenIsRed = IsClose(chosenColor, new Color32(229, 57, 53, 255));
            if (clickCorrect && !chosenIsGreen)
            {
                Fail($"Вопрос {questionNumber}: правильный ответ не подсвечен зелёным ({chosenColor}).");
                yield break;
            }
            if (!clickCorrect && !chosenIsRed)
            {
                Fail($"Вопрос {questionNumber}: неправильный ответ не подсвечен красным ({chosenColor}).");
                yield break;
            }
            if (!IsClose(correctColor, new Color32(76, 175, 80, 255)))
            {
                Fail($"Вопрос {questionNumber}: правильная кнопка не подсвечена зелёным.");
                yield break;
            }
            for (int i = 0; i < 4; i++)
            {
                if (game.AnswerButtons[i].interactable)
                {
                    Fail($"Вопрос {questionNumber}: кнопки не заблокированы после ответа.");
                    yield break;
                }
            }
            if (game.ScoreText.text != $"Счёт: {expectedScore}")
            {
                Fail($"Вопрос {questionNumber}: счёт '{game.ScoreText.text}' вместо 'Счёт: {expectedScore}'.");
                yield break;
            }

            yield return new WaitForSeconds(1.6f);
        }

        if (!game.ResultsScreen.activeSelf || game.GameScreen.activeSelf)
        {
            Fail("После 10 вопросов не открыт экран результатов.");
            yield break;
        }
        if (!game.ResultScoreText.text.Contains($"Итоговый счёт: {expectedScore} из 100") ||
            !game.ResultCorrectText.text.Contains($"Правильных ответов: {expectedCorrect} из 10") ||
            !game.ResultWrongText.text.Contains($"Неправильных ответов: {10 - expectedCorrect}"))
        {
            Fail($"Тексты результатов неверны: '{game.ResultScoreText.text}', '{game.ResultCorrectText.text}', '{game.ResultWrongText.text}'.");
            yield break;
        }

        game.MenuButton.onClick.Invoke();
        if (!game.MenuScreen.activeSelf || game.ResultsScreen.activeSelf)
        {
            Fail("Кнопка 'Главное меню' не вернула в меню.");
            yield break;
        }

        game.StartButton.onClick.Invoke();
        yield return new WaitForSeconds(0.3f);
        if (!game.GameScreen.activeSelf || game.QuestionCounterText.text != "1 / 10" || game.ScoreText.text != "Счёт: 0")
        {
            Fail($"Повторный запуск не сбросил прогресс: '{game.QuestionCounterText.text}', '{game.ScoreText.text}'.");
            yield break;
        }

        for (int questionNumber = 1; questionNumber <= 10; questionNumber++)
        {
            string correctTranslation = FindTranslation(game, game.EnglishWordText.text);
            int correctButtonIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (game.AnswerTexts[i].text == correctTranslation)
                {
                    correctButtonIndex = i;
                }
            }
            if (correctButtonIndex < 0)
            {
                Fail($"Повторная игра, вопрос {questionNumber}: правильный вариант не найден.");
                yield break;
            }
            game.AnswerButtons[correctButtonIndex].onClick.Invoke();
            yield return new WaitForSeconds(1.6f);
        }

        if (!game.ResultsScreen.activeSelf ||
            !game.ResultScoreText.text.Contains("Итоговый счёт: 100 из 100") ||
            !game.ResultCorrectText.text.Contains("Правильных ответов: 10 из 10") ||
            !game.ResultWrongText.text.Contains("Неправильных ответов: 0"))
        {
            Fail($"Результаты повторной игры неверны: '{game.ResultScoreText.text}', '{game.ResultCorrectText.text}', '{game.ResultWrongText.text}'.");
            yield break;
        }

        Debug.Log("RUNTIME CHECK PASSED");
        EditorApplication.Exit(0);
    }

    static string FindTranslation(EnglishUpGame game, string englishWord)
    {
        for (int i = 0; i < EnglishUpGame.EnglishWords.Length; i++)
        {
            if (EnglishUpGame.EnglishWords[i] == englishWord)
            {
                return EnglishUpGame.RussianTranslations[i];
            }
        }
        return null;
    }

    static bool IsClose(Color color, Color32 target)
    {
        return Mathf.Abs(color.r - target.r) < 2f &&
               Mathf.Abs(color.g - target.g) < 2f &&
               Mathf.Abs(color.b - target.b) < 2f;
    }

    static void Fail(string message)
    {
        Debug.LogError($"RUNTIME CHECK FAILED: {message}");
        EditorApplication.Exit(1);
    }
}
