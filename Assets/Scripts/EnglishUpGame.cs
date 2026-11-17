using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnglishUpGame : MonoBehaviour
{
    public class QuizQuestion
    {
        public string EnglishWord;
        public string RussianTranslation;
        public Sprite Image;
    }

    public static readonly string[] EnglishWords =
    {
        "Apple", "Banana", "Orange", "Cat", "Dog", "House", "Book", "Car", "Sun", "Tree",
        "School", "Water", "Milk", "Bread", "Chair", "Table", "Flower", "Fish", "Bird", "Computer"
    };

    public static readonly string[] RussianTranslations =
    {
        "Яблоко", "Банан", "Апельсин", "Кошка", "Собака", "Дом", "Книга", "Машина", "Солнце", "Дерево",
        "Школа", "Вода", "Молоко", "Хлеб", "Стул", "Стол", "Цветок", "Рыба", "Птица", "Компьютер"
    };

    static readonly int QuestionsPerGame = 10;
    static readonly int PointsPerAnswer = 10;
    static readonly float AnswerDelaySeconds = 1.2f;

    static readonly Color NormalButtonColor = new Color32(255, 255, 255, 255);
    static readonly Color CorrectButtonColor = new Color32(76, 175, 80, 255);
    static readonly Color WrongButtonColor = new Color32(229, 57, 53, 255);
    static readonly Color NormalTextColor = new Color32(23, 62, 122, 255);
    static readonly Color HighlightTextColor = new Color32(255, 255, 255, 255);

    public GameObject MenuScreen;
    public GameObject GameScreen;
    public GameObject ResultsScreen;

    public Button StartButton;
    public Button ExitButton;
    public Button PlayAgainButton;
    public Button MenuButton;

    public TMP_Text QuestionCounterText;
    public TMP_Text ScoreText;
    public TMP_Text EnglishWordText;
    public Image CardImage;
    public Button[] AnswerButtons;
    public TMP_Text[] AnswerTexts;

    public TMP_Text ResultScoreText;
    public TMP_Text ResultCorrectText;
    public TMP_Text ResultWrongText;

    public Sprite[] QuestionImages;

    readonly List<QuizQuestion> allQuestions = new List<QuizQuestion>();
    readonly List<QuizQuestion> remainingQuestions = new List<QuizQuestion>();
    readonly List<int> currentOptions = new List<int>();

    int sessionQuestionCount;
    int currentQuestionNumber;
    int score;
    int correctAnswers;
    int correctOptionIndex;

    void Awake()
    {
        StartButton.onClick.AddListener(StartGame);
        ExitButton.onClick.AddListener(QuitGame);
        PlayAgainButton.onClick.AddListener(StartGame);
        MenuButton.onClick.AddListener(ShowMenu);
        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            int answerIndex = i;
            AnswerButtons[i].onClick.AddListener(() => OnAnswerChosen(answerIndex));
        }
    }

    void Start()
    {
        allQuestions.Clear();
        for (int i = 0; i < EnglishWords.Length; i++)
        {
            QuizQuestion question = new QuizQuestion();
            question.EnglishWord = EnglishWords[i];
            question.RussianTranslation = RussianTranslations[i];
            question.Image = i < QuestionImages.Length ? QuestionImages[i] : null;
            allQuestions.Add(question);
        }
        ShowMenu();
    }

    void ShowMenu()
    {
        MenuScreen.SetActive(true);
        GameScreen.SetActive(false);
        ResultsScreen.SetActive(false);
    }

    void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        score = 0;
        correctAnswers = 0;
        currentQuestionNumber = 0;
        remainingQuestions.Clear();

        List<QuizQuestion> availableQuestions = allQuestions.FindAll(question => question.Image != null);
        if (availableQuestions.Count == 0)
        {
            Debug.LogWarning("Нет изображений для вопросов. Добавьте спрайты в Assets/Content/Images и назначьте их в поле QuestionImages.");
            ShowMenu();
            return;
        }

        Shuffle(availableQuestions);
        remainingQuestions.AddRange(availableQuestions);
        sessionQuestionCount = Mathf.Min(QuestionsPerGame, remainingQuestions.Count);

        ResetAnswerButtons();
        MenuScreen.SetActive(false);
        ResultsScreen.SetActive(false);
        GameScreen.SetActive(true);
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestionNumber >= sessionQuestionCount)
        {
            ShowResults();
            return;
        }

        currentQuestionNumber++;
        QuizQuestion question = remainingQuestions[0];
        remainingQuestions.RemoveAt(0);

        EnglishWordText.text = question.EnglishWord;
        CardImage.sprite = question.Image;
        CardImage.enabled = question.Image != null;
        QuestionCounterText.text = $"{currentQuestionNumber} / {sessionQuestionCount}";
        ScoreText.text = $"Счёт: {score}";

        int correctTranslationIndex = IndexOfTranslation(question.RussianTranslation);
        currentOptions.Clear();
        currentOptions.Add(correctTranslationIndex);
        while (currentOptions.Count < 4)
        {
            int optionIndex = Random.Range(0, RussianTranslations.Length);
            if (!currentOptions.Contains(optionIndex))
            {
                currentOptions.Add(optionIndex);
            }
        }
        Shuffle(currentOptions);
        correctOptionIndex = currentOptions.IndexOf(correctTranslationIndex);

        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            AnswerTexts[i].text = RussianTranslations[currentOptions[i]];
        }
        ResetAnswerButtons();
    }

    void OnAnswerChosen(int chosenIndex)
    {
        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            AnswerButtons[i].interactable = false;
        }

        bool isCorrect = chosenIndex == correctOptionIndex;
        if (isCorrect)
        {
            score += PointsPerAnswer;
            correctAnswers++;
        }

        AnswerButtons[chosenIndex].image.color = isCorrect ? CorrectButtonColor : WrongButtonColor;
        AnswerTexts[chosenIndex].color = HighlightTextColor;
        AnswerButtons[correctOptionIndex].image.color = CorrectButtonColor;
        AnswerTexts[correctOptionIndex].color = HighlightTextColor;
        ScoreText.text = $"Счёт: {score}";

        StartCoroutine(GoToNextQuestion());
    }

    IEnumerator GoToNextQuestion()
    {
        yield return new WaitForSeconds(AnswerDelaySeconds);
        ResetAnswerButtons();
        ShowQuestion();
    }

    void ResetAnswerButtons()
    {
        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            AnswerButtons[i].interactable = true;
            AnswerButtons[i].image.color = NormalButtonColor;
            AnswerTexts[i].color = NormalTextColor;
        }
    }

    void ShowResults()
    {
        int wrongAnswers = sessionQuestionCount - correctAnswers;
        ResultScoreText.text = $"Итоговый счёт: {score} из {sessionQuestionCount * PointsPerAnswer}";
        ResultCorrectText.text = $"Правильных ответов: {correctAnswers} из {sessionQuestionCount}";
        ResultWrongText.text = $"Неправильных ответов: {wrongAnswers}";
        GameScreen.SetActive(false);
        ResultsScreen.SetActive(true);
    }

    static int IndexOfTranslation(string translation)
    {
        for (int i = 0; i < RussianTranslations.Length; i++)
        {
            if (RussianTranslations[i] == translation)
            {
                return i;
            }
        }
        return -1;
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
