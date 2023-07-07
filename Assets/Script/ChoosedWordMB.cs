using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChoosedWordMB : MonoBehaviour
{
  [SerializeField] private ChoosedCharMB charMB;
  private LevelManager _dataManager = LevelManager.Instance;
  private GameManager _gameManager => GameManager.Instance;
  private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private List<ChoosedCharMB> _chars;
  private List<ChoosedCharMB> _charsGameObject;

  private void Awake()
  {
    _chars = new List<ChoosedCharMB>();
    for (int i = 0; i < 20; i++)
    {
      var newChar = GameObject.Instantiate(
        charMB,
        new Vector3(0, 0, 0),
        Quaternion.identity,
        gameObject.transform
      );
      //newChar.gameObject.SetActive(false);
      _chars.Add(newChar);
    }
    _charsGameObject = new();
  }

  public void CreateWord()
  {

  }

  public void DrawWord(string choosedWord)
  {
    // calculate scale char gameObject.
    var charScale = choosedWord.Length > 9 ? 9f / choosedWord.Length : 1f;
    // Debug.Log($"charScale={charScale}::: {choosedWord}::: ");
    gameObject.GetComponent<GridLayoutGroup>().cellSize = new Vector2(charScale, charScale);
    ResetWord();
    // gameObject.transform.localPosition = new Vector3(-choosedWord.Length / 2f + .5f, 0, 0);
    for (int i = 0; i < choosedWord.Length; i++)
    {
      var currentChar = choosedWord.ElementAt(i);
      var currentCharMB = _chars.ElementAt(i);
      _charsGameObject.Add(currentCharMB);
      currentCharMB.gameObject.SetActive(true);
      currentCharMB.SetChar(currentChar);
      currentCharMB.SetSize(charScale);
    }
  }

  public async UniTask OpenHiddenWord(HiddenWordMB hiddenWordMB)
  {
    await _levelManager.ShowHelp(Constants.Helps.HELP_FLASK_HIDDENBOARD);

    List<UniTask> listTasks = new();
    for (int i = 0; i < _charsGameObject.Count; i++)
    {
      var currentCharMB = _charsGameObject.ElementAt(i);

      var needHiddenChar = hiddenWordMB.Chars.ElementAt(i); //]Find(t => t.charTextValue == currentCharMB.charTextValue);

      if (needHiddenChar == null) continue;

      // needHiddenChar.OccupiedNode.SetOpen();

      listTasks.Add(currentCharMB.OpenCharHiddenWord(needHiddenChar, i * (100 + i * 10)));
    }

    await UniTask.WhenAll(listTasks);
    // _gameManager.StateManager.IncrementRate(1);
  }

  public async UniTask ExistHiddenWord(HiddenWordMB hiddenWordMB)
  {
    List<UniTask> listTasks = new();
    listTasks.Add(hiddenWordMB.FocusOpenWord());
    for (int i = 0; i < _charsGameObject.Count; i++)
    {
      var currentCharMB = _charsGameObject.ElementAt(i);

      // var needHiddenChar = hiddenWordMB.Chars.Find(t => t.charTextValue == currentCharMB.charTextValue);

      // if (needHiddenChar == null) continue;

      // listTasks.Add(currentCharMB.CheckExist(needHiddenChar));
      listTasks.Add(currentCharMB.CheckExist());
    }
    AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.existWord);
    await UniTask.WhenAll(listTasks);
  }


  public async UniTask OpenAllowWord()
  {

    char lastOpenChar = new char();
    List<UniTask> listTasks = new();

    for (int i = 0; i < _charsGameObject.Count; i++)
    {
      var currentCharMB = _charsGameObject.ElementAt(i);

      listTasks.Add(currentCharMB.OpenCharAllowWord(i * (100 + i * 10)));
      // await currentCharMB.OpenCharAllowWord();
      lastOpenChar = currentCharMB.textChar;
    }
    await UniTask.WhenAll(listTasks);

    // _levelManager.CreateCoin(
    //   _levelManager.buttonDirectory.transform.position,
    //   _levelManager.topSide.spriteCoinPosition,
    //   1
    // ).Forget();

    _gameManager.StateManager.IncrementCoin(1);

    _levelManager.CreateLetter(
      _levelManager.buttonDirectory.transform.position,
      _levelManager.buttonFlask.transform.position,
      lastOpenChar
    ).Forget();
    // _gameManager.StateManager.OpenCharHiddenWord(lastOpenChar);
    // _gameManager.StateManager.IncrementRate(1);
  }


  public async UniTask ExistAllowWord()
  {
    List<UniTask> listTasks = new();
    for (int i = 0; i < _charsGameObject.Count; i++)
    {
      var currentCharMB = _charsGameObject.ElementAt(i);

      listTasks.Add(currentCharMB.CheckExist());
    }
    AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.existWord);
    await UniTask.WhenAll(listTasks);
  }

  public async UniTask NoWord()
  {
    AudioManager.Instance.PlayClipEffect(GameManager.Instance.GameSettings.Audio.noWord);
    await NotFoundWord();

    List<UniTask> listTasks = new();
    for (int i = 0; i < _charsGameObject.Count; i++)
    {
      var currentCharMB = _charsGameObject.ElementAt(i);

      // await currentCharMB.CheckNo();
      listTasks.Add(currentCharMB.CheckNo());
    }
    await UniTask.WhenAll(listTasks);
  }

  public void ResetWord()
  {
    foreach (var ch in _charsGameObject)
    {
      ch.gameObject.SetActive(false);
      ch.SetDefault();
    }
    _charsGameObject.Clear();
  }

  public async UniTask NotFoundWord()
  {
    Vector3 initialPosition = transform.localPosition;
    Vector3 upPosition = initialPosition - new Vector3(-.5f, 0, 0);
    var duration = .1f;

    for (float time = 0; time < duration * 2; time += Time.deltaTime)
    {
      float progress = Mathf.PingPong(time, duration) / duration;
      transform.localPosition = Vector3.Lerp(initialPosition, upPosition, progress);
      await UniTask.Yield();// yield return null;
    }
    transform.localPosition = initialPosition;
  }

}
