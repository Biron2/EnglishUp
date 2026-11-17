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
        public List<string> Options = new List<string>();
        public int CorrectAnswerIndex;
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

    [SerializeField] private Color _normalButtonColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color _correctButtonColor = new Color32(76, 175, 80, 255);
    [SerializeField] private Color _wrongButtonColor = new Color32(229, 57, 53, 255);
    [SerializeField] private Color _normalTextColor = new Color32(23, 62, 122, 255);
    [SerializeField] private Color _highlightTextColor = new Color32(255, 255, 255, 255);

    [SerializeField] private LevelData[] _levels;
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
        if (_levels == null || _levels.Length == 0)
        {
            _levels = new LevelData[] { ScriptableObject.CreateInstance<LevelData>() };
            CreateDefaultQuestions(_levels[0]);
        }
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            int answerIndex = i;
            _answerButtons[i].onClick.AddListener(() => _onAnswerChosen(answerIndex));
        }
    }

    void Start()
    {
        _allQuestions.Clear();
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
        _allQuestions.Clear();

        LevelData levelData = (_levels != null && _levels.Length > 0) ? _levels[0] : null;
        if (levelData == null)
        {
            Debug.LogWarning("Уровень не назначен. Создайте LevelData через Assets/Create/Configs/Levels.");
            _showMenu();
            return;
        }

        foreach (QuestionData question in levelData.Questions)
        {
            QuizQuestion quizQuestion = new QuizQuestion();
            quizQuestion.EnglishWord = question.EnglishWord;
            quizQuestion.RussianTranslation = question.CorrectTranslation;
            quizQuestion.Options = new List<string>(question.Options);
            quizQuestion.CorrectAnswerIndex = question.CorrectAnswerIndex;
            quizQuestion.Image = question.Image;
            _allQuestions.Add(quizQuestion);
        }

        List<QuizQuestion> availableQuestions = new List<QuizQuestion>(_allQuestions);
        _sessionQuestionCount = Mathf.Min(_questionsPerGame, availableQuestions.Count);
        Shuffle(availableQuestions);
        _remainingQuestions.AddRange(availableQuestions);

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

        _currentOptions.Clear();
        for (int i = 0; i < question.Options.Count; i++)
        {
            _currentOptions.Add(i);
        }
        Shuffle(_currentOptions);
        _correctOptionIndex = _currentOptions.IndexOf(question.CorrectAnswerIndex);
        while (_currentOptions.Count > _answerButtons.Length)
        {
            _currentOptions.RemoveAt(_currentOptions.Count - 1);
        }

        for (int i = 0; i < _answerButtons.Length; i++)
        {
            _answerTexts[i].text = question.Options[_currentOptions[i]];
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

    private void CreateDefaultQuestions(LevelData levelData)
    {
        levelData.LevelNumber = 1;
        levelData.Questions.Clear();
        for (int i = 0; i < EnglishWords.Length && i < RussianTranslations.Length; i++)
        {
            QuestionData question = ScriptableObject.CreateInstance<QuestionData>();
            question.EnglishWord = EnglishWords[i];
            question.CorrectTranslation = RussianTranslations[i];
            question.Options = new List<string>();
            question.Options.Add(RussianTranslations[i]);
            question.Options.Add(RussianTranslations[(i + 1) % RussianTranslations.Length]);
            question.Options.Add(RussianTranslations[(i + 2) % RussianTranslations.Length]);
            question.Options.Add(RussianTranslations[(i + 3) % RussianTranslations.Length]);
            question.CorrectAnswerIndex = 0;
            question.Image = null;
            levelData.Questions.Add(question);
        }
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
