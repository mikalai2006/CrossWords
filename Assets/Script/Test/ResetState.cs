using UnityEngine;

public class ResetState : MonoBehaviour
{
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  bool ctrl = false;

  private async void Update()
  {
    if (Input.GetKeyDown(KeyCode.LeftControl))
    {
      ctrl = true;
      Debug.Log("Press Ctrl");
    }

    if (Input.GetKeyDown(KeyCode.F9) && ctrl && _gameManager.LevelManager == null)
    {
      await _gameManager.StateManager.Reset();

      _gameManager.DataManager.Save(true);
      Debug.Log("Reset state");
      ctrl = false;
    }
  }
}
