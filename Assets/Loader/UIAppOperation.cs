using System;

using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;

namespace Loader
{
  public class UIAppOperation : ILoadingOperation
  {
    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {

      await LocalizationSettings.InitializationOperation;

      var t = await Helpers.GetLocaledString("loading");
      onSetNotify?.Invoke(t);

      onProgress?.Invoke(0.9f);

      var environment = await GameManager.Instance.AssetProvider.LoadAsset(Constants.UILabels.UI_APP);

      if (environment.TryGetComponent(out UIApp component) == false)
        throw new NullReferenceException("Object of type UIApp is null");

      component.Init(environment);

      onProgress?.Invoke(1f);

    }
  }
}