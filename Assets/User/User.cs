using System.Collections.Generic;
using UnityEngine;

namespace User
{
  [System.Serializable]
  public class AppInfoContainer
  {
    private GameManager _gameManager => GameManager.Instance;
    private GameSetting _gameSetting => GameManager.Instance.GameSettings;
    private StateManager _stateManager => GameManager.Instance.StateManager;
    public string uid;
    public UserInfo UserInfo;
    public UserSettings setting;
    public List<string> helps;

    public AppInfoContainer()
    {
      UserInfo = new UserInfo();
      setting = new();
      helps = new();
    }

    public void AddHelpItem(string item)
    {
      helps.Add(item);
      SaveSettings();
    }

    public void SaveSettings()
    {
      string appInfo = JsonUtility.ToJson(_gameManager.AppInfo);

      string namePlayPref = _gameManager.KeyPlayPref;

      PlayerPrefs.SetString(namePlayPref, appInfo);
      // Debug.Log($"SaveSettings::: appInfo=[{appInfo}");
    }

    public override string ToString()
    {
      return string.Format(
        "name={0}\r\nDeviceId={1}",
        UserInfo.name,
        uid
        );
    }
  }

  [System.Serializable]
  public class UserInfo
  {
    // public string Id { get; set; }
    // public string Name { get; set; }
    // public string Lang { get; set; }
    public string uid;
    public string name;
    public string photo;
    // public string userId { get; set; }
    public UserInfoAuth UserInfoAuth;
    public UserInfo()
    {

      // UserInfoAuth = new UserInfoAuth();
    }

  }

  [System.Serializable]
  public class UserInfoAuth
  {
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
  }


  [System.Serializable]
  public class UserSettings
  {
    public string lang;
    public float muv;
    public float auv;
    public int td;
    public string theme;
    public bool dod;
  }

  // [System.Serializable]
  // public class HelpItem
  // {
  //   public string key;
  //   public bool status;
  // }
}