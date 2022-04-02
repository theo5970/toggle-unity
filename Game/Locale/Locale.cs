using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


public static class Locale
{
    public static readonly Dictionary<SystemLanguage, string> languageNameDic = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.Korean, "한국어" },
            { SystemLanguage.English, "English" },
        };
    private static Dictionary<SystemLanguage, Dictionary<string, string>> localeDic = new Dictionary<SystemLanguage, Dictionary<string, string>>();

    public static readonly List<SystemLanguage> supportedLanguages = new List<SystemLanguage>
    {
        SystemLanguage.Korean, SystemLanguage.English
    };


    private static bool errorOccured = false;

    public static bool isLoadedAny { get; private set; }
    public static event Action onLoad;
    public static event Action<string> onError;

    public static SystemLanguage currentLanguage { get; private set; }

    private static IEnumerator RequestStreamingAsset(string path, Action<string> onSuccess, Action<string> onError)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, path);
        UnityWebRequest request = UnityWebRequest.Get(fullPath);
        yield return request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error))
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
            errorOccured = true;
        }
    }

    public static IEnumerator Load(SystemLanguage language)
    {
        errorOccured = false;

        if (localeDic.ContainsKey(language))
        {
            currentLanguage = language;
            onLoad?.Invoke();
        }
        else
        {
            string languageName = language.ToString().ToLower();
            yield return RequestStreamingAsset(languageName + ".json", json =>
            {
                var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                localeDic.Add(language, translation);

                currentLanguage = language;
            }, onError);

            if (!errorOccured)
            {
                onLoad?.Invoke();
            }
            isLoadedAny = true;

        }
    }

    public static string Get(string key)
    {
        if (!localeDic.ContainsKey(currentLanguage))
        {
            return GenerateRandomString();
        }

        var translation = localeDic[currentLanguage];
        if (translation.TryGetValue(key, out string value))
        {
            return value;
        }
        else
        {
            return GenerateRandomString();
        }
    }


    private static string GenerateRandomString()
    {
        StringBuilder sb = new StringBuilder();
        int length = UnityEngine.Random.Range(4, 20);
        for (int i = 0; i < length; i++)
        {
            sb.Append(Convert.ToChar(UnityEngine.Random.Range(33, 126)).ToString());
        }
        return sb.ToString();
    }
}
