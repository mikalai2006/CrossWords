// Assets/Editor/JSLibFileCreator.cs
using System.IO;
using UnityEditor;

public class JSLibFileCreator
{
  [MenuItem("Assets/Create/JS Script", priority = 80)]
  private static void CreateJSLibFile()
  {
    // Шаблон скрипта, что бы файл не был пустым изначально
    var asset =
        "mergeInto(LibraryManager.library,\n" +
        "{\n" +
          "\t// Your code here\n" +
        "});";
    // Берем путь до текущей открытой папки в окне Project
    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    if (path == "")
    {
      path = "Assets";
    }
    else if (Path.GetExtension(path) != "")
    {
      path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
    }
    // Создаем .jslib файл с шаблоном
    ProjectWindowUtil.CreateAssetWithContent(AssetDatabase.GenerateUniqueAssetPath(path + "/JSScript.jslib"), asset);
    // Сохраняем ассеты
    AssetDatabase.SaveAssets();
  }
}