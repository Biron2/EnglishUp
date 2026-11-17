using UnityEngine;
using System.Collections.Generic;

public class QuestionData : ScriptableObject
{
    public string EnglishWord;
    public string CorrectTranslation;
    public List<string> Options = new List<string>();
    public int CorrectAnswerIndex;
    public Sprite Image;
}
