using Assets;

using Cysharp.Threading.Tasks;

public class UIChampionOperation : LocalAssetLoader
{
  public async UniTask<DataDialogResult> ShowAndHide()
  {
    var window = await Load();
    var result = await window.ProcessAction();
    Unload();
    return result;
  }

  public UniTask<UIChampion> Load()
  {
    return LoadInternal<UIChampion>(Constants.UILabels.UI_CHAMPION);
  }

  public void Unload()
  {
    UnloadInternal();
  }
}