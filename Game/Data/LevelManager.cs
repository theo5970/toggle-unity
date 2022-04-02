using System.Collections.Generic;
using Newtonsoft.Json;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Data
{
    public class LevelManager : ClassSingleton<LevelManager>
    {
        private bool isLoaded = false;
        private Dictionary<string, LevelPack> levelPacks = new Dictionary<string, LevelPack>();
        private TextAsset[] levelPackTextAssets;

        // 모든 클래식 레벨 불러오기
        private void LoadAll()
        {
            levelPackTextAssets = Resources.LoadAll<TextAsset>("LevelPacks");

            for (int i = 0; i < levelPackTextAssets.Length; i++)
            {
                TextAsset textAsset = levelPackTextAssets[i];

                LevelPack levelPack = JsonConvert.DeserializeObject<LevelPack>(textAsset.text);
                levelPacks.Add(levelPack.name, levelPack);
            }
            isLoaded = true;
        }

        // 현재 로드된 레벨 개수 가져오기
        public int GetLevelCount(string packName)
        {
            return levelPacks[packName].levels.Count;
        }

        // 특정 레벨 데이터 가져오기
        public ToggleClassicLevel GetLevel(string packName, int index)
        {
            return levelPacks[packName].levels[index];
        }

        public bool ContainsLevelPack(string packName)
        {
            return levelPacks.ContainsKey(packName);
        }
        public IEnumerable<LevelPack> GetPacks()
        {
            return levelPacks.Values;
        }

        protected override void Init()
        {
            if (!isLoaded)
            {
                LoadAll();
            }
        }
    }
}