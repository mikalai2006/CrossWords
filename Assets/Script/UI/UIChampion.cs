using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class UIChampion : UIBase
{
  [DllImport("__Internal")]
  private static extern void GetLeaderBoard();
  [SerializeField] private VisualTreeAsset UserInfoDoc;
  [SerializeField] private VisualTreeAsset LeaderDoc;
  private VisualElement _userInfoBlok;
  private Label _userCoin;
  private Label _userRate;
  private VisualElement _leaderBoard;
  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;

  public override async void Start()
  {
    base.Start();

    Title.text = await Helpers.GetLocaledString("achievements");

    CloseButton.clickable.clicked += () =>
    {
      ClickExitButton();
    };

    _userInfoBlok = Wrapper.Q<VisualElement>("UserInfoBlok");
    _leaderBoard = Wrapper.Q<VisualElement>("LeaderBoard");
    _leaderBoard.style.display = DisplayStyle.None;

    base.Initialize(Wrapper);

    await DrawUserInfoBlok();

#if UNITY_EDITOR
    _gameManager.DataManager.GetLeaderBoard("{\"leaderboard\":{\"title\":[{\"lang\":\"ru\",\"value\":\"Лидеры по количеству слов\"}]},\"userRank\":25,\"entries\":[{\"rank\":24,\"score\":90,\"name\":\"Tamara Ivanovna Semenovatoreva\",\"lang\":\"ru\",\"photo\":\"\"},{\"rank\":25,\"score\":80,\"name\":\"Mikalai P.2\",\"lang\":\"ru\",\"photo\":\"https://games-sdk.yandex.ru/games/api/sdk/v1/player/avatar/66VOVRVF2GJAXS5VWT3X54YATTEZAJLGXTPIXJTG3465T5HXLNQFMZIOJ7WYALX2PEC2DIAHLM6FC7ABRLOA27IRF55DP6DXJU7JDS4IFW63KJWT4IFLT2I26N44GVCAAX6FGHPPVKQY65KZZOXXYODUUKJMK2Y25M2VUDFYRPJDR3TS4JVBUOZNWFE2QNABMFRQEVLJRRIODYNB2JKIIK76YMZEEA3VQHV3M6Q=/islands-retina-medium\"},{\"rank\":26,\"score\":70,\"name\":\"Mikalai P.3\",\"lang\":\"ru\",\"photo\":\"https://games-sdk.yandex.ru/games/api/sdk/v1/player/avatar/66VOVRVF2GJAXS5VWT3X54YATTEZAJLGXTPIXJTG3465T5HXLNQFMZIOJ7WYALX2PEC2DIAHLM6FC7ABRLOA27IRF55DP6DXJU7JDS4IFW63KJWT4IFLT2I26N44GVCAAX6FGHPPVKQY65KZZOXXYODUUKJMK2Y25M2VUDFYRPJDR3TS4JVBUOZNWFE2QNABMFRQEVLJRRIODYNB2JKIIK76YMZEEA3VQHV3M6Q=/islands-retina-medium\"}]}");
#endif
#if ysdk
        GetLeaderBoard();
#endif
    await DrawLeaderListBlok(_gameManager.DataManager.leaderBoard);
  }

  public async UniTask<DataDialogResult> ProcessAction()
  {
    _result = new DataDialogResult();

    _processCompletionSource = new TaskCompletionSource<DataDialogResult>();

    return await _processCompletionSource.Task;
  }

  private async UniTask DrawLeaderListBlok(LeaderBoard board)
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

    var playerSetting = _gameManager.PlayerSetting;
    _gameManager.Progress.Refresh();

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
  }

  private void ClickExitButton()
  {
    AudioManager.Instance.Click();

    _result.isOk = true;

    _processCompletionSource.SetResult(_result);
  }
}
