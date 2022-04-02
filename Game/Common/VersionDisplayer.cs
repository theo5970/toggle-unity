using TMPro;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class VersionDisplayer : MonoBehaviour
    {
        void Start()
        {
            TextMeshProUGUI versionText = GetComponent<TextMeshProUGUI>();

#if TEST_BUILD

            versionText.text = $"[TEST BUILD] v{Application.version}";
#else
            gameObject.SetActive(false);
#endif
        }
    }
}
