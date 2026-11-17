using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Configs/Levels")]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public List<QuestionData> Questions = new List<QuestionData>();
}
