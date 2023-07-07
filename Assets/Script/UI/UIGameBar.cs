using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Loader;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class UIGameBar : UILocaleBase
{
  [DllImport("__Internal")]
  private static extern void GetLeaderBoard();
  [SerializeField] private UIDocument _uiDoc;
  // private Button _exitButton;
  private VisualElement _root;
  private VisualElement _userShortInfo;
  private Label _userRate;
  private Label _userCoin;
  private Label _userName;
  private Label _letterCount;
  private VisualElement _userCoinImg;
  private VisualElement _letterImg;
  private VisualElement _userRateImg;
  private VisualElement _cogImg;
  private VisualElement _shopImg;
  private VisualElement _avaImg;
  private Button _settingsButton;
  private Button _shopButton;
  private Button _letterButton;
  private void Awake()
  {
    GameManager.OnChangeTheme += ChangeTheme;
    GameManager.OnAfterStateChanged += AfterStateChanged;
    StateManager.OnChangeState += SetValue;
    DialogLevel.OnHideDialog += ShowAside;
    DialogLevel.OnShowDialog += HideAside;
  }

  private void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
    GameManager.OnAfterStateChanged -= AfterStateChanged;
    StateManager.OnChangeState -= SetValue;
    DialogLevel.OnHideDialog -= ShowAside;
    DialogLevel.OnShowDialog -= HideAside;
  }

  private void SetValue(StateGame state)
  {
    if (state.activeDataGame.activeLevel != null)
    {
      if (_letterCount.text != state.activeDataGame.activeLevel.coins.ToString())
      {
        RunAnimateCoin(new Vector3(3, 9.2f));
      }

      if (_userCoin.text != state.coins.ToString())
      {
        RunAnimateCoin(new Vector3(-3, 9.2f)); // _userCoinImg.worldTransform.GetPosition()
      }

      _letterCount.text = string.Format("{0}", state.activeDataGame.activeLevel.coins);
      _userCoin.text = string.Format("{0}", state.coins);
      _userRate.text = string.Format("{0}", state.rate);
    }
  }

  private void AfterStateChanged(GameState state)
  {
    switch (state)
    {
      case GameState.CloseLevel:
        HideUserInfo();
        break;
      case GameState.LoadLevel:
        ShowUserInfo();
        break;
    }
  }

  public virtual void Start()
  {
    _root = _uiDoc.rootVisualElement;
    _userShortInfo = _root.Q<VisualElement>("UserShortInfo");
    _letterButton = _root.Q<Button>("BtnLetter");
    _letterButton.clickable.clicked += ShowInfoLetter;

    _letterCount = _root.Q<Label>("LetterCount");
    _letterImg = _root.Q<VisualElement>("LetterImg");

    HideUserInfo();

    _userCoin = _root.Q<Label>("UserCoin");
    _userCoinImg = _root.Q<VisualElement>("UserCoinImg");

    _userRate = _root.Q<Label>("UserRate");
    _userRateImg = _root.Q<VisualElement>("UserRateImg");

    _userName = _root.Q<Label>("UserName");

    _cogImg = _root.Q<VisualElement>("ImgCog");
    _shopImg = _root.Q<VisualElement>("ImgShop");
    _avaImg = _root.Q<VisualElement>("Ava");

    _settingsButton = _root.Q<Button>("MenuBtn");
    _settingsButton.clickable.clicked += () =>
    {
      ShowSettings();
    };

    _shopButton = _root.Q<Button>("ShopBtn");
    _shopButton.clickable.clicked += () =>
    {
      ShowShop();
    };

    // SetValue(_gameManager.StateManager.stateGame);

    // #if ysdk
    //     GetLeaderBoard();
    // #endif

    // var diffDate = string.IsNullOrEmpty(_gameManager.StateManager.stateGame.lastTime)
    //   ? (DateTime.Now - DateTime.Now)
    //   : (DateTime.Now - DateTime.Parse(_gameManager.StateManager.stateGame.lastTime));
    // // Debug.Log($"diffDate={diffDate}|||[{DateTime.Now}:::::{_gameManager.StateManager.stateGame.lastTime}]");
    // if (string.IsNullOrEmpty(_gameManager.StateManager.stateGame.lastTime) || diffDate.TotalHours > _gameSettings.countHoursDailyGift)
    // {
    //   ShowDailyDialog();
    // }

    var dialogDashboard = new UIDashboardOperation();
    dialogDashboard.ShowAndHide().Forget();

    ChangeTheme();
    base.Initialize(_uiDoc.rootVisualElement);
  }

  private void RunAnimateCoin(Vector3 pos)
  {
    if (_gameManager.LevelManager == null) return;

    // var positionObject = Camera.main.ScreenToWorldPoint(pos);
    // var pos3 = new Vector3(positionObject.x + 5, -positionObject.y, 0);
    // Debug.Log($"{positionObject}|||{pos}|||{pos3}");
    var configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    var _CachedSystem = GameObject.Instantiate(
      configEntity.activateEffect,
      pos,
      Quaternion.identity
    );

    var main = _CachedSystem.main;
    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, _gameManager.LevelManager.ManagerHiddenWords.scaleGrid / 2);

    var col = _CachedSystem.colorOverLifetime;
    col.enabled = true;

    Gradient grad = new Gradient();
    grad.SetKeys(new GradientColorKey[] {
      new GradientColorKey(_gameManager.Theme.bgFindAllowWord, 1.0f),
      new GradientColorKey(_gameManager.Theme.bgHiddenWord, 0.0f)
      }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f)
    });

    col.color = grad;
    _CachedSystem.Play();

    Destroy(_CachedSystem, .5f);
  }


  private async void ShowInfoLetter()
  {
    _gameManager.InputManager.Disable();

    var configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Letter);

    var message = await Helpers.GetLocaledString("letter_desc");
    var dialogConfirm = new DialogProvider(new DataDialog()
    {
      sprite = configEntity.sprite,
      message = message
    });

    await dialogConfirm.ShowAndHide();
    _gameManager.InputManager.Enable();
  }

  private async void ChangeTheme()
  {
    var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    var configLetter = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Letter);

    _letterImg.style.backgroundImage = new StyleBackground(configLetter.sprite);
    _letterImg.style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.colorSecondary);
    _letterCount.style.color = _gameManager.Theme.colorSecondary;

    _userCoinImg.style.backgroundImage = new StyleBackground(configCoin.sprite);
    _userRateImg.style.backgroundImage = new StyleBackground(_gameSetting.spriteRate);
    _cogImg.style.backgroundImage = new StyleBackground(base._gameSetting.spriteCog);
    _cogImg.style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.colorSecondary);

    //     // load avatar
    //     string placeholder = _gameManager.AppInfo.UserInfo.photo;
    //     Texture2D avatarTexture = await Helpers.LoadTexture(placeholder);
    //     if (avatarTexture != null)
    //     {
    //       _avaImg.style.backgroundImage = new StyleBackground(avatarTexture);
    //     }
    //     else
    //     {
    // {
    //   _avaImg.style.backgroundImage = new StyleBackground(_gameSetting.spriteUser);
    // }

    _userCoinImg.style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.colorSecondary);
    _userRateImg.style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.colorSecondary);

    _shopImg.style.backgroundImage = new StyleBackground(base._gameSetting.spriteShop);
    _shopImg.style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.colorSecondary);

    _userName.text = await Helpers.GetName();

    base.Theming(_uiDoc.rootVisualElement);
  }
  // private async void ShowDailyDialog()
  // {
  //   var title = await Helpers.GetLocaledString("dailycoins_t");
  //   var message = await Helpers.GetLocalizedPluralString("dailycoins_d", new Dictionary<string, object>() {
  //     { "count", _gameSettings.countHoursDailyGift },
  //   });
  //   var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
  //   var configStar = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Star);
  //   var entities = new List<ShopItem<GameEntity>>() {
  //         new ShopItem<GameEntity>(){
  //           entity = configCoin,
  //           cost = 0,
  //           count = 500
  //         },
  //         new ShopItem<GameEntity>(){
  //           entity = configStar,
  //           cost = 0,
  //           count = 3
  //         }
  //       };

  //   var dialog = new DialogProvider(new DataDialog()
  //   {
  //     title = title,
  //     message = message,
  //     entities = entities,
  //     showCancelButton = false,
  //   });

  //   _gameManager.InputManager.Disable();
  //   var result = await dialog.ShowAndHide();
  //   if (result.isOk)
  //   {
  //     foreach (var entityItem in entities)
  //     {
  //       if (entityItem.entity.typeEntity == TypeEntity.Coin)
  //       {
  //         _gameManager.StateManager.IncrementTotalCoin(entityItem.count);
  //       }
  //       else
  //       {
  //         _gameManager.StateManager.BuyHint(entityItem);
  //       }
  //     }
  //     _gameManager.StateManager.SetLastTime();
  //   }
  //   _gameManager.InputManager.Enable();
  // }

  // private void SetValue(StateGame state)
  // {
  //   _userCoin.text = string.Format("{0}", state.coins);
  //   _userRate.text = string.Format("{0}", state.rate);
  // }

  private void HideUserInfo()
  {
    _userShortInfo.style.display = DisplayStyle.None;
    _letterButton.style.display = DisplayStyle.None;
  }

  private void ShowUserInfo()
  {
    _userShortInfo.style.display = DisplayStyle.Flex;
    _letterButton.style.display = DisplayStyle.Flex;
  }

  private async void ShowSettings()
  {
    AudioManager.Instance.Click();

    _gameManager.InputManager.Disable();
    var settingsDialog = new UISettingsOperation();
    await settingsDialog.ShowAndHide();
    _gameManager.InputManager.Enable();
  }

  private async void ShowShop()
  {
    AudioManager.Instance.Click();

    _gameManager.InputManager.Disable();
    var dialogWindow = new UIShopOperation();
    var result = await dialogWindow.ShowAndHide();
    _gameManager.InputManager.Enable();
  }

  private void ShowAside()
  {
    // _aside.style.display = DisplayStyle.Flex;
    _settingsButton.style.visibility = Visibility.Visible;
    _shopButton.style.visibility = Visibility.Visible;
  }

  private void HideAside()
  {
    // _aside.style.display = DisplayStyle.None;
    _settingsButton.style.visibility = Visibility.Hidden;
    _shopButton.style.visibility = Visibility.Hidden;
  }

  // private void ClickExitButton()
  // {
  //   AudioManager.Instance.Click();
  //   Debug.Log("ClickExitButton");
  // }

}
