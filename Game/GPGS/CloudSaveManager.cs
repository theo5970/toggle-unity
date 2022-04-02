using System;
using System.Collections;
using System.IO;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

using Toggle.Game.Common;
using Toggle.LevelEditor;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.GPGS
{
    [Flags]
    public enum CloudSaveError
    {
        None = 0,
        NotAuthenticated = 2,
        FileNotFound = 4,
        HeaderWrong = 8
    }
    public class CloudSaveManager : Singleton<CloudSaveManager>
    {

        public const string FileName = "toggle.save";

        public event Action<CloudSaveError> onError;
        public event Action<SavedGameRequestStatus> onLoad;
        public event Action<SavedGameRequestStatus> onSave;

#if UNITY_ANDROID
        #region 클라우드 세이브 열기
        public void OpenCloudSave(Action<SavedGameRequestStatus, ISavedGameMetadata> onSavedGameOpened)
        {
            CloudSaveError error = CloudSaveError.None;
            if (!Social.localUser.authenticated)
            {
                error |= CloudSaveError.NotAuthenticated;
            }

            if (error != CloudSaveError.None)
            {
                onError?.Invoke(error);
            }
            else
            {
                ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
                savedGameClient.OpenWithAutomaticConflictResolution(FileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, onSavedGameOpened);
            }
        }
        #endregion

        #region 세이브 저장
        public void SaveToCloud()
        {
            OpenCloudSave(OnSaveResponse);
        }

        private void OnSaveResponse(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                DataManager.Instance.saveData.previousCloudSaveTime = DateTime.Now.Ticks;
                DataManager.Instance.UpdatePlayedTime();
                TimeSpan playedTime = new TimeSpan(DataManager.Instance.saveData.playedTimeTicks);

                byte[] data = SerializeSave();
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                    .WithUpdatedDescription("Last save: " + DateTime.Now.ToString())
                    .WithUpdatedPlayedTime(playedTime)
                    .Build();

                ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
                savedGameClient.CommitUpdate(metadata, update, data, SaveCallback);
            }
            else
            {
                onSave?.Invoke(status);
            }
        }

        private void SaveCallback(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                dataManager.saveData.previousCloudSaveTime = DateTime.Now.Ticks;
                dataManager.Save();
            }
            onSave?.Invoke(status);
        }
        #endregion

        #region 세이브 로드
        public void LoadFromCloud()
        {
            OpenCloudSave(OnLoadResponse);
        }

        private void OnLoadResponse(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
                savedGameClient.ReadBinaryData(metadata, LoadCallback);
            }
            else
            {
                onLoad?.Invoke(status);
            }
        }

        private void LoadCallback(SavedGameRequestStatus status, byte[] data)
        {
            if (data.Length == 0)
            {
                onError?.Invoke(CloudSaveError.FileNotFound);
            }
            else
            {
                if (DeserializeSave(data))
                {
                    DataManager.Instance.Load();
                    onLoad?.Invoke(status);
                }
            }
        }
        #endregion

        #region 세이브 직렬화 및 역직렬화
        private static readonly int FirstHeader = 0x7458656F;    // 'theo'

        private static class Properties // 프로퍼티 정의
        {
            public const int PlaySave = 0x00AB0001;
            public const int Editor = 0x00AB0002;
        }

        private byte[] SerializeSave()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    // Header
                    writer.Write(FirstHeader);

                    // PlaySave
                    byte[] playSaveBytes = File.ReadAllBytes(DataManager.filePath);
                    writer.Write(Properties.PlaySave);
                    writer.Write(playSaveBytes.Length);
                    writer.Write(playSaveBytes);

                    // Editor
                    if (File.Exists(EditorDataManager.filePath))
                    {
                        byte[] editorBytes = File.ReadAllBytes(EditorDataManager.filePath);
                        writer.Write(Properties.Editor);
                        writer.Write(editorBytes.Length);
                        writer.Write(editorBytes);
                    }
                }
                result = memoryStream.ToArray();
            }
            return result;
        }

        private bool DeserializeSave(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int header = reader.ReadInt32();
                    if (header != FirstHeader)
                    {
                        onError?.Invoke(CloudSaveError.HeaderWrong);
                        return false;
                    }

                    ReadProperties(reader, bytes.Length);
                }
            }
            return true;
        }

        private void ReadProperties(BinaryReader reader, int streamLength)
        {
            Stream stream = reader.BaseStream;
            while (stream.Position < streamLength)
            {
                int propertyType = reader.ReadInt32();
                switch (propertyType)
                {
                    case Properties.PlaySave:
                        ReadBytesAndWriteFile(reader, DataManager.filePath);
                        break;
                    case Properties.Editor:
                        ReadBytesAndWriteFile(reader, EditorDataManager.filePath);
                        break;
                }
            }
        }

        private void ReadBytesAndWriteFile(BinaryReader reader, string filePath)
        {
            int fileSize = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(fileSize);

            File.WriteAllBytes(filePath, bytes);
        }
        #endregion

        private WaitForSecondsRealtime oneSecondWait = new WaitForSecondsRealtime(1);
        private WaitForSecondsRealtime autoSaveWait = new WaitForSecondsRealtime(60);
        private DataManager dataManager;
        private void Start()
        {
            dataManager = DataManager.Instance;

            StartCoroutine(AutoSaveCoroutine());
            StartCoroutine(LoadLastSave());
        }


        #region 자동 불러오기

        private IEnumerator LoadLastSave()
        {
            GameSave saveData = dataManager.saveData;
            while (!Social.localUser.authenticated)
            {
                yield return oneSecondWait;
            }

            if (saveData.playCount == 0 && saveData.playedTimeTicks < TimeSpan.FromSeconds(60).Ticks)
            {
                LoadFromCloud();
            }
        }
        
        #endregion
        #region 자동 저장
        IEnumerator AutoSaveCoroutine()
        {
            GameSave saveData = dataManager.saveData;
            while (!Social.localUser.authenticated)
            {
                yield return oneSecondWait;
            }

            while (true)
            {
                yield return autoSaveWait;

                TimeSpan backupTimeDiff = new TimeSpan(DateTime.Now.Ticks - saveData.previousCloudSaveTime);

                if (saveData.isAutoSave && saveData.playCount > 0
                    && backupTimeDiff.TotalHours > 24)
                {
                    SaveToCloud();

                    Debug.Log("자동 백업함.");
                }
            }
        }

        public DateTime GetLastSaveDateTime()
        {
            return new DateTime(dataManager.saveData.previousCloudSaveTime);
        }
        #endregion
#endif
    }
}