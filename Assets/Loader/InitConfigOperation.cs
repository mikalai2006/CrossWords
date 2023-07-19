using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using User;

namespace Loader
{
  public class InitConfigOperation : ILoadingOperation
  {
    [DllImport("__Internal")]
    private static extern string GetLang();

    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {

      await LocalizationSettings.InitializationOperation.Task;

      string langString = "";

#if ysdk
      langString = GetLang();
#endif

      AppInfoContainer playPrefData = new();

      GameManager.Instance.SetLangCodePlayPref(langString);

      string namePlayPref = GameManager.Instance.KeyPlayPref;

      if (PlayerPrefs.HasKey(namePlayPref))
      {
        playPrefData = JsonUtility.FromJson<AppInfoContainer>(PlayerPrefs.GetString(namePlayPref));
        langString = playPrefData.setting.lang;
      }

      if (!string.IsNullOrEmpty(langString))
      {
        Locale needSetLocale = LocalizationSettings.AvailableLocales.Locales.Find(t => t.Identifier.Code == langString);
        if (langString != LocalizationSettings.SelectedLocale.Identifier.Code)
        {
          LocalizationSettings.SelectedLocale = needSetLocale;
          Debug.Log($"needSetLocale={needSetLocale}");
        }
      }

      string t = await Helpers.GetLocaledString("loading");
      onSetNotify?.Invoke(t);

      onProgress?.Invoke(0.1f);
      GameManager.Instance.ResourceSystem = ResourceSystem.Instance;
      await ResourceSystem.Instance.LoadCollectionsAsset<GameEntity>(Constants.Labels.LABEL_ENTITY);

      GameManager.Instance.ResourceSystem = ResourceSystem.Instance;
      await ResourceSystem.Instance.LoadCollectionsAsset<GameBonus>(Constants.Labels.LABEL_BONUS);

      onProgress?.Invoke(0.2f);
      GameManager.Instance.ResourceSystem = ResourceSystem.Instance;
      await ResourceSystem.Instance.LoadCollectionsAsset<GameTheme>(Constants.Labels.LABEL_THEME);

      // Set theme.
      if (PlayerPrefs.HasKey(namePlayPref))
      {
        List<GameTheme> allThemes = ResourceSystem.Instance.GetAllTheme();
        GameTheme userTheme = allThemes.Where(t => t.name == playPrefData.setting.theme).FirstOrDefault();
        GameManager.Instance.SetTheme(userTheme);
      }
    }
  }
}