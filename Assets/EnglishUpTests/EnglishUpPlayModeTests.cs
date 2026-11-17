using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class EnglishUpPlayModeTests
{
    [UnityTest]
    public IEnumerator FullGameSessionPasses()
    {
        SceneManager.LoadScene("EnglishUp");
        yield return null;
        yield return null;

        EnglishUpGame game = Object.FindFirstObjectByType<EnglishUpGame>();
        Assert.IsNotNull(game, "Компонент EnglishUpGame не найден в сцене.");

        Assert.IsNotNull(game.MenuScreen);
        Assert.IsNotNull(game.GameScreen);
        Assert.IsNotNull(game.ResultsScreen);
        Assert.IsNotNull(game.StartButton);
        Assert.IsNotNull(game.ExitButton);
        Assert.IsNotNull(game.PlayAgainButton);
        Assert.IsNotNull(game.MenuButton);
        Assert.IsNotNull(game.QuestionCounterText);
        Assert.IsNotNull(game.ScoreText);
        Assert.IsNotNull(game.EnglishWordText);
        Assert.IsNotNull(game.CardImage);
        Assert.IsNotNull(game.ResultScoreText);
        Assert.IsNotNull(game.ResultCorrectText);
        Assert.IsNotNull(game.ResultWrongText);
        Assert.IsNotNull(game.AnswerButtons);
        Assert.AreEqual(4, game.AnswerButtons.Length);
        Assert.IsNotNull(game.AnswerTexts);
        Assert.AreEqual(4, game.AnswerTexts.Length);

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

        Assert.IsTrue(game.MenuScreen.activeSelf, "После запуска должен быть активен экран меню.");
        Assert.IsFalse(game.GameScreen.activeSelf);
        Assert.IsFalse(game.ResultsScreen.activeSelf);

        game.StartGame();
        yield return new WaitForSeconds(0.3f);

        int expectedScore = 0;
        int expectedCorrect = 0;
        for (int questionNumber = 1; questionNumber <= 10; questionNumber++)
        {
            Assert.IsTrue(game.GameScreen.activeSelf, $"Вопрос {questionNumber}: экран игры неактивен.");
            Assert.AreEqual($"{questionNumber} / 10", game.QuestionCounterText.text, $"Вопрос {questionNumber}: счётчик вопросов.");

            string correctTranslation = FindTranslation(game.EnglishWordText.text);
            Assert.IsNotNull(correctTranslation, $"Вопрос {questionNumber}: неизвестное слово '{game.EnglishWordText.text}'.");
            Assert.IsNotNull(game.CardImage.sprite, $"Вопрос {questionNumber}: изображение не назначено.");
            Assert.IsTrue(game.CardImage.enabled, $"Вопрос {questionNumber}: изображение выключено.");

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
            Assert.GreaterOrEqual(correctButtonIndex, 0, $"Вопрос {questionNumber}: правильный перевод отсутствует среди кнопок.");
            Assert.GreaterOrEqual(wrongButtonIndex, 0, $"Вопрос {questionNumber}: неправильный перевод отсутствует среди кнопок.");

            bool clickCorrect = questionNumber % 2 == 1;
            if (clickCorrect)
            {
                expectedScore += 10;
                expectedCorrect++;
            }
            game.AnswerButtons[clickCorrect ? correctButtonIndex : wrongButtonIndex].onClick.Invoke();

            Color chosenColor = game.AnswerButtons[clickCorrect ? correctButtonIndex : wrongButtonIndex].image.color;
            Color correctColor = game.AnswerButtons[correctButtonIndex].image.color;
            if (clickCorrect)
            {
                AssertColor(chosenColor, new Color32(76, 175, 80, 255), $"Вопрос {questionNumber}: правильный ответ должен быть зелёным.");
            }
            else
            {
                AssertColor(chosenColor, new Color32(229, 57, 53, 255), $"Вопрос {questionNumber}: неправильный ответ должен быть красным.");
            }
            AssertColor(correctColor, new Color32(76, 175, 80, 255), $"Вопрос {questionNumber}: правильная кнопка должна быть зелёной.");

            for (int i = 0; i < 4; i++)
            {
                Assert.IsFalse(game.AnswerButtons[i].interactable, $"Вопрос {questionNumber}: кнопки не заблокированы после ответа.");
            }
            Assert.AreEqual($"Счёт: {expectedScore}", game.ScoreText.text, $"Вопрос {questionNumber}: счёт.");

            yield return new WaitForSeconds(1.6f);
        }

        Assert.IsTrue(game.ResultsScreen.activeSelf, "После 10 вопросов не открыт экран результатов.");
        Assert.IsFalse(game.GameScreen.activeSelf);
        StringAssert.Contains($"Итоговый счёт: {expectedScore} из 100", game.ResultScoreText.text);
        StringAssert.Contains($"Правильных ответов: {expectedCorrect} из 10", game.ResultCorrectText.text);
        StringAssert.Contains($"Неправильных ответов: {10 - expectedCorrect}", game.ResultWrongText.text);

        game.MenuButton.onClick.Invoke();
        Assert.IsTrue(game.MenuScreen.activeSelf, "Кнопка 'Главное меню' не вернула в меню.");
        Assert.IsFalse(game.ResultsScreen.activeSelf);

        game.StartButton.onClick.Invoke();
        yield return new WaitForSeconds(0.3f);
        Assert.IsTrue(game.GameScreen.activeSelf, "Повторный запуск не открыл экран игры.");
        Assert.AreEqual("1 / 10", game.QuestionCounterText.text, "Повторный запуск не сбросил счётчик вопросов.");
        Assert.AreEqual("Счёт: 0", game.ScoreText.text, "Повторный запуск не сбросил счёт.");

        for (int questionNumber = 1; questionNumber <= 10; questionNumber++)
        {
            string correctTranslation = FindTranslation(game.EnglishWordText.text);
            int correctButtonIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (game.AnswerTexts[i].text == correctTranslation)
                {
                    correctButtonIndex = i;
                }
            }
            Assert.GreaterOrEqual(correctButtonIndex, 0, $"Повторная игра, вопрос {questionNumber}: правильный вариант не найден.");

            game.AnswerButtons[correctButtonIndex].onClick.Invoke();
            yield return new WaitForSeconds(1.6f);
        }

        Assert.IsTrue(game.ResultsScreen.activeSelf, "Повторная игра не открыла экран результатов.");
        StringAssert.Contains("Итоговый счёт: 100 из 100", game.ResultScoreText.text);
        StringAssert.Contains("Правильных ответов: 10 из 10", game.ResultCorrectText.text);
        StringAssert.Contains("Неправильных ответов: 0", game.ResultWrongText.text);
    }

    static string FindTranslation(string englishWord)
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

    static void AssertColor(Color actual, Color32 expected, string message)
    {
        Assert.Less(Mathf.Abs(actual.r - expected.r), 2f, message);
        Assert.Less(Mathf.Abs(actual.g - expected.g), 2f, message);
        Assert.Less(Mathf.Abs(actual.b - expected.b), 2f, message);
    }
}
