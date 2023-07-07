using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  public float percent;
  public int totalNeedWords;
  public int countNeedOpenWords;
  public int currentRate;
  public GamePlayerSetting prevSetting;
  public GamePlayerSetting playerSetting;
  public GamePlayerSetting nextSetting;

  #region Unity methods
  // private void Awake() {
  //   StateManager.OnChangeState +=;
  // }

  // private void OnDestroy() {

  // }
  #endregion

  public void Refresh()
  {
    var activeDataGame = _stateManager.stateGame.activeDataGame;
    var allSettings = _gameSetting.PlayerSettings.OrderBy(t => t.countFindWordsForUp).ToList();

    int indexPlayerSetting = allSettings.FindIndex(t => t.idPlayerSetting == activeDataGame.rank);

    playerSetting = allSettings[indexPlayerSetting];
    prevSetting = indexPlayerSetting > 0 && indexPlayerSetting < allSettings.Count - 1
      ? allSettings[indexPlayerSetting - 1]
      : null;
    nextSetting = indexPlayerSetting > -1 && indexPlayerSetting < allSettings.Count - 1
      ? allSettings[indexPlayerSetting + 1]
      : null;

    totalNeedWords = playerSetting.countFindWordsForUp - (prevSetting != null ? prevSetting.countFindWordsForUp : 0);
    currentRate = activeDataGame.rate - (prevSetting != null ? prevSetting.countFindWordsForUp : 0);

    countNeedOpenWords = totalNeedWords - currentRate;

    percent = GetPercent(currentRate);
  }

  public float GetPercent(int countWords)
  {
    float procentComplete = countWords * 100f / (float)totalNeedWords;
    float result = procentComplete;

    return (float)System.Math.Round(result, 2);
  }

#if UNITY_EDITOR
  public override string ToString()
  {
    return string.Format(
      "needWords={0}\r\nplayer={1}\r\nPrevplayer={2}\r\nNextplayer={3}\r\npercent={4}\r\ncountNeedOpenWords={5}\r\ncurrentRate={6}",
      totalNeedWords,
      playerSetting?.idPlayerSetting,
      prevSetting?.idPlayerSetting,
      nextSetting?.idPlayerSetting,
      percent,
      countNeedOpenWords,
      currentRate
      );
  }
#endif
}