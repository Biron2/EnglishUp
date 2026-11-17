using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnglishUpGame : MonoBehaviour
{
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

    private static readonly int QuestionsPerGame = 10;
    private static readonly int PointsPerAnswer = 10;
    private static readonly float AnswerDelaySeconds = 1.2f;

    private static readonly Color NormalButtonColor = new Color32(255, 255, 255, 255);
    private static readonly Color CorrectButtonColor = new Color32(76, 175, 80, 255);
    private static readonly Color WrongButtonColor = new Color32(229, 57, 53, 255);
    private static readonly Color NormalTextColor = new Color32(23, 62, 122, 255);
    private static readonly Color HighlightTextColor = new Color32(255, 255, 255, 255);

    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private GameObject GameScreen;
    [SerializeField] private GameObject ResultsScreen;

    [SerializeField] private Button StartButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private Button PlayAgainButton;
    [SerializeField] private Button MenuButton;

    [SerializeField] private TMP_Text QuestionCounterText;
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text EnglishWordText;
    [SerializeField] private Image CardImage;
    [SerializeField] private Button[] AnswerButtons;
    [SerializeField] private TMP_Text[] AnswerTexts;

    [SerializeField] private TMP_Text ResultScoreText;
    [SerializeField] private TMP_Text ResultCorrectText;
    [SerializeField] private TMP_Text ResultWrongText;

    [SerializeField] private Sprite[] QuestionImages;

    private readonly List<QuizQuestion> allQuestions = new List<QuizQuestion>();
    private readonly List<QuizQuestion> remainingQuestions = new List<QuizQuestion>();
    private readonly List<int> currentOptions = new List<int>();

    private int sessionQuestionCount;
    private int currentQuestionNumber;
    private int score;
    private int correctAnswers;
    private int correctOptionIndex;

    private void Awake()
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

    private void Start()
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

    private  void ShowMenu()
    {
        MenuScreen.SetActive(true);
        GameScreen.SetActive(false);
        ResultsScreen.SetActive(false);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void StartGame()
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
