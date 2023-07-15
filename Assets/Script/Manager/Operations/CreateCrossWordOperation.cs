using System.Collections.Generic;
using Loader;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.Localization;

public class CreateCrossWordOperation : ILoadingOperation
{
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private ManagerHiddenWords _managerHiddenWords => GameManager.Instance.LevelManager.ManagerHiddenWords;
  protected GameManager _gameManager => GameManager.Instance;
  private readonly ManagerHiddenWords _root;

  // public CreateCrossWordOperation(ManagerHiddenWords generator)
  // {
  //   _root = generator;
  // }

  public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
  {
    // var t = new LocalizedString(Constants.LanguageTable.LANG_TABLE_UILANG, "createdgameobject").GetLocalizedString();
    // onSetNotify(t + "towns ...");
    bool newLevel = false;
    var word = _stateManager.ActiveWordConfig;

    _levelManager.buttonShuffle.gameObject.SetActive(true);
    _levelManager.buttonStar.gameObject.SetActive(true);
    _levelManager.buttonFrequency.gameObject.SetActive(true);
    _managerHiddenWords.crossWords.Clear();

    var data = _stateManager.dataGame.activeLevel;

    if (!string.IsNullOrEmpty(data.word))
    {
      _managerHiddenWords.SetWordForChars(data.word);

      _managerHiddenWords.OpenWords = data.openWords.ToDictionary(t => t, t => 0);

      foreach (var item in data.openChars)
      {
        _managerHiddenWords.OpenChars.Add(item.Key, item.Value);
      }
      foreach (var item in data.ent)
      {
        _managerHiddenWords.Entities.Add(item.Key, item.Value);
      }
    }
    else
    {
      _managerHiddenWords.SetWordForChars(word);
      newLevel = true;
    }

    // Create allow words.
    List<string> allowWords = _managerHiddenWords.CreateAllowWords(word);

    // // Create hidden words.
    // List<string> _hiddenWords = new();
    // if (newLevel)
    // {
    //   // _hiddenWords = CreateHiddenWords();
    //   // CreateHints();
    // }
    // else
    // {
    //   _hiddenWords = data.hiddenWords;
    // }

    _managerHiddenWords.CreateGrid(word);

    _managerHiddenWords.CreateGridWords(word, newLevel ? allowWords.OrderBy(t => -t.Length).ToList() : data.crossWords);

    _managerHiddenWords.DrawCrossWords();

    // CreateBonusWord(word);

    // CreateGameObjectHiddenWords(_hiddenWords);

    // // OnChangeData?.Invoke();
    //OpenOpenedChars();

    // // OpenNeighbours().Forget();

    // Create entities.
    var keysEntity = _managerHiddenWords.Entities.Keys.ToList();
    foreach (var item in keysEntity)
    {
      _levelManager.AddEntity(item, _managerHiddenWords.Entities[item], true).Forget();
    }

    // Create bonuses.
    var keysBonuses = _stateManager.dataGame.bonus.Keys.ToList();
    foreach (var key in keysBonuses)
    {
      // _levelManager.topSide.AddBonus(key);
      _stateManager.UseBonus(0, key);
    }

    // // Create bonus entities.
    if (newLevel) await _managerHiddenWords.CreateEntities();
    onProgress(1f);
    await UniTask.Delay(1);
  }
}
