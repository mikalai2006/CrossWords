using UnityEngine;

[CreateAssetMenu]
public class GamePlayerSetting : ScriptableObject
{
  public string idPlayerSetting;
  public TextLocalize text;

  [Space(10)]
  [Header("---Difficulty settings")]
  [Space(3)]
  [Tooltip("Коэффициент сложности")]
  [Range(0f, 1f)] public float coefDifficulty;
  [Tooltip("Количество слов для перехода на новый уровень сложности")]
  public int countFindWordsForUp;
  [Tooltip("Максимальная длина слов, которые будут использоваться для табло")]
  public int maxLengthWord;
  // [Tooltip("Максимальная длина слова для набора символов")]
  // public int maxLengthWordForChars;
  // [Tooltip("Минимальная длина слова для набора символов")]
  // public int minLengthWordForChars;
  [Range(5, 200)] public int maxFindWords;
  [Tooltip("Коэффициент начисления начального количества подсказок - частая буква")]
  [Range(0f, .3f)] public float coefFrequency;
  [Tooltip("Коэффициент начисления начального количества подсказок - случайная буква")]
  [Range(0f, .3f)] public float coefStar;
  [Tooltip("Коэффициент начисления начального количества подсказок - бомба")]
  [Range(0f, .2f)] public float coefBomb;
  [Tooltip("Коэффициент начисления начального количества подсказок - молния")]
  [Range(0f, .2f)] public float coefLighting;
  // [Tooltip("Максимальное количество символов на поле")]
  // [Range(0, 1f)] public float coefHiddenChar;

  [Space(5)]
  [Header("---Bonuses")]
  [Space(3)]
  public BonusCount bonusCount;

}


[System.Serializable]
public struct BonusCount
{
  public int charInOrder;
  public int wordInOrder;
  public int charBonus;
  public int charStar;
  public int charHint;
  public int charBomb;
  public int charLighting;
  public int charCoin;
  public int errorNullBonus;
  public int errorNullOrderWord;
  public int maxStar;
  public int maxBomb;
  public int maxLighting;
  public int maxFrequency;
  // public int needCreateCoin;
}

// [System.Serializable]
// public struct BonusCount
// {
//   [Tooltip("Количество найденных символов для получения бонуса")]
//   [Range(1, 100)] public int charBonus;
//   [Tooltip("Сколько ошибок в угадывании слова - обнуляют бонусный прогресс")]
//   [Range(1, 20)] public int errorClear;
//   [Tooltip("Количество букв последовательно открытых для добавления доп. коина на поле слов")]
//   [Range(1, 100)] public int charCoin;
//   [Tooltip("Количество букв последовательно открытых для добавления доп. звезды на поле слов")]
//   [Range(1, 100)] public int charStar;
//   [Tooltip("Количество букв последовательно открытых для добавления подсказки")]
//   [Range(1, 100)] public int charHint;
// }