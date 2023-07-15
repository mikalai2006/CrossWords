using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ManagerHiddenWords : MonoBehaviour
{
  [DllImport("__Internal")]
  private static extern void SetToLeaderBoard(int value);
  [DllImport("__Internal")]
  private static extern void GetLeaderBoard();
  // public static event Action OnChangeData;
  [SerializeField] public Tilemap tilemap;
  [SerializeField] public Tilemap tilemapEntities;
  [SerializeField] private Grid _GridObject;
  public GridHelper GridHelper { get; private set; }
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  protected GameManager _gameManager => GameManager.Instance;
  [SerializeField] private LineManager _lineManager;

  [SerializeField] private ChoosedWordMB _choosedWordMB;
  public SerializableDictionary<string, WordHidden> crossWords = new SerializableDictionary<string, WordHidden>();
  private string _wordForChars;
  public string WordForChars => _wordForChars;
  // public Dictionary<string, int> NeedWords;
  public Dictionary<string, int> AllowlWords;
  public Dictionary<string, int> OpenWords;
  public Dictionary<string, int> OpenCrossWords;
  // public Dictionary<string, int> OpenNeedWords;
  public List<CharMB> listChoosedGameObjects;
  public string choosedWord => string.Join("", listChoosedGameObjects.Select(t => t.GetComponent<CharMB>().charTextValue).ToList());

  public SerializableDictionary<Vector3, string> OpenChars = new();
  public SerializeEntity Entities = new();
  public SerializeEntity EntitiesRuntime = new();
  // public List<GameObject> EntitiesGameObjects = new();

  public float scaleGrid;
  private int minGridSize = 9;

  private void Awake()
  {

    listChoosedGameObjects = new();

    // NeedWords = new();

    OpenWords = new();

    OpenCrossWords = new();

    AllowlWords = new();

    // OpenNeedWords = new();

    ButtonShuffle.OnShuffleWord += SetWordForChars;
  }

  private void OnDestroy()
  {
    ButtonShuffle.OnShuffleWord -= SetWordForChars;
  }

  /// <summary>
  /// Init level
  /// </summary>
  public async UniTask Init() // GameLevel levelConfig, GameLevelWord wordConfig
  {
    bool newLevel = false;
    var word = _stateManager.ActiveWordConfig;

    _levelManager.buttonShuffle.gameObject.SetActive(true);
    _levelManager.buttonStar.gameObject.SetActive(true);
    _levelManager.buttonFrequency.gameObject.SetActive(true);
    crossWords.Clear();

    var data = _stateManager.dataGame.activeLevel;

    if (!string.IsNullOrEmpty(data.word))
    {
      SetWordForChars(data.word);

      OpenWords = data.openWords.ToDictionary(t => t, t => 0);
      OpenCrossWords = data.openCrossWords.ToDictionary(t => t, t => 0);

      foreach (var item in data.openChars)
      {
        OpenChars.Add(item.Key, item.Value);
      }
      foreach (var item in data.ent)
      {
        Entities.Add(item.Key, item.Value);
      }
    }
    else
    {
      SetWordForChars(word);
      newLevel = true;
    }

    // Create allow words.
    List<string> allowWords = CreateAllowWords(word);

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

    CreateGrid(word);

    CreateGridWords(word, newLevel ? allowWords.OrderBy(t => -t.Length).ToList() : data.crossWords);

    DrawCrossWords();

    // CreateBonusWord(word);

    // CreateGameObjectHiddenWords(_hiddenWords);

    // // OnChangeData?.Invoke();
    //OpenOpenedChars();

    // // OpenNeighbours().Forget();

    // Create entities.
    var keysEntity = Entities.Keys.ToList();
    foreach (var item in keysEntity)
    {
      _levelManager.AddEntity(item, Entities[item], true).Forget();
    }

    // Create bonuses.
    var keysBonuses = _stateManager.dataGame.bonus.Keys.ToList();
    foreach (var key in keysBonuses)
    {
      // _levelManager.topSide.AddBonus(key);
      _stateManager.UseBonus(0, key);
    }

    // // Create bonus entities.
    if (newLevel) await CreateEntities();

    await UniTask.Yield();
  }


  // public void OpenOpenedChars()
  // {
  //   for (int i = 0; i < OpenChars.Count; i++)
  //   {
  //     var item = OpenChars.ElementAt(i);
  //     var node = GridHelper.GetNode(item.Key);
  //     node.OccupiedChar.CharGameObject.ShowCharAsHint(false).Forget();
  //     // node.SetHint();
  //   }
  // }

  public void AddOpenChar(CharHidden occupiedChar)
  {
    if (!OpenChars.ContainsKey(occupiedChar.OccupiedNode.arrKey))
    {
      OpenChars.Add(occupiedChar.OccupiedNode.arrKey, occupiedChar.CharValue.ToString());
    }
    Debug.Log($"RemoveOpenChar::: {occupiedChar.CharValue}=> {OpenChars.Count}[{occupiedChar.OccupiedNode.arrKey}]");
  }

  public void RemoveOpenChar(CharHidden occupiedChar)
  {
    OpenChars.Remove(occupiedChar.OccupiedNode.arrKey);
  }


  public List<string> CreateAllowWords(string startWord)
  {
    var potentialWords = _gameManager.Words.data
        .Where(t => t.Length <= WordForChars.Length)
        .OrderBy(t => -t.Length)
        .ToList();

    foreach (var word in potentialWords)
    {
      var res = Helpers.IntersectWithRepetitons(WordForChars, word);

      if (res.Count() == word.Length)
      {
        AllowlWords.Add(word, 0);
      }
    }

    // Debug.LogWarning($"Add {NeedWords.Count} potential words ({WordForChars}) [maxCountWords={maxCountWords}]");
    return AllowlWords.Keys.ToList();
  }


  public void CreateGrid(string startWord)
  {
    // string maxWord = GetMaxLengthWord();

    var defaultGridSize = startWord.Length * 2 + Mathf.RoundToInt(_gameManager.PlayerSetting.coefDifficulty * 5);
    Debug.Log($"Set size grid::: {startWord.Length * 2}/{defaultGridSize}");
    if (defaultGridSize < minGridSize)
    {
      defaultGridSize = minGridSize;
    }

    GridHelper = new GridHelper(defaultGridSize, defaultGridSize);
    // Set transform grid.
    float scale = (float)minGridSize / defaultGridSize;
    // Debug.Log($"Scale grid ={scale}");
    _GridObject.transform.localScale = new Vector3(scale, scale, 1);
    // _GridObject.transform.localPosition = new Vector3(_GridObject.transform.localPosition.x, defaultGridSize, 0);
    scaleGrid = scale;
  }

  public void DrawCrossWords()
  {
    foreach (var item in crossWords)
    {
      item.Value.Draw();
      if (OpenCrossWords.ContainsKey(item.Key))
      {
        item.Value.SetOpen();
      }
    }
  }

  public void CreateGridWords(string startWord, List<string> allowWords)
  {
    // string maxWord = GetMaxLengthWord();

    Debug.Log($"Start word={startWord}");

    int deff = GridHelper.Grid.GetHeight() - startWord.Length;
    int y = deff - deff / 2;
    GridNode startNode = GridHelper.GetNode(GridHelper.Grid.GetWidth() / 2, y);

    WordHidden newWord = (WordHidden)GridHelper.SetWord(startWord, startNode, DirectionWord.Vertical);

    crossWords.Add(startWord, newWord);

    foreach (var wordItem in allowWords)
    {
      string word = wordItem;

      if (word == startWord) continue;

      WordItemStartNode itemStartNode = GridHelper.FindStartNodeForWord(word);
      if (itemStartNode.node != null)
      {
        // string wordForSpawn = word;
        // if (itemStartNode.directionWord == DirectionWord.Vertical)
        // {
        //   wordForSpawn = string.Join("", word.Reverse());
        // }
        WordHidden newHiddenWord = (WordHidden)GridHelper.SetWord(word, itemStartNode.node, itemStartNode.directionWord);

        crossWords.Add(word, newHiddenWord);

        // newHiddenWord.Draw();
        // if (OpenWords.ContainsKey(word))
        // {
        //   newHiddenWord.SetOpen();
        // }

      }
    }
    // Debug.LogWarning($"Add {NeedWords.Count} potential words ({WordForChars}) [maxCountWords={maxCountWords}]");
  }
  // public HiddenWordMB CreateBonusWord(string word)
  // {
  //   // find node for spawn word.
  //   Debug.Log(word);
  //   var nodes = GridHelper.FindNodeForSpawnBonusWord(word);

  //   if (nodes.Count == 0)
  //   {
  //     return null;
  //   }

  //   var newObj = GameObject.Instantiate(
  //         _hiddenWordMB,
  //         nodes[0].position,
  //         Quaternion.identity,
  //         tilemap.transform
  //       );
  //   newObj.transform.localPosition = new Vector3(nodes[0].x, nodes[0].y);
  //   // hiddenWordsMB.Add(newObj, false);
  //   newObj.Init(this);
  //   newObj.DrawBonusWord(word, nodes);
  //   // node.SetOccupiedChar();

  //   return newObj;
  // }
  // public HiddenWord CreateWord(string word, int index)
  // {
  //   // find node for spawn word.
  //   var nodes = GridHelper.FindNodeForSpawnWord(word, index);

  //   if (nodes.Count == 0)
  //   {
  //     return null;
  //   }

  //   var newObj = GameObject.Instantiate(
  //         _hiddenWordMB,
  //         nodes[0].position,
  //         Quaternion.identity,
  //         tilemap.transform
  //       );
  //   newObj.transform.localPosition = new Vector3(nodes[0].x, nodes[0].y);
  //   // hiddenWordsMB.Add(newObj, false);
  //   newObj.Init(this);
  //   newObj.DrawWord(word, nodes);
  //   // node.SetOccupiedChar();

  //   return newObj;
  // }

  public async UniTask CheckChoosedWord()
  {
    _gameManager.InputManager.Disable();

    if (choosedWord.Length > 1)
    {
      if (crossWords.ContainsKey(choosedWord))
      {
        if (OpenCrossWords.ContainsKey(choosedWord))
        {
          // already open hidden word.
          await _choosedWordMB.ExistHiddenWord(crossWords[choosedWord]);
          // CheckWord(choosedWord);
        }
        else
        {
          await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_HIDDEN_WORD);

          // open new hidden word.
          OpenWords.Add(choosedWord, 1);
          OpenCrossWords.Add(choosedWord, 1);
          await _choosedWordMB.OpenHiddenWord(crossWords[choosedWord]);
          _stateManager.OpenHiddenWord(choosedWord);
        }
      }
      else if (AllowlWords.ContainsKey(choosedWord))
      {
        if (OpenWords.ContainsKey(choosedWord))
        {
          // already open allow word.
          // await _choosedWordMB.OpenAllowWord(colba);
          await _choosedWordMB.ExistAllowWord();
        }
        else
        {
          // open new allow word.
          OpenWords.Add(choosedWord, 1);
          // if (OpenWords.ContainsKey(choosedWord))
          // {
          //   await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_NEED_WORD);
          //   OpenNeedWords.Add(choosedWord, 1);
          // }
          // else
          // {
          await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_ALLOW_WORD);
          // }
          await _choosedWordMB.OpenAllowWord();
          _stateManager.OpenAllowWord(choosedWord);

        }
      }
      else
      {
        // Debug.Log($"------Not found {choosedWord}");
        await _choosedWordMB.NoWord();
        await _levelManager.ShowHelp(Constants.Helps.HELP_CHOOSE_ERROR);
        _stateManager.DeRunPerk(choosedWord);
      }
    }
    else
    {
      _choosedWordMB.ResetWord();
    }

    foreach (var obj in listChoosedGameObjects)
    {
      obj.GetComponent<CharMB>().ResetObject();
    }
    _lineManager.ResetLine();
    listChoosedGameObjects.Clear();

    await CheckOpenWords();

    bool isEndRound = CheckStatusRound();

    // OnChangeData?.Invoke();
    if (!isEndRound) _gameManager.InputManager.Enable();
  }


  public async UniTask CheckOpenWords()
  {
    foreach (var crossWordItem in crossWords)
    {
      WordHidden crossWord = (WordHidden)crossWordItem.Value;
      int countHintChars = crossWord.Chars.Where(t => t.OccupiedNode.StateNode.HasFlag(StateNode.Hint)).Count();

      if (
        (countHintChars > 0 && crossWord.isMayBeOpen)
        ||
        (crossWord.isOpen && !OpenCrossWords.ContainsKey(crossWord.Word))
      )
      {
        await crossWord.AutoOpenWord();
      }
    }

    _stateManager.RefreshData(false);
  }


  public bool CheckStatusRound()
  {
    // bool isOpenAllNeedWords = OpenNeedWords.Count == Mathf.Min(_gameManager.PlayerSetting.maxFindWords, NeedWords.Count);// AllowWords.Count;
    bool isOpenAllHiddenWords = OpenCrossWords.Count == crossWords.Count; //.Keys.Intersect(crossWords.Keys).Count() == crossWords.Count();
    if (isOpenAllHiddenWords)
    {
      // await UniTask.Delay(500);
      Debug.Log("Next level");
      NextLevel().Forget();
    }
    // else if (isOpenAllHiddenWords)
    // {
    //   Debug.Log("Refresh hiddenWords");
    //   // RefreshHiddenWords();
    // }
    else
    {
      // GameManager.Instance.DataManager.Save();
      if (choosedWord.Length > 1) _stateManager.RefreshData(false);
    }

    return isOpenAllHiddenWords;
  }

  // public async UniTask OpenNeighbours()
  // {
  //   // open equals chars.
  //   List<GridNode> equalsCharNodes = GridHelper.FindEqualsHiddenNeighbours();
  //   Debug.Log($"equalsCharNodes count={equalsCharNodes.Count}");
  //   foreach (var equalCharNode in equalsCharNodes)
  //   {
  //     await equalCharNode.OccupiedChar.ShowCharAsNei(false);
  //   }
  // }

  public void AddChoosedChar(CharMB charGameObject)
  {
    if (!listChoosedGameObjects.Contains(charGameObject))
    {
      AudioManager.Instance.PlayClipEffect(_gameSetting.Audio.addChar);
      _lineManager.AddPoint(charGameObject.transform.position);
      listChoosedGameObjects.Add(charGameObject);
      _lineManager.DrawLine();
    }
    else if (listChoosedGameObjects.Count > 1)
    {
      var lastSymbol = listChoosedGameObjects.ElementAt(listChoosedGameObjects.Count - 1);
      var preLastIndex = listChoosedGameObjects.Count == 1 ? 0 : listChoosedGameObjects.Count - 2;
      var preLastSymbol = listChoosedGameObjects.ElementAt(preLastIndex);
      if (preLastSymbol == charGameObject)
      {
        AudioManager.Instance.PlayClipEffect(_gameSetting.Audio.removeChar);
        listChoosedGameObjects.Remove(lastSymbol);
        _lineManager.RemovePoint(lastSymbol.transform.position);
        lastSymbol.GetComponent<CharMB>().ResetObject();
        _lineManager.DrawLine();
      }
    }
    _choosedWordMB.DrawWord(choosedWord);
  }


  // private void RefreshHiddenWords()
  // {
  //   Entities.Clear();
  //   EntitiesRuntime.Clear();

  //   // Helpers.DestroyChildren(tilemapEntities.transform);
  //   Helpers.DestroyChildren(tilemap.transform);

  //   HiddenWords.Clear();

  //   var _hiddenWords = CreateHiddenWords();

  //   SetScaleGrid(_hiddenWords);

  //   // CreateHints();

  //   CreateGameObjectHiddenWords(_hiddenWords);

  //   _stateManager.RefreshData(true);
  //   // CreateEntities();
  //   _stateManager.UseBonus(1, TypeBonus.Index);
  // }


  public async UniTask NextLevel()
  {
    _gameManager.InputManager.Disable();

    // await UniTask.Delay(1000);
    // _levelManager.buttonBomb.Hide();
    // _levelManager.buttonHint.Hide();
    // _levelManager.buttonLighting.Hide();
    // _levelManager.buttonStar.Hide();
    // _levelManager.buttonShuffle.Hide();

    // _levelManager.stat.Hide();
    // _levelManager.ResetSymbols();

    // _stateManager.RefreshData();
    // foreach (var wordItem in crossWords)
    // {
    //   wordItem.Value.GameObjectHiddeWord.gameObject.SetActive(false);
    // }

    var result = await _levelManager.dialogLevel.ShowDialogEndRound();

    if (result.isOk)
    {

#if ysdk
      SetToLeaderBoard(_stateManager.stateGame.rate);
#endif

      await _levelManager.ShowHelp(Constants.Helps.HELP_DOD_DIALOG);

      // Check next level status player.
      await _levelManager.CheckNextLevelPlayer();

      var newConfigWord = _stateManager.GetNextWord();

      // dicrement bonuses.
      var keysBonuses = _stateManager.dataGame.bonus.Keys.ToList();
      foreach (var bonusKey in keysBonuses)
      {
        int valueBonus;
        _stateManager.dataGame.bonus.TryGetValue(bonusKey, out valueBonus);
        if (valueBonus > 0)
        {
          _stateManager.UseBonus(-1, bonusKey);
        }
      }

      _levelManager.InitLevel(newConfigWord).Forget();

#if ysdk
      await _gameManager.AdManager.ShowDialogAddRateGame();
      GetLeaderBoard();
#endif

      _gameManager.AdManager.ShowAdvFullScr();

      Helpers.DestroyChildren(tilemapEntities.transform);
    }

    // _gameManager.InputManager.Enable();
  }


  // private void CreateHints()
  // {
  //   var countNeedFindWords = NeedWords.Count;

  //   _stateManager.dataGame.activeLevel.hints.Clear();

  //   var countFrequency = (int)System.Math.Ceiling((countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefFrequency);
  //   _stateManager.dataGame.activeLevel.hints.Add(TypeEntity.Frequency, countFrequency);

  //   var countStar = (int)System.Math.Ceiling((countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefStar);
  //   _stateManager.dataGame.activeLevel.hints.Add(TypeEntity.Star, countStar);

  //   // _stateManager.dataGame.hint += _stateManager.dataGame.activeLevel.hintLevel;
  //   // _stateManager.dataGame.star += _stateManager.dataGame.activeLevel.starLevel;
  // }

  public async UniTask CreateEntities()
  {
    var countNeedFindWords = crossWords.Count;

    _stateManager.dataGame.activeLevel.hints.Clear();

    List<GridNode> nodesBonusWord = GridHelper.GetAllGridNodes()
      .Where(t => t.OccupiedChar != null && t.OccupiedChar.OccupiedWord != null && t.OccupiedChar.OccupiedWord.Word == WordForChars)
      .ToList();
    foreach (var node in nodesBonusWord)
    {
      await _levelManager.AddEntity(node.arrKey, TypeEntity.Coin, true);
    }


    int countS;
    _stateManager.dataGame.hints.TryGetValue(TypeEntity.RandomLetter, out countS);
    if (countS < _gameManager.PlayerSetting.bonusCount.maxStar)
    {
      var colS = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefStar;
      var countStar = (int)System.Math.Ceiling(colS);
      for (int i = 0; i < countStar; i++)
      {
        var node = GridHelper.GetRandomNodeWithHiddenChar();
        await _levelManager.AddEntity(node.arrKey, TypeEntity.RandomLetter, true);
      }
    }

    // int countB;
    // _stateManager.dataGame.hints.TryGetValue(TypeEntity.Bomb, out countB);
    // if (countB < _gameManager.PlayerSetting.bonusCount.maxBomb)
    // {
    //   var colB = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefBomb;
    //   var countBomb = System.Math.Round(colB);
    //   for (int i = 0; i < countBomb; i++)
    //   {
    //     var node = GridHelper.GetRandomNodeWithHiddenChar();
    //     await _levelManager.AddEntity(node.arrKey, TypeEntity.Bomb, true);
    //   }
    // }

    // int countL;
    // _stateManager.dataGame.hints.TryGetValue(TypeEntity.Lighting, out countL);
    // if (countL < _gameManager.PlayerSetting.bonusCount.maxLighting)
    // {
    //   var colL = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefLighting;
    //   var countLighting = System.Math.Round(colL);
    //   for (int i = 0; i < countLighting; i++)
    //   {
    //     var node = GridHelper.GetRandomNodeWithHiddenChar();
    //     await _levelManager.AddEntity(node.arrKey, TypeEntity.Lighting, true);
    //   }
    // }

    // int countF;
    // _stateManager.dataGame.hints.TryGetValue(TypeEntity.Frequency, out countF);
    // if (countF < _gameManager.PlayerSetting.bonusCount.maxFrequency)
    // {
    //   var colF = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefFrequency;
    //   var countFrequency = System.Math.Round(colF);
    //   for (int i = 0; i < countFrequency; i++)
    //   {
    //     var node = GridHelper.GetRandomNodeWithHiddenChar();
    //     await _levelManager.AddEntity(node.arrKey, TypeEntity.Frequency, true);
    //   }
    // }

    // Debug.Log($"colS={colS}|colF={colF}|colL={colL}|colB={colB}");
    // Debug.Log($"countStar={countStar}|countFrequency={countFrequency}|countLighting={countLighting}|countBomb={countBomb}");
  }


  /// <summary>
  /// Set word for radial word.
  /// </summary>
  /// <param name="word">Word</param>
  public void SetWordForChars(string word)
  {
    // get word by max length.
    _wordForChars = word;
  }


  // private string GetMaxLengthWord()
  // {
  //   return NeedWords.Keys.ToList().OrderBy(t => -t.Length).First();
  // }

  public void Reset()
  {
    _levelManager.ResetSymbols();

    foreach (var wordItem in crossWords)
    {
      wordItem.Value.Destroy();
    }

    crossWords.Clear();

    // NeedWords.Clear();

    OpenWords.Clear();

    OpenCrossWords.Clear();

    OpenChars.Clear();

    Entities.Clear();

    EntitiesRuntime.Clear();

    AllowlWords.Clear();

    // OpenNeedWords.Clear();

    // Helpers.DestroyChildren(tilemapEntities.transform);
    Helpers.DestroyChildren(tilemap.transform);
  }
}
