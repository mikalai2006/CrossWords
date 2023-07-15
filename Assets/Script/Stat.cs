using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
  [SerializeField] private TMPro.TextMeshProUGUI _countWords;
  [SerializeField] private TMPro.TextMeshProUGUI _countWordsMask;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private GameManager _gameManager => GameManager.Instance;
  private float maxWidthProgress;
  [SerializeField] private RectTransform _bgProgress;
  [SerializeField] private RectTransform spriteProgress;
  [SerializeField] private Image _bar;
  // [SerializeField] private GameObject _effectometer;
  // [SerializeField] private RectTransform _spriteProgressOrderChar;
  [SerializeField] private Image _barOrderChar;

  private void Awake()
  {
    ChangeTheme();

    StateManager.OnChangeState += SetValue;
    UISettings.OnChangeLocale += Localize;
    GameManager.OnChangeTheme += ChangeTheme;

    maxWidthProgress = _bgProgress.rect.width;
  }

  private void OnDestroy()
  {
    StateManager.OnChangeState -= SetValue;
    UISettings.OnChangeLocale -= Localize;
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  private void ChangeTheme()
  {
    _countWords.color = _gameManager.Theme.colorPrimary;
    _bar.color = _gameManager.Theme.colorAccent;

    _barOrderChar.color = _gameManager.Theme.colorAccent;
  }

  public void SetValue(StateGame state)
  {
    Localize();

    SetProgressValue(state);
  }

  private void SetProgressValue(StateGame state)
  {
    float width = 0;
    if (state.activeDataGame.activeLevel.countCrossWords > 0)
    {
      width = (state.activeDataGame.activeLevel.openCrossWords.Count * 100f / state.activeDataGame.activeLevel.countCrossWords) * (maxWidthProgress / 100f);
    }

    spriteProgress.DOSizeDelta(new Vector3(width, spriteProgress.rect.height), _gameSetting.timeGeneralAnimation);
    //.sizeDelta = new Vector3(width, 1f);
  }

  private async void Localize()
  {

    // View new data.
    var textCountWords = await Helpers.GetLocalizedPluralString(
          "foundcountword",
           new Dictionary<string, object> {
            {"count",  _levelManager.ManagerHiddenWords.OpenCrossWords.Count},
            {"count2", _gameManager.StateManager.dataGame.activeLevel.countCrossWords},
            // {"count3", _levelManager.ManagerHiddenWords.AllowPotentialWords.Count},
            // {"count4", _levelManager.ManagerHiddenWords.OpenWords.Count},
          }
        );
    _countWords.text = textCountWords;
    _countWordsMask.text = textCountWords;
  }

  public void Hide()
  {
    gameObject.SetActive(false);
  }

  public void Show()
  {
    gameObject.SetActive(true);
  }
}
