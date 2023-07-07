using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class StateManager : MonoBehaviour
{
  public static event Action<StateGame> OnChangeState;
  public static event Action OnGenerateBonus;
  // public static event Action<GameTheme> OnChangeUserSetting;
  public DataGame dataGame;
  public StateGame stateGame;
  public string ActiveWordConfig;

  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;

  public async UniTask<StateGame> Init(StateGame _stateGame)
  {
    await LocalizationSettings.InitializationOperation.Task;
    string codeCurrentLang = LocalizationSettings.SelectedLocale.Identifier.Code;

    GamePlayerSetting playerSetting;
    DataGame dataGame;
    StateGameItem stateGameItemCurrentLang = _stateGame != null && _stateGame.items.Count > 0
      ? _stateGame.items.Find(t => t.lang == codeCurrentLang)
      : null;

    // init new state.
    if (_stateGame == null) _stateGame = new();

    if (_stateGame.items.Count == 0 || stateGameItemCurrentLang == null) // string.IsNullOrEmpty(dataGame.rank)
    {
      // Debug.Log($"{name} ::: Init new gamedata for [{LocalizationSettings.SelectedLocale.Identifier.Code}]");

      playerSetting = _gameManager.GameSettings.PlayerSettings
        .OrderBy(t => t.countFindWordsForUp)
        .First();

      // Add default bonuses.
      var defaultBonuses = new SerializableDictionary<TypeBonus, int>();
      var allBonuses = _gameManager.ResourceSystem.GetAllBonus();
      if (allBonuses.Count > 0)
      {
        foreach (var bonus in _gameManager.ResourceSystem.GetAllBonus())
        {
          if (bonus.isAlways)
          {
            defaultBonuses.Add(bonus.typeBonus, 1);
          }
        }
      }

      dataGame = new DataGame()
      {
        rank = playerSetting.idPlayerSetting,
        bonus = defaultBonuses
      };

      var newStateGame = new StateGameItem()
      {
        dataGame = dataGame,
        lang = LocalizationSettings.SelectedLocale.Identifier.Code,
      };

      _stateGame.items.Add(newStateGame);
    }
    else
    {
      dataGame = _stateGame.items.Find(t => t.lang == codeCurrentLang).dataGame;
      playerSetting = _gameManager.GameSettings.PlayerSettings.Find(t => t.idPlayerSetting == dataGame.rank);
    }

    stateGame = _stateGame;

    // _gameManager.SetPlayerSetting(playerSetting);

    await SetActiveDataGame();

    return _stateGame;
  }

  public async UniTask SetActiveDataGame()
  {
    var localeStateGameItem = stateGame.items.Find(t => t.lang == LocalizationSettings.SelectedLocale.Identifier.Code);
    if (localeStateGameItem != null)
    {
      // Debug.Log($"{name} ::: Set localeStateGameItem for [{LocalizationSettings.SelectedLocale.Identifier.Code}]");

      stateGame.activeDataGame = dataGame = localeStateGameItem.dataGame;
      var playerSetting = _gameManager.GameSettings.PlayerSettings.Find(t => t.idPlayerSetting == dataGame.rank);
      _gameManager.SetPlayerSetting(playerSetting);
    }
    else
    {
      await Init(stateGame);
    };
    // Debug.Log($"{name} ::: Init lastWord[{LocalizationSettings.SelectedLocale.Identifier.Code}] = {dataGame.lastWord}");
  }


  public void RefreshData(bool saveDb)
  {
    var managerHiddenWords = _levelManager.ManagerHiddenWords;

    stateGame.rate = stateGame.items.Select(t => t.dataGame.rate).Sum();
    // stateGame.coins = stateGame.items.Select(t => t.dataGame.coins).Sum();

    dataGame.activeLevel.word = managerHiddenWords.WordForChars;

    dataGame.activeLevel.countDopWords = managerHiddenWords.OpenWords.Count - managerHiddenWords.OpenNeedWords.Count;

    dataGame.activeLevel.openChars.Clear();
    foreach (var item in managerHiddenWords.OpenChars)
    {
      dataGame.activeLevel.openChars.Add(item.Key, item.Value);
    }

    dataGame.activeLevel.ent.Clear();
    foreach (var item in managerHiddenWords.Entities)
    {
      dataGame.activeLevel.ent.Add(item.Key, item.Value);
    }
    dataGame.activeLevel.openWords = managerHiddenWords.OpenWords.Keys.ToList();
    // dataGame.activeLevel.countOpenWords = managerHiddenWords.OpenWords.Count;
    dataGame.activeLevel.needWords = managerHiddenWords.NeedWords.Keys.ToList();
    dataGame.activeLevel.countNeedWords = managerHiddenWords.NeedWords.Count;
    dataGame.activeLevel.hiddenWords = dataGame.activeLevel.openWords.Count == dataGame.activeLevel.countNeedWords
      ? new()
      : managerHiddenWords.HiddenWords.Keys.ToList();
    dataGame.activeLevel.countOpenChars = managerHiddenWords.OpenWords.Select(t => t.Key.Length).Sum();

    _gameManager.DataManager.Save(saveDb);
    OnChangeState.Invoke(stateGame);
  }


  public void IncrementCoin(int quantity)
  {
    // AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.addCoin);

    dataGame.activeLevel.coins += quantity;

    RefreshData(false);
  }


  public void IncrementTotalCoin(int quantity)
  {
    AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.addCoin);

    // dataGame.coins += quantity;
    stateGame.coins += quantity;

    // RefreshData();
    _gameManager.DataManager.Save(true);
    OnChangeState?.Invoke(stateGame);
  }


  public void OpenHiddenWord(string word)
  {
    // dataGame.rate += 1;

    dataGame.activeLevel.bonusCount.wordInOrder += 1;
    dataGame.activeLevel.bonusCount.errorNullBonus = 0;
    dataGame.activeLevel.bonusCount.errorNullOrderWord = 0;

    if (dataGame.activeLevel.bonusCount.wordInOrder >= _gameManager.PlayerSetting.bonusCount.wordInOrder)
    {
      dataGame.activeLevel.bonusCount.wordInOrder -= _gameManager.PlayerSetting.bonusCount.wordInOrder;
      UseBonus(1, TypeBonus.Index);
    }

    RefreshData(false);
  }

  public void IncrementRate(int count)
  {
    dataGame.rate += count;

    RefreshData(false);
  }

  public void OpenAllowWord(string word)
  {
    // dataGame.rate += 1;

    dataGame.activeLevel.bonusCount.wordInOrder += 1;
    dataGame.activeLevel.bonusCount.errorNullBonus = 0;
    dataGame.activeLevel.bonusCount.errorNullOrderWord = 0;

    if (dataGame.activeLevel.bonusCount.wordInOrder >= _gameManager.PlayerSetting.bonusCount.wordInOrder)
    {
      dataGame.activeLevel.bonusCount.wordInOrder -= _gameManager.PlayerSetting.bonusCount.wordInOrder;
      UseBonus(1, TypeBonus.Index);
    }
    // statePerk.countWordInOrder += 1;
    // statePerk.countErrorForNullBonus = 0;

    RefreshData(false);
  }


  public void OpenCharAllowWord(char textChar)
  {
    // dataGame.activeLevel.bonusCount.charInOrder += 1;
    dataGame.activeLevel.bonusCount.charBonus += 1;
    dataGame.activeLevel.bonusCount.charHint += 1;
    dataGame.activeLevel.bonusCount.charStar += 1;
    dataGame.activeLevel.bonusCount.charBomb += 1;
    dataGame.activeLevel.bonusCount.charLighting += 1;

    // Add bonus index.
    if (dataGame.activeLevel.bonusCount.charBonus >= _gameManager.PlayerSetting.bonusCount.charBonus)
    {
      dataGame.activeLevel.bonusCount.charBonus -= _gameManager.PlayerSetting.bonusCount.charBonus;
      dataGame.activeLevel.index++;
    }

    // Add hint.
    if (dataGame.activeLevel.bonusCount.charHint >= _gameManager.PlayerSetting.bonusCount.charHint)
    {
      dataGame.activeLevel.bonusCount.charHint -= _gameManager.PlayerSetting.bonusCount.charHint;
      // dataGame.hints[TypeEntity.Hint]++;
      UseHint(1, TypeEntity.Frequency);
    }

    // Check add star to grid.
    if (dataGame.activeLevel.bonusCount.charStar >= _gameManager.PlayerSetting.bonusCount.charStar)
    {
      dataGame.activeLevel.bonusCount.charStar -= _gameManager.PlayerSetting.bonusCount.charStar;
      // dataGame.hints[TypeEntity.Star]++;
      UseHint(1, TypeEntity.Star);
    }

    // Add bomb.
    if (dataGame.activeLevel.bonusCount.charBomb >= _gameManager.PlayerSetting.bonusCount.charBomb)
    {
      dataGame.activeLevel.bonusCount.charBomb -= _gameManager.PlayerSetting.bonusCount.charBomb;
      // dataGame.hints[TypeEntity.Bomb]++;
      UseHint(1, TypeEntity.Bomb);
    }

    // Add Lighting.
    if (dataGame.activeLevel.bonusCount.charLighting >= _gameManager.PlayerSetting.bonusCount.charLighting)
    {
      dataGame.activeLevel.bonusCount.charLighting -= _gameManager.PlayerSetting.bonusCount.charLighting;
      UseHint(1, TypeEntity.Lighting);
    }

    RefreshData(false);
  }

  public void OpenCharHiddenWord(char _char)
  {
    dataGame.activeLevel.bonusCount.charCoin += 1;
    dataGame.activeLevel.bonusCount.charInOrder += 1;

    // Check add coin to grid.
    if (dataGame.activeLevel.bonusCount.charCoin >= _gameManager.PlayerSetting.bonusCount.charCoin)
    {
      dataGame.activeLevel.bonusCount.charCoin -= _gameManager.PlayerSetting.bonusCount.charCoin;
      // dataGame.activeLevel.bonusCount.needCreateCoin++;
    }

    if (dataGame.activeLevel.bonusCount.charInOrder >= _gameManager.PlayerSetting.bonusCount.charInOrder)
    {
      dataGame.activeLevel.bonusCount.charInOrder -= _gameManager.PlayerSetting.bonusCount.charInOrder;


      // TODO create bonus
      OnGenerateBonus?.Invoke();
    }

    RefreshData(false);
    // OpenCharAllowWord(_char);
  }

  public void DeRunPerk(string word)
  {
    if (word.Length > 1)
    {
      dataGame.activeLevel.bonusCount.errorNullBonus++;
      dataGame.activeLevel.bonusCount.errorNullOrderWord++;
    }
    if (dataGame.activeLevel.bonusCount.errorNullOrderWord == _gameManager.PlayerSetting.bonusCount.errorNullOrderWord)
    {
      dataGame.activeLevel.bonusCount.wordInOrder = 0;
    }
    if (dataGame.activeLevel.bonusCount.errorNullBonus == _gameManager.PlayerSetting.bonusCount.errorNullBonus)
    {
      dataGame.activeLevel.bonusCount.wordInOrder = 0;
      dataGame.activeLevel.bonusCount.charInOrder = 0;
      dataGame.activeLevel.bonusCount.charBonus = 0;
      dataGame.activeLevel.bonusCount.charCoin = 0;
      dataGame.activeLevel.bonusCount.charHint = 0;
      dataGame.activeLevel.bonusCount.charStar = 0;
      dataGame.activeLevel.bonusCount.charBomb = 0;
      dataGame.activeLevel.bonusCount.charLighting = 0;
      dataGame.activeLevel.bonusCount.errorNullBonus = 0;
    }
    RefreshData(false);
  }


  public List<ItemWord> GetAllowNotCompleteWords(List<string> completedWords)
  {
    var allAllowLevelWords = _gameManager.GameSettings.GameLevels
      .Where(t =>
        t.locale.Identifier.Code == LocalizationSettings.SelectedLocale.Identifier.Code
        && t.minRate <= dataGame.rate
      )
      .ToList();
    List<ItemWord> allAllowWords = new();
    foreach (var el in allAllowLevelWords)
    {
      var allLevelWords = el.levelWords.Where(t => !completedWords.Contains(t)).ToList();
      for (int i = 0; i < allLevelWords.Count; i++)
      {
        allAllowWords.Add(new ItemWord()
        {
          word = allLevelWords[i],
          rate = el.minRate
        });
      }
      // allAllowWords.AddRange(el.levelWords);
    };

    // if not found not competed word.
    if (allAllowWords.Count == 0)
    {
      var alllWords = _gameManager.Words.data
        .Where(t =>
          t.Length > 4
          && t.Length <= 12
        )
        .OrderBy(t => UnityEngine.Random.value)
        .ToList();

      // generate random word.
      string randomWord = alllWords.ElementAt(0);
      allAllowWords.Add(new ItemWord()
      {
        word = randomWord,
        rate = 0
      });
    }

    return allAllowWords.ToList();
  }

  public void SetActiveLevel(string wordConfig)
  {
    // if (wordConfig == null)
    // {
    //   //TODO Not found new level - GAME OVER
    //   return;
    // }
    if (dataGame.levels.Count == 0 || string.IsNullOrEmpty(wordConfig))
    {
      if (dataGame.completed.Contains(wordConfig) || string.IsNullOrEmpty(wordConfig))
      {
        var notCompletedWords = GetAllowNotCompleteWords(dataGame.completed);

        // Find not completed word.
        // var notCompletedWords = allAllowWords.Where(t => !dataGame.completed.Contains(t.word));

        if (notCompletedWords.Count() > 0)
        {
          wordConfig = notCompletedWords.OrderBy(t => t.rate).ElementAt(0).word; // .ThenBy(t => t.word.Length)
        }
      }
    }

    // Debug.Log($"wordConfig={wordConfig}");

    dataGame.lastWord = ActiveWordConfig = wordConfig;
    // // ActiveWordConfig = word;
    // // dataGame.lastLevelWord = wordConfig.idLevel;
    // dataGame.lastWord = wordConfig;

    if (dataGame.levels.Find(t => t.id == wordConfig) == null)
    {
      dataGame.levels.Add(new DataLevel()
      {
        id = wordConfig,
        // word = word,
        // hint = (int)Mathf.Round(wordConfig.word.Length * _gameSetting.GameLevels.coefHint),
        // star = (int)Mathf.Round(wordConfig.word.Length * _gameSetting.GameLevels.coefStar)
      });

    }
    // var indexActiveLevel = dataGame.levels.FindIndex(t => t.id == wordConfig.idLevel && t.word == word);
    dataGame.activeLevel = dataGame.levels.Find((t) => t.id == wordConfig);
    // Debug.Log($"Set active level ={indexActiveLevel}| {dataGame.activeLevel.openChars.Count}");

    // SetDefaultPerk();
  }

  public GamePlayerSetting SetNewLevelPlayer()
  {
    GamePlayerSetting result = null;

    int countFindWordsForUp = _gameManager.PlayerSetting.countFindWordsForUp;

    if (dataGame.rate < countFindWordsForUp) return null;


    var allPlayerSettings = _gameSetting.PlayerSettings.OrderBy(t => t.countFindWordsForUp).ToList();

    int indexPlayerSetting = allPlayerSettings.FindIndex(t => t.idPlayerSetting == _gameManager.PlayerSetting.idPlayerSetting);

    if (indexPlayerSetting + 1 < allPlayerSettings.Count)
    {
      indexPlayerSetting++;
    }

    dataGame.rank = allPlayerSettings[indexPlayerSetting].idPlayerSetting;

    _gameManager.SetPlayerSetting(allPlayerSettings[indexPlayerSetting]);

    return result;
  }

  public string GetNextWord()
  {
    // check completed level.
    if (dataGame.activeLevel.openWords.Count >= dataGame.activeLevel.countNeedWords)
    {
      if (!dataGame.completed.Contains(dataGame.activeLevel.id))
      {
        dataGame.completed.Add(dataGame.activeLevel.id);
      }
    }
    dataGame.levels.Remove(dataGame.activeLevel);

    // Find not completed word.
    string result = null;

    var notCompletedWords = GetAllowNotCompleteWords(dataGame.completed);

    // var notCompletedWords = allAllowWords.Where(t => !dataGame.completed.Contains(t.word));

    if (notCompletedWords.Count() > 0)
    {
      result = notCompletedWords.OrderBy(t => t.rate).ElementAt(0).word; // .ThenBy(t => t.word.Length)
    }

    return result;
  }



  public StateGame GetData()
  {
    return stateGame;
  }



  public void UseBonus(int count, TypeBonus typeBonus)
  {
    int currentCount;

    dataGame.bonus.TryGetValue(typeBonus, out currentCount);

    dataGame.bonus[typeBonus] = currentCount + count;

    // await _levelManager.topSide.AddBonus(typeBonus);

    // RefreshData();
    if (count > 0)
    {
      _gameManager.audioManager.PlayClipEffect(_gameSetting.Audio.addBonus);
    }

    OnChangeState?.Invoke(stateGame);
  }

  public void UseHint(int count, TypeEntity typeEntity)
  {
    int currentCount;

    dataGame.hints.TryGetValue(typeEntity, out currentCount);

    dataGame.hints[typeEntity] = currentCount + count;

    // RefreshData();
    OnChangeState?.Invoke(stateGame);
  }


  public void BuyHint(ShopItem<GameEntity> item)
  {
    Debug.Log($"Buy {item.entity.typeEntity}");

    int currentCount;

    dataGame.hints.TryGetValue(item.entity.typeEntity, out currentCount);

    dataGame.hints[item.entity.typeEntity] = item.count + currentCount;

    stateGame.coins -= item.cost;
    // if (_levelManager != null)
    _gameManager.DataManager.Save(true);
    OnChangeState?.Invoke(stateGame);
  }
  public void BuyHintByForAdv(ShopAdvBuyItem<TypeEntity> item)
  {
    Debug.Log($"Buy hint for adv {item.typeItem}");

    int currentCount;

    dataGame.hints.TryGetValue(item.typeItem, out currentCount);

    dataGame.hints[item.typeItem] = item.count + currentCount;

    _gameManager.DataManager.Save(true);

    OnChangeState?.Invoke(stateGame);
  }


  public void BuyBonus(ShopItem<GameBonus> item)
  {
    Debug.Log($"Buy bonus {item.entity.typeBonus}");

    // int currentCount;

    // dataGame.bonus.TryGetValue(item.entity.typeBonus, out currentCount);

    // dataGame.bonus[item.entity.typeBonus] = item.count + currentCount;

    stateGame.coins -= item.cost;
    // if (_levelManager != null)
    UseBonus(item.count, item.entity.typeBonus);
    _gameManager.DataManager.Save(true);
    // OnChangeState?.Invoke(stateGame);
  }

  public void BuyBonusForAdv(ShopAdvBuyItem<TypeBonus> item)
  {
    Debug.Log($"Buy bonus for adv {item.typeItem}");

    UseBonus(item.count, item.typeItem);

    _gameManager.DataManager.Save(true);
  }

  public void SetLastTime()
  {
    stateGame.lastTime = System.DateTime.Now.ToString();
  }

  public async UniTask Reset()
  {
    _gameManager.StateManager.stateGame = new StateGame();
    await _gameManager.StateManager.Init(null);

    OnChangeState?.Invoke(stateGame);
  }
}


public struct ItemWord
{
  public string word;
  public int rate;
}