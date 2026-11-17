using System.Collections.Generic;
using UnityEngine;

public class QuizQuestion
{
    public string EnglishWord;
    public string RussianTranslation;
    public List<string> Options = new List<string>();
    public int CorrectAnswerIndex;
    public Sprite Image;
}