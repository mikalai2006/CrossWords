using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[System.Serializable]
public class ParseWords : MonoBehaviour
{
  // [SerializeField] public List<WordsFile> files;
  async void Start()
  {
    await LocalizationSettings.InitializationOperation.Task;

    List<Words> words = new();

    foreach (var file in GameManager.Instance.GameSettings.wordFiles)
    {
      var wordsFile = JsonUtility.FromJson<Words>(file.file.text);

      words.Add(new Words()
      {
        data = wordsFile.data,
        locale = file.locale,
        localeCode = file.locale.Identifier.Code
      });

      Debug.Log($"Load {wordsFile.data.Length} words by of {file.locale.LocaleName}!");
    }
    // var wordsEn = JsonUtility.FromJson<Words>(jsonFileEn.text);

    GameManager.Instance.InitWords(words);

    // GameManager.Instance.Words.data.OrderBy(t => Random.value);

  }
}

[System.Serializable]
public class Words
{
  public Locale locale;
  public string[] data;
  public string localeCode;
}

[System.Serializable]
public class WordsFile
{
  public TextLocalize text;
  public Locale locale;
  public TextAsset file;
}
