using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu]
public class GameLevel : ScriptableObject
{
  public string idLevel;
  public TextLocalize text;
  [Space(10)]
  [Header("---Difficulty settings")]
  public int minRate;
  public Locale locale;

  [Space(10)]
  [Header("Word settings")]
  public List<string> levelWords;
}

[System.Serializable]
public struct TextLocalize
{
  public LocalizedString title;
  public LocalizedString description;
}
