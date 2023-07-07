using System.Collections.Generic;
using Assets;

using Cysharp.Threading.Tasks;

using UnityEngine;


public struct DataDialogResult
{
  public bool isOk;
}

public struct DataDialog
{
  public string title;
  public string message;
  public Sprite sprite;
  public bool showCancelButton;
  public List<ShopItem<GameEntity>> entities;
  public List<ShopItem<GameBonus>> bonuses;
}


public class DialogProvider : LocalAssetLoader
{
  private DataDialog _dataDialog;

  public DialogProvider(DataDialog dataDialog)
  {
    _dataDialog = dataDialog;
  }

  public async UniTask<DataDialogResult> ShowAndHide()
  {
    var loginWindow = await Load();
    var result = await loginWindow.ProcessAction(_dataDialog);
    Unload();
    return result;
  }

  public UniTask<UIDialog> Load()
  {
    return LoadInternal<UIDialog>(Constants.UILabels.UI_DIALOG);
  }

  public void Unload()
  {
    UnloadInternal();
  }
}