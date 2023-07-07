using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UIDirectory : UILocaleBase
{
  [SerializeField] private UIDocument _uiDoc;
  [SerializeField] private VisualTreeAsset _directoryItem;
  [SerializeField] private VisualElement _root;
  // private GameObject _enviromnment;
  [SerializeField] private ScrollView _listItems;
  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;
  private string _activeWord;

  private void Awake()
  {
    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  public async void Start()
  {
    _root = _uiDoc.rootVisualElement;

    var exitBtn = _root.Q<Button>("ExitBtn");
    exitBtn.clickable.clicked += () => ClickClose();

    _listItems = _root.Q<ScrollView>("ListItems");

    FillItems();

    // Text total find.
    var textCountWords = await Helpers.GetLocalizedPluralString(
      "foundcountwordtotal",
        new Dictionary<string, object> {
        {"count", _gameManager.LevelManager.ManagerHiddenWords.OpenWords.Count},
        {"count2", _gameManager.LevelManager.ManagerHiddenWords.AllowPotentialWords.Count},
      }
    );
    _root.Q<Label>("TotalText").text = textCountWords;

    ChangeTheme();
  }

  private void ChangeTheme()
  {
    _root.Q<VisualElement>("DirectoryBlokWrapper").style.backgroundColor = new StyleColor(_gameManager.Theme.bgColor);

    FillItems();

    base.Initialize(_root);
  }

  private void Hide()
  {
    _root.style.display = DisplayStyle.None;
  }

  private void FillItems()
  {
    _listItems.Clear();

    var dataGame = _gameManager.StateManager.dataGame;
    List<string> reverse = Enumerable.Reverse(dataGame.activeLevel.openWords).ToList();
    foreach (var item in reverse)
    {
      var blokItem = _directoryItem.Instantiate();
      blokItem.Q<VisualElement>("DirectoryItem").style.backgroundColor = _gameManager.Theme.bgColor;
      blokItem.Q<Label>("Word").text = item;

      // Button request.
      var buttonGoRead = blokItem.Q<Button>("GoRead");
      // buttonGoRead.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(_gameSettings.spriteBuy);
      buttonGoRead.clickable.clicked += async () =>
        {
          AudioManager.Instance.Click();

          // TODO Request.
          _activeWord = item;
          await GoRequest(item);

        };

      _listItems.Add(blokItem);
    }
  }

  private async UniTask<string> GoRequest(string word)
  {
    await LocalizationSettings.InitializationOperation.Task;

    string langForRequest = LocalizationSettings.SelectedLocale.Identifier.Code;
    if (string.IsNullOrEmpty(langForRequest))
    {
      langForRequest = "ru";
    }

    string path = string.Format(
      _gameSetting.APIDirectory.pathExpression,
      langForRequest,
      word
    );
    UnityWebRequest webRequest = UnityWebRequest.Get(path);
    // webRequest.SetRequestHeader("Authorization", "Basic " + _gameSettings.APIDirectory.token);
    webRequest.SetRequestHeader("Content-Type", "application/json");
    // webRequest.SetRequestHeader("Access-Control-Allow-Credentials", "true");
    // webRequest.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
    // webRequest.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
    // webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");

    await UniTask.Yield();

    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      var message = await Helpers.GetLocaledString("disconnect");
      var dialog = new DialogProvider(new DataDialog()
      {
        message = message
      });
      await dialog.ShowAndHide();
      return "";
    }

    try
    {
      await webRequest.SendWebRequest();

      var responseText = JsonUtility.FromJson<APIDirectoryResponse>(webRequest.downloadHandler.text);

      if (!string.IsNullOrEmpty(responseText.extract))
      {
        var message = Helpers.StripHTML(responseText.extract);
        var dialog = new DialogProvider(new DataDialog()
        {
          title = _activeWord,
          message = message,
          showCancelButton = false
        });
        await dialog.ShowAndHide();
      }
      else
      {
        var message = await Helpers.GetLocaledString("nofoundword");
        var dialog = new DialogProvider(new DataDialog()
        {
          message = message,
          showCancelButton = false
        });
        await dialog.ShowAndHide();
      }

      return webRequest.downloadHandler.text;

    }
    catch (System.Exception error)
    {
      Debug.Log(error);
      if (webRequest.result == UnityWebRequest.Result.ConnectionError
            || webRequest.result == UnityWebRequest.Result.DataProcessingError
            || webRequest.result == UnityWebRequest.Result.ProtocolError)
      {
        var message = await Helpers.GetLocaledString("request_error");
        var dialog = new DialogProvider(new DataDialog()
        {
          message = message,
          showCancelButton = false
        });
        await dialog.ShowAndHide();

        return webRequest.error;
      }
      return webRequest.downloadHandler.error;
    }
  }

  private void ClickClose()
  {
    AudioManager.Instance.Click();
    _result.isOk = false;
    _processCompletionSource.SetResult(_result);
  }

  public async UniTask<DataDialogResult> ProcessAction()
  {

    _processCompletionSource = new TaskCompletionSource<DataDialogResult>();

    return await _processCompletionSource.Task;
  }
}

[System.Serializable]
public struct APIDirectoryResponse
{
  public string extract;
  public string extract_html;
  public int pageid;
}
