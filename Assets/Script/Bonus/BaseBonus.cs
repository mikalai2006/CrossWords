using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public abstract class BaseBonus : MonoBehaviour
{
  protected LevelManager _levelManager => GameManager.Instance.LevelManager;
  protected GameSetting _gameSetting => GameManager.Instance.GameSettings;
  protected StateManager _stateManager => GameManager.Instance.StateManager;
  protected GameManager _gameManager => GameManager.Instance;

  protected Vector3 initScale;
  // protected Vector3 initPosition;
  protected Vector3 initScaleCounterObject;
  protected GameBonus configBonus;
  public GameBonus Config => configBonus;
  [SerializeField] public GameObject bonusObject;
  [SerializeField] protected Canvas canvasObject;
  [SerializeField] protected SpriteRenderer spriteBg;
  [SerializeField] protected SpriteMask spriteMask;
  [SerializeField] protected SpriteRenderer spriteProgress;
  [SerializeField] protected TMPro.TextMeshProUGUI counterText;
  [SerializeField] protected SortingGroup order;
  protected float progressBasePositionY = -1.25f;
  protected bool statusShowCounter = false;
  protected int valueCounter;
  protected int value;
  protected InputManager _inputManager;
  private Camera _camera;
  private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> asset;

  #region UnityMethods
  protected virtual void Awake()
  {
    _inputManager = new InputManager();
    _camera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();

    initScale = transform.localScale;

    // initPosition = bonusObject.transform.position;

    SetSortOrder(1);

    ChangeTheme();

    initScaleCounterObject = counterText.gameObject.transform.localScale;
    counterText.text = "";

    // gameObject.SetActive(false);

    // ResetProgressBar();

    if (configBonus != null)
    {
      spriteBg.sprite = configBonus.sprite;
      spriteMask.sprite = configBonus.sprite;
    }

    // StateManager.OnChangeState += SetValue;
    GameManager.OnChangeTheme += ChangeTheme;
    _inputManager.ClickChar += OnClick;
  }
  protected virtual void OnDestroy()
  {
    // StateManager.OnChangeState -= SetValue;
    GameManager.OnChangeTheme -= ChangeTheme;
    _inputManager.ClickChar -= OnClick;
  }
  #endregion

  private void ChangeTheme()
  {
    if (value > 0)
    {
      spriteBg.color = _gameManager.Theme.entityColor;
    }
    else
    {
      spriteBg.color = _gameManager.Theme.colorDisable;
    }
    counterText.color = _gameManager.Theme.bgColor;
    spriteProgress.color = _gameManager.Theme.colorAccent;
  }


  public virtual void Init(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> asset)
  {
    this.asset = asset;
  }


  public virtual void SetValue(StateGame state)
  {
    if (valueCounter == value) return;

    if (value > 0 && valueCounter != value)
    {
      Show();
    }
    else if (value == 0)
    {

      Hide();
    }

    valueCounter = value;

    ChangeTheme();
  }

  public virtual void Show()
  {
    // gameObject.SetActive(true);

    transform
      .DOPunchScale(new Vector3(.5f, .5f, 0), _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBack)
      .OnComplete(() =>
      {
        SetDefault();
      });
  }

  public void Hide()
  {
    // transform
    //   .DOScale(0f, _gameSetting.timeGeneralAnimation)
    //   .SetEase(Ease.OutBack)
    //   .OnComplete(() =>
    //   {
    //     // Destroy(gameObject);
    //     // gameObject.SetActive(false);
    //   });
  }

  public virtual void SetValueProgressBar(StateGame state)
  {
  }

  public virtual void ResetProgressBar()
  {
    spriteProgress.transform
      .DOMoveY(progressBasePositionY, _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack);
  }

  public async void OnClick(InputAction.CallbackContext context)
  {
    var rayHit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(_inputManager.clickPosition()));
    if (!rayHit.collider) return;

    if (rayHit.collider.gameObject == gameObject)
    {

      _gameManager.audioManager.Click();

      transform
          .DOPunchScale(new Vector3(.2f, .2f, 0), _gameSetting.timeGeneralAnimation)
          .SetEase(Ease.OutBack);
      var title = await Helpers.GetLocaledString(configBonus.text.title);
      var message = await Helpers.GetLocaledString(configBonus.text.description);
      var dialog = new DialogProvider(new DataDialog()
      {
        sprite = configBonus.sprite,
        title = title,
        message = message,
        showCancelButton = false
      });

      _gameManager.InputManager.Disable();
      await dialog.ShowAndHide();
      _gameManager.InputManager.Enable();
    }
  }

  public void SetDefault()
  {
    transform.localScale = initScale;
    SetSortOrder(1);
    // bonusObject.transform.localPosition = new Vector3(0, 0, 0);
  }

  public void SetDefaultWithAnimate()
  {
    var startPosition = bonusObject.transform.localPosition;
    bonusObject.transform
      .DOLocalMove(new Vector3(0, 0, 0), _gameSetting.timeGeneralAnimation)
      .From(startPosition, true)
      .OnComplete(() =>
      {
        SetDefault();
      });
  }

  public void SetSortOrder(int value)
  {
    order.sortingOrder = value;
    canvasObject.sortingOrder = value;
  }
}
