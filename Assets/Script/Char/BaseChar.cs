using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class CharBase
{
  protected GameSetting _gameSetting => GameManager.Instance.GameSettings;
  protected StateManager _stateManager => GameManager.Instance.StateManager;
  protected LevelManager _levelManager => GameManager.Instance.LevelManager;
  protected GameManager _gameManager => GameManager.Instance;
  protected UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> _asset;

  protected string charValue;
  public string CharValue => charValue;
  public GridNode OccupiedNode;
  private WordHidden _occupiedWord;
  public WordHidden OccupiedWord => _occupiedWord;
  public bool isOpen => OccupiedNode.StateNode.HasFlag(StateNode.Open);
  public bool isHinted => OccupiedNode.StateNode.HasFlag(StateNode.Hint);


  public virtual void Init(string _char, GridNode node, WordHidden wordHidden)
  {
    charValue = _char;
    _occupiedWord = wordHidden;
  }

  public virtual async UniTask Draw()
  {
    await UniTask.Yield();
  }

  public virtual void SetOpen()
  {
    OccupiedNode.SetOpen();
  }

  public virtual void Destroy()
  {
  }
}