using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Toggle.Core;
using Toggle.Core.Json;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Toggle.LevelEditor
{
    public class EditorToggleLevel
    {
        public double createTimeStamp;
        public double editTimeStamp;
        public string title;
        public bool isVerified;
        public bool isUploaded;
        public int uploadId;

        public int minimumClickCount = int.MaxValue;
    
        [JsonConverter(typeof(ToggleLevelConverter))]
        public ToggleLevel data;

        [JsonConverter(typeof(Base64Vector2IntListConverter))]
        public List<Vector2Int> generateOrders = new List<Vector2Int>();
    
        [JsonConverter(typeof(Base64Vector2IntListConverter))]
        public List<Vector2Int> orders = new List<Vector2Int>();
    }

    public class EditorLevelCollection
    {
        public int version = 2;
        public List<EditorToggleLevel> levels;

        public void SortByTime()
        {
            levels.Sort((x, y) => y.createTimeStamp.CompareTo(x.createTimeStamp));
        }
    }

    public class NewLevelEvent : UnityEvent<EditorToggleLevel>
    {

    }

    public class EditorDataManager : Singleton<EditorDataManager>
    {
        private static string _filePath;
        public static string filePath
        {
            get
            {
                if (_filePath == null)
                {
                    _filePath = Application.persistentDataPath + "/editorLevels.dat";
                }
                return _filePath;
            }
        }

        public NewLevelEvent newLevelEvent;


        private static EditorLevelCollection collection;
        private static bool isLoaded = false;

        private void Awake()
        {
            newLevelEvent = new NewLevelEvent();
            if (!isLoaded)
            {
                LoadAll();
            }
        }

        public static void ScheduleReloadAll()
        {
            isLoaded = false;
        }

        public void CreateEmptyLevel(string title, int columns, int rows)
        {
            StringBuilder stringBuilder = CommonUtils.GetStringBuilder();
            EditorToggleLevel editorToggleLevel = new EditorToggleLevel();
            editorToggleLevel.isVerified = false;
            editorToggleLevel.title = title;
            editorToggleLevel.createTimeStamp = CommonUtils.GetCurrentTimeStamp();
            editorToggleLevel.editTimeStamp = editorToggleLevel.createTimeStamp;

            ToggleLevel data = ToggleLevel.CreateEmpty(columns, rows);
            editorToggleLevel.data = data;

            collection.levels.Add(editorToggleLevel);
            newLevelEvent.Invoke(editorToggleLevel);

            SaveAll();
        }

        public void DeleteLevel(EditorToggleLevel toggleLevel)
        {
            collection.levels.Remove(toggleLevel);
            SaveAll();
        }

        public void LoadAll()
        {
            if (File.Exists(filePath))
            {
                string jsonText = File.ReadAllText(filePath);
                collection = JsonConvert.DeserializeObject<EditorLevelCollection>(jsonText);
                collection.SortByTime();
            }
            else
            {
                collection = new EditorLevelCollection();
                collection.levels = new List<EditorToggleLevel>();

                SaveAll();
            }
            isLoaded = true;
        }

        public void SaveAll()
        {
            collection.SortByTime();

            string jsonText = JsonConvert.SerializeObject(collection);
            File.WriteAllText(filePath, jsonText);
        }

        public int GetLevelCount()
        {
            return collection.levels.Count;
        }

        public EditorToggleLevel GetLevelData(int index)
        {
            return collection.levels[index];
        }

        // Use this for initialization
        void Start()
        {
            Debug.Log($"Timestamp: {CommonUtils.TimeStampToDateTime(CommonUtils.GetCurrentTimeStamp())}");
        }
    }
}