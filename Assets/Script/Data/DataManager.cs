using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using User;

public class DataManager : Singleton<DataManager>
{
  [DllImport("__Internal")]
  private static extern void SaveExtern(string data);
  [DllImport("__Internal")]
  private static extern void LoadExtern();
  [DllImport("__Internal")]
  private static extern void GetUserInfoExtern();
  [DllImport("__Internal")]
  public static extern void AddCoinsExtern(int value);

  public static event System.Action<int> OnAddCoins;
  public static event System.Action<StateGame> OnLoadData;
  public static event System.Action OnOpenCharExtern;
  public static event System.Action<ShopAdvBuyItem<TypeEntity>> OnAddHintExtern;
  public static event System.Action<ShopAdvBuyItem<TypeBonus>> OnAddBonusExtern;
  public static event System.Action<UserInfo> OnLoadUserInfo;
  public static event System.Action<LeaderBoard> OnLoadLeaderBoard;
  public LeaderBoard leaderBoard { get; private set; }

  [Header("File Storage Config")]
  [SerializeField] private string fileNameGame;
  [SerializeField] private string fileNameMap;
  [SerializeField] private bool useEncryption;
  private FileDataHandler _fileDataHandler;
  private CancellationTokenSource cancelTokenSource;
  private GameManager _gameManager => GameManager.Instance;
  [SerializeField] private StateGame _stateGame;
  public StateGame StateGame { get { return _stateGame; } }


  public void Init(string idDevice)
  {
    cancelTokenSource = new CancellationTokenSource();

    _fileDataHandler = new FileDataHandler(Application.persistentDataPath, idDevice, useEncryption);
  }

  public async UniTask<StateGame> Load()
  {
    _stateGame = await _fileDataHandler.LoadData();
    // Debug.Log($"{name}::: JSON ::: Load {JsonUtility.ToJson(_stateGame)}");

    PlayerPrefs.SetString("state", JsonUtility.ToJson(_stateGame));

    OnLoadData?.Invoke(_stateGame);
    return _stateGame;
  }


  public void LoadAsYsdk()
  {
    LoadExtern();
  }

  public StateGame SetPlayerData(string data)
  {
    _stateGame = JsonUtility.FromJson<StateGame>(data);
    // Debug.Log($"{name}::: YSDK ::: LoadPlayerData {JsonUtility.ToJson(_stateGame)}");

    PlayerPrefs.SetString("state", JsonUtility.ToJson(_stateGame));

    OnLoadData?.Invoke(_stateGame);
    return _stateGame;
  }

  public void LoadUserInfoAsYsdk()
  {
    GetUserInfoExtern();
  }


  public void AddCoins(int value)
  {
    OnAddCoins?.Invoke(value);
  }


  public void AddHint(string data)
  {
    ShopAdvBuyItem<TypeEntity> value = JsonUtility.FromJson<ShopAdvBuyItem<TypeEntity>>(data);
    OnAddHintExtern?.Invoke(value);
  }

  public void AddCoinsByAdv(int value)
  {
    AddCoinsExtern(value);
  }

  public void AddBonus(string data)
  {
    ShopAdvBuyItem<TypeBonus> value = JsonUtility.FromJson<ShopAdvBuyItem<TypeBonus>>(data);
    OnAddBonusExtern?.Invoke(value);
  }

  public void OpenChar()
  {
    OnOpenCharExtern?.Invoke();
  }

  public void SetUserInfo(string stringUserInfo)
  {
    UserInfo userInfo = JsonUtility.FromJson<UserInfo>(stringUserInfo);
    // Debug.Log($"{name}::: YSDK ::: SetUserInfo {stringUserInfo}");

    OnLoadUserInfo?.Invoke(userInfo);
  }


  public void GetLeaderBoard(string stringLeaderBoard)
  {
    LeaderBoard leaderBoard = JsonUtility.FromJson<LeaderBoard>(stringLeaderBoard);
    // Debug.Log($"{name}::: YSDK ::: GetLeaderBoard {stringLeaderBoard}");
    this.leaderBoard = leaderBoard;
    OnLoadLeaderBoard?.Invoke(leaderBoard);
  }

  public void Save(bool saveDb)
  {
    if (!cancelTokenSource.Token.IsCancellationRequested)
    {
      cancelTokenSource.Cancel();
      cancelTokenSource.Dispose();
    }
    cancelTokenSource = new CancellationTokenSource();
    Saved(saveDb, cancelTokenSource.Token).Forget();
  }

  private async UniTask Saved(bool saveDb, CancellationToken cancellationToken)
  {
    await UniTask.Delay(_gameManager.GameSettings.debounceTime);

    if (!cancellationToken.IsCancellationRequested)
    {
      _stateGame = _gameManager.StateManager.GetData();

      PlayerPrefs.SetString("state", JsonUtility.ToJson(_stateGame));

      Debug.Log("Save state completed!");

      if (saveDb)
      {
        SavedExt();
      }
    }
  }

  public void SavedExt()
  {
    if (_gameManager == null) return;

    _stateGame = _gameManager.StateManager.GetData();

#if android
    _fileDataHandler.SaveData(_stateGame);
#endif

    string jsonString = JsonUtility.ToJson(_stateGame);
#if ysdk
      SaveExtern(jsonString);
#endif
    Debug.Log("Save state to DB completed!");
  }

  protected override void OnApplicationQuit()
  {
    SavedExt();

    Debug.Log("Application ending after " + Time.time + " seconds");

    base.OnApplicationQuit();
  }
}
