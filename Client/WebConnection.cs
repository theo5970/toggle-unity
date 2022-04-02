using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text;

using Toggle.Core;
using Toggle.Utils;
using Toggle.Game.Common;
using Toggle.LevelEditor;

using Toggle.Core.Generator;
using Toggle.Core.Function;
using Toggle.LevelSearch;

using UnityEngine;
using UnityEngine.Networking;

namespace Toggle.Client
{
    public struct ClassicPlayRecord
    {
        public string type;
        public string packName;
        public int levelIndex;
        public int clickCount;
        public int minClickCount;
        public int totalClickCount;
        public float clearTime;
        public int undoCount;
        public int redoCount;
        public int hintCount;
        public int restartCount;
    }

    public struct AuthResult
    {
        public bool isSuccess;
        public string failMessage;
    }

    public struct UploadResult
    {
        public enum Type { Upload, Update};

        public int levelId;
        public bool isSuccess;
        public string failMessage;
        public Type uploadType;
    }

    public struct SearchResult
    {
        public List<OnlineLevel> levels;
        public int page;
        public int totalPages;
    }

    public class OnlineLevel
    {
        public int id;
        public string title;
        public string creator;
        public int typeFlags;
        public int downloadCount;
        public int likeCount;
        public int minimumClicks;
        public int clearUserCount;
        public long updatedDatetime;
    }

    public class WebConnection : ClassSingleton<WebConnection>
    {
        public struct Response
        {
            public long responseCode;
            public string text;
        }

        private static string userAgent = Global.ServerUserAgent;
        private static string host = Global.ServerHost;

        public event Action onLoggedIn;
        public event Action onLoggedOut;

        public string userId { get; private set; }
        public bool isLoggedIn { get; private set; } = false;
        public string sessionId { get; private set; }

        public void SetSessionId(string sessionId)
        {
            this.sessionId = sessionId;
        }

        protected override void Init()
        {
            base.Init();

            sessionId = DataManager.Instance.saveData.sessionId;
        }

        public async UniTask<Response> DownloadStringPostJson(string url, string requestJson,
            Action<UnityWebRequest> setRequest = null)
        {
            Response response = new Response();
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson.ToString());

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("x-user-agent", userAgent);
                request.SetRequestHeader("Content-Type", "application/json");
                if (setRequest != null) setRequest(request);

                await request.SendWebRequest();

