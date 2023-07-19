using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharHiddenMB : MonoBehaviour //, IPointerDownHandler
{
  [DllImport("__Internal")]
  private static extern void OpenCharExtern();
  [SerializeField] private TMPro.TextMeshProUGUI _charText;
  private GameManager _gameManager => GameManager.Instance;
  private LevelManager _levelManager => LevelManager.Instance;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  [SerializeField] private Image _image;
  private Vector3 _initScale;
  [SerializeField] private RectTransform _canvas;
  private InputManager _inputManager;
  private Camera _camera;
  private CharHidden _charInstance;

  #region Unity methods
  private void OnEnable()
  {
    _inputManager = new InputManager();
    _inputManager.ClickChar += OnClick;
  }

  private void OnDisable()
  {
    _inputManager.ClickChar -= OnClick;
  }

  public void Awake()
  {

    _camera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();

    _initScale = transform.localScale;

    SetDefault();

    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
  }
  #endregion

  public void Init(CharHidden charInstance)
  {
    _charInstance = charInstance;

    _charText.text = _charInstance.CharValue;
  }

  public void ChangeTheme()
  {
    _image.color = _gameManager.Theme.bgHiddenWord;
    _image.sprite = _gameManager.Theme.bgImageHiddenWord;

    if (_charInstance.OccupiedNode == null) return;
    // if (!OccupiedNode.StateNode.HasFlag(StateNode.Open)) return;

    if (_charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Open))
    {
      _image.color = _gameManager.Theme.bgOpentHiddenWord;
      _image.sprite = _gameManager.Theme.bgImageHiddenWord;
      _charText.color = _gameManager.Theme.textOpentHiddenWord;
    }
    else if (_charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Hint))
    {
      _image.color = _gameManager.Theme.bgOpenNeiHiddenWord;
      _charText.color = _gameManager.Theme.textOpenNeiHiddenWord;
    }
  }

  public void SetChar(char currentChar)
  {
    ChangeTheme();
    _charText.text = currentChar.ToString();
    // charTextValue = currentChar;
  }


  private void SetDefault()
  {
    _image.color = _gameManager.Theme.bgHiddenWord;
    _image.sprite = _gameManager.Theme.bgImageHiddenWord;
    _charText.gameObject.SetActive(false);
  }


  // public void RunOpenEffect()
  // {
  //   var _CachedSystem = GameObject.Instantiate(
  //     _gameSetting.Boom,
  //     transform.position,
  //     Quaternion.identity
  //   );

  //   var main = _CachedSystem.main;
  //   // main.startColor = new ParticleSystem.MinMaxGradient(_gameSetting.Theme.bgHiddenWord, _gameSetting.Theme.textFindHiddenWord);
  //   main.startSize = new ParticleSystem.MinMaxCurve(0.05f, _levelManager.ManagerHiddenWords.scaleGrid / 2);

  //   var col = _CachedSystem.colorOverLifetime;
  //   col.enabled = true;

  //   Gradient grad = new Gradient();
  //   grad.SetKeys(new GradientColorKey[] {
  //     new GradientColorKey(_gameManager.Theme.bgHiddenWord, 1.0f),
  //     new GradientColorKey(_gameManager.Theme.bgHiddenWord, 0.0f)
  //     }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f)
  //   });

  //   col.color = grad;

  //   _CachedSystem.Play();

  //   Destroy(_CachedSystem.gameObject, 2f);
  // }


  public async UniTask ShowChar(bool runEffect, string _char)
  {
    Vector3 initialScale = transform.localScale;

    int valueBonusSaveHintLetter;
    _stateManager.dataGame.bonus.TryGetValue(TypeBonus.SaveHintLetter, out valueBonusSaveHintLetter);

    // Add coin.
    if (
      runEffect
      && (
        !_charInstance.isHinted
        || valueBonusSaveHintLetter > 0
      )
    && !_charInstance.isOpen
    )
    {
      // play sound.
      _gameManager.audioManager.PlayClipEffect(_gameSetting.Audio.openHiddenChar);

      // _levelManager.CreateCoin(
      //   transform.position,
      //   _levelManager.topSide.spriteCoinPosition,
      //   1
      // ).Forget();
      _stateManager.IncrementCoin(1);

      _stateManager.OpenCharHiddenWord(_char);
      // _levelManager.CreateLetter(transform.position, _levelManager.buttonDirectory.transform.position, _charInstance.CharValue).Forget();
    }

    _charInstance.OccupiedNode.SetOpen();

    Open(runEffect);

    ChangeTheme();


    _charInstance.OccupiedNode.SetHint(false);

    // if (runEffect)
    // {
    //   await OpenNeighbours(runEffect);
    // }

    await UniTask.Yield();
  }


  public async UniTask ShowCharAsHint(bool runEffect)
  {
    _levelManager.ManagerHiddenWords.AddOpenChar(_charInstance);

    _charInstance.OccupiedNode.SetHint();

    Open(runEffect);

    ChangeTheme();

    if (!runEffect) return;

    // play sound.
    _gameManager.audioManager.PlayClipEffect(_gameSetting.Audio.openHintChar);

    // Check open all chars of by word.
    // int countOpenChar = 0;
    // foreach (var hiddenChar in _charInstance.OccupiedWord.Chars)
    // {
    //   if (
    //     hiddenChar.OccupiedNode.StateNode.HasFlag(StateNode.Hint)
    //     || hiddenChar.OccupiedNode.StateNode.HasFlag(StateNode.Open)
    //     )
    //   {
    //     countOpenChar++;
    //   }
    // }
    // if (countOpenChar == _charInstance.OccupiedWord.Word.Length)
    if (_charInstance.OccupiedWord.isMayBeOpen && runEffect)
    {
      _charInstance.OccupiedWord.AutoOpenWord().Forget();
    }

    // if (_stateManager.dataGame.bonus.ContainsKey(TypeBonus.OpenNeighbours))
    // {
    //   await OpenNeighbours(runEffect);
    // }
    await UniTask.Yield();
  }

  public void Open(bool runEffect)
  {
    // if (runEffect) //  && _gameManager.AppInfo.setting.animation
    // {
    //   RunOpenEffect();
    // }

    // Remove open char.
    if (
      _stateManager.dataGame.activeLevel.openChars.ContainsKey(_charInstance.OccupiedNode.arrKey)
      &&
      _levelManager.ManagerHiddenWords.OpenCrossWords.ContainsKey(_charInstance.OccupiedWord.Word)
      )
    {
      _levelManager.ManagerHiddenWords.RemoveOpenChar(_charInstance);
    }
    _charText.gameObject.SetActive(true);

    // Check exist bonus of by node.
    if (_charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Bonus))
    {
      _charInstance.OccupiedNode.BonusEntity.Collect();
    }
    // Check exist entity of by node.
    if (_charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Entity))
    {
      _charInstance.OccupiedNode.OccupiedEntity.Run().Forget();
    }
  }


  public async UniTask FocusOpenChar()
  {
    var startScale = transform.localScale;

    Vector3 initialScale = transform.localScale;
    Vector3 upScale = new Vector3(1.2f, 1.2f, 0f);
    var duration = .5f;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
      float progress = elapsedTime / duration;
      transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
      elapsedTime += Time.deltaTime;
      await UniTask.Yield();
    }
    transform.localScale = initialScale;
  }


  // public async void OnPointerDown(PointerEventData eventData)
  public async void OnClick(InputAction.CallbackContext context)
  {
    var rayHit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(_inputManager.clickPosition()));
    if (!rayHit.collider) return;

    if (rayHit.collider.gameObject == gameObject)
    {

      if (
        _charInstance.OccupiedNode != null
        && (
          _charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Open)
          ||
          _charInstance.OccupiedNode.StateNode.HasFlag(StateNode.Hint)
          )
        ) return;

      // Show message
      _gameManager.InputManager.Disable();

      transform
        .DOPunchScale(new Vector3(.2f, .2f, 0), _gameSetting.timeGeneralAnimation)
        .SetEase(Ease.OutBack)
        .OnComplete(() =>
        {
          transform.localScale = _initScale;

          // if (!interactible) pointer.enabled = true;
        });

      var message = await Helpers.GetLocaledString("openletterbyads");
      var dialogConfirm = new DialogProvider(new DataDialog()
      {
        message = message,
        showCancelButton = true,
      });

      var result = await dialogConfirm.ShowAndHide();
      if (result.isOk)
      {
        // Open char by ads.
        OpenCharExtern();

        DataManager.OnOpenCharExtern += OpenByAds;

      }
      _gameManager.InputManager.Enable();
    }
  }

  private void OpenByAds()
  {
    ShowCharAsHint(true).Forget();

    DataManager.OnOpenCharExtern -= OpenByAds;
  }


  // public async UniTask OpenNeighbours(bool runEffect)
  // {
  //   int valueBonusOpenNeighbours;
  //   _stateManager.dataGame.bonus.TryGetValue(TypeBonus.OpenNeighbours, out valueBonusOpenNeighbours);

  //   if (valueBonusOpenNeighbours <= 0) return;

  //   // open equals chars.
  //   List<GridNode> equalsCharNodes = GameManager.Instance.LevelManager.ManagerHiddenWords.GridHelper.FindNeighboursNodesOfByEqualChar(_charInstance.OccupiedNode);
  //   foreach (var equalCharNode in equalsCharNodes)
  //   {
  //     await equalCharNode.OccupiedChar.CharGameObject.ShowCharAsHint(runEffect);
  //   }
  //   //await UniTask.Yield();
  // }




#if UNITY_EDITOR
  public override string ToString()
  {
    return "HiddenCharMB::: [text=" + _charInstance.CharValue + "] \n";
  }
#endif
}
