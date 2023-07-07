using System;
using UnityEngine;
using User;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections.Generic;
using Loader;
using System.Linq;
using UnityEngine.Localization.Settings;
using Cysharp.Threading.Tasks;

public class GameManager : StaticInstance<GameManager>
{
  public static event Action OnChangeTheme;
  public static event Action<GameState> OnBeforeStateChanged;
  public static event Action<GameState> OnAfterStateChanged;
  [SerializeField] private string namePlayPref;
  private string langCodePlayPref;
  public string KeyPlayPref => string.Format("{0}_{1}", namePlayPref, langCodePlayPref);
  public AppInfoContainer AppInfo { get; private set; }
  public GameSetting GameSettings;
  public GameTheme Theme { get; private set; }
  public AudioManager audioManager { get; private set; }
  public DataManager DataManager { get; private set; }
  public StateManager StateManager;

  [HideInInspector] public List<Words> WordsAll { get; private set; }
  [HideInInspector] public Words Words { get; private set; }
  public LoadingScreenProvider LoadingScreenProvider { get; private set; }
  public InitUserProvider InitUserProvider { get; private set; }
  public AssetProvider AssetProvider { get; private set; }
  public GameState State { get; private set; }
  public AdManager AdManager;
  public ResourceSystem ResourceSystem { get; internal set; }

  public LevelManager LevelManager { get; private set; }
  [HideInInspector] public GamePlayerSetting PlayerSetting { get; private set; }

  public SceneInstance environment { get; private set; }
  public InputManager InputManager { get; private set; }

  [SerializeField] private ProgressManager _progressManager;
  public ProgressManager Progress => _progressManager;

  protected override void Awake()
  {
    base.Awake();
  }

  // private void OnDestroy()
  // {
  //   DataManager.OnLoadData -= InitPlayerInfo;
  // }

  void Start()
  {
#if UNITY_EDITOR
    Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

    // ChangeState(GameState.StartApp);

    AppInfo = new AppInfoContainer();

    Theme = GameSettings.ThemeDefault;

    LoadingScreenProvider = new LoadingScreenProvider();

    InitUserProvider = new InitUserProvider();

    AssetProvider = new AssetProvider();

    InputManager = new();

    DataManager = DataManager.Instance;

    audioManager = AudioManager.Instance;
  }

  public void ChangeState(GameState newState, object Params = null)
  {
    OnBeforeStateChanged?.Invoke(newState);

    State = newState;
    switch (newState)
    {
      case GameState.StartApp:
        HandleStartApp();
        break;
      case GameState.CreateGame:
        HandleCreateGame();
        break;
      case GameState.RunLevel:
        HandleRunLevel();
        break;
      case GameState.CloseLevel:
        HandleCloseLevel();
        break;
      case GameState.StopEffect:
        break;
      case GameState.StartEffect:
        break;
      case GameState.ShowMenu:
        break;
      case GameState.LoadLevel:
        // HandleLoadLevel();
        break;
      default:
        // throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        break;
    }

    OnAfterStateChanged?.Invoke(newState);
  }

  private void HandleStartApp()
  {
    ChangeTheme();

  }

  public void SetDefaultState()
  {
    var appInfo = new AppInfoContainer();
  }

  private void ChangeTheme()
  {
    Camera.main.backgroundColor = Theme.bgColor;
  }

  private async void HandleRunLevel()
  {
    await AssetProvider.UnloadAdditiveScene(environment);
  }
  private async void HandleCloseLevel()
  {
    LevelManager = null;
    await AssetProvider.UnloadAdditiveScene(environment);
  }
  private async void HandleCreateGame()
  {
    var operations = new Queue<ILoadingOperation>();
    operations.Enqueue(new GameInitOperation());
    await LoadingScreenProvider.LoadAndDestroy(operations);
    // LevelManager.CreateLevel();
  }
  // private async void HandleLoadLevel()
  // {
  //   // var operations = new Queue<ILoadingOperation>();
  //   // operations.Enqueue(new GameInitOperation());
  //   // await LoadingScreenProvider.LoadAndDestroy(operations);
  //   // //var dataGame = DataManager.Load();
  //   // // LevelManager.LoadLevel();

  // }


  public void SetTheme(GameTheme newTheme)
  {
    if (newTheme == null) newTheme = GameSettings.ThemeDefault;

    if (AppInfo != null && AppInfo.setting != null)
    {
      AppInfo.setting.theme = newTheme.name;

      Theme = newTheme;

      ChangeTheme();

      // DataManager.Save();

      OnChangeTheme?.Invoke();
    }
  }

  public void InitGameLevel(LevelManager levelManager, SceneInstance environment)
  {
    this.LevelManager = levelManager;
    this.environment = environment;
  }

  public void InitWords(List<Words> words)
  {
    this.WordsAll = words;
  }

  public async UniTask SetAppInfo(AppInfoContainer dataInfo)
  {
    AppInfo = dataInfo;


    // Locale needSetLocale = LocalizationSettings.AvailableLocales.Locales.Find(t => t.Identifier.Code == AppInfo.setting.lang);
    // if (AppInfo.setting.lang != LocalizationSettings.SelectedLocale.Identifier.Code)
    // {
    //   LocalizationSettings.SelectedLocale = needSetLocale;
    // }

    await SetActiveWords();

    // Set theme.
    List<GameTheme> allThemes = ResourceSystem.GetAllTheme();
    GameTheme userTheme = allThemes.Where(t => t.name == AppInfo.setting.theme).FirstOrDefault();
    SetTheme(userTheme);

    AppInfo.SaveSettings();

    await UniTask.Yield();
  }

  public async UniTask SetActiveWords()
  {
    // set language.
    await LocalizationSettings.InitializationOperation.Task;
    Words words = WordsAll.Find(t => t.locale.Identifier.Code == LocalizationSettings.SelectedLocale.Identifier.Code);

    Words = words;
    Debug.Log($"Set words for {Words.locale.Identifier.Code} |<{words.data.Count()}> |[{Words.localeCode}] [{LocalizationSettings.SelectedLocale.Identifier.Code}]");
  }

  public void SetLangCodePlayPref(string code)
  {
    langCodePlayPref = code;
    Debug.Log($"Init key playpref as {code}");
  }

  public void SetPlayerSetting(GamePlayerSetting playerSetting)
  {
    Debug.Log($"Set player info {playerSetting.idPlayerSetting}");
    PlayerSetting = playerSetting;

    Progress.Refresh();

    Debug.LogWarning(Progress.ToString());
  }
}

[Serializable]
public enum GameState
{
  StartApp = 0,
  CreateGame = 1,
  StartEffect = 2,
  StopEffect = 3,
  ShowMenu = 4,
  RunLevel = 5,
  CloseLevel = 6,
  LoadLevel = 7,
}