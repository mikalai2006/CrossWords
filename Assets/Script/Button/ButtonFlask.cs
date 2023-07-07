using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonFlask : BaseButton
{
  private int countOpenChar;

  #region UnityMethods
  protected override void Awake()
  {
    base.Awake();

    spriteBg.sprite = configButton.sprite;
    spriteMask.sprite = configButton.sprite;

    interactible = false;

    StateManager.OnChangeState += SetValue;
    StateManager.OnGenerateBonus += GenerateBonus;
  }
  protected override void OnDestroy()
  {
    base.OnDestroy();

    StateManager.OnChangeState -= SetValue;
    StateManager.OnGenerateBonus -= GenerateBonus;
  }
  #endregion


  public override void SetValue(StateGame state)
  {
    countOpenChar = state.activeDataGame.activeLevel.bonusCount.charInOrder;

    // base.SetValue(state);
    SetValueProgressBar(state);
  }

  public override void SetValueProgressBar(StateGame state)
  {
    // base.SetValueProgressBar(state);
    var maxValue = _gameManager.PlayerSetting.bonusCount.charInOrder;

    var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)countOpenChar / maxValue;

    spriteProgress.transform
      .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBounce);
  }


  public override async void OnPointerDown(PointerEventData eventData)
  {
    base.OnPointerDown(eventData);

    var title = await Helpers.GetLocaledString(configButton.text.title);
    var message = await Helpers.GetLocaledString(configButton.text.description);
    var dialog = new DialogProvider(new DataDialog()
    {
      sprite = configButton.sprite,
      title = title,
      message = message
    });

    _gameManager.InputManager.Disable();
    await dialog.ShowAndHide();
    _gameManager.InputManager.Enable();

  }

  private async void GenerateBonus()
  {
    int rand = UnityEngine.Random.Range(0, 100);

    if (_gameManager.LevelManager.IsEndLevel()) return;

    Sprite sprite;
    TextLocalize localizeObj;
    if (rand <= 50)
    {
      var allBonuses = _gameManager.ResourceSystem.GetAllBonus().Where(t => t.isMybeGenerate);
      var randBonus = allBonuses.OrderBy(t => UnityEngine.Random.value).ElementAt(0);
      _gameManager.StateManager.UseBonus(1, randBonus.typeBonus);

      sprite = randBonus.sprite;
      localizeObj = randBonus.text;
    }
    else
    {
      var allEntity = _gameManager.ResourceSystem.GetAllEntity().Where(t => t.isUseGenerator);
      var randEntity = allEntity.OrderBy(t => UnityEngine.Random.value).ElementAt(0);

      _gameManager.audioManager.PlayClipEffect(randEntity.soundAddEntity);
      _gameManager.StateManager.UseHint(1, randEntity.typeEntity);

      sprite = randEntity.sprite;
      localizeObj = randEntity.text;
    }

    if (!localizeObj.title.IsEmpty && _gameManager.AppInfo.setting.dod)
    {
      // Show message
      _gameManager.InputManager.Disable();

      var title = await Helpers.GetLocaledString("generator_message_t");
      var titleObj = await Helpers.GetLocaledString(localizeObj.title);
      var message = await Helpers.GetLocalizedPluralString("generator_message_d", new Dictionary<string, string>(){
      { "name", titleObj }
    });
      var dialogConfirm = new DialogProvider(new DataDialog()
      {
        title = title,
        sprite = sprite,
        message = message
      });

      await dialogConfirm.ShowAndHide();
      _gameManager.InputManager.Enable();
    }
  }

}
