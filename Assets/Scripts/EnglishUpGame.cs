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

    private static readonly int _questionsPerGame = 10;
    private static readonly int _pointsPerAnswer = 10;
    private static readonly float _answerDelaySeconds = 1.2f;

    private static readonly Color _normalButtonColor = new Color32(255, 255, 255, 255);
    private static readonly Color _correctButtonColor = new Color32(76, 175, 80, 255);
    private static readonly Color _wrongButtonColor = new Color32(229, 57, 53, 255);
    private static readonly Color _normalTextColor = new Color32(23, 62, 122, 255);
    private static readonly Color _highlightTextColor = new Color32(255, 255, 255, 255);

    [SerializeField] private GameObject _menuScreen;
    [SerializeField] private GameObject _gameScreen;
    [SerializeField] private GameObject _resultsScreen;

    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _menuButton;

    [SerializeField] private TMP_Text _questionCounterText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _englishWordText;
    [SerializeField] private Image _cardImage;
    [SerializeField] private Button[] _answerButtons;
    [SerializeField] private TMP_Text[] _answerTexts;

    [SerializeField] private TMP_Text _resultScoreText;
    [SerializeField] private TMP_Text _resultCorrectText;
    [SerializeField] private TMP_Text _resultWrongText;

    [SerializeField] private Sprite[] _questionImages;

    private readonly List<QuizQuestion> _allQuestions = new List<QuizQuestion>();
    private readonly List<QuizQuestion> _remainingQuestions = new List<QuizQuestion>();
    private readonly List<int> _currentOptions = new List<int>();

    private int _sessionQuestionCount;
    private int _currentQuestionNumber;
    private int _score;
    private int _correctAnswers;
    private int _correctOptionIndex;

    void Awake()
    {
        _startButton.onClick.AddListener(StartGame);
        _exitButton.onClick.AddListener(_quitGame);
        _playAgainButton.onClick.AddListener(StartGame);
        _menuButton.onClick.AddListener(_showMenu);
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            int answerIndex = i;
            _answerButtons[i].onClick.AddListener(() => _onAnswerChosen(answerIndex));
        }
    }

    void Start()
    {
        _allQuestions.Clear();
        for (int i = 0; i < EnglishWords.Length; i++)
        {
            QuizQuestion question = new QuizQuestion();
            question.EnglishWord = EnglishWords[i];
            question.RussianTranslation = RussianTranslations[i];
            question.Image = i < _questionImages.Length ? _questionImages[i] : null;
            _allQuestions.Add(question);
        }
        _showMenu();
    }

    private void _showMenu()
    {
        _menuScreen.SetActive(true);
        _gameScreen.SetActive(false);
        _resultsScreen.SetActive(false);
    }

    private void _quitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        _score = 0;
        _correctAnswers = 0;
        _currentQuestionNumber = 0;
        _remainingQuestions.Clear();

        List<QuizQuestion> availableQuestions = _allQuestions.FindAll(question => question.Image != null);
        if (availableQuestions.Count == 0)
        {
            Debug.LogWarning("Нет изображений для вопросов. Добавьте спрайты в Assets/Content/Images и назначьте их в поле QuestionImages.");
            _showMenu();
            return;
        }

        Shuffle(availableQuestions);
        _remainingQuestions.AddRange(availableQuestions);
        _sessionQuestionCount = Mathf.Min(_questionsPerGame, _remainingQuestions.Count);

        _resetAnswerButtons();
        _menuScreen.SetActive(false);
        _resultsScreen.SetActive(false);
        _gameScreen.SetActive(true);
        _showQuestion();
    }

    private void _showQuestion()
    {
        if (_currentQuestionNumber >= _sessionQuestionCount)
        {
            _showResults();
            return;
        }

        _currentQuestionNumber++;
        QuizQuestion question = _remainingQuestions[0];
        _remainingQuestions.RemoveAt(0);

        _englishWordText.text = question.EnglishWord;
        _cardImage.sprite = question.Image;
        _cardImage.enabled = question.Image != null;
        _questionCounterText.text = $"{_currentQuestionNumber} / {_sessionQuestionCount}";
        _scoreText.text = $"Счёт: {_score}";

        int correctTranslationIndex = _indexOfTranslation(question.RussianTranslation);
        _currentOptions.Clear();
        _currentOptions.Add(correctTranslationIndex);
        while (_currentOptions.Count < 4)
        {
            int optionIndex = Random.Range(0, RussianTranslations.Length);
            if (!_currentOptions.Contains(optionIndex))
            {
                _currentOptions.Add(optionIndex);
            }
        }
        Shuffle(_currentOptions);
        _correctOptionIndex = _currentOptions.IndexOf(correctTranslationIndex);

        for (int i = 0; i < _answerButtons.Length; i++)
        {
            _answerTexts[i].text = RussianTranslations[_currentOptions[i]];
        }
        _resetAnswerButtons();
    }

    private void _onAnswerChosen(int chosenIndex)
    {
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            _answerButtons[i].interactable = false;
        }

        bool isCorrect = chosenIndex == _correctOptionIndex;
        if (isCorrect)
        {
            _score += _pointsPerAnswer;
            _correctAnswers++;
        }

        _answerButtons[chosenIndex].image.color = isCorrect ? _correctButtonColor : _wrongButtonColor;
        _answerTexts[chosenIndex].color = _highlightTextColor;
        _answerButtons[_correctOptionIndex].image.color = _correctButtonColor;
        _answerTexts[_correctOptionIndex].color = _highlightTextColor;
        _scoreText.text = $"Счёт: {_score}";

        StartCoroutine(_goToNextQuestion());
    }

    private IEnumerator _goToNextQuestion()
    {
        yield return new WaitForSeconds(_answerDelaySeconds);
        _resetAnswerButtons();
        _showQuestion();
    }

    private void _resetAnswerButtons()
    {
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            _answerButtons[i].interactable = true;
            _answerButtons[i].image.color = _normalButtonColor;
            _answerTexts[i].color = _normalTextColor;
        }
    }

    private void _showResults()
    {
        int wrongAnswers = _sessionQuestionCount - _correctAnswers;
        _resultScoreText.text = $"Итоговый счёт: {_score} из {_sessionQuestionCount * _pointsPerAnswer}";
        _resultCorrectText.text = $"Правильных ответов: {_correctAnswers} из {_sessionQuestionCount}";
        _resultWrongText.text = $"Неправильных ответов: {wrongAnswers}";
        _gameScreen.SetActive(false);
        _resultsScreen.SetActive(true);
    }

    private int _indexOfTranslation(string translation)
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

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
