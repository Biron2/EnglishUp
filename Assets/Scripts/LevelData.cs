using UnityEngine;
using System.Collections.Generic;

public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public List<QuestionData> Questions = new List<QuestionData>();
}
