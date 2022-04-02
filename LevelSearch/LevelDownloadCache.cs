using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toggle.Client;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.LevelSearch
{
    public class SavedOnlineLevel
    {
        public OnlineLevel info;
        public bool isLiked;
        public string levelCode;
    }

    public class LevelDownloadCache : ClassSingleton<LevelDownloadCache>
    {
        private string filePath;

        private Dictionary<int, SavedOnlineLevel> cacheDic;

        private bool isLoaded = false;
        public void Load()
        {
            if (!isLoaded)
            {
                filePath = Path.Combine(Application.persistentDataPath, "downloadedLevels.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    cacheDic = JsonConvert.DeserializeObject<Dictionary<int, SavedOnlineLevel>>(json);
                }
                else
                {
                    cacheDic = new Dictionary<int, SavedOnlineLevel>();
                    Save();
                }

                isLoaded = true;
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(cacheDic, Formatting.None);
            File.WriteAllText(filePath, json);
        }

        public void Add(int levelId, SavedOnlineLevel savedOnlineLevel)
        {
            if (!cacheDic.ContainsKey(levelId))
            {
                cacheDic.Add(levelId, savedOnlineLevel);
            }
        }

        public bool TryGet(int levelId, out SavedOnlineLevel savedOnlineLevel)
        {
            return cacheDic.TryGetValue(levelId, out savedOnlineLevel);
        }
    }
}
