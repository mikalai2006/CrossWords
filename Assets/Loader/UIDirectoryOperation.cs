using Assets;

using Cysharp.Threading.Tasks;

public class UIDirectoryOperation : LocalAssetLoader
{
  public UIDirectoryOperation()
  {
  }

  public async UniTask<DataDialogResult> ShowAndHide()
  {
    var window = await Load();
    var result = await window.ProcessAction();
    Unload();
    return result;
  }

  public UniTask<UIDirectory> Load()
  {
    return LoadInternal<UIDirectory>(Constants.UILabels.UI_DIRECTORY);
  }

  public void Unload()
  {
    UnloadInternal();
  }
}