                response.responseCode = request.responseCode;
                response.text = request.downloadHandler.text;
            }
            return response;
        }

        public async UniTask SendPostJson(string url, string requestJson, Action<UnityWebRequest> setRequest = null)
        {
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson.ToString());

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("x-user-agent", userAgent);
                request.SetRequestHeader("Content-Type", "application/json");
                if (setRequest != null) setRequest(request);

                await request.SendWebRequest();
            }
        }

        public async UniTask<Response> SendGet(string url, Action<UnityWebRequest> setRequest = null)
        {
            Response response = new Response();
            using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("x-user-agent", userAgent);
                if (setRequest != null) setRequest(request);

                await request.SendWebRequest();

                response.responseCode = request.responseCode;
                response.text = request.downloadHandler.text;
            }
            return response;
        }

        private JObject tempJObj = new JObject();

        public async UniTask UpdateLoginState()
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return;
            }
            Response response = await SendGet($"{host}/auth/check", req => req.SetRequestHeader("x-session-id", sessionId));
            UnityEngine.Debug.Log(response.text);

            JObject jobj = JObject.Parse(response.text);

            if (jobj["result"].Value<bool>() == true)
            {
                if (!isLoggedIn)
                {
                    isLoggedIn = true;
                    userId = jobj["userId"].Value<string>();
                    onLoggedIn?.Invoke();
                }
            }
            else
            {
                if (isLoggedIn)
                {
                    isLoggedIn = false;
                    userId = null;
                    sessionId = null;
                    onLoggedOut?.Invoke();
                }
            }
        }
        public async UniTask<AuthResult> SignIn(string userId, string userName)
        {
            AuthResult ret = new AuthResult();
            try
            {
                tempJObj.RemoveAll();

                tempJObj.Add("id", userId);
                tempJObj.Add("userName", userName);

                Response response = await DownloadStringPostJson($"{host}/auth/signin", tempJObj.ToString(Formatting.None));
                ret.isSuccess = true;
                sessionId = response.text;

                this.userId = userId;
                isLoggedIn = true;
                onLoggedIn?.Invoke();
            }
            catch (UnityWebRequestException ex)
            {
                ret.isSuccess = false;
                ret.failMessage = string.IsNullOrEmpty(ex.Text) ? ex.Message : ex.Text;
            }

            return ret;
        }


        public async UniTask SendClassicPlayData(ClassicPlayRecord record)
        {
            record.type = "classic";

            string json = JsonConvert.SerializeObject(record);
            await SendPostJson($"{host}/ingame/playdata", json);
        }

        private int GetFunctionTypeFlags(ToggleLevel toggleLevel)
        {
            FunctionTypeFlags result = FunctionTypeFlags.None;

            foreach (FunctionSubType subType in toggleLevel.buttons)
            {
                result |= FunctionTypeFlagsUtils.ConvertSubTypeToCombination(subType);
            }

            return (int)result;
        }

        public async UniTask<UploadResult> UploadLevel(EditorToggleLevel editorLevel, string levelCode)
        {
            UploadResult result = new UploadResult();
            result.uploadType = UploadResult.Type.Upload;

            tempJObj.RemoveAll();
            tempJObj.Add("title", editorLevel.title);
            tempJObj.Add("levelCode", levelCode);
            tempJObj.Add("minClicks", editorLevel.minimumClickCount);
            tempJObj.Add("typeFlags", GetFunctionTypeFlags(editorLevel.data));

            try
            {
                Response response = await DownloadStringPostJson($"{host}/ingame/level/upload", tempJObj.ToString(Formatting.None), req => req.SetRequestHeader("x-session-id", sessionId));

                result.levelId = int.Parse(response.text);
                result.isSuccess = true;
                
            }
            catch (UnityWebRequestException ex)
            {
                result.isSuccess = false;
                result.failMessage = string.IsNullOrEmpty(ex.Text) ? ex.Message : ex.Text;
            }

            return result;
        }

        public async UniTask<UploadResult> UpdateLevel(EditorToggleLevel editorLevel, string levelCode)
        {
            UploadResult result = new UploadResult();
            result.uploadType = UploadResult.Type.Update;

            tempJObj.RemoveAll();
            tempJObj.Add("levelId", editorLevel.uploadId);
            tempJObj.Add("title", editorLevel.title);
            tempJObj.Add("levelCode", levelCode);
            tempJObj.Add("minClicks", editorLevel.minimumClickCount);
            tempJObj.Add("typeFlags", GetFunctionTypeFlags(editorLevel.data));

            try
            {
                Response response = await DownloadStringPostJson($"{host}/ingame/level/update", tempJObj.ToString(Formatting.None),
                    req => req.SetRequestHeader("x-session-id", sessionId));
                result.isSuccess = true;

            }
            catch (UnityWebRequestException ex)
            {
                result.isSuccess = false;
                result.failMessage = string.IsNullOrEmpty(ex.Text) ? ex.Message : ex.Text;
            }

            return result;
        }


        public async UniTask<SearchResult> SearchLevels(LevelSearchOptions options)
        {
            SearchResult searchResult = new SearchResult();

            tempJObj.RemoveAll();
            tempJObj.Add("filter", options.filter.ToString());
            tempJObj.Add("page", options.page);

            if (options.filter == LevelSearchOptions.Filter.Keyword)
            {
                tempJObj.Add("keyword", options.keyword);
            }

            try
            {
                Response response = await DownloadStringPostJson($"{host}/ingame/level/search", tempJObj.ToString(Formatting.None), 
                    req => req.SetRequestHeader("x-session-id", sessionId));

                searchResult = JsonConvert.DeserializeObject<SearchResult>(response.text);

            }
            catch (UnityWebRequestException ex)
            {
                Debug.Log(string.IsNullOrEmpty(ex.Text) ? ex.Message : ex.Text);
            }

            return searchResult;
        }

        public async UniTask<string> DownloadLevel(int levelId)
        {
            tempJObj.RemoveAll();
            tempJObj.Add("levelId", levelId);

            Response response = await DownloadStringPostJson($"{host}/ingame/level/download", tempJObj.ToString(Formatting.None),
                req => req.SetRequestHeader("x-session-id", sessionId));

            return response.text;
        }

        public async UniTask LikeLevel(int levelId)
        {
            tempJObj.RemoveAll();
            tempJObj.Add("levelId", levelId);

            await SendPostJson($"{host}/ingame/level/like", tempJObj.ToString(Formatting.None),
                req => req.SetRequestHeader("x-session-id", sessionId));
        }

        public async UniTask CancelLikeLevel(int levelId)
        {
            tempJObj.RemoveAll();
            tempJObj.Add("levelId", levelId);

            await SendPostJson($"{host}/ingame/level/cancel-like", tempJObj.ToString(Formatting.None),
                req => req.SetRequestHeader("x-session-id", sessionId));
        }
    }
}
