
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
  private Dictionary<BaseWord, bool> _crosswords;
  public Dictionary<BaseWord, bool> Crosswords => _crosswords;
  public bool isOpen => _chars.Find(t => !t.isOpen) == null;
  public bool isMayBeOpen => _chars.Find(t => !t.isOpen && !t.isHinted) == null;

  protected string _word;
  public string Word => _word;

  public BaseWord()
  {
    _chars = new();
    _crosswords = new();
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
    // UnityEngine.Debug.Log($"AutoOpen word::: {Word}|[{_word}]|[{_levelManager.ManagerHiddenWords.OpenCrossWords.Count}]");
    if (!_levelManager.ManagerHiddenWords.OpenCrossWords.ContainsKey(_word))
      _levelManager.ManagerHiddenWords.OpenCrossWords.Add(_word, 1);
    // UnityEngine.Debug.Log($"AutoOpen word:::[{_levelManager.ManagerHiddenWords.OpenCrossWords.Count}]");
    // if (!_levelManager.ManagerHiddenWords.OpenNeedWords.ContainsKey(_word)) _levelManager.ManagerHiddenWords.OpenNeedWords.Add(_word, 1);

    List<UniTask> tasks = new();
    foreach (var charObj in Chars)
    {
      if (charObj.OccupiedNode.StateNode.HasFlag(StateNode.Hint))
        tasks.Add(charObj.CharGameObject.ShowChar(true, charObj.CharValue));
    }
    await UniTask.WhenAll(tasks);

    _levelManager.ManagerHiddenWords.CheckStatusRound();

  }


  public void AddCrossWord(BaseWord wordHidden)
  {
    string crossWord = wordHidden.Word;
    if (!_crosswords.ContainsKey(wordHidden))
    {
      _crosswords.Add(wordHidden, false);
      wordHidden.AddCrossWord(this);
    }
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
