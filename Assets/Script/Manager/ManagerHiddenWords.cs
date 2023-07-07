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
  [SerializeField] private HiddenWordMB _hiddenWordMB;
  public SerializableDictionary<string, HiddenWordMB> HiddenWords = new SerializableDictionary<string, HiddenWordMB>();
  private string _wordForChars;
  public string WordForChars => _wordForChars;
  public Dictionary<string, int> NeedWords;
  public Dictionary<string, int> AllowPotentialWords;
  public Dictionary<string, int> OpenWords;
  public Dictionary<string, int> OpenNeedWords;
  public List<CharMB> listChoosedGameObjects;
  public string choosedWord => string.Join("", listChoosedGameObjects.Select(t => t.GetComponent<CharMB>().charTextValue).ToList());

  public SerializableDictionary<Vector2, string> OpenChars = new();
  public SerializeEntity Entities = new();
  public SerializeEntity EntitiesRuntime = new();
  // public List<GameObject> EntitiesGameObjects = new();

  public float scaleGrid;
  private int minGridSize = 9;

  private void Awake()
  {

    listChoosedGameObjects = new();

    NeedWords = new();

    OpenWords = new();

    AllowPotentialWords = new();

    OpenNeedWords = new();

    ButtonShuffle.OnShuffleWord += SetWordForChars;
  }

  private void OnDestroy()
  {
    ButtonShuffle.OnShuffleWord -= SetWordForChars;
  }

  /// <summary>
  /// Init level
  /// </summary>
  /// <param name="levelConfig">Config level</param>
  /// <param name="wordConfig">Config word</param>
  public async UniTask Init() // GameLevel levelConfig, GameLevelWord wordConfig
  {
    bool newLevel = false;
    var word = _stateManager.ActiveWordConfig;

    _levelManager.buttonShuffle.gameObject.SetActive(true);
    _levelManager.buttonStar.gameObject.SetActive(true);
    _levelManager.buttonFrequency.gameObject.SetActive(true);
    HiddenWords.Clear();

    var data = _stateManager.dataGame.activeLevel;

    if (!string.IsNullOrEmpty(data.word))
    {
      SetWordForChars(data.word);

      OpenWords = data.openWords.ToDictionary(t => t, t => 0);

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

    CreateAllowWords();

    List<string> _hiddenWords = new();
    if (!string.IsNullOrEmpty(data.word))
    {
      _hiddenWords = data.hiddenWords;
    }
    else
    {
      _hiddenWords = CreateHiddenWords();
      // CreateHints();
    }

    SetScaleChars(_hiddenWords);

    CreateGameObjectHiddenWords(_hiddenWords);

    // OnChangeData?.Invoke();
    OpenOpenedChars();

    // OpenNeighbours().Forget();

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

    // Create bonus entities.
    if (newLevel) await CreateEntities();

    await UniTask.Yield();
  }


  public void OpenOpenedChars()
  {
    for (int i = 0; i < OpenChars.Count; i++)
    {
      var item = OpenChars.ElementAt(i);
      var node = GridHelper.GetNode(item.Key);
      node.OccupiedChar.ShowCharAsHint(false).Forget();
      // node.SetHint();
    }
  }

  public void AddOpenChar(HiddenCharMB occupiedChar)
  {
    if (!OpenChars.ContainsKey(occupiedChar.OccupiedNode.arrKey))
    {
      OpenChars.Add(occupiedChar.OccupiedNode.arrKey, occupiedChar.charTextValue.ToString());
    }
  }

  public void RemoveOpenChar(HiddenCharMB occupiedChar)
  {
    OpenChars.Remove(occupiedChar.OccupiedNode.arrKey);
  }

  public void CreateGameObjectHiddenWords(List<string> words)
  {
    HiddenWords.Clear();

    var listWords = words; //.OrderBy(t => -t.Length).ToList();
    for (int i = 0; i < listWords.Count; i++)
    {
      var wordGameObject = CreateWord(listWords[i], i);
      // hiddenWords.Add(listWords[i], wordGameObject);

      if (wordGameObject == null)
      {
        HiddenWords.Remove(listWords[i]);
        continue;
      }

      HiddenWords[listWords[i]] = wordGameObject;

      if (OpenWords.ContainsKey(listWords[i]))
      {
        if (HiddenWords.ContainsKey(listWords[i]))
        {
          HiddenWords[listWords[i]].ShowWord().Forget();
        }
      }
    }
  }

  // public void CheckWord(string word)
  // {
  //   if (HiddenWords.ContainsKey(word))
  //   {
  //     HiddenWords[word].ShowWord().Forget();
  //   }
  // }

  public List<string> CreateHiddenWords()
  {
    List<string> hiddenWords = new();

    // Calculate count hidden chars.
    int countChars = Mathf.RoundToInt(((float)_stateManager.dataGame.rate / (float)_gameManager.PlayerSetting.countFindWordsForUp) * _gameSetting.maxCountHiddenChar);
    if (countChars < _gameSetting.minCountHiddenChar) countChars = _gameSetting.minCountHiddenChar;
    // Debug.Log($"Max count hidden char={countChars}");

    var countHiddenChars = 0;
    foreach (var word in NeedWords)
    {
      if (countHiddenChars > countChars) break;
      if (OpenWords.ContainsKey(word.Key)) continue;

      var newCountChar = countHiddenChars + word.Key.Length; //  + (int)(WordForChars.Length / 3)
      if (newCountChar < countChars)
      {
        hiddenWords.Add(word.Key);
        countHiddenChars += word.Key.Length;
      }
    }

    // Debug.Log($"Add {AllowWords.Count} potential words");
    return hiddenWords;
  }

  public void CreateAllowWords()
  {
    var potentialWords = _gameManager.Words.data
        .Where(t => t.Length <= WordForChars.Length)
        .OrderBy(t => Random.value)
        .ToList();

    var maxCountWords = _gameManager.PlayerSetting.maxFindWords > potentialWords.Count
      ? potentialWords.Count
      : _gameManager.PlayerSetting.maxFindWords;

    // Debug.LogWarning($"maxCountWords={maxCountWords}[maxCountWords={potentialWords.Count}]");
    // load need words.
    var savedNeedWords = _stateManager.dataGame.activeLevel.needWords;

    foreach (var word in potentialWords)
    {
      var res = Helpers.IntersectWithRepetitons(WordForChars, word);

      if (res.Count() == word.Length)
      {
        // create new need words.
        if (
          word.Length <= _gameManager.PlayerSetting.maxLengthWord
          && NeedWords.Count < maxCountWords
          && savedNeedWords == null
        )
        {
          NeedWords.Add(word, 0);
        }
        AllowPotentialWords.Add(word, 0);
      }
    }

    if (savedNeedWords != null)
    {
      NeedWords = savedNeedWords.ToDictionary(t => t, t => 0);

      foreach (var openWord in NeedWords)
      {
        if (OpenWords.ContainsKey(openWord.Key))
        {
          OpenNeedWords.Add(openWord.Key, 0);
        }
      }
    }
    // Debug.LogWarning($"Add {NeedWords.Count} potential words ({WordForChars}) [maxCountWords={maxCountWords}]");
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

  public HiddenWordMB CreateWord(string word, int index)
  {
    // find node for spawn word.
    var nodes = GridHelper.FindNodeForSpawnWord(word, index);

    if (nodes.Count == 0)
    {
      return null;
    }

    var newObj = GameObject.Instantiate(
          _hiddenWordMB,
          nodes[0].position,
          Quaternion.identity,
          tilemap.transform
        );
    newObj.transform.localPosition = new Vector3(nodes[0].x, nodes[0].y);
    // hiddenWordsMB.Add(newObj, false);
    newObj.Init(this);
    newObj.DrawWord(word, nodes);
    // node.SetOccupiedChar();

    return newObj;
  }

  public async UniTask CheckChoosedWord()
  {
    _gameManager.InputManager.Disable();

    if (choosedWord.Length > 1)
    {
      if (HiddenWords.ContainsKey(choosedWord))
      {
        if (OpenWords.ContainsKey(choosedWord))
        {
          // already open hidden word.
          await _choosedWordMB.ExistHiddenWord(HiddenWords[choosedWord]);
          // CheckWord(choosedWord);
        }
        else
        {
          await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_HIDDEN_WORD);

          // open new hidden word.
          OpenWords.Add(choosedWord, 1);
          OpenNeedWords.Add(choosedWord, 1);
          await _choosedWordMB.OpenHiddenWord(HiddenWords[choosedWord]);
          _stateManager.OpenHiddenWord(choosedWord);
        }
      }
      else if (AllowPotentialWords.ContainsKey(choosedWord))
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
          if (NeedWords.ContainsKey(choosedWord))
          {
            await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_NEED_WORD);
            await _levelManager.ShowHelp(Constants.Helps.HELP_FLASK_HIDDEN);
            OpenNeedWords.Add(choosedWord, 1);
          }
          else
          {
            await _levelManager.ShowHelp(Constants.Helps.HELP_FIND_ALLOW_WORD);
            await _levelManager.ShowHelp(Constants.Helps.HELP_FLASK_ALLOW);
          }
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

    await CheckStatusRound();

    // OnChangeData?.Invoke();
    _gameManager.InputManager.Enable();
  }

  public async UniTask CheckStatusRound()
  {
    bool isOpenAllNeedWords = OpenNeedWords.Count == Mathf.Min(_gameManager.PlayerSetting.maxFindWords, NeedWords.Count);// AllowWords.Count;
    bool isOpenAllHiddenWords = OpenWords.Keys.Intersect(HiddenWords.Keys).Count() == HiddenWords.Count();
    if (isOpenAllNeedWords)
    {
      await UniTask.Delay(500);
      Debug.Log("Next level");
      await NextLevel();
    }
    else if (isOpenAllHiddenWords)
    {
      Debug.Log("Refresh hiddenWords");
      await _levelManager.ShowHelp(Constants.Helps.HELP_INDEX);
      RefreshHiddenWords();
    }
    else
    {
      // GameManager.Instance.DataManager.Save();
      if (choosedWord.Length > 1) _stateManager.RefreshData(false);
    }

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

  private void SetScaleChars(List<string> _hiddenWords)
  {
    var countHiddenChars = _hiddenWords.Select(t => t.Length).Sum();

    var defaultGridSize = minGridSize;
    var defaultCountChars = System.Math.Pow(defaultGridSize, 2);

    var sizeGridXY = defaultGridSize;
    if (countHiddenChars > defaultCountChars)
    {
      sizeGridXY = System.Convert.ToInt32(System.MathF.Ceiling((float)System.Math.Sqrt(countHiddenChars))); //_stateManager.ActiveWordConfig.maxCountHiddenChar
      sizeGridXY += _gameSetting.addinitiallyRow;
    }

    // Debug.Log($"countHiddenChars={countHiddenChars}, defaultCountChars ={defaultCountChars}, sizeGridXY={sizeGridXY}");
    // Debug.Log($"word max length={wordWithMaxLength}, Need count rows={minNeedRows}, sizeGridXY={sizeGridXY}");
    GridHelper = new GridHelper(sizeGridXY, sizeGridXY);
    // Set transform grid.
    float scale = (float)defaultGridSize / sizeGridXY;
    // Debug.Log($"Scale grid ={scale}");
    _GridObject.transform.localScale = new Vector3(scale, scale, 1);
    scaleGrid = scale;
  }


  private void RefreshHiddenWords()
  {
    Entities.Clear();
    EntitiesRuntime.Clear();

    // Helpers.DestroyChildren(tilemapEntities.transform);
    Helpers.DestroyChildren(tilemap.transform);

    HiddenWords.Clear();

    var _hiddenWords = CreateHiddenWords();

    SetScaleChars(_hiddenWords);

    // CreateHints();

    CreateGameObjectHiddenWords(_hiddenWords);

    _stateManager.RefreshData(true);
    // CreateEntities();
    _stateManager.UseBonus(1, TypeBonus.Index);
  }


  public async UniTask NextLevel()
  {
    _gameManager.InputManager.Disable();

    await UniTask.Delay(1000);
    // _levelManager.buttonBomb.Hide();
    // _levelManager.buttonHint.Hide();
    // _levelManager.buttonLighting.Hide();
    // _levelManager.buttonStar.Hide();
    // _levelManager.buttonShuffle.Hide();

    // _levelManager.stat.Hide();
    // _levelManager.ResetSymbols();

    // _stateManager.RefreshData();
    foreach (var wordItem in HiddenWords)
    {
      wordItem.Value.gameObject.SetActive(false);
    }

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

      _levelManager.InitLevel(newConfigWord);

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

  private async UniTask CreateEntities()
  {
    var countNeedFindWords = NeedWords.Count;

    _stateManager.dataGame.activeLevel.hints.Clear();

    int countS;
    _stateManager.dataGame.hints.TryGetValue(TypeEntity.Star, out countS);
    if (countS < _gameManager.PlayerSetting.bonusCount.maxStar)
    {
      var colS = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefStar;
      var countStar = (int)System.Math.Ceiling(colS);
      for (int i = 0; i < countStar; i++)
      {
        var node = GridHelper.GetRandomNodeWithHiddenChar();
        await _levelManager.AddEntity(node.arrKey, TypeEntity.Star, true);
      }
    }

    int countB;
    _stateManager.dataGame.hints.TryGetValue(TypeEntity.Bomb, out countB);
    if (countB < _gameManager.PlayerSetting.bonusCount.maxBomb)
    {
      var colB = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefBomb;
      var countBomb = System.Math.Round(colB);
      for (int i = 0; i < countBomb; i++)
      {
        var node = GridHelper.GetRandomNodeWithHiddenChar();
        await _levelManager.AddEntity(node.arrKey, TypeEntity.Bomb, true);
      }
    }

    int countL;
    _stateManager.dataGame.hints.TryGetValue(TypeEntity.Lighting, out countL);
    if (countL < _gameManager.PlayerSetting.bonusCount.maxLighting)
    {
      var colL = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefLighting;
      var countLighting = System.Math.Round(colL);
      for (int i = 0; i < countLighting; i++)
      {
        var node = GridHelper.GetRandomNodeWithHiddenChar();
        await _levelManager.AddEntity(node.arrKey, TypeEntity.Lighting, true);
      }
    }

    int countF;
    _stateManager.dataGame.hints.TryGetValue(TypeEntity.Frequency, out countF);
    if (countF < _gameManager.PlayerSetting.bonusCount.maxFrequency)
    {
      var colF = (countNeedFindWords - countNeedFindWords * _gameManager.PlayerSetting.coefDifficulty) * _gameManager.PlayerSetting.coefFrequency;
      var countFrequency = System.Math.Round(colF);
      for (int i = 0; i < countFrequency; i++)
      {
        var node = GridHelper.GetRandomNodeWithHiddenChar();
        await _levelManager.AddEntity(node.arrKey, TypeEntity.Frequency, true);
      }
    }

    // Debug.Log($"colS={colS}|colF={colF}|colL={colL}|colB={colB}");
    // Debug.Log($"countStar={countStar}|countFrequency={countFrequency}|countLighting={countLighting}|countBomb={countBomb}");
  }


  public void Reset()
  {
    _levelManager.ResetSymbols();

    foreach (var wordItem in HiddenWords)
    {
      GameObject.Destroy(wordItem.Value.gameObject);
    }

    HiddenWords.Clear();

    NeedWords.Clear();

    OpenWords.Clear();

    OpenChars.Clear();

    Entities.Clear();

    EntitiesRuntime.Clear();

    AllowPotentialWords.Clear();

    OpenNeedWords.Clear();

    // Helpers.DestroyChildren(tilemapEntities.transform);
    Helpers.DestroyChildren(tilemap.transform);
  }
}
