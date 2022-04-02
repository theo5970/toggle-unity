using TMPro;
using Toggle.Game.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Toggle.Game.MainMenu
{
    public class PackSelectButton : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] public PackSelectView parent;

        public string packName;
        
        public TextMeshProUGUI displayNameText;
        public TextMeshProUGUI progressText;
        private DataManager dataManager;

        void Start()
        {
            parent = transform.GetComponentInParent<PackSelectView>();

            dataManager = DataManager.Instance;
            dataManager.onReload += UpdateProgressText;
            UpdateProgressText();
        }

        private void UpdateProgressText()
        {
            var packSave = dataManager.GetPackSave(packName);
            progressText.text = $"{packSave.CountClearedLevels()} / {packSave.states.Count}";
        }

        private void OnDestroy()
        {
            if (dataManager != null) dataManager.onReload -= UpdateProgressText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            parent.SwitchToLevelSelect(packName);
        }
    }
}