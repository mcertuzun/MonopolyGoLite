using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    public class LocalSaveService : ISaveService
    {
        readonly string _filePath;

        public LocalSaveService(string filePath = null)
        {
            _filePath = filePath ?? Application.persistentDataPath + "/save.json";
        }

        public bool HasSave()
        {
            return System.IO.File.Exists(_filePath);
        }

        public SaveData Load()
        {
            string json = System.IO.File.ReadAllText(_filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public void Save(SaveData data)
        {
            data.lastSavedAt = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(_filePath, json);
        }

        public void Delete()
        {
            if (System.IO.File.Exists(_filePath))
                System.IO.File.Delete(_filePath);
        }
    }
}
