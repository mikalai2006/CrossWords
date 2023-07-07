// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using Loader;
// using UnityEngine;
// using UnityEngine.UIElements;

// public class UILevels : UILocaleBase
// {
//   [SerializeField] private UIDocument _uiDoc;
//   [SerializeField] private VisualTreeAsset LevelBlok;
//   [SerializeField] private VisualTreeAsset LevelSection;
//   [SerializeField] private VisualElement _root;
//   private GameObject _enviromnment;
//   [SerializeField] private ScrollView _listLevels;
//   // [SerializeField] private ScrollView _listCompletedLevels;
//   private TaskCompletionSource<DataResultLevelDialog> _processCompletionSource;
//   private DataResultLevelDialog _result;

//   private void Awake()
//   {

//     LevelManager.OnInitLevel += Hide;
//   }

//   private void OnDestroy()
//   {

//     LevelManager.OnInitLevel -= Hide;
//   }

//   public virtual void Start()
//   {
//     _root = _uiDoc.rootVisualElement;
//     _root.Q<VisualElement>("LevelsBlokWrapper").style.backgroundColor = new StyleColor(_gameSettings.Theme.bgColor);

//     var exitBtn = _root.Q<Button>("ExitBtn");
//     exitBtn.clickable.clicked += () => ClickClose();

//     _listLevels = _root.Q<ScrollView>("ListLevels");
//     _listLevels.Clear();

//     // _listCompletedLevels = _root.Q<ScrollView>("ListCompletedLevels");
//     // _listCompletedLevels.Clear();

//     FillLevels();

//     base.Initialize(_root);
//   }

//   private void Hide()
//   {
//     _root.style.display = DisplayStyle.None;
//   }

//   private async void FillLevels()
//   {
//     // var dataGame = _gameManager.DataManager.DataGame;
//     // int minLevel = _gameSettings.GameLevelWords.FindIndex(t => t.idLevel == dataGame.lastLevel);
//     // var allowLevels = minLevel != -1
//     //   ? _gameSettings.GameLevelWords.GetRange(minLevel, _gameSettings.GameLevelWords.Count - minLevel)
//     //   : _gameSettings.GameLevelWords;

//     // foreach (var level in allowLevels)
//     // {
//     //   var section = LevelSection.Instantiate();
//     //   section.Q<Label>("Name").text = await Helpers.GetLocaledString(level.text.title);
//     //   var sectionListWords = section.Q<VisualElement>("ListWords");
//     //   sectionListWords.Clear();

//     //   for (int i = 0; i < level.words.Count; i++)
//     //   {
//     //     var currentWord = level.words[i];
//     //     var levelData = dataGame != null
//     //       ? dataGame.levels.Find(t => t.id == level.idLevel && t.word == currentWord)
//     //       : null;
//     //     var userRate = dataGame != null
//     //       ? dataGame.rate
//     //       : 0;
//     //     var levelBlok = LevelBlok.Instantiate();

//     //     var btnGo = levelBlok.Q<Button>("GoBtn");
//     //     btnGo.clickable.clicked += () =>
//     //     {
//     //       InitLevel(level, currentWord);
//     //     };
//     //     var success = levelBlok.Q<VisualElement>("Success");
//     //     success.style.backgroundImage = new StyleBackground(_gameSettings.spriteCheck);
//     //     success.style.display = DisplayStyle.None;
//     //     var lockElement = levelBlok.Q<VisualElement>("Lock");
//     //     lockElement.style.backgroundImage = new StyleBackground(_gameSettings.spriteLock);
//     //     lockElement.style.display = DisplayStyle.None;
//     //     var description = levelBlok.Q<Label>("Description");
//     //     var progressBlok = levelBlok.Q<VisualElement>("ProgressBarBox");
//     //     levelBlok.Q<Label>("Name").text = currentWord;

//     //     if (levelData == null || levelData.openWords.Count == 0)
//     //     {
//     //       if (level.minRate <= userRate)
//     //       {
//     //         btnGo.text = await Helpers.GetLocaledString("start");
//     //         lockElement.style.display = DisplayStyle.None;
//     //       }
//     //       else
//     //       {
//     //         btnGo.style.display = DisplayStyle.None;
//     //         lockElement.style.display = DisplayStyle.Flex;
//     //       }
//     //       description.style.display = DisplayStyle.None;
//     //       progressBlok.style.display = DisplayStyle.None;
//     //     }
//     //     else
//     //     {
//     //       var dataPlural = new Dictionary<string, int> {
//     //         {"count",  levelData.openWords.Count},
//     //         {"count2", levelData.countWords},
//     //       };
//     //       var arguments = new[] { dataPlural };
//     //       var textCountWords = await Helpers.GetLocalizedPluralString(
//     //           new UnityEngine.Localization.LocalizedString(Constants.LanguageTable.LANG_TABLE_LOCALIZE, "foundcountword"),
//     //           arguments,
//     //           dataPlural
//     //           );
//     //       description.text = string.Format(
//     //         "{0}",
//     //         textCountWords
//     //         );

//     //       var procentOpen = levelData.openWords.Count * 100 / levelData.countWords;
//     //       var progressBar = levelBlok.Q<VisualElement>("ProgressBar");
//     //       progressBar.style.width
//     //         = new StyleLength(new Length(procentOpen, LengthUnit.Percent));
//     //       if (levelData.openWords.Count == levelData.countWords)
//     //       {
//     //         progressBlok.style.display = DisplayStyle.None;
//     //         btnGo.style.display = DisplayStyle.None;
//     //         success.style.display = DisplayStyle.Flex;
//     //         description.text = await Helpers.GetLocaledString("сompleted");
//     //       }

//     //       btnGo.text = await Helpers.GetLocaledString("continue");
//     //     }

//     //     sectionListWords.Add(levelBlok);
//     //   }

//     //   _listLevels.Add(section);
//     // }
//   }

//   // private async void InitLevel(GameLevelWord wordConfig)
//   // {
//   //   if (!_gameManager.environment.Scene.IsValid())
//   //   {
//   //     // _gameManager.ChangeState(GameState.CloseLevel);

//   //     var operations = new Queue<ILoadingOperation>();
//   //     operations.Enqueue(new GameInitOperation());
//   //     await _gameManager.LoadingScreenProvider.LoadAndDestroy(operations);
//   //   }
//   //   ClickClose();
//   //   _gameManager.LevelManager.InitLevel(wordConfig);
//   // }

//   private void ClickClose()
//   {
//     _result.isOk = false;
//     _processCompletionSource.SetResult(_result);
//   }

//   // public void Init(GameObject environment)
//   // {
//   //   _enviromnment = environment;
//   // }

//   public async UniTask<DataResultLevelDialog> ProcessAction()
//   {

//     _processCompletionSource = new TaskCompletionSource<DataResultLevelDialog>();

//     return await _processCompletionSource.Task;
//   }
// }
