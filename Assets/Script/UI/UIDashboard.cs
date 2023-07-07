using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Loader;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class UIDashboard : UILocaleBase
{
  [DllImport("__Internal")]
  private static extern void GetLeaderBoard();
  [SerializeField] private UIDocument _uiDoc;
  [SerializeField] private VisualTreeAsset UserInfoDoc;
  [SerializeField] private VisualTreeAsset LeaderDoc;
  private VisualElement _menu;
  private Button _exitButton;
  // private Button _newGameButton;
  private VisualElement _userInfoBlok;
  private Label _userCoin;
  private Label _userRate;
  private VisualElement _leaderBoard;
  // private Button _loadGameMenuButton;
  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;

  private void Awake()
  {
    UISettings.OnChangeLocale += RefreshMenu;
    GameManager.OnChangeTheme += RefreshMenu;
    StateManager.OnChangeState += SetValue;
    DataManager.OnLoadLeaderBoard += DrawLeaderListBlok;
  }

  private void OnDestroy()
  {
    UISettings.OnChangeLocale -= RefreshMenu;
    GameManager.OnChangeTheme -= RefreshMenu;
    StateManager.OnChangeState -= SetValue;
    DataManager.OnLoadLeaderBoard -= DrawLeaderListBlok;
  }

  public virtual void Start()
  {
    _menu = _uiDoc.rootVisualElement.Q<VisualElement>("MenuBlok");
    _userInfoBlok = _uiDoc.rootVisualElement.Q<VisualElement>("UserInfoBlok");

    _leaderBoard = _uiDoc.rootVisualElement.Q<VisualElement>("LeaderBoard");
    _leaderBoard.style.display = DisplayStyle.None;

    _exitButton = _menu.Q<Button>("ExitBtn");
    _exitButton.clickable.clicked += () =>
    {
      ClickExitButton();
    };


#if UNITY_EDITOR
    _gameManager.DataManager.GetLeaderBoard("{\"leaderboard\":{\"title\":[{\"lang\":\"ru\",\"value\":\"Лидеры по количеству слов\"}]},\"userRank\":25,\"entries\":[{\"rank\":24,\"score\":90,\"name\":\"Tamara Ivanovna Semenovatoreva\",\"lang\":\"ru\",\"photo\":\"\"},{\"rank\":25,\"score\":80,\"name\":\"Mikalai P.2\",\"lang\":\"ru\",\"photo\":\"https://games-sdk.yandex.ru/games/api/sdk/v1/player/avatar/66VOVRVF2GJAXS5VWT3X54YATTEZAJLGXTPIXJTG3465T5HXLNQFMZIOJ7WYALX2PEC2DIAHLM6FC7ABRLOA27IRF55DP6DXJU7JDS4IFW63KJWT4IFLT2I26N44GVCAAX6FGHPPVKQY65KZZOXXYODUUKJMK2Y25M2VUDFYRPJDR3TS4JVBUOZNWFE2QNABMFRQEVLJRRIODYNB2JKIIK76YMZEEA3VQHV3M6Q=/islands-retina-medium\"},{\"rank\":26,\"score\":70,\"name\":\"Mikalai P.3\",\"lang\":\"ru\",\"photo\":\"https://games-sdk.yandex.ru/games/api/sdk/v1/player/avatar/66VOVRVF2GJAXS5VWT3X54YATTEZAJLGXTPIXJTG3465T5HXLNQFMZIOJ7WYALX2PEC2DIAHLM6FC7ABRLOA27IRF55DP6DXJU7JDS4IFW63KJWT4IFLT2I26N44GVCAAX6FGHPPVKQY65KZZOXXYODUUKJMK2Y25M2VUDFYRPJDR3TS4JVBUOZNWFE2QNABMFRQEVLJRRIODYNB2JKIIK76YMZEEA3VQHV3M6Q=/islands-retina-medium\"}]}");
#endif
#if ysdk
    GetLeaderBoard();
#endif
    // await DrawLeaderListBlok();

    RefreshMenu();

    var diffDate = string.IsNullOrEmpty(_gameManager.StateManager.stateGame.lastTime)
      ? (DateTime.Now - DateTime.Now)
      : (DateTime.Now - DateTime.Parse(_gameManager.StateManager.stateGame.lastTime));
    // Debug.Log($"diffDate={diffDate}|||[{DateTime.Now}:::::{_gameManager.StateManager.stateGame.lastTime}]");
    if (string.IsNullOrEmpty(_gameManager.StateManager.stateGame.lastTime) || diffDate.TotalHours > _gameSetting.countHoursDailyGift)
    {
      ShowDailyDialog();
    }

    // base.Initialize(_uiDoc.rootVisualElement);
  }

  private void SetValue(StateGame state)
  {
    // _userCoin.text = string.Format("{0}", state.coins);
    // _userRate.text = string.Format("{0}", state.rate);

    RefreshMenu();
  }

  public async UniTask<DataDialogResult> ProcessAction()
  {
    _result = new DataDialogResult();

    _processCompletionSource = new TaskCompletionSource<DataDialogResult>();

    return await _processCompletionSource.Task;
  }

  private async void ShowDailyDialog()
  {
    var title = await Helpers.GetLocaledString("dailycoins_t");
    var message = await Helpers.GetLocalizedPluralString("dailycoins_d", new Dictionary<string, object>() {
      { "count", _gameSetting.countHoursDailyGift },
    });
    var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    var configStar = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Star);
    var entities = new List<ShopItem<GameEntity>>() {
          new ShopItem<GameEntity>(){
            entity = configCoin,
            cost = 0,
            count = 500
          },
          new ShopItem<GameEntity>(){
            entity = configStar,
            cost = 0,
            count = 3
          }
        };

    var dialog = new DialogProvider(new DataDialog()
    {
      title = title,
      message = message,
      entities = entities,
      showCancelButton = false,
    });

    _gameManager.InputManager.Disable();
    var result = await dialog.ShowAndHide();
    if (result.isOk)
    {
      foreach (var entityItem in entities)
      {
        if (entityItem.entity.typeEntity == TypeEntity.Coin)
        {
          _gameManager.StateManager.IncrementTotalCoin(entityItem.count);
        }
        else
        {
          _gameManager.StateManager.BuyHint(entityItem);
        }
      }
      _gameManager.StateManager.SetLastTime();
    }
    _gameManager.InputManager.Enable();
  }

  private async void RefreshMenu()
  {

    await DrawUserInfoBlok();

    // #if ysdk && !UNITY_EDITOR
    //     await DrawLeaderListBlok();
    // #endif

    base.Initialize(_uiDoc.rootVisualElement);
  }


  private async void DrawLeaderListBlok(LeaderBoard board)
  {
    LeaderBoard leaderBoard = _gameManager.DataManager.leaderBoard;

    if (leaderBoard == null) return;

    if (leaderBoard.entries.Count == 0)
    {
      _leaderBoard.style.display = DisplayStyle.None;
      return;
    }
    else
    {
      _leaderBoard.style.display = DisplayStyle.Flex;
    }

    await LocalizationSettings.InitializationOperation.Task;

    var dataState = _gameManager.StateManager.dataGame;
    LeaderBoardInfoTitle titleBoard = leaderBoard.leaderboard.title.Find((t) => t.lang == LocalizationSettings.SelectedLocale.Identifier.Code);
    if (string.IsNullOrEmpty(titleBoard.value))
    {
      _leaderBoard.Q<Label>("NameBoard").text = titleBoard.value;
    }
    var _leaderList = _leaderBoard.Q<VisualElement>("LeaderList");
    _leaderList.Clear();

    int countLeaderShow = leaderBoard.entries.Count > 5 ? 5 : leaderBoard.entries.Count;

    for (int i = 0; i < countLeaderShow; i++)
    {
      var blok = LeaderDoc.Instantiate();

      var leader = leaderBoard.entries[i];

      var rank = blok.Q<Label>("Rank");
      rank.text = leader.rank.ToString();

      var name = blok.Q<Label>("Name");
      name.text = leader.name;

      var ava = blok.Q<VisualElement>("Ava");
      Texture2D avatarTexture = await Helpers.LoadTexture(leader.photo);
      if (avatarTexture != null)
      {
        ava.style.backgroundImage = new StyleBackground(avatarTexture);
      }
      else
      {
        ava.style.backgroundImage = new StyleBackground(_gameSetting.spriteUser);
        ava.style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;
      }

      var score = blok.Q<Label>("Score");
      score.text = leader.score.ToString();

      // blok.Q<Label>("Ava").style.backgroundImage = new StyleBackground(avaSprite);
      _leaderList.Add(blok);
    }

    base.Initialize(_leaderBoard);
  }

  private async UniTask DrawUserInfoBlok()
  {
    await LocalizationSettings.InitializationOperation.Task;

    var stateGame = _gameManager.StateManager.stateGame;
    var dataGame = _gameManager.StateManager.dataGame;
    if (string.IsNullOrEmpty(dataGame.rank)) return;


    var blok = UserInfoDoc.Instantiate();

    var progressBlok = blok.Q<VisualElement>("ProgressBlok");

    var playerSetting = _gameManager.PlayerSetting;
    _gameManager.Progress.Refresh();

    var nameFile = blok.Q<Label>("NameFile");
    var titleFile = _gameManager.GameSettings.wordFiles.Find(t => t.locale.Identifier.Code == LocalizationSettings.SelectedLocale.Identifier.Code);
    nameFile.text = await Helpers.GetLocaledString(titleFile.text.title);


    var allPlayerSetting = _gameSetting.PlayerSettings.OrderBy(t => -t.countFindWordsForUp).First();

    if (_gameManager.StateManager.stateGame.rate >= allPlayerSetting.countFindWordsForUp)
    {
      progressBlok.Clear();
      Label newLabel = new Label()
      {
        text = await Helpers.GetLocaledString("endheadprogress"),
      };
      newLabel.AddToClassList("text-primary");
      newLabel.AddToClassList("text-wrap");
      newLabel.AddToClassList("text-xs");
      progressBlok.Add(newLabel);
    }
    else
    {

      var progress = blok.Q<VisualElement>("ProgressBar");
      progress.style.backgroundColor = new StyleColor(_gameManager.Theme.colorAccent);

      var status = blok.Q<Label>("Status");
      var foundWords = blok.Q<Label>("FoundWords");

      var percentFindWords = _gameManager.Progress.percent;
      progress.style.width = new StyleLength(new Length(percentFindWords, LengthUnit.Percent));

      string nameStatus = await Helpers.GetLocaledString(playerSetting.text.title);
      status.text = nameStatus;

      var textCountWords = await Helpers.GetLocalizedPluralString(
            "foundwords_dialog",
             new Dictionary<string, object> {
            {"name", nameStatus},
            {"count",  _gameManager.Progress.countNeedOpenWords},
            {"procent",  percentFindWords},
            }
          );
      foundWords.text = string.Format("{0}", textCountWords);

      // blok.Q<Label>("Rate").text = string.Format("{0}", stateGame.rate);
      // blok.Q<VisualElement>("RateImg").style.backgroundImage = new StyleBackground(_gameSettings.spriteRate);

      // var textCoin = await Helpers.GetLocalizedPluralString(
      //       "coin",
      //        new Dictionary<string, object> {
      //         {"count",  stateGame.coins},
      //       }
      //     );
      // blok.Q<Label>("Coin").text = string.Format("{0} <size=12>{1}</size>", dataGame.activeLevel.coins, textCoin);
      // var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
      // blok.Q<VisualElement>("CoinImg").style.backgroundImage = new StyleBackground(configCoin.sprite);

    }


    var _newGameButton = blok.Q<Button>("NewGameBtn");
    _newGameButton.clickable.clicked += () =>
    {
      ClickNewGameButton();
    };
    _newGameButton.style.display = DisplayStyle.None;

    var _loadGameMenuButton = blok.Q<Button>("LoadGameBtn");
    _loadGameMenuButton.clickable.clicked += () =>
    {
      ClickLoadGameButton();
    };
    _loadGameMenuButton.style.display = DisplayStyle.None;

    // _userInfoBlok.style.display = DisplayStyle.None;
    if (_gameManager.StateManager.dataGame.completed.Count == 0 && _gameManager.StateManager.dataGame.levels.Count == 0)
    {
      _newGameButton.style.display = DisplayStyle.Flex;
    }
    else
    // if (_gameManager.StateManager.dataGame.levels.Count != 0 || _gameManager.StateManager.dataGame.sl.Count != 0)
    {
      _loadGameMenuButton.style.display = DisplayStyle.Flex;
      // _userInfoBlok.style.display = DisplayStyle.Flex;
    }


    // Set short info user.
    var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);

    _userCoin = blok.Q<Label>("UserCoin");
    _userCoin.text = _gameManager.StateManager.stateGame.coins.ToString();
    var userCoinImg = blok.Q<VisualElement>("UserCoinImg");
    userCoinImg.style.backgroundImage = new StyleBackground(configCoin.sprite);

    _userRate = blok.Q<Label>("UserRate");
    _userRate.text = _gameManager.StateManager.stateGame.rate.ToString();
    var userRateImg = blok.Q<VisualElement>("UserRateImg");
    userRateImg.style.backgroundImage = new StyleBackground(_gameSetting.spriteRate);

    var userName = blok.Q<Label>("UserName");
    userName.text = await Helpers.GetName();

    userCoinImg.style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;
    userRateImg.style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;

    // load avatar
    string placeholder = _gameManager.AppInfo.UserInfo.photo;
    var imgAva = blok.Q<VisualElement>("Ava");
    Texture2D avatarTexture = await Helpers.LoadTexture(placeholder);
    if (avatarTexture != null)
    {
      imgAva.style.backgroundImage = new StyleBackground(avatarTexture);
    }
    else
    {
      imgAva.style.backgroundImage = new StyleBackground(_gameSetting.spriteUser);
      imgAva.style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;
    }

    _userInfoBlok.Clear();
    _userInfoBlok.Add(blok);
    base.Initialize(_userInfoBlok);

    nameFile.style.color = new StyleColor(_gameManager.Theme.colorAccent);
  }


  private async void ClickLoadGameButton()
  {
    AudioManager.Instance.Click();

    StateManager.OnChangeState -= SetValue;

    var operations = new Queue<ILoadingOperation>();
    operations.Enqueue(new GameInitOperation());
    await _gameManager.LoadingScreenProvider.LoadAndDestroy(operations);

    var activeLastWord = _gameManager.StateManager.dataGame.lastWord;

    _gameManager.LevelManager.InitLevel(activeLastWord);

    _gameManager.ChangeState(GameState.LoadLevel);

    _result.isOk = true;

    _processCompletionSource.SetResult(_result);
  }

  private async void ClickNewGameButton()
  {
    AudioManager.Instance.Click();

    StateManager.OnChangeState -= SetValue;

    await LocalizationSettings.InitializationOperation.Task;

    var operations = new Queue<ILoadingOperation>();
    operations.Enqueue(new GameInitOperation());
    await _gameManager.LoadingScreenProvider.LoadAndDestroy(operations);

    GameLevel firstLevel = _gameSetting.GameLevels.Where(t => t.locale.Identifier.Code == LocalizationSettings.SelectedLocale.Identifier.Code).OrderBy(t => t.minRate).FirstOrDefault();
    string activeLastLevelWord = "";
    if (firstLevel != null)
    {
      activeLastLevelWord = firstLevel.levelWords.ElementAt(0);
    }

    _gameManager.LevelManager.InitLevel(activeLastLevelWord);

    _gameManager.ChangeState(GameState.LoadLevel);

    _result.isOk = true;

    _processCompletionSource.SetResult(_result);
  }

  private void ClickExitButton()
  {
    AudioManager.Instance.Click();
    Debug.Log("ClickExitButton");
  }
}
