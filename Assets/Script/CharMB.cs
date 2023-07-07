using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharMB : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] private TMPro.TextMeshProUGUI _charText;
  private InputManager _inputManager;
  private Camera _camera;
  public char charTextValue;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private GameSetting _gameSettings => GameManager.Instance.GameSettings;
  private GameManager _gameManager => GameManager.Instance;
  [SerializeField] private BoxCollider2D _collider;
  [SerializeField] private Image _image;
  [SerializeField] private RectTransform _canvas;
  private CancellationTokenSource cancelTokenSource;

  private void OnEnable()
  {
    _inputManager = new InputManager();
    _inputManager.Click += OnClick;
    _inputManager.Enable();
  }

  private void OnDisable()
  {
    _inputManager.Click -= OnClick;
    _inputManager.Disable();
  }

  private void Awake()
  {
    transform.localPosition = transform.localPosition + new Vector3(0, 0, -2);

    _camera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();

    ResetObject();

    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {

    GameManager.OnChangeTheme -= ChangeTheme;
  }

  private void ChangeTheme()
  {
    if (_gameManager.Theme.bgImageChar != null) _image.sprite = _gameManager.Theme.bgImageChar;

    _image.color = _gameManager.Theme.bgColorChar;

    _charText.color = _gameManager.Theme.colorTextChar;
  }

  public void ResetObject()
  {

    ChangeTheme();
  }


  public void Init(char symbol)
  {
    _charText.text = symbol.ToString();

    charTextValue = symbol;
  }

  public void SetSize(float size)
  {
    _charText.fontSize = size;

    _canvas.sizeDelta = new Vector2(size, size);

    _collider.size = new Vector2(size, size);
  }

  public async void OnClick(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      var rayHit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(_inputManager.clickPosition()));
      if (!rayHit.collider) return;

      if (rayHit.collider.gameObject == gameObject)
      {
        _inputManager.SetDragging(true);

        var control = context.control.ToString().Split('/');
        if (control[1] == "Mouse")
        {
          cancelTokenSource = new CancellationTokenSource();
          StartWaitDelay(cancelTokenSource.Token).Forget();
        }
      }
    }
    else if (context.canceled && _inputManager.Dragging)
    {
      _inputManager.SetDragging(false);
      // await UniTask.Delay(100);
      await _levelManager.ManagerHiddenWords.CheckChoosedWord();
    }
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (_inputManager.Dragging)
    {
      cancelTokenSource = new CancellationTokenSource();
      StartWaitDelay(cancelTokenSource.Token).Forget();
      // ClickedOnUi();
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (_inputManager.Dragging)
    {
      StopWaitDelay();
    }
  }

  private void StopWaitDelay()
  {
    cancelTokenSource.Cancel();
    cancelTokenSource.Dispose();
  }

  public async UniTask StartWaitDelay(CancellationToken cancellationToken)
  {
    if (_levelManager.ManagerHiddenWords.listChoosedGameObjects.Count != 0)
    {
      await UniTask.Delay(_gameManager.AppInfo.setting.td, cancellationToken: cancellationToken);
    }

    if (!cancellationToken.IsCancellationRequested && _inputManager.Dragging)
    {
      ChooseSymbol();
    }
  }

  // public bool ClickedOnUi()
  // {
  //   PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
  //   eventDataCurrentPosition.position = _inputManager.clickPosition();
  //   List<RaycastResult> results = new List<RaycastResult>();
  //   EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
  //   foreach (var item in results)
  //   {
  //     Debug.Log($"item=>{item.gameObject.name}");
  //     if (item.gameObject.name == "Panel Settings")
  //     {
  //       return true;
  //     }
  //   }
  //   return false;
  // }

  public void ChooseSymbol()
  {
    _levelManager.ManagerHiddenWords.AddChoosedChar(this);
    if (_gameManager.Theme.bgImageCharChoose != null) _image.sprite = _gameManager.Theme.bgImageCharChoose;
    _image.color = _gameManager.Theme.bgColorChooseChar;
    _charText.color = _gameManager.Theme.colorTextChooseChar;
  }

  public async UniTask SetPosition(Vector3 newPos)
  {
    Vector3 initialPosition = transform.position;

    float elapsedTime = 0f;
    float duration = .2f;
    float startTime = Time.time;

    while (elapsedTime < duration)
    {
      float progress = (Time.time - startTime) / duration;
      transform.position = Vector3.Lerp(initialPosition, newPos, progress);
      await UniTask.Yield();
      elapsedTime += Time.deltaTime;
    }
    transform.position = newPos;
  }
}
