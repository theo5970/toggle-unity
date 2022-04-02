using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocaleTMPUI : MonoBehaviour
{
    [SerializeField]
    private string translationKey;

    private TextMeshProUGUI uiText;
    void Awake()
    {
        Locale.onLoad += OnLanguageLoad;
        uiText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        uiText.text = Locale.Get(translationKey);
    }

    private void OnLanguageLoad()
    {
        uiText.text = Locale.Get(translationKey);
    }

    private void OnDestroy()
    {
        Locale.onLoad -= OnLanguageLoad;
    }

    public void SetTranslationKey(string newKey)
    {
        translationKey = newKey;
        uiText.text = Locale.Get(translationKey);
    }
}
