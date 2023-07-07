using Assets;

using Cysharp.Threading.Tasks;

public class UIDashboardOperation : LocalAssetLoader
{

  public async UniTask<DataDialogResult> ShowAndHide()
  {
    var window = await Load();
    var result = await window.ProcessAction();
    Unload();
    return result;
  }

  public UniTask<UIDashboard> Load()
  {
    return LoadInternal<UIDashboard>(Constants.UILabels.UI_DASHBOARD);
  }

  public void Unload()
  {
    UnloadInternal();
  }
}