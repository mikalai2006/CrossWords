
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public abstract class BaseWord
{
  // protected GameSetting _gameSetting => GameManager.Instance.GameSettings;
  // protected StateManager _stateManager => GameManager.Instance.StateManager;
  protected LevelManager _levelManager => GameManager.Instance.LevelManager;
  // protected GameManager _gameManager => GameManager.Instance;
  protected List<CharHidden> _chars;
  public List<CharHidden> Chars => _chars;
  // public WordHiddenMB GameObjectHiddeWord;
  private DirectionWord _directionWord;
  public DirectionWord DirectionWord => _directionWord;

  protected string _word;
  public string Word => _word;

  public BaseWord()
  {
    _chars = new List<CharHidden>();
  }

  public virtual void Init(string word, DirectionWord dir)
  {
    _word = word;
    _directionWord = dir;
  }

  public virtual void Draw()
  {

  }
  public virtual void Open(bool runEffect)
  {

  }
  public void SetOpen()
  {
    foreach (CharHidden charHidden in _chars)
    {
      // Draw hidden char MonoBehaviour
      charHidden.SetOpen();
    }
  }

  public async UniTask FocusOpenWord()
  {
    List<UniTask> tasks = new();
    foreach (var charObj in Chars)
    {
      tasks.Add(charObj.CharGameObject.FocusOpenChar());
    }
    await UniTask.WhenAll(tasks);
  }

  public async UniTask AutoOpenWord()
  {
    if (!_levelManager.ManagerHiddenWords.OpenWords.ContainsKey(_word)) _levelManager.ManagerHiddenWords.OpenWords.Add(_word, 1);
    // if (!_levelManager.ManagerHiddenWords.OpenNeedWords.ContainsKey(_word)) _levelManager.ManagerHiddenWords.OpenNeedWords.Add(_word, 1);

    // List<UniTask> tasks = new();
    // foreach (var charObj in Chars)
    // {
    //   tasks.Add(charObj.ShowChar(true, charObj.charTextValue));
    // }
    // await UniTask.WhenAll(tasks);

    await _levelManager.ManagerHiddenWords.CheckStatusRound();

  }

  public virtual void Destroy()
  {
    foreach (var charObj in Chars)
    {
      if (charObj.CharGameObject != null)
      {
        charObj.Destroy();
      }
    }
  }
}
