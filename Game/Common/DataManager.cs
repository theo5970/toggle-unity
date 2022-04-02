using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Toggle.Game.Data;
using Toggle.Utils;
using UnityEngine;


namespace Toggle.Game.Common
{
    // 파일에 저장될 세이브데이터 클래스
    [System.Serializable]
    public class GameSave
    {
        public bool isAutoSave = true;
        public bool soundPlay = true;
        public long playedTimeTicks;
        public long previousCloudSaveTime;
        public string version = "";
        public int playCount = 0;
        public int hintCount = 3;
        public int clickCount;
        public string sessionId = "";
        public SystemLanguage language;
        public Dictionary<string, LevelPackSave> packSaves = new Dictionary<string, LevelPackSave>();
    }



    public class DataManager : Singleton<DataManager>
    {
        public GameSave saveData { get; private set; }

        public bool isLoaded { get; private set; }

        public event Action onReload;

        private static string _filePath;
        public static string filePath
        {
            get
            {
                if (_filePath == null)
                {
                    _filePath = Application.persistentDataPath + "/save.dat";
                }
                return _filePath;
            }
        }

        private long ticksAtUpdate;
        private LevelManager levelManager;

        private void Awake()
        {
            isLoaded = false;
            ticksAtUpdate = DateTime.Now.Ticks;
        }

        // 세이브파일 불러오기
        public void Load()
        {
            levelManager = LevelManager.Instance;
            if (File.Exists(filePath))
            {
                LoadExists();
            }
            else
            {
                // 존재하지 않으면 새로 만들기
                CreateGameSave();
                Save();
            }

            if (!isLoaded)
            {
                isLoaded = true;
            }
            else
            {
                onReload?.Invoke();
            }
        }

        private bool shouldRewriteFile = false;
        private void LoadExists()
        {
            shouldRewriteFile = false;
            string json = File.ReadAllText(filePath);
            saveData = JsonConvert.DeserializeObject<GameSave>(json);
            AudioListener.volume = saveData.soundPlay ? 1 : 0;

            ProcessLevelSaves();
            if (shouldRewriteFile) Save();     // 변조가 발생했을 경우 복구한 내용으로 다시 저장.
        }

        private void ProcessLevelSaves()
        {
            foreach (LevelPack pack in levelManager.GetPacks())
            {
                if (!saveData.packSaves.ContainsKey(pack.name))
                {
                    saveData.packSaves.Add(pack.name, CreatePackSave(pack));
                    shouldRewriteFile = true;
                }
            }

            if (saveData.version != Application.version)
            {
                saveData.version = Application.version;
                shouldRewriteFile = true;
            }

            foreach (string packName in saveData.packSaves.Keys.ToArray())
            {
                LevelPackSave packSave = saveData.packSaves[packName];
                if (!levelManager.ContainsLevelPack(packSave.packName))
                {
                    saveData.packSaves.Remove(packName);
                    continue;
                }

                int packLevelCount = levelManager.GetLevelCount(packSave.packName);
                int countDiff = packSave.states.Count - packLevelCount;
                countDiff = Mathf.Abs(countDiff);

                bool isLevelCountChanged = (countDiff > 0);

                if (packSave.states.Count > packLevelCount)
                {
                    packSave.states.RemoveRange(packLevelCount, countDiff);
                }
                else
                {
                    for (int k = 0; k < countDiff; k++)
                    {
                        packSave.states.Add(0);
                    }
                }

                if (isLevelCountChanged)
                {
                    shouldRewriteFile = true;
                }
                else
                {
                    if (ProcessPackSaveViolation(packSave))
                    {
                        Debug.LogError($"팩 세이브가 변조됨! 팩 이름: {packName}");
                        shouldRewriteFile = true;
                    }
                }
            }


        }

        private bool ProcessPackSaveViolation(LevelPackSave packSave)
        {
            if (packSave.checksum != packSave.CalculateChecksum())
            {

                for (int i = 0; i < packSave.states.Count; i++)
                {
                    // 변조 되었을 경우 상태 모두 초기화
                    packSave.states[i] = 0; // (시도하지 않음)
                }

                packSave.checksum = packSave.CalculateChecksum();
                return true;
            }

            return false;
        }

        private void CreateGameSave()
        {
            saveData = new GameSave();
            saveData.version = Application.version;
            saveData.previousCloudSaveTime = 0;
            saveData.language = Application.systemLanguage;

            foreach (var levelPack in levelManager.GetPacks())
            {
                saveData.packSaves.Add(levelPack.name, CreatePackSave(levelPack));
            }
        }

        private LevelPackSave CreatePackSave(LevelPack pack)
        {
            LevelPackSave packSave = new LevelPackSave
            {
                packName = pack.name,
                states = new List<byte>()
            };

            for (int i = 0; i < pack.levels.Count; i++)
            {
                packSave.states.Add(0);
            }
            packSave.checksum = packSave.CalculateChecksum();

            return packSave;
        }

        public void UpdatePlayedTime()
        {
            saveData.playedTimeTicks += DateTime.Now.Ticks - ticksAtUpdate;
            ticksAtUpdate = DateTime.Now.Ticks;
        }

        // 세이브파일 저장하기
        public void Save()
        {
            UpdatePlayedTime();

            foreach (var packSaveKeyValuePair in saveData.packSaves)
            {
                LevelPackSave packSave = packSaveKeyValuePair.Value;
                packSave.checksum = packSave.CalculateChecksum();
            }

            string jsonText = JsonConvert.SerializeObject(saveData);
            File.WriteAllText(filePath, jsonText);
        }



        public LevelPackSave GetPackSave(string packName)
        {
            return saveData.packSaves[packName];
        }

        void Start()
        {
            Load();

            TimeSpan playedTime = new TimeSpan(saveData.playedTimeTicks);
            Debug.Log(string.Format("총 플레이 시간: {0:F0}시간 {1:F0}분 {2:F0}초", playedTime.TotalHours, playedTime.Minutes, playedTime.Seconds));
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}