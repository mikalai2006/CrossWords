using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseButton : MonoBehaviour, IPointerDownHandler
{
  protected LevelManager _levelManager => GameManager.Instance.LevelManager;
  protected GameSetting _gameSetting => GameManager.Instance.GameSettings;
  protected StateManager _stateManager => GameManager.Instance.StateManager;
  protected GameManager _gameManager => GameManager.Instance;

  protected MonoBehaviour pointer;
  protected Vector3 initScale;
  protected Vector3 initScaleSprites;
  protected Vector3 initPosition;
  protected Vector3 initScaleCounterObject;
  protected GameEntity configEntity;
  [SerializeField] protected GameButton configButton;
  [SerializeField] protected Button button;
  [SerializeField] protected GameObject spritesObject;
  [SerializeField] protected SpriteRenderer spriteBg;
  [SerializeField] protected SpriteMask spriteMask;
  [SerializeField] protected SpriteRenderer spriteProgress;
  [SerializeField] protected GameObject counterObject;
  [SerializeField] protected TMPro.TextMeshProUGUI counterText;
  [SerializeField] protected Image counterImage;
  protected float progressBasePositionY = -1.25f;
  protected bool statusShowCounter = false;
  protected int valueCounter;
  protected int value;
  protected bool interactible = true;
  protected bool isShowCounter = true;

  #region UnityMethods
  protected virtual void Awake()
  {
    pointer = GetComponent<IPointerDownHandler>() as MonoBehaviour;

    initScale = transform.localScale;
    initScaleSprites = spritesObject.transform.localScale;
    initPosition = spritesObject.transform.position;
    initScaleCounterObject = counterObject.transform.localScale;

    counterText.text = "0";
    counterObject.transform.localScale = new Vector3(0, 0, 0);

    ResetProgressBar();

    ChangeTheme();

    if (configEntity != null)
    {
      spriteBg.sprite = configEntity.sprite;
      spriteMask.sprite = configEntity.sprite;
    }

    GameManager.OnChangeTheme += ChangeTheme;
    GameManager.OnAfterStateChanged += AfterStateChanged;
  }

  protected virtual void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
    GameManager.OnAfterStateChanged -= AfterStateChanged;
  }

  private void AfterStateChanged(GameState state)
  {
    switch (state)
    {
      case GameState.StartEffect:
        pointer.enabled = false;
        break;
      case GameState.StopEffect:
        pointer.enabled = true;
        break;
    }
    ChangeTheme();
  }
  #endregion

  public virtual void ChangeTheme()
  {
    counterImage.color = _gameManager.Theme.colorBgHint;
    counterText.color = _gameManager.Theme.colorTextHint;
    spriteProgress.color = _gameManager.Theme.colorAccent;
    button.image.color = _gameManager.Theme.colorBgButton;

    if (!pointer.enabled)
    {
      spriteBg.color = _gameManager.Theme.colorDisable;
      return;
    }

    if (statusShowCounter)
    {
      spriteBg.color = _gameManager.Theme.entityColor;
    }
    else
    {
      spriteBg.color = _gameManager.Theme.colorDisable;
    }
  }

  public virtual void SetValue(StateGame state)
  {
    // Debug.Log($"star={data.activeLevel.star}/status={_statusShowCounter}");
    if (value > 0)
    {
      if (!statusShowCounter)
      {
        ShowCounter();
        // AddChar();
      }
      else if (value != valueCounter && isShowCounter)
      {
        counterObject.transform
          .DOPunchScale(new Vector3(.5f, .5f, 0), _gameSetting.timeGeneralAnimation)
          .SetEase(Ease.OutBack)
          .OnComplete(() =>
          {
            statusShowCounter = true;
            counterObject.transform.localScale = initScaleCounterObject;
          });
      }
    }
    else // if (_statusShowCounter)
    {

      HideCounter();
    }
    valueCounter = value;
    counterText.text = value.ToString();

    // TODO animation get hit.

    SetValueProgressBar(state);
  }


  private void ShowCounter()
  {
    if (!isShowCounter) return;

    // Debug.Log($"Run Show");
    counterObject.transform
      .DOScale(1f, _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack)
      .From(0f, true)
      .OnComplete(() =>
      {
        // Debug.Log($"complete show");
        statusShowCounter = true;
        ChangeTheme();
      });
    counterObject.transform
      .DOPunchScale(new Vector3(0.5f, 0.5f, 0), _gameSetting.timeGeneralAnimation)
      .SetDelay(_gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack);
    // spriteBg.color = _gameManager.Theme.colorPrimary;
  }

  public void HideCounter()
  {
    // Debug.Log($"Run hide");
    counterObject.transform
      .DOScale(0f, _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack)
      .OnComplete(() =>
      {
        // Debug.Log($"complete hide");
        statusShowCounter = false;
        ChangeTheme();
      });
    // spriteBg.color = _gameManager.Theme.colorDisable;
    //gameObject.SetActive(false);
  }

  public virtual void SetValueProgressBar(StateGame state)
  {
  }

  public virtual void ResetProgressBar()
  {
    spriteProgress.transform
      .DOLocalMoveY(progressBasePositionY, _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack);
  }

  public async virtual void OnPointerDown(PointerEventData eventData)
  {
    if (!pointer.enabled) return;

    if (value != 0) _gameManager.ChangeState(GameState.StartEffect);

    _gameManager.audioManager.Click();

    // pointer.enabled = false;

    transform
        .DOPunchScale(new Vector3(.2f, .2f, 0), _gameSetting.timeGeneralAnimation)
        .SetEase(Ease.OutBack)
        .OnComplete(() =>
        {
          statusShowCounter = true;
          transform.localScale = initScale;

          // if (!interactible) pointer.enabled = true;
        });

    if (!interactible)
    {
      _gameManager.ChangeState(GameState.StopEffect);
      return;
    }

    if (value != 0)
    {
      DataDialogResult result = new()
      {
        isOk = true,
      };

      // DoDialog.
      if (_gameManager.AppInfo.setting.dod)
      {
        _gameManager.InputManager.Disable();

        var name = await Helpers.GetLocaledString(configEntity.text.title);
        var message = await Helpers.GetLocalizedPluralString("confirm_runhint", new Dictionary<string, string>() {
        {"name", name}
      });
        var title = await Helpers.GetLocaledString("confirm_title");
        var dialog = new DialogProvider(new DataDialog()
        {
          title = title,
          sprite = configEntity.sprite,
          message = message,
          showCancelButton = true
        });

        result = await dialog.ShowAndHide();
        _gameManager.InputManager.Enable();
      }

      if (result.isOk)
      {
        RunHint();
      }
      else
      {
        _gameManager.ChangeState(GameState.StopEffect);
      }
    }
    else
    {

      _gameManager.InputManager.Disable();

      string titleHint = await Helpers.GetLocaledString(configEntity.text.title);
      var message = await Helpers.GetLocalizedPluralString("nothint", new System.Collections.Generic.Dictionary<string, string>() {
        {"name", titleHint}
      });
      var dialog = new DialogProvider(new DataDialog()
      {
        sprite = configEntity.sprite,
        message = message,
        showCancelButton = true
      });

      var result = await dialog.ShowAndHide();
      if (result.isOk)
      {
        // open shop.
        var dialogWindow = new UIShopOperation();
        await dialogWindow.ShowAndHide();
        _gameManager.InputManager.Enable();
      }
      else
      {
        _gameManager.InputManager.Enable();
      }

    }
  }

  public virtual void RunHint()
  {
    // var node = _levelManager.ManagerHiddenWords.GridHelper.GetRandomNodeWithChar();
    // if (node != null)
    // {
    //   node.OccupiedChar.ShowCharAsNei(true).Forget();
    //   _levelManager.ManagerHiddenWords.AddOpenChar(node.OccupiedChar);
    //   _stateManager.UseStar();
    //   node.SetHint();
    // }
  }


  protected virtual void SetDefault()
  {
    spritesObject.transform.localScale = initScaleSprites;
    spritesObject.transform.position = initPosition;
  }

  public void Hide()
  {
    gameObject.SetActive(false);
  }

  public void Show()
  {
    gameObject.SetActive(true);
  }

  public virtual async UniTask<BaseEntity> CreateEntity(Vector3 positionForSpawn, GameObject target = null)
  {
    var asset = Addressables.InstantiateAsync(
      configEntity.prefab,
      positionForSpawn,
      Quaternion.identity,
      target == null ? _levelManager.ManagerHiddenWords.tilemapEntities.transform : target.transform
      );
    var newObj = await asset.Task;

    var newEntity = newObj.GetComponent<BaseEntity>();
    newEntity.InitStandalone(asset);

    // _stateManager.RefreshData();

    return newEntity;
  }


  #region Effects
  public virtual void RunEffect()
  {
    var _CachedSystem = GameObject.Instantiate(
      _gameSetting.Boom,
      transform.position,
      Quaternion.identity
    );

    var main = _CachedSystem.main;
    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, _levelManager.ManagerHiddenWords.scaleGrid / 2);

    var col = _CachedSystem.colorOverLifetime;
    col.enabled = true;

    Gradient grad = new Gradient();
    grad.SetKeys(new GradientColorKey[] {
      new GradientColorKey(_gameManager.Theme.bgFindAllowWord, 1.0f),
      new GradientColorKey(_gameManager.Theme.bgHiddenWord, 0.0f)
      }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f)
    });

    col.color = grad;
    _CachedSystem.Play();
    if (_CachedSystem.isPlaying || _CachedSystem.isStopped) Destroy(_CachedSystem.gameObject, 2f);
  }
  #endregion

  public async UniTask AddChar()
  {
    Vector3 initialScale = initScaleSprites;
    Vector3 initialPosition = initPosition;
    Vector3 upScale = new Vector3(1.5f, 1.5f, 0);

    float elapsedTime = 0f;
    float duration = .2f;
    float startTime = Time.time;

    AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.addToFlask);
    while (elapsedTime < duration)
    {
      float progress = (Time.time - startTime) / duration;
      spritesObject.transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
      await UniTask.Yield();
      elapsedTime += Time.deltaTime;
    }
    RunEffect();

    SetDefault();
  }

}
