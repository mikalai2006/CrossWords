using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using User;
using UnityEngine.Events;

public class InitUserWindow : UILocaleBase
{
  [SerializeField] private UIDocument _uiDoc;
  public UIDocument MenuApp => _uiDoc;

  private readonly string _nameFieldLogin = "Login";
  private readonly string _nameFieldPassword = "Password";
  private readonly string _nameButtonLogin = "ButtonLogin";

  private const int MIN_LENGTH_NAME = 3;
  // [SerializeField] private string _urlLogin = "https://storydata.ru/api/v1/auth";
  private TextField _fieldName;
  private TextField _fieldPassword;
  private Button _buttonLogin;
  private VisualElement _form;
  private AppInfoContainer _result = new();

  private TaskCompletionSource<AppInfoContainer> _loginCompletionSource;


  public UnityEvent loginAction;

  private void Awake()
  {
    _form = MenuApp.rootVisualElement.Q<VisualElement>("Form");

    _fieldName = MenuApp.rootVisualElement.Q<TextField>(_nameFieldLogin);
    _fieldName.RegisterCallback<InputEvent>(e =>
    {
      OnValidFormField();
    });

    _fieldPassword = MenuApp.rootVisualElement.Q<TextField>(_nameFieldPassword);
    _buttonLogin = MenuApp.rootVisualElement.Q<Button>(_nameButtonLogin);
    _buttonLogin.clickable.clicked += () =>
    {
      _buttonLogin.SetEnabled(false);
      OnSimpleLoginClicked();
      _buttonLogin.SetEnabled(true);
    };

    OnValidFormField();
    // _form.style.display = DisplayStyle.None;

    base.Initialize(_uiDoc.rootVisualElement);

  }

  public async Task<AppInfoContainer> ProcessLogin()
  {
    _loginCompletionSource = new TaskCompletionSource<AppInfoContainer>();

    return await _loginCompletionSource.Task;
  }

  private void OnValidFormField()
  {
    if (_fieldName.text.Length < MIN_LENGTH_NAME)
    {
      _buttonLogin.SetEnabled(false);
    }
    else
    {
      _buttonLogin.SetEnabled(true);
    }
  }

  private void OnSimpleLoginClicked()
  {

    if (_fieldName.text.Length < MIN_LENGTH_NAME || _fieldPassword.text == "")
    {
      return;
    }

    _result.UserInfo.name = _fieldName.text;

    _loginCompletionSource.SetResult(_result);

    loginAction?.Invoke();
  }
  // private void LoginAsDeviceId()
  // {
  //   string deviceId = DeviceInfo.GetDeviceId();
  //   _loginCompletionSource.SetResult(new AppInfoContainer()
  //   {
  //     DeviceId = deviceId
  //   });

  //   loginAction?.Invoke();
  // }

  // private async Task<string> GetUserInfo(string token)
  // {
  //   UnityWebRequest webRequest = UnityWebRequest.Get(_urlLogin + "/iam");
  //   webRequest.SetRequestHeader("Authorization", "Basic " + token);
  //   webRequest.SetRequestHeader("Content-Type", "application/json");

  //   await UniTask.Yield();
  //   await webRequest.SendWebRequest();

  //   // while (!webRequest.isDone)
  //   // {
  //   //   await Task.Yield();
  //   // }

  //   if (webRequest.result == UnityWebRequest.Result.ConnectionError)
  //   {
  //     Debug.Log("Error while sending" + webRequest.error);
  //     return webRequest.error;
  //   }
  //   else
  //   {
  //     return webRequest.downloadHandler.text;
  //   }
  // }
  // private async Task<string> AsyncLogin()
  // {
  //   DataLogin data = new DataLogin()
  //   {
  //     login = _fieldName.text,
  //     password = _fieldPassword.text
  //   };

  //   string jsonBody = JsonUtility.ToJson(data);

  //   byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

  //   UnityWebRequest request = new UnityWebRequest();
  //   request.url = _urlLogin + "/sign-in";
  //   request.method = UnityWebRequest.kHttpVerbPOST;
  //   request.useHttpContinue = false;

  //   Debug.Log("Send POST to path: " + request.uri.OriginalString);
  //   request.uploadHandler = new UploadHandlerRaw(rawBody);
  //   request.downloadHandler = new DownloadHandlerBuffer();
  //   // request.SetRequestHeader("Authorization", "Basic " + GetAuthenticationKey());
  //   request.SetRequestHeader("Content-Type", "application/json");
  //   request.SetRequestHeader("Accept", "*/*");

  //   await Task.Yield();
  //   await request.SendWebRequest();

  //   if (request.result == UnityWebRequest.Result.ConnectionError)
  //   {
  //     Debug.Log("Error while sending" + request.error);
  //     return request.error;
  //   }
  //   else
  //   {
  //     return request.downloadHandler.text;
  //   }
  // }



  // private void OnFacebookLoginClicked()
  // {
  //   //TODO implement later
  // }
}

