using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class UISettings : UILocaleBase
{
  [SerializeField] private UIDocument _uiDoc;
  public static event Action OnChangeLocale;
  private VisualElement _root;
  private VisualElement _languageBlock;
  private Toggle _doDialog;
  private Button _closeButton;
  private Button _toMenuAppButton;
  private Button _okButton;
  private DropdownField _dropdownLanguage;
  private SliderInt _sliderTimeDelay;
  private Slider _sliderVolumeMusic;
  private Slider _sliderVolumeEffect;
  private DropdownField _dropdownTheme;

  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;

  public virtual void Start()
  {
    _root = _uiDoc.rootVisualElement.Q<VisualElement>("SettingsBlok");

    _languageBlock = _root.Q<VisualElement>("LanguageBlock");

    var menuBlok = _root.Q<VisualElement>("Menu");
    _dropdownLanguage = menuBlok.Q<DropdownField>("Language");
    _sliderTimeDelay = menuBlok.Q<SliderInt>("TimeDelay");
    _sliderVolumeMusic = menuBlok.Q<Slider>("VolumeMusic");
    _sliderVolumeEffect = menuBlok.Q<Slider>("VolumeEffect");
    _dropdownTheme = menuBlok.Q<DropdownField>("Theme");
    _doDialog = menuBlok.Q<Toggle>("DoDialog");

    _okButton = _root.Q<Button>("Ok");
    _closeButton = _root.Q<Button>("CloseMenuBtn");

    _okButton.clickable.clicked += () =>
    {
      CloseSettings();
    };

    _closeButton.clickable.clicked += () =>
    {
      CloseSettings();
    };

    _toMenuAppButton = _root.Q<Button>("ToStartMenuBtn");
    _toMenuAppButton.clickable.clicked += () =>
    {
      ClickToStartMenuButton();
    };
    if (_gameManager.LevelManager == null)
    {
      _toMenuAppButton.style.display = DisplayStyle.None;
    }
    else
    {
      _toMenuAppButton.style.display = DisplayStyle.Flex;
    }


    CreateListSettings();

    ChangeTheme(null);

    base.Initialize(_uiDoc.rootVisualElement);
  }


  public async UniTask<DataDialogResult> ProcessAction()
  {
    _result = new DataDialogResult();

    _processCompletionSource = new TaskCompletionSource<DataDialogResult>();

    return await _processCompletionSource.Task;
  }

  private void ChangeTheme(ChangeEvent<string> evt)
  {
    if (evt != null)
    {
      var allThemes = _gameManager.ResourceSystem.GetAllTheme();
      GameTheme chooseTheme = allThemes.Find(t => t.name == evt.newValue);

      _gameManager.SetTheme(chooseTheme);
      _gameManager.AppInfo.SaveSettings();
    }

    _root.Q<VisualElement>("MenuBlokWrapper").style.backgroundColor = new StyleColor(_gameManager.Theme.bgColor);

    base.Theming(_uiDoc.rootVisualElement);
  }

  private void ClickToStartMenuButton()
  {
    AudioManager.Instance.Click();

    GameManager.Instance.ChangeState(GameState.CloseLevel);


    _result.isOk = false;

    _processCompletionSource.SetResult(_result);

    var dialogDashboard = new UIDashboardOperation();
    dialogDashboard.ShowAndHide().Forget();
  }

  // private void ClickOpenMenuButton()
  // {
  //   AudioManager.Instance.Click();
  //   _gameManager.InputManager.Disable();
  //   ShowMenu();
  //   CreateMenu();
  // }

  private void CreateListSettings()
  {
    var userSettings = _gameManager.AppInfo.setting;

    _sliderVolumeEffect.value = userSettings.auv;
    _audioManager.EffectSource.volume = _sliderVolumeEffect.value;
    _sliderVolumeEffect.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
    {
      _audioManager.EffectSource.volume = evt.newValue;
      userSettings.auv = evt.newValue;
      _gameManager.AppInfo.SaveSettings();
    });

    _sliderVolumeMusic.value = userSettings.muv;
    _audioManager.MusicSource.volume = _sliderVolumeMusic.value;
    _sliderVolumeMusic.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
    {
      _audioManager.MusicSource.volume = evt.newValue;
      userSettings.muv = evt.newValue;
      _gameManager.AppInfo.SaveSettings();
    });

    _sliderTimeDelay.value = userSettings.td;
    _sliderTimeDelay.RegisterValueChangedCallback((ChangeEvent<int> evt) =>
    {
      userSettings.td = evt.newValue;
      _gameManager.AppInfo.SaveSettings();
    });

    _dropdownLanguage.value = LocalizationSettings.SelectedLocale.LocaleName;
    _dropdownLanguage.choices.Clear();
    for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
    {
      Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
      _dropdownLanguage.choices.Add(locale.LocaleName);
    }
    _dropdownLanguage.RegisterValueChangedCallback(ChooseLanguage);

    if (_gameManager.LevelManager != null)
    {
      _languageBlock.style.display = DisplayStyle.None;
    }
    else
    {
      _languageBlock.style.display = DisplayStyle.Flex;
    }


    // Theme.
    var allThemes = _gameManager.ResourceSystem.GetAllTheme();
    _dropdownTheme.choices.Clear();
    for (int i = 0; i < allThemes.Count; i++)
    {
      GameTheme theme = allThemes[i];
      _dropdownTheme.choices.Add(theme.name);
    }
    _dropdownTheme.value = userSettings.theme;
    _dropdownTheme.RegisterValueChangedCallback(ChangeTheme);

    // DoDialog.
    _doDialog.value = userSettings.dod;
    _doDialog.RegisterValueChangedCallback(ChangeDoDialog);
  }

  private void ChangeDoDialog(ChangeEvent<bool> evt)
  {
    var userSettings = _gameManager.AppInfo.setting;
    userSettings.dod = evt.newValue;
    _gameManager.AppInfo.SaveSettings();
  }

  private async void ChooseLanguage(ChangeEvent<string> evt)
  {
    string nameLanguage = evt.newValue;

    var userSettings = _gameManager.AppInfo.setting;

    await LocalizationSettings.InitializationOperation.Task;

    // Debug.Log($"Choose lang={nameLanguage} | {selectedLocale} | {selectedLocale == nameLanguage}");
    Locale currentLocale = LocalizationSettings.AvailableLocales.Locales.Find(t => t.LocaleName == nameLanguage);
    if (currentLocale.Identifier.Code != LocalizationSettings.SelectedLocale.Identifier.Code)
    {
      LocalizationSettings.SelectedLocale = currentLocale;//LocalizationSettings.AvailableLocales.Locales[indexLocale];
      userSettings.lang = currentLocale.Identifier.Code;
      _gameManager.AppInfo.SaveSettings();
      base.Initialize(_uiDoc.rootVisualElement);
      // Words words = _gameManager.WordsAll.Find(t => t.locale.Identifier.Code == LocalizationSettings.SelectedLocale.Identifier.Code);
      await _gameManager.SetActiveWords();

      await _gameManager.StateManager.SetActiveDataGame();

      // await UniTask.Delay(500);
    }


    OnChangeLocale?.Invoke();
  }

  private void CloseSettings()
  {
    AudioManager.Instance.Click();

    _result.isOk = true;

    _processCompletionSource.SetResult(_result);

    // _gameManager.InputManager.Enable();
  }
}
