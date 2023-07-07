using System.Collections.Generic;
using UnityEngine;
using User;
using Loader;

public class StartApp : MonoBehaviour
{
  private LoadingScreenProvider loadingProvider => GameManager.Instance.LoadingScreenProvider;

  private async void Start()
  {
    // AppInfoContainer appInfoContainer = new();

    var loadingOperations = new Queue<ILoadingOperation>();
    loadingOperations.Enqueue(GameManager.Instance.AssetProvider);
    loadingOperations.Enqueue(new InitConfigOperation());
    loadingOperations.Enqueue(new InitUserOperation());
    loadingOperations.Enqueue(new UIAppOperation());
    // GameManager.Instance.SetAppInfo(appInfoContainer);
    await loadingProvider.LoadAndDestroy(loadingOperations);

    GameManager.Instance.ChangeState(GameState.StartApp);
  }
}
