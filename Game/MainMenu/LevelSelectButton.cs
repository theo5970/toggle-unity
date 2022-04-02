using TMPro;
using Toggle.Game.Common;
using Toggle.Game.Data;
using Toggle.Game.Mode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Toggle.Game.MainMenu
{
    public class LevelSelectButton : MonoBehaviour, IPointerClickHandler
    {
        private LevelPackSave packSave;
        private int levelIndex;

        private Image background;
        private TextMeshProUGUI numberText;

        public LevelSelectGrid selectGrid;
        public LevelSelectButtonSkin buttonSkin;
        public Image[] starImages;

        void Awake()
        {
            background = transform.GetChild(0).GetComponent<Image>();
            numberText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }
        
        public void SetLoadData(LevelPackSave levelPackSave, int index)
        {
            packSave = levelPackSave;
            levelIndex = index;
            
            numberText.text = (index + 1).ToString();

            int starCount = levelPackSave.GetStarCount(levelIndex);
            LevelTryState levelState = levelPackSave.GetLevelState(levelIndex);
            background.color = buttonSkin.GetColorByTryState(levelState, starCount);

            if (levelState == LevelTryState.Clear)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    Image starImage = starImages[i];
                    Color newColor = Color.black;
                    newColor.a = 0.85f;

                    if (i + 1 > starCount)
                    {
                        newColor.a = 0.1f;
                    }
                    starImage.color = newColor;
                }

            }
            else
            {
                Color newColor = Color.black;
                newColor.a = 0.06f;

                for (int i = 0; i < starImages.Length; i++)
                {
                    starImages[i].color = newColor;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 클릭 시 플레이씬으로 넘어가게 처리
            ClassicMode.SetLevel(packSave, levelIndex);
            GameManager.SetGameMode(typeof(ClassicMode));
            MainMenuScene.SetRecentLevelView(packSave.packName, selectGrid.currentPage);
            SceneManager.LoadScene("PlayScene");
        }
    }
}
