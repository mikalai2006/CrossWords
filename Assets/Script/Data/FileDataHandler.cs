using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FileDataHandler
{
  private readonly string _dataDirPath;
  //   private readonly string _dataFileName;
  private readonly string _nameFile;
  private readonly bool _useEncryption = false;
  private readonly string _encryptionCodeWord = "word";

  public FileDataHandler(string dataDirPath, string _fileName, bool useEncryption)
  {
    this._dataDirPath = dataDirPath;
    this._useEncryption = useEncryption;
    this._nameFile = _fileName;
  }

  public async UniTask<StateGame> LoadData()
  {
    string fullPath = Path.Combine(_dataDirPath, _nameFile);

    StateGame loadedData = null;

    if (File.Exists(fullPath))
    {
      try
      {
        string dataToLoad = "";

        using (FileStream stream = new FileStream(fullPath, FileMode.Open))
        {
          using (StreamReader reader = new StreamReader(stream))
          {
            dataToLoad = await reader.ReadToEndAsync();
          }
        }

        loadedData = JsonUtility.FromJson<StateGame>(dataToLoad);

        if (_useEncryption)
        {
          dataToLoad = EncryptDecrypt(dataToLoad);
        }

      }
      catch (Exception e)
      {
        Debug.LogError("Error Load file::: " + fullPath + "\n" + e);
      }
    }

    return loadedData;
  }

  public void SaveData(StateGame data)
  {
    string fullPath = Path.Combine(_dataDirPath, _nameFile);

    try
    {
      Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

      string dataToStore = JsonUtility.ToJson(data);

      if (_useEncryption)
      {
        dataToStore = EncryptDecrypt(dataToStore);
      }

      using (FileStream stream = new FileStream(fullPath, FileMode.Create))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          writer.Write(dataToStore);
        }
      }
    }
    catch (Exception e)
    {
      Debug.LogError("Error Save file::: " + fullPath + "\n" + e);
    }
  }

  private string EncryptDecrypt(string data)
  {
    string modifierData = "";

    for (int i = 0; i < data.Length; i++)
    {
      modifierData += (char)(data[i] ^ _encryptionCodeWord[i % _encryptionCodeWord.Length]);
    }

    return modifierData;
  }
}
