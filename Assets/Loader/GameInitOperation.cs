using System;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Localization;
// using UnityEngine.SceneManagement;

namespace Loader
{
  public class GameInitOperation : ILoadingOperation
  {
    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {
      onProgress?.Invoke(0.1f);

      var t = await Helpers.GetLocaledString("loading");
      onSetNotify?.Invoke(t);
      var environment = await GameManager.Instance.AssetProvider.LoadSceneAdditive(Constants.Scenes.SCENE_GAME);
      var rootObjects = environment.Scene.GetRootGameObjects();

      LevelManager LevelManager = GameObject.FindGameObjectWithTag("LevelManager")?.GetComponent<LevelManager>();

      if (LevelManager != null)
      {
        GameManager.Instance.InitGameLevel(LevelManager, environment);
        // GameManager.Instance.LevelManager = LevelManager;
        // GameManager.Instance.environment = environment;
        // // LevelManager.CreateLevel();
      }

      // UIGameAside UIGameAside = GameObject.FindGameObjectWithTag("GameAside")?.GetComponent<UIGameAside>();

      // if (UIGameAside != null)
      // {
      //     UIGameAside.Init(environment);
      // }

      // editorGame.Init(environment);
      // editorGame.BeginNewGame();
      onProgress?.Invoke(.9f);
    }
  }
}