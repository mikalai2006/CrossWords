using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Loader;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace User
{
  public class InitUserOperation : ILoadingOperation
  {
    private AppInfoContainer _playPrefData;
    private TaskCompletionSource<UserInfo> _getUserInfoCompletionSource;
    private TaskCompletionSource<StateGame> _getUserDataCompletionSource;
    private TaskCompletionSource<UserSettings> _getUserSettingCompletionSource;
    private Action<float> _onProgress;
    private Action<string> _onSetNotify;

    // public InitUserOperation(AppInfoContainer appInfoContainer)
    // {
    //   _appInfoContainer = appInfoContainer;
    // }

    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {
      var _gameManager = GameManager.Instance;

      _playPrefData = new();

      _onProgress = onProgress;
      _onSetNotify = onSetNotify;

      // var t = await Helpers.GetLocaledString("loading");
      _onSetNotify?.Invoke("...");
      _onProgress?.Invoke(0.3f);

      string namePlaypref = GameManager.Instance.KeyPlayPref;

      GameManager.Instance.DataManager.Init(namePlaypref);

      if (PlayerPrefs.HasKey(namePlaypref))
      {
        _playPrefData = JsonUtility.FromJson<AppInfoContainer>(PlayerPrefs.GetString(namePlaypref));
      }
      else
      {
        _playPrefData.uid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); //DeviceInfo.GetDeviceId();

        await LocalizationSettings.InitializationOperation.Task;
        var setting = new UserSettings()
        {
          auv = _gameManager.GameSettings.Audio.volumeEffect,
          lang = LocalizationSettings.SelectedLocale.name,
          muv = _gameManager.GameSettings.Audio.volumeMusic,
          theme = _gameManager.GameSettings.ThemeDefault.name,
          dod = true,
          td = _gameManager.GameSettings.timeDelayOverChar // time delay
        };
        _playPrefData.setting = setting;
      }

      // Get user info.
      DataManager.OnLoadUserInfo += SetUserInfo;
      _playPrefData.UserInfo = await GetUserInfo();
      DataManager.OnLoadUserInfo -= SetUserInfo;

      // Get game data.
      DataManager.OnLoadData += InitData;
      var dataGame = await GetUserData();
      DataManager.OnLoadData -= InitData;

      await GameManager.Instance.SetAppInfo(_playPrefData);
      // Set user setting to PlayPref.
      // string jsonData = JsonUtility.ToJson(_playPrefData);
      // PlayerPrefs.SetString(namePlaypref, jsonData);


      _onProgress?.Invoke(.5f);
    }


    private void SetUserInfo(UserInfo userInfo)
    {
      _playPrefData.UserInfo = userInfo;
      _getUserInfoCompletionSource.SetResult(userInfo);
    }


    public async Task<UserInfo> GetUserInfo()
    {
      _getUserInfoCompletionSource = new TaskCompletionSource<UserInfo>();

#if android
      UserInfo userInfo = _playPrefData.UserInfo;
      if (string.IsNullOrEmpty(userInfo.name))
      {
        // Load form for input name.
        var result = await GameManager.Instance.InitUserProvider.ShowAndHide();
        userInfo = result.UserInfo;
      }
      SetUserInfo(userInfo);
#endif

#if ysdk
      GameManager.Instance.DataManager.LoadUserInfoAsYsdk();
#endif

      return await _getUserInfoCompletionSource.Task;
    }


    private async UniTask<StateGame> GetUserData()
    {
      _getUserDataCompletionSource = new();

      //AppInfoContainer result = null;

#if android
      StateGame stateGame;
      stateGame = await GameManager.Instance.DataManager.Load();
      // InitData(dataGame);
#endif

#if ysdk
      GameManager.Instance.DataManager.LoadAsYsdk();
#endif

      return await _getUserDataCompletionSource.Task;
    }

    private async void InitData(StateGame stateGame)
    {
      var _gameManager = GameManager.Instance;

      stateGame = await _gameManager.StateManager.Init(stateGame);

      // GamePlayerSetting playerSetting;
      // DataGame dataGame;
      // StateGameItem stateGameItemCurrentLang = stateGame != null && stateGame.items.Count > 0
      //   ? stateGame.items.Find(t => t.lang == codeCurrentLang)
      //   : null;

      // // init new state.
      // if (stateGame == null || stateGame.items.Count == 0 || stateGameItemCurrentLang == null) // string.IsNullOrEmpty(dataGame.rank)
      // {
      //   stateGame = new();

      //   playerSetting = _gameManager.GameSettings.PlayerSetting
      //     .OrderBy(t => t.countFindWordsForUp)
      //     .First();

      //   dataGame = new DataGame()
      //   {
      //     rank = playerSetting.idPlayerSetting
      //   };

      //   var newStateGame = new StateGameItem()
      //   {
      //     dataGame = dataGame,
      //     lang = LocalizationSettings.SelectedLocale.Identifier.Code,
      //   };

      //   stateGame.items.Add(newStateGame);
      // }
      // else
      // {
      //   dataGame = stateGame.items.Find(t => t.lang == codeCurrentLang).dataGame;
      //   playerSetting = _gameManager.GameSettings.PlayerSetting.Find(t => t.idPlayerSetting == dataGame.rank);
      // }

      // Debug.Log("InitData2");
      // _gameManager.PlayerSetting = playerSetting;

      _getUserDataCompletionSource.SetResult(stateGame);
    }

  }
}