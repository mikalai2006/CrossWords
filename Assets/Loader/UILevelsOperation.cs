// using Assets;

// using Cysharp.Threading.Tasks;
// public struct DataResultLevelDialog
// {
//   public bool isOk;
// }
// public class UILevelsOperation : LocalAssetLoader
// {
//   public UILevelsOperation()
//   {
//   }

//   public async UniTask<DataResultLevelDialog> ShowAndHide()
//   {
//     var window = await Load();
//     var result = await window.ProcessAction();
//     Unload();
//     return result;
//   }

//   public UniTask<UILevels> Load()
//   {
//     return LoadInternal<UILevels>(Constants.UILabels.UI_LEVELS);
//   }

//   public void Unload()
//   {
//     UnloadInternal();
//   }
// }