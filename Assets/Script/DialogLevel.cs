using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogLevel : MonoBehaviour
{
  public static event Action OnShowDialog;
  public static event Action OnHideDialog;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private GameManager _gameManager => GameManager.Instance;
  private float duration => _gameSetting.timeGeneralAnimation;

  [SerializeField] private GameObject _bg;
  [SerializeField] private Image _wrapperSprite;
  [SerializeField] private TMPro.TextMeshProUGUI _textMessageSmall;
  [SerializeField] private TMPro.TextMeshProUGUI _textHeader;
  [SerializeField] private TMPro.TextMeshProUGUI _textMessage;
  // [SerializeField] private SpriteRenderer _spriteStar;
  // [SerializeField] private GameObject _wrapStar;
  // [SerializeField] private GameObject _countStarObject;
  // private TMPro.TextMeshProUGUI _textCountStar;
  // [SerializeField] private GameObject _indexStarObject;
  // [SerializeField] private TMPro.TextMeshProUGUI _textStarTotalCoin;
  // private TMPro.TextMeshProUGUI _indexCountStar;
  // [SerializeField] private GameObject _wrapHint;
  // [SerializeField] private SpriteRenderer _spriteHint;
  // [SerializeField] private GameObject _indexHintObject;
  // [SerializeField] private GameObject _countHintObject;
  // private TMPro.TextMeshProUGUI _textCountHint;
  // private TMPro.TextMeshProUGUI _indexCountHint;
  // [SerializeField] private TMPro.TextMeshProUGUI _textHintTotalCoin;
  [Space(10)]
  [Header("Buttons")]
  [SerializeField] private Button _buttonNext;
  [SerializeField] private TMPro.TextMeshProUGUI _textButtonNext;
  [SerializeField] private Button _buttonDouble;
  [SerializeField] private TMPro.TextMeshProUGUI _textButtonDouble;
  [SerializeField] private Image _pictoButtonDouble;
  [SerializeField] private Button _buttonOk;
  [SerializeField] private TMPro.TextMeshProUGUI _textButtonOk;

  [Space(10)]
  [Header("Hints")]
  [SerializeField] private GameObject _hintObject;

  [Space(10)]
  [Header("Total")]
  [SerializeField] private GameObject _totalObject;
  [SerializeField] private SpriteRenderer spriteCoin;
  [SerializeField] private TMPro.TextMeshProUGUI _textTotalCoin;

  [Space(10)]
  [Header("Progress")]
  [SerializeField] private GameObject _progressObject;
  [SerializeField] private Image _progressImageBar;
  [SerializeField] private TMPro.TextMeshProUGUI _progressText;
  private float maxWidthProgress = 8f;

  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;

  private Vector3 defaultPositionWrapper = new Vector3(0, 20, 0);
  private Vector3 visiblePositionWrapper = new Vector3(0, 6.5f, 0);
  // private Vector3 _initPositionColba;
  // private Vector3 _initPositionHint;
  private int _countTotalCoins;
  private Dictionary<BaseEntity, int> _hintsRound = new();
  // private int _valueTotalHintCoin;
  // private int _valueTotalStarCoin;

  private void Awake()
  {
    // // _textCountStar = _countStarObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    // // _textCountHint = _countHintObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    // _indexCountStar = _indexStarObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    // _indexCountHint = _indexHintObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    SetDefault();
    ChangeTheme();

    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  private void ChangeTheme()
  {
    _buttonDouble.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = _gameManager.Theme.colorPrimary;
    _buttonNext.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = _gameManager.Theme.colorPrimary;
    _buttonOk.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = _gameManager.Theme.colorPrimary;

    _textTotalCoin.color = _gameManager.Theme.colorPrimary;
    spriteCoin.color = _gameManager.Theme.colorPrimary;

    _wrapperSprite.color = _gameManager.Theme.bgColor;

    _textMessageSmall.color = _gameManager.Theme.colorSecondary;

    _textMessage.color = _gameManager.Theme.colorPrimary;

    _textHeader.color = _gameManager.Theme.colorAccent;

    _textMessageSmall.color = _gameManager.Theme.colorPrimary;

    _progressImageBar.color = _gameManager.Theme.colorAccent;

    _progressText.color = _gameManager.Theme.colorPrimary;

    _pictoButtonDouble.sprite = _gameSetting.spriteDouble;

    _pictoButtonDouble.color = _gameManager.Theme.colorPrimary;
  }

  public async UniTask<DataDialogResult> ShowDialogEndRound()
  {
    OnShowDialog?.Invoke();
    _gameManager.ChangeState(GameState.StartEffect);

    _gameManager.Progress.Refresh();

    _processCompletionSource = new();
    _result = new();

    SetDefault();
    _buttonNext.onClick.AddListener(CloseDialogEndRound);
    SetProgressValue(_gameManager.Progress.currentRate, 0.1f);

    _textHeader.text = await Helpers.GetLocalizedPluralString(
      "roundresult",
        new Dictionary<string, int> {
        {"value", _stateManager.dataGame.completed.Count + 1},
      }
    );

    // int totalFindedWords = _stateManager.dataGame.activeLevel.countDopWords + _stateManager.dataGame.activeLevel.countNeedWords;
    _countTotalCoins = 0; // _stateManager.dataGame.activeLevel.openWords.Count;

    // _textMessage.text = await Helpers.GetLocaledString("completelevel");

    _textTotalCoin.text = _countTotalCoins.ToString();

    _textMessageSmall.text = await Helpers.GetLocalizedPluralString(
          "completelevel_d",
           new Dictionary<string, int> {
            {"count", _stateManager.dataGame.activeLevel.openWords.Count},
          }
        );


    _bg.SetActive(true);
    _totalObject.SetActive(true);
    gameObject.SetActive(true);
    _progressObject.SetActive(true);

    // var initScale = transform.localScale;
    // Sequence mySequence = DOTween.Sequence();
    // mySequence.Append(
    transform
    .DOMove(visiblePositionWrapper, duration)
    .From(defaultPositionWrapper, true)
    .SetEase(Ease.OutCubic)
    .OnComplete(async () =>
    {

      int countCoinLevel = _countTotalCoins + _stateManager.dataGame.activeLevel.coins;

      for (int i = _countTotalCoins; i <= countCoinLevel; i++)
      {
        _textTotalCoin.text = i.ToString();// _countTotalCoins.ToString();
        await UniTask.DelayFrame(1);
      }
      AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);

      var indexBonus = _levelManager.topSide.Bonuses.Where(t => t.Key == TypeBonus.Index).FirstOrDefault().Value;

      int valueIndexBonus = _stateManager.dataGame.bonus.Where(t => t.Key == TypeBonus.Index).FirstOrDefault().Value + 1;

      int _countTotalOfByIndex = valueIndexBonus * countCoinLevel;

      _countTotalCoins = _countTotalOfByIndex;

      if (indexBonus != null)
      {
        indexBonus.SetSortOrder(60);
        indexBonus.bonusObject.gameObject.transform
          .DOMove(spriteCoin.transform.position + new Vector3(0.3f, 0, 0), duration) // new Vector3(-2.9f, 6.13f, 0)
          .OnComplete(async () =>
          {
            for (int i = countCoinLevel; i <= _countTotalOfByIndex; i++)
            {
              _textTotalCoin.text = i.ToString();
              await UniTask.DelayFrame(1);
            }

            AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);

            SetProgressValue(_gameManager.Progress.currentRate + _stateManager.stateGame.activeDataGame.activeLevel.openWords.Count, _gameSetting.timeGeneralAnimation);

          });
      }

      _buttonNext.gameObject.SetActive(true);
      _buttonDouble.gameObject.SetActive(true);
    });
    // );

    // transform
    //   .DOScale(initScale, duration * 3)
    //   .From(Vector3.zero)
    //   .SetEase(Ease.OutElastic)
    //   .OnComplete(() =>
    //   {
    //     _totalObject.SetActive(true);
    //     _textTotalCoin.text = _countTotalCoins.ToString();
    //     AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);
    //   })
    // mySequence.Append(
    //   _textTotalCoin.transform.DOPunchScale(new Vector3(.1f, .1f, 0), duration, 5, 1)
    // );

    // var indexBonus = _levelManager.topSide.Bonuses.Where(t => t.Key == TypeBonus.Index).FirstOrDefault().Value;

    // if (indexBonus != null)
    // {
    //   mySequence.Append(
    //     indexBonus.gameObject.transform
    //       .DOMove(spriteCoin.transform.localPosition + new Vector3(0, 3.5f), duration)
    //       .OnComplete(() =>
    //       {
    //         int valueIndexBonus = _stateManager.dataGame.bonus.Where(t => t.Key == TypeBonus.Index).FirstOrDefault().Value + 1;

    //         _countTotalCoins = valueIndexBonus * _countTotalCoins;

    //         _textTotalCoin.text = _countTotalCoins.ToString();

    //         AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);
    //       })
    //   );


    // }
    return await _processCompletionSource.Task;
  }

  /// <summary>
  /// Double coins (run from unity action of by button double)
  /// </summary>
  public void OnClickDoubleButton()
  {
    _buttonDouble.gameObject.SetActive(false);
    DataManager.OnAddCoins += OnDoubleCoins;
    _gameManager.DataManager.AddCoinsByAdv(_countTotalCoins);

    // OnDoubleCoins();
  }

  public async void OnDoubleCoins(int value)
  {
    int startCoins = _countTotalCoins;

    _countTotalCoins *= 2;

    for (int i = startCoins; i <= _countTotalCoins; i++)
    {
      _textTotalCoin.text = i.ToString();

      await UniTask.Delay(1);
    }
  }

  private void GoCoins()
  {
    GameEntity configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);

    _stateManager.IncrementTotalCoin(_countTotalCoins);
    // await _levelManager.CreateCoin(
    //   spriteCoin.transform.position,
    //   _levelManager.topSide.targetTotalCoinObject.transform.position,
    //   _countTotalCoins
    // );
  }

  public async void CloseDialogEndRound()
  {
    _gameManager.audioManager.Click();

    DataManager.OnAddCoins -= OnDoubleCoins;

    int valueIndexBonus = _stateManager.dataGame.bonus.Where(t => t.Key == TypeBonus.Index).FirstOrDefault().Value;

    BaseBonus bonusIndexObject;

    _levelManager.topSide.Bonuses.TryGetValue(TypeBonus.Index, out bonusIndexObject);

    if (bonusIndexObject != null)
    {
      bonusIndexObject.SetDefaultWithAnimate();
    }

    _stateManager.UseBonus(-valueIndexBonus, TypeBonus.Index);

    GoCoins();

    await UniTask.Delay(500);

    // _levelManager.buttonHint.gameObject.transform
    //   .DOJump(_initPositionHint, 2, 1, duration)
    //   .SetEase(Ease.OutQuad);
    // _levelManager.buttonStar.gameObject.transform
    //    .DOJump(_initPositionColba, 2, 1, duration)
    //    .SetEase(Ease.OutQuad);
    // // _levelManager.hint.gameObject.transform.DOMove(_initPositionHint, duration).SetEase(Ease.Linear);
    // // _levelManager.colba.gameObject.transform.DOMove(_initPositionColba, duration).SetEase(Ease.Linear);

    // await UniTask.Delay(500);
    transform
      .DOMove(defaultPositionWrapper, duration)
      .SetEase(Ease.Linear)
      .OnComplete(() =>
      {
        _stateManager.IncrementRate(_stateManager.stateGame.activeDataGame.activeLevel.openWords.Count);

        // gameObject.SetActive(false);
        // transform.localPosition = defaultPositionWrapper;
        _buttonNext.gameObject.SetActive(false);
        _buttonDouble.gameObject.SetActive(false);
        _totalObject.SetActive(false);
        OnHideDialog?.Invoke();
        _gameManager.ChangeState(GameState.StopEffect);

        _result.isOk = true;
        _processCompletionSource.SetResult(_result);
      });


  }


  public async UniTask<DataDialogResult> ShowDialogStartRound()
  {
    OnShowDialog?.Invoke();
    _gameManager.ChangeState(GameState.StartEffect);

    _processCompletionSource = new();
    _result = new();

    SetDefault();

    _buttonOk.onClick.AddListener(CloseDialogStartRound);

    _textHeader.text = string.Format("{0} {1}",
      await Helpers.GetLocaledString("round"),
      _stateManager.dataGame.completed.Count + 1
    );
    _textMessage.text = "";

    if (_stateManager.dataGame.activeLevel.openWords.Count == 0)
    {
      // Create hint entity.
      var countHints = 0;

      if (_stateManager.dataGame.activeLevel.hints.ContainsKey(TypeEntity.Star))
      {
        var valueStar = _stateManager.dataGame.activeLevel.hints.GetValueOrDefault(TypeEntity.Star);
        var newEntityStar = await _levelManager.buttonStar.CreateEntity(Vector3.zero, _hintObject);
        _hintsRound.Add(newEntityStar, valueStar);
        newEntityStar.counterObject.transform.DOScale(1, duration);
        newEntityStar.counterText.text = valueStar.ToString();
        countHints += valueStar;
      }
      if (_stateManager.dataGame.activeLevel.hints.ContainsKey(TypeEntity.Frequency))
      {
        var valueHint = _stateManager.dataGame.activeLevel.hints.GetValueOrDefault(TypeEntity.Frequency);
        var newEntity = await _levelManager.buttonFrequency.CreateEntity(Vector3.zero, _hintObject);
        _hintsRound.Add(newEntity, valueHint);
        newEntity.counterObject.transform.DOScale(1, duration);
        newEntity.counterText.text = valueHint.ToString();
        countHints += valueHint;
      }

      // var textMessageHints = await Helpers.GetLocalizedPluralString(
      //     "givestarthints",
      //     new Dictionary<string, object> {
      //     {"count", countHints}
      //     }
      //   );
      // if (countHints == 0)
      // {
      //   textMessageHints = await Helpers.GetLocalizedPluralString(
      //     "givestarthints_no",
      //     new Dictionary<string, object> {
      //     {"count", countHints}
      //     }
      //   );
      // }
      _textMessageSmall.text = string.Format("{0}",
        // await Helpers.GetLocaledString("policyround"),
        await Helpers.GetLocalizedPluralString(
          "roundconditdesc",
          new Dictionary<string, object> {
          {"count", _stateManager.dataGame.activeLevel.countNeedWords},
          {"count2", _levelManager.ManagerHiddenWords.AllowPotentialWords.Count}
          }
        )
      );
    }
    else
    {
      _textMessageSmall.text = string.Format("{0}\r\n{1}",
        // await Helpers.GetLocaledString("policyround"),
        await Helpers.GetLocalizedPluralString(
          "roundconditdesc",
          new Dictionary<string, object> {
          {"count", _stateManager.dataGame.activeLevel.countNeedWords},
          {"count2", _levelManager.ManagerHiddenWords.AllowPotentialWords.Count}
          }
        ),
        await Helpers.GetLocalizedPluralString(
          "currentprogress",
          new Dictionary<string, object> {
          {"count", _stateManager.dataGame.activeLevel.openWords.Count - _stateManager.dataGame.activeLevel.countDopWords},
          {"count2", _stateManager.dataGame.activeLevel.needWords.Count - _stateManager.dataGame.activeLevel.openWords.Count+ _stateManager.dataGame.activeLevel.countDopWords}
          }
        )
      );
    }

    _bg.SetActive(true);
    _buttonOk.gameObject.SetActive(true);
    gameObject.SetActive(true);

    transform
      .DOMove(visiblePositionWrapper, duration)
      .From(defaultPositionWrapper, true)
      .SetEase(Ease.OutQuart);

    return await _processCompletionSource.Task;
  }

  public void CloseDialogStartRound()
  {
    _gameManager.audioManager.Click();

    int countHints = _hintsRound.Count;

    foreach (var item in _hintsRound)
    {
      GameObject targetMoveHint = null;
      switch (item.Key.configEntity.typeEntity)
      {
        case TypeEntity.Star:
          targetMoveHint = _levelManager.buttonStar.gameObject;
          break;
        case TypeEntity.Frequency:
          targetMoveHint = _levelManager.buttonFrequency.gameObject;
          break;
      }
      item.Key.gameObject.transform
        .DOMove(targetMoveHint.transform.position, duration)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
          Destroy(item.Key);
          _hintsRound.Remove(item.Key);
          _stateManager.UseHint(item.Value, item.Key.configEntity.typeEntity);
        });
    }

    _stateManager.dataGame.activeLevel.hints.Clear();

    float koef = countHints == 0 ? 1 : 1.5f;
    transform
      .DOMove(defaultPositionWrapper, duration * koef)
      .SetEase(Ease.Linear)
      .OnComplete(() =>
      {
        SetDefault();

        OnHideDialog?.Invoke();
        _gameManager.ChangeState(GameState.StopEffect);

        _result.isOk = true;
        _processCompletionSource.SetResult(_result);
      });

  }



  public async UniTask<DataDialogResult> ShowDialogNewRankUser()
  {
    OnShowDialog?.Invoke();
    _gameManager.ChangeState(GameState.StartEffect);
    _processCompletionSource = new();
    _result = new();

    SetDefault();

    _buttonOk.onClick.AddListener(CloseDialogNewRankUser);

    _textHeader.text = string.Format("{0}", await Helpers.GetLocaledString("newrank"));

    string newRank = await Helpers.GetLocaledString(_gameManager.PlayerSetting.text.title);

    _textHeader.text = newRank;

    _textMessageSmall.text = string.Format("{0}",
      await Helpers.GetLocalizedPluralString(
        "newrank_d",
        new Dictionary<string, object> {
        {"name", newRank}
        }
      )
    );

    _bg.SetActive(true);
    _buttonOk.gameObject.SetActive(true);
    gameObject.SetActive(true);

    transform
      .DOMove(visiblePositionWrapper, duration * 2)
      .From(defaultPositionWrapper, true)
      .SetEase(Ease.OutBack);

    return await _processCompletionSource.Task;
  }

  public void CloseDialogNewRankUser()
  {
    _gameManager.audioManager.Click();

    transform
      .DOMove(defaultPositionWrapper, duration * 2)
      .SetEase(Ease.InBack)
      .OnComplete(() =>
      {
        gameObject.SetActive(false);
        _bg.SetActive(false);
        SetDefault();
        _bg.SetActive(false);

        OnHideDialog?.Invoke();
        _gameManager.ChangeState(GameState.StopEffect);

        _result.isOk = true;
        _processCompletionSource.SetResult(_result);
      });
  }

  private void SetDefault()
  {
    Helpers.DestroyChildren(_hintObject.transform);

    gameObject.SetActive(false);
    gameObject.transform.position = defaultPositionWrapper;
    _bg.SetActive(false);
    _totalObject.SetActive(false);

    _buttonNext.gameObject.SetActive(false);
    _buttonNext.onClick.RemoveAllListeners();

    _buttonDouble.gameObject.SetActive(false);
    // _buttonDouble.onClick.RemoveAllListeners();

    _buttonOk.gameObject.SetActive(false);
    _buttonOk.onClick.RemoveAllListeners();

    _progressObject.SetActive(false);
    // _textCountStar.text = "";
    // _textCountHint.text = "";
    // _indexCountStar.text = "?";
    // _indexCountHint.text = "?";
    // _textStarTotalCoin.text = "?";
    // _textHintTotalCoin.text = "?";
    _textTotalCoin.text = "";
    // _wrapStar.SetActive(false);
    // _wrapHint.SetActive(false);
  }

  private void SetProgressValue(int value, float delay)
  {

    float width = 0;
    if (value > 0)
    {
      width = ((value) * 100f / _gameManager.Progress.totalNeedWords) * (maxWidthProgress / 100f);
    }

    RectTransform progressRect = _progressImageBar.GetComponent<RectTransform>();

    if (progressRect != null)
    {
      progressRect.DOSizeDelta(new Vector3(width, progressRect.rect.height), delay).OnComplete(async () =>
      {
        string nameStatus = await Helpers.GetLocaledString(_gameManager.PlayerSetting.text.title);


        // int countNeedOpenWords =  _gameManager.PlayerSetting.countFindWordsForUp - _gameManager.StateManager.stateGame.activeDataGame.rate;
        // int procent = 100 - Mathf.RoundToInt(countNeedOpenWords * 100f / (float)_gameManager.PlayerSetting.countFindWordsForUp);
        var percent = _gameManager.Progress.GetPercent(_gameManager.Progress.currentRate + _stateManager.stateGame.activeDataGame.activeLevel.openWords.Count);

        if (percent > 100f)
        {
          _progressText.text = await Helpers.GetLocalizedPluralString("completesection_dialog", new Dictionary<string, object>() {
          { "name", nameStatus}
        });
        }
        else
        {
          _progressText.text = await Helpers.GetLocalizedPluralString("foundwords_dialog", new Dictionary<string, object>() {
          { "name", nameStatus},
          { "count", _gameManager.Progress.countNeedOpenWords - _stateManager.stateGame.activeDataGame.activeLevel.openWords.Count},
          { "procent", percent}
        });
        }

      });
    }
    //.sizeDelta = new Vector3(width, 1f);
  }

  //   var positionTo = _spriteStar.transform.position;
  //   Vector3[] waypoints = new[] {
  //     // _initPositionColba,
  //     _initPositionColba + new Vector3(.5f, -1f),
  //     // positionTo - new Vector3(0.5f, 0.5f),
  //     // positionTo - new Vector3(1, 1),
  //     positionTo,
  //   };
  //   mySequence.Append(
  //    _levelManager.buttonStar.gameObject.transform
  //      .DOJump(positionTo, 2, 1, duration)
  //      .SetEase(Ease.OutQuad)
  //  );

  // mySequence.Append(
  //   _indexStarObject.transform
  //     .DOPunchScale(new Vector3(1, 1, 0), duration, 5, 1)
  //     .OnComplete(() =>
  //     {
  //       AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);
  //       _levelManager.buttonStar.HideCounter();
  //       _levelManager.buttonStar.ResetProgressBar();

  //       _indexCountStar.text = _stateManager.dataGame.activeLevel.star.ToString();
  //       _valueTotalStarCoin = _stateManager.dataGame.activeLevel.star * _stateManager.dataGame.activeLevel.star;
  //       _textStarTotalCoin.text = _valueTotalStarCoin.ToString();
  //     })
  // );

  //   var positionToHint = _spriteHint.transform.position;

  //   mySequence.Append(
  //    _levelManager.buttonHint.gameObject.transform
  //      .DOJump(positionToHint, 2, 1, duration)
  //      .SetEase(Ease.OutQuad)
  //  );

  //   mySequence.Append(
  //     _indexHintObject.transform
  //       .DOPunchScale(new Vector3(1, 1, 0), duration, 5, 1)
  //       .OnComplete(() =>
  //       {
  //         AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.calculateCoin);
  //         _levelManager.buttonHint.HideCounter();
  //         _levelManager.buttonHint.ResetProgressBar();

  //         _indexCountHint.text = _stateManager.dataGame.activeLevel.hint.ToString();

  //         _valueTotalHintCoin = _stateManager.dataGame.activeLevel.hint * _stateManager.dataGame.activeLevel.hint;

  //         _textHintTotalCoin.text = _valueTotalHintCoin.ToString();
  //         _totalObject.SetActive(true);
  //       })
  //   );

}
