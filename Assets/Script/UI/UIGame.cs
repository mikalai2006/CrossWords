// using UnityEngine;
// using UnityEngine.UIElements;

// public class UIGame : UILocaleBase
// {
//   [SerializeField] private UIDocument _uiDoc;
//   private Button _shuffleButton;
//   private GameSetting GameSetting;
//   [SerializeField] private AudioManager _audioManager => GameManager.Instance.audioManager;

//   private void Awake()
//   {
//     UISettings.OnChangeLocale += ChangeLocale;
//   }

//   private void OnDestroy()
//   {
//     UISettings.OnChangeLocale -= ChangeLocale;
//   }

//   public virtual void Start()
//   {
//     GameSetting = GameManager.Instance.GameSettings;

//     _shuffleButton = _uiDoc.rootVisualElement.Q<Button>("ShuffleBtn");
//     _shuffleButton.clickable.clicked += () =>
//     {
//       GameManager.Instance.LevelManager.ShuffleChars();
//       // _audioManager.PlayClipEffect();
//     };

//     base.Localize(_uiDoc.rootVisualElement);
//   }

//   private void ClickShuffle()
//   {
//   }

//   private void ChangeLocale()
//   {
//     base.Localize(_uiDoc.rootVisualElement);
//   }

// }
