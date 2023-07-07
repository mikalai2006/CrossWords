using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChoosedCharMB : MonoBehaviour
{
  [SerializeField] private TMPro.TextMeshProUGUI _charText;
  [SerializeField] public RectTransform RectTransform;

  [HideInInspector] public char textChar;
  private GameManager _gameManager => GameManager.Instance;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  [SerializeField] private Image _image;
  private GameSetting _gameSetting;
  private Vector3 _initPosition;
  private Vector3 _initScale;

  public void Start()
  {
    _gameSetting = GameManager.Instance.GameSettings;
    _initPosition = gameObject.transform.position;
    _initScale = gameObject.transform.localScale;
    SetDefault();
  }

  public void SetChar(char currentChar)
  {
    _charText.text = currentChar.ToString();
    textChar = currentChar;
  }

  public void SetSize(float size)
  {
    _charText.fontSize = size;
    RectTransform.sizeDelta = new Vector2(size, size);
  }

  public async UniTask OpenCharHiddenWord(HiddenCharMB needHiddenChar, int delay)
  {
    _image.color = _gameManager.Theme.bgFindHiddenWord;
    _charText.color = _gameManager.Theme.textFindHiddenWord;
    await UniTask.Delay(delay); // UniTask.Delay(delay, ignoreTimeScale: false);

    Vector3 initialScale = transform.localScale;
    Vector3 initialPosition = transform.position;
    Vector3 upScale = new Vector3(0.5f, 0.5f, 0);

    float elapsedTime = 0f;
    float duration = .5f;
    float startTime = Time.time;

    while (elapsedTime < duration)
    {
      // The center of the arc
      Vector3 center = (initialPosition + needHiddenChar.gameObject.transform.position) * 0.5F;

      // move the center a bit downwards to make the arc vertical
      center -= new Vector3(0, 1, 0);
      // Interpolate over the arc relative to center
      Vector3 riseRelCenter = initialPosition - center;
      Vector3 setRelCenter = needHiddenChar.gameObject.transform.position - center;

      float progress = (Time.time - startTime) / duration; //elapsedTime / duration;
      transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
      transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, progress);
      transform.position += center;
      await UniTask.Yield();
      elapsedTime += Time.deltaTime;
    }
    transform.localScale = new Vector3(0, 0, 0);
    // transform.localScale = initialScale;
    // AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.openWord);

    await needHiddenChar.ShowChar(true, needHiddenChar.charTextValue); //.Forget();
  }

  public async UniTask OpenCharAllowWord(int delay)
  {
    var targetToMove = _levelManager.buttonDirectory;

    _image.color = _gameManager.Theme.bgFindAllowWord;
    _charText.color = _gameManager.Theme.textFindAllowWord;

    await UniTask.Delay(delay);

    Vector3 initialScale = transform.localScale;
    Vector3 initialPosition = transform.position;
    Vector3 upScale = new Vector3(0.5f, 0.5f, 0);

    float elapsedTime = 0f;
    float duration = .5f;
    float startTime = Time.time;

    while (elapsedTime < duration)
    {
      // The center of the arc
      Vector3 center = (initialPosition + targetToMove.gameObject.transform.position) * 0.5F;

      // move the center a bit downwards to make the arc vertical
      center += new Vector3(0.5f, 0.5f, 0);
      // Interpolate over the arc relative to center
      Vector3 riseRelCenter = initialPosition - center;
      Vector3 setRelCenter = targetToMove.gameObject.transform.position - center;

      float progress = (Time.time - startTime) / duration; //elapsedTime / duration;
      transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
      transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, progress);
      transform.position += center;
      await UniTask.Yield();
      elapsedTime += Time.deltaTime;
    }

    transform.localScale = new Vector3(0, 0, 0);

    await targetToMove.AddChar();

    _stateManager.OpenCharAllowWord(textChar);
  }

  public async UniTask CheckNo()
  {
    _image.color = _gameManager.Theme.bgNotFoundWord;
    _charText.color = _gameManager.Theme.textNotFoundWord;

    await UniTask.Delay(300);

    SetDefault();
  }

  public void SetDefault()
  {
    gameObject.SetActive(false);
    _image.color = _gameManager.Theme.bgChoosedWord;
    _charText.color = _gameManager.Theme.textChoosedWord;
    transform.position = _initPosition;
    transform.localScale = _initScale;
  }
  public async UniTask CheckExist()
  {
    Vector3 initialScale = transform.localScale;
    Vector3 upScale = new Vector3(1.5f, 1.5f, 0f);
    var duration = .5f;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
      // float progress = Mathf.PingPong(time, duration) / duration;
      float progress = elapsedTime / duration;
      transform.localScale = Vector3.Slerp(initialScale, upScale, progress);
      elapsedTime += Time.deltaTime;
      await UniTask.Yield();
    }
    transform.localScale = initialScale;

    SetDefault();
  }




}
