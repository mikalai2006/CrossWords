using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelManager : Singleton<LevelManager>
{
  public static event Action OnInitLevel;
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  public LineManager LineManager;
  [Header("File Storage Config")]
  public ManagerHiddenWords ManagerHiddenWords;
  private List<CharMB> _symbols;
  public List<CharMB> Symbols => _symbols;
  public GameObject SymbolsField;
  public GameObject Pointer;
  public TopSide topSide;
  public DialogLevel dialogLevel;
  public ButtonStar buttonStar;
  public ButtonFrequency buttonFrequency;
  public ButtonBomb buttonBomb;
  public ButtonDirectory buttonDirectory;
  public ButtonLighting buttonLighting;
  public ButtonShuffle buttonShuffle;
  public ButtonFlask buttonFlask;
  public Stat stat;

  protected override void Awake()
  {
    base.Awake();
    _symbols = new();
    Pointer.GetComponent<SpriteRenderer>().color = _gameManager.Theme.colorPrimary;
    Pointer.SetActive(false);
  }

  public async void InitLevel(string wordConfig)
  {
    _gameManager.InputManager.Disable();

    ResetLevel();

    OnInitLevel?.Invoke();

#if UNITY_EDITOR
    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    stopWatch.Start();
#endif

    GameManager.Instance.StateManager.SetActiveLevel(wordConfig);

    await ManagerHiddenWords.Init(); // levelConfig, wordConfig

    // Check complete level.
    if (_stateManager.dataGame.levels.Count > 0)
    {
      if (IsEndLevel())
      {
        await ManagerHiddenWords.NextLevel();
        // GameManager.Instance.StateManager.RefreshData();
        return;
      }
    }

    CreateChars(ManagerHiddenWords.WordForChars);

    if (_stateManager.dataGame.activeLevel.openWords.Count == 0) await buttonShuffle.RefreshChars();

    // GameManager.Instance.DataManager.Save();
    _stateManager.RefreshData(true);

    // Show start info.
    _gameManager.InputManager.Disable();
    var result = await dialogLevel.ShowDialogStartRound();
    if (result.isOk)
    {
      _gameManager.InputManager.Enable();
      await AutoChooseWord();
    }


#if UNITY_EDITOR
    stopWatch.Stop();
    System.TimeSpan timeTaken = stopWatch.Elapsed;
    Debug.LogWarning($"Init Level by time {timeTaken.ToString(@"m\:ss\.ffff")}");
#endif
  }

  public bool IsEndLevel()
  {
    var currentLevel = _stateManager.dataGame.activeLevel; //dataGame.levels.Find(t => t.id == _stateManager.dataGame.lastWord);

    bool result = currentLevel.openWords.Count - currentLevel.countDopWords == currentLevel.countNeedWords && currentLevel.openWords.Count > 0;
    return result;
  }

  public void CreateChars(string str)
  {
    // Debug.Log($"str={str}");
    float baseRadius = GameManager.Instance.GameSettings.radius;
    var countCharGO = str.ToArray();
    float radius = baseRadius + (countCharGO.Length / 2) * 0.1f;
    for (int i = 0; i < countCharGO.Length; ++i)
    {
      float circleposition = (float)i / (float)countCharGO.Length;
      float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius + SymbolsField.transform.position.x;
      float y = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius + SymbolsField.transform.position.y;
      var symbolGO = GameObject.Instantiate(
         _gameSetting.PrefabSymbol,
         new Vector3(x, y, 0.0f),
         Quaternion.identity,
         SymbolsField.transform
     );
      symbolGO.Init(countCharGO.ElementAt(i));
      var size = radius - ((radius - baseRadius) * baseRadius) - .5f;
      symbolGO.SetSize(size);
      _symbols.Add(symbolGO);
    }
  }

  public async UniTask AutoChooseWord()
  {
    var isShow = _gameManager.AppInfo.helps.Find(t => t == Constants.Helps.HELP_CHOOSE_CHAR);
    if (!string.IsNullOrEmpty(isShow)) return;

    _gameManager.InputManager.Disable();

    float duration = _gameSetting.timeGeneralAnimation;
    var activeLevel = _stateManager.dataGame.activeLevel;
    Pointer.SetActive(true);

    string ranWordForHelp = activeLevel.hiddenWords
      .Where(t => !activeLevel.openWords.Contains(t))
      .ElementAt(0);

    List<CharMB> cacheChoosedSymbol = new();

    foreach (var _char in ranWordForHelp)
    {
      var symb = _symbols.Find(t => t.charTextValue == _char && !cacheChoosedSymbol.Contains(t));

      cacheChoosedSymbol.Add(symb);

      Pointer.transform
        .DOMove(symb.transform.position, duration)
        .OnUpdate(() =>
        {
          LineManager.DrawLine(Pointer.transform.position);
        })
        .OnComplete(() =>
        {
          symb.ChooseSymbol();
          // LineManager.AddPoint(symb.transform.position);
          // ManagerHiddenWords.listChoosedGameObjects.Add(symb);
        });

      await UniTask.Delay((int)(duration * 1000));
    }


    await UniTask.Delay(1000);

    foreach (var obj in ManagerHiddenWords.listChoosedGameObjects)
    {
      obj.GetComponent<CharMB>().ResetObject();
    }
    LineManager.ResetLine();
    ManagerHiddenWords.listChoosedGameObjects.Clear();

    _gameManager.AppInfo.AddHelpItem(Constants.Helps.HELP_CHOOSE_CHAR);
    Pointer.SetActive(false);
    _gameManager.InputManager.Enable();
  }


  public async UniTask<bool> ShowHelp(string key)
  {
    var isShow = _gameManager.AppInfo.helps.Find(t => t == key);
    if (!string.IsNullOrEmpty(isShow)) return false;

    TaskCompletionSource<bool> _processCompletionSource = new();

    _gameManager.InputManager.Disable();

    var messageConfirm = await Helpers.GetLocaledString(key);
    // var title = await Helpers.GetLocaledString("confirm_title");
    var dialogConfirm = new DialogProvider(new DataDialog()
    {
      // title = title,
      message = messageConfirm,
      showCancelButton = false
    });

    var result = await dialogConfirm.ShowAndHide();
    if (result.isOk)
    {
      _processCompletionSource.SetResult(true);
      _gameManager.AppInfo.AddHelpItem(key);

      _gameManager.InputManager.Enable();
    }


    return await _processCompletionSource.Task;
  }

  public void ResetSymbols()
  {
    foreach (var symbol in _symbols)
    {
      GameObject.Destroy(symbol.gameObject);
    }
    _symbols.Clear();
    // await UniTask.Yield();
  }

  public async UniTask CheckNextLevelPlayer()
  {
    if (_gameManager.PlayerSetting.countFindWordsForUp <= _stateManager.dataGame.rate)
    {
      // check rate user.
      GamePlayerSetting newRankUser = _stateManager.SetNewLevelPlayer();

      var resultDialog = await dialogLevel.ShowDialogNewRankUser();

      // if (resultDialog.isOk)
      // {
      // }
    }

    // _gameManager.InputManager.Disable();
    // var dialogWindow = new UILevelsOperation();
    // var result = await dialogWindow.ShowAndHide();
    // _gameManager.InputManager.Enable();
    await UniTask.Yield();
  }

  private void ResetLevel()
  {
    ResetSymbols();

    ManagerHiddenWords.Reset();
  }


  public async UniTask<BaseEntity> AddEntity(Vector2Int pos, TypeEntity typeEntity, bool asBonus)
  {
    var node = ManagerHiddenWords.GridHelper.GetNode(pos);
    // pos == Vector2Int.zero
    //   ? ManagerHiddenWords.GridHelper.GetRandomNodeWithHiddenChar()
    //   : ManagerHiddenWords.GridHelper.GetNode(pos);
    var configsAllEntities = _gameManager.ResourceSystem.GetAllEntity();
    GameEntity entityConfig = configsAllEntities.Find(t => t.typeEntity == typeEntity);
    // switch (typeEntity)
    // {
    //   case TypeEntity.Bomb:
    //     entityConfig = configsAllEntities.Find(t => t.typeEntity == TypeEntity.Bomb);
    //     break;
    //   case TypeEntity.Lighting:
    //     entityConfig = configsAllEntities.Find(t => t.typeEntity == TypeEntity.Lighting);
    //     break;
    //   case TypeEntity.Coin:
    //     entityConfig = configsAllEntities.Find(t => t.typeEntity == TypeEntity.Coin);
    //     break;
    //   case TypeEntity.Frequency:
    //     entityConfig = configsAllEntities.Find(t => t.typeEntity == TypeEntity.Frequency);
    //     break;
    // }

    var asset = Addressables.InstantiateAsync(
      entityConfig.prefab,
      node.position,
      Quaternion.identity,
      ManagerHiddenWords.tilemapEntities.transform
      );
    var newObj = await asset.Task;

    var newEntity = newObj.GetComponent<BaseEntity>();
    newEntity.Init(node, asset, asBonus);
    //node.StateNode |= StateNode.Entity;
    if (asBonus)
    {
      if (!ManagerHiddenWords.Entities.ContainsKey(node.arrKey))
      {
        ManagerHiddenWords.Entities.Add(node.arrKey, typeEntity);
      }
    }
    else
    {
      if (!ManagerHiddenWords.EntitiesRuntime.ContainsKey(node.arrKey))
      {
        ManagerHiddenWords.EntitiesRuntime.Add(node.arrKey, typeEntity);
      }
    }

    // GameManager.Instance.DataManager.Save();
    _stateManager.RefreshData(false);

    return newEntity;
  }


  public void RemoveEntity(BaseEntity entity)
  {
    if (ManagerHiddenWords.Entities.ContainsKey(entity.OccupiedNode.arrKey))
    {
      if (entity.OccupiedNode.StateNode.HasFlag(StateNode.Bonus))
      {
        ManagerHiddenWords.Entities.Remove(entity.OccupiedNode.arrKey);
        entity.OccupiedNode.SetBonusEntity(null);
      }
    }

    if (ManagerHiddenWords.EntitiesRuntime.ContainsKey(entity.OccupiedNode.arrKey))
    {
      if (entity.OccupiedNode.StateNode.HasFlag(StateNode.Entity))
      {
        ManagerHiddenWords.EntitiesRuntime.Remove(entity.OccupiedNode.arrKey);
        entity.OccupiedNode.SetOccupiedEntity(null);
      }
    }
    // GameManager.Instance.DataManager.Save();
    _stateManager.RefreshData(false);
  }


  public async UniTask<GameObject> CreateCoin(Vector2 pos, Vector3 positionTo, int quantity = 1)
  {
    var coinConfig = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    // var newObj = GameObject.Instantiate(
    //    coinConfig.prefab,
    //    transform.position,
    //    Quaternion.identity
    //  );
    var asset = Addressables.InstantiateAsync(
      coinConfig.prefab,
      pos,
      Quaternion.identity
      );
    var newObj = await asset.Task;
    var newEntity = newObj.GetComponent<BaseEntity>();
    newEntity.InitStandalone(asset);
    newEntity.SetColor(_gameManager.Theme.colorAccent);
    var positionFrom = pos;
    Vector3[] waypoints = {
          positionFrom,
          positionFrom + new Vector2(1.5f, 0f),
          positionTo - new Vector3(1.5f, 0.5f),
          positionTo,
        };

    newObj.gameObject.transform
      .DOPath(waypoints, 1f, PathType.CatmullRom)
      .SetEase(Ease.OutCubic)
      .OnComplete(() =>
      {
        newEntity.AddTotalCoins(quantity);
      }
      );

    return newObj;
  }


  public async UniTask<GameObject> CreateLetter(Vector2 pos, Vector3 positionTo, char _char)
  {
    TaskCompletionSource<GameObject> _processCompletionSource = new();

    var coinConfig = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Letter);
    // var newObj = GameObject.Instantiate(
    //    coinConfig.prefab,
    //    transform.position,
    //    Quaternion.identity
    //  );
    var asset = Addressables.InstantiateAsync(
      coinConfig.prefab,
      pos,
      Quaternion.identity
      );
    var newObj = await asset.Task;
    var newEntity = newObj.GetComponent<BaseEntity>();
    newEntity.InitStandalone(asset);
    newEntity.SetColor(_gameManager.Theme.colorAccent);
    var positionFrom = pos;
    // var positionTo = topSide.spriteCoinPosition;
    Vector3[] waypoints = {
          positionFrom,
          positionFrom - new Vector2(1f, 0f),
          positionTo - new Vector3(1f, 0.5f),
          positionTo,
        };

    newObj.gameObject.transform
      .DOPath(waypoints, 1f, PathType.CatmullRom)
      .SetEase(Ease.OutCubic)
      .OnComplete(() =>
      {
        _processCompletionSource.SetResult(newObj);
        newEntity.AddLetter(_char);
      }
      );

    return await _processCompletionSource.Task;
  }

}
