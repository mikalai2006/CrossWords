using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonDirectory : BaseButton
{
  #region UnityMethods
  protected override void Awake()
  {
    base.Awake();

    spriteBg.sprite = _gameSetting.spriteDirectory;
    spriteMask.sprite = _gameSetting.spriteDirectory;

    interactible = false;
    // isShowCounter = false;

    StateManager.OnChangeState += SetValue;
  }
  protected override void OnDestroy()
  {
    base.OnDestroy();
    StateManager.OnChangeState -= SetValue;
  }
  #endregion

  public override void ChangeTheme()
  {
    base.ChangeTheme();

    spriteBg.color = _gameManager.Theme.entityColor;
  }

  public override void RunHint()
  {
    _gameManager.ChangeState(GameState.StopEffect);
  }

  public override void SetValue(StateGame state)
  {
    value = state.activeDataGame.activeLevel.openWords.Count;

    base.SetValue(state);
  }


  public override async void OnPointerDown(PointerEventData eventData)
  {
    base.OnPointerDown(eventData);

    _gameManager.InputManager.Disable();
    var dialogWindow = new UIDirectoryOperation();
    var result = await dialogWindow.ShowAndHide();
    _gameManager.InputManager.Enable();
  }


  public override void SetValueProgressBar(StateGame state)
  {
    var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)state.activeDataGame.activeLevel.openWords.Count / (float)_levelManager.ManagerHiddenWords.AllowPotentialWords.Count;
    spriteProgress.transform
      .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBounce);
  }

}
