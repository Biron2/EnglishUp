using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnglishUpGame : MonoBehaviour
{
    private static readonly int _questionsPerGame = 10;
    private static readonly int _pointsPerAnswer = 10;
    private static readonly float _answerDelaySeconds = 1.2f;

    [SerializeField] private Color _normalButtonColor;
    [SerializeField] private Color _correctButtonColor;
    [SerializeField] private Color _wrongButtonColor;
    [SerializeField] private Color _normalTextColor;
    [SerializeField] private Color _highlightTextColor;

    [SerializeField] private LevelData[] _levels;
    [SerializeField] private GameObject _menuScreen;
    [SerializeField] private GameObject _gameScreen;
    [SerializeField] private GameObject _resultsScreen;
    [SerializeField] private GameObject _levelsScreen;

    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _backFromLevelsButton;
    [SerializeField] private Button _resetProgressButton;
    [SerializeField] private Button[] _levelButtons;

    [SerializeField] private TMP_Text _questionCounterText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _englishWordText;
    [SerializeField] private Image _cardImage;
    [SerializeField] private Button[] _answerButtons;
    [SerializeField] private TMP_Text[] _answerTexts;

    [SerializeField] private TMP_Text _resultScoreText;
    [SerializeField] private TMP_Text _resultCorrectText;
    [SerializeField] private TMP_Text _resultWrongText;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _correctAnswerClip;
    [SerializeField] private AudioClip _wrongAnswerClip;
    [SerializeField] private AudioClip _levelWinClip;
    [SerializeField] private AudioClip _levelLoseClip;

    private readonly List<QuizQuestion> _allQuestions = new List<QuizQuestion>();
    private readonly List<QuizQuestion> _remainingQuestions = new List<QuizQuestion>();
    private readonly List<int> _currentOptions = new List<int>();

    private int _sessionQuestionCount;
    private int _currentQuestionNumber;
    private int _score;
    private int _correctAnswers;
    private int _correctOptionIndex;
    private int _currentLevelIndex;
    private int _maxUnlockedLevel;

    void Awake()
    {
        _startButton.onClick.AddListener(_showLevelsScreen);
        _exitButton.onClick.AddListener(_quitGame);
        _playAgainButton.onClick.AddListener(StartGame);
        _menuButton.onClick.AddListener(_showMenu);
        if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(StartNextLevel);
        if (_backFromLevelsButton != null) _backFromLevelsButton.onClick.AddListener(_showMenu);
        if (_resetProgressButton != null) _resetProgressButton.onClick.AddListener(_resetProgress);
        if (_levelButtons != null)
        {
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int index = i;
                _levelButtons[i].onClick.AddListener(() => _onLevelSelected(index));
            }
        }

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        }
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            int answerIndex = i;
            _answerButtons[i].onClick.AddListener(() => _onAnswerChosen(answerIndex));
        }
    }

    void Start()
    {
        _maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        _allQuestions.Clear();
        _showMenu();
    }

    private void _showMenu()
    {
        _currentLevelIndex = 0;
        _menuScreen.SetActive(true);
        _gameScreen.SetActive(false);
        _resultsScreen.SetActive(false);
        if (_levelsScreen != null) _levelsScreen.SetActive(false);
    }

    private void _showLevelsScreen()
    {
        _menuScreen.SetActive(false);
        _gameScreen.SetActive(false);
        _resultsScreen.SetActive(false);
        if (_levelsScreen != null) _levelsScreen.SetActive(true);
        _updateLevelButtons();
    }

    private void _updateLevelButtons()
    {
        if (_levelButtons == null) return;
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            if (i < _levels.Length)
            {
                bool unlocked = (i + 1) <= _maxUnlockedLevel;
                _levelButtons[i].interactable = unlocked;
                TMP_Text t = _levelButtons[i].GetComponentInChildren<TMP_Text>();
                if (t != null) t.text = "Уровень " + (i + 1);
            }
            else
            {
                _levelButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void _resetProgress()
    {
        _maxUnlockedLevel = 1;
        PlayerPrefs.SetInt("MaxUnlockedLevel", _maxUnlockedLevel);
        PlayerPrefs.Save();
        _updateLevelButtons();
    }
    private void _onLevelSelected(int index)
    {
        if (index < 0 || index >= _levels.Length) return;
        if ((index + 1) > _maxUnlockedLevel) return;
        _currentLevelIndex = index;
        StartGame();
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

        LevelData levelData = (_levels != null && _levels.Length > 0) ? _levels[_currentLevelIndex % _levels.Length] : null;
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
        if (_levelsScreen != null) _levelsScreen.SetActive(false);
        _gameScreen.SetActive(true);
        _showQuestion();
    }

    public void StartNextLevel()
    {
        if (_levels == null || _levels.Length == 0) return;
        _currentLevelIndex++;
        if (_currentLevelIndex >= _levels.Length) _currentLevelIndex = 0;
        StartGame();
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
        _levelText.text = $"Уровень {_currentLevelIndex + 1}";

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

        if (isCorrect && _correctAnswerClip != null) _audioSource.PlayOneShot(_correctAnswerClip);
        else if (!isCorrect && _wrongAnswerClip != null) _audioSource.PlayOneShot(_wrongAnswerClip);

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
        if (_maxUnlockedLevel < _currentLevelIndex + 2 && _correctAnswers * 2 >= _sessionQuestionCount)
        {
            _maxUnlockedLevel = _currentLevelIndex + 2;
            PlayerPrefs.SetInt("MaxUnlockedLevel", _maxUnlockedLevel);
            PlayerPrefs.Save();
        }
        _updateLevelButtons();
        if (_correctAnswers * 2 >= _sessionQuestionCount)
        {
            if (_levelWinClip != null) _audioSource.PlayOneShot(_levelWinClip);
        }
        else
        {
            if (_levelLoseClip != null) _audioSource.PlayOneShot(_levelLoseClip);
        }
        _gameScreen.SetActive(false);
        _resultsScreen.SetActive(true);
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
