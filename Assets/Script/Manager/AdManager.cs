using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AdManager : Singleton<LevelManager>
{
  [DllImport("__Internal")]
  private static extern void ShowAdvFullScreen();
  [DllImport("__Internal")]
  private static extern void AddHintExtern(string data);
  [DllImport("__Internal")]
  public static extern void AddBonusExtern(string data);
  [DllImport("__Internal")]
  private static extern void RateGame();
  [DllImport("__Internal")]
  private static extern void GetCanReview();
  private string _lastTimeShowAdv;
  private bool _isCanReview;
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;

  protected override void Awake()
  {
    base.Awake();
    SetLastTime();
  }

  private void Start()
  {
#if ysdk
    GetCanReview();
#endif
  }

  public void ShowAdvFullScr()
  {
    var diffDate = DateTime.Now - DateTime.Parse(_lastTimeShowAdv);

    if (diffDate.TotalMinutes > (double)_gameSetting.adsPerTime)
    {
#if ysdk
      ShowAdvFullScreen();
#endif
      SetLastTime();
      UnityEngine.Debug.Log("Show auto adv");
    }
  }

  public void SetLastTime()
  {
    _lastTimeShowAdv = System.DateTime.Now.ToString();
  }

  public void AddHintByAdv(ShopAdvBuyItem<TypeEntity> data)
  {
    string stringAddedHint = JsonUtility.ToJson(data);

    Debug.Log($"AddHintByAdv {stringAddedHint}");

    AddHintExtern(stringAddedHint);

    _lastTimeShowAdv = System.DateTime.Now.ToString();
  }


  public void AddBonusByAdv(ShopAdvBuyItem<TypeBonus> data)
  {
    string stringAddedBonus = JsonUtility.ToJson(data);
    Debug.Log($"AddBonusByAdv {stringAddedBonus}");

    AddBonusExtern(stringAddedBonus);

    _lastTimeShowAdv = System.DateTime.Now.ToString();
  }


  public async UniTask ShowDialogAddRateGame()
  {
    Debug.Log($"{name}::: ShowDialogAddRateGame {_isCanReview}");

    if (!_isCanReview) return;
    if (_stateManager.stateGame.rate < _gameSetting.minRateForReview) return;

    _gameManager.InputManager.Disable();

    var messageConfirm = await Helpers.GetLocaledString("dialog_rategame_message");
    var dialogConfirm = new DialogProvider(new DataDialog()
    {
      // title = title,
      message = messageConfirm,
      showCancelButton = true
    });

    var result = await dialogConfirm.ShowAndHide();
    if (result.isOk)
    {
      RateGame();
    }

    _gameManager.InputManager.Enable();
  }

  public void SetRateGame(bool status)
  {
    if (status)
    {
      _stateManager.IncrementTotalCoin(_gameSetting.countCoinForReview);
    }
    else
    {

    }
    Debug.Log($"{name}::: SetRateGame {status}");
  }


  public void SetCanReview(bool status)
  {
    _isCanReview = status;

    Debug.Log($"{name}::: SetCanReview {status}");
  }
}
