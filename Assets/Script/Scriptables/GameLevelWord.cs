using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameLevelWord : ScriptableObject
{
  // public string idLevelWord;
  public string word;

  [Space(10)]
  [Header("Bonus settings")]
  public int minRate;

}
