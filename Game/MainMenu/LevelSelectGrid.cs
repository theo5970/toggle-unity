using System.Collections.Generic;
using TMPro;
using Toggle.Core;
using Toggle.Core.Generator;
using Toggle.Game.Common;
using Toggle.Game.Data;
using Toggle.Game.Mode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Toggle.Game.MainMenu
{
    public class LevelSelectGrid : MonoBehaviour
    {
        public int currentPage { get; private set; }
        private int buttonsPerPage;

        // 최대 페이지 수
        private int maxPageIndex
        {
            get
            {
                int result = totalLevels / buttonsPerPage;
                if (totalLevels % buttonsPerPage == 0)
                {
                    result--;
                }

                return result;
            }
        }

        private LevelManager levelManager;
        private List<GameObject> buttonPool;
        private int totalLevels;

        private LevelPackSave packSave;

        private RectTransform rectTransform;

        public Transform gridRoot;
        public GameObject cellPrefab;

        private GridLayoutGroup gridLayoutGroup;

        public TextMeshProUGUI clearStatText;
        public Button prevButton;
        public Button nextButton;

        private List<bool> easterEggClickList = new List<bool>(16);

        void Awake()
        {
            currentPage = 0;
            prevButton.interactable = false;
            nextButton.interactable = false;

            buttonPool = new List<GameObject>();

            for (int i = 0; i < 200; i++)
            {
                GameObject cell = Instantiate(cellPrefab, gridRoot);
                cell.SetActive(false);
                cell.transform.localScale = Vector3.one;
                buttonPool.Add(cell);
            }

            // 뒤로버튼
            prevButton.onClick.AddListener(() =>
            {
                LoadPage(--currentPage);
                PushToEasterEgg(false);
            });

            // 앞으로버튼
            nextButton.onClick.AddListener(() =>
            {
                LoadPage(++currentPage);
                PushToEasterEgg(true);
            });

            gridLayoutGroup = gridRoot.GetComponent<GridLayoutGroup>();
            rectTransform = gridRoot.GetComponent<RectTransform>();
            var cellRectTransform = cellPrefab.GetComponent<RectTransform>();

            var rect = rectTransform.rect;
            float gridArea = rect.width * rect.height;

            float cellArea = (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x) *
                             (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y);


            buttonsPerPage = Mathf.FloorToInt(Mathf.FloorToInt(gridArea / cellArea) * 0.1f) * 10;
        }

        #region 페이지 업데이트 기능
        public void SetPackAndUpdate(string packName)
        {
            levelManager = LevelManager.Instance;
            totalLevels = levelManager.GetLevelCount(packName);
            packSave = DataManager.Instance.GetPackSave(packName);

            LoadPage(0);

            int clearedLevelCount = packSave.CountClearedLevels();
            float clearPercent = ((float) clearedLevelCount / totalLevels) * 100;

            clearStatText.text = $"Clear {clearPercent:F0}% ({clearedLevelCount} / {totalLevels})";
        }

        // 버튼 활성화여부 업데이트
        public void UpdatePageButtons()
        {
            if (currentPage == 0)
            {
                // 1페이지의 경우 뒤로버튼 X, 다음버튼 O
                prevButton.interactable = false;
                nextButton.interactable = (totalLevels > buttonsPerPage);
            }
            else
            {
                // 그 외에는 뒤로버튼 항상 O
                // 최대 페이지수 넘어가지 않으면 다음버튼 O
                prevButton.interactable = true;
                nextButton.interactable = (currentPage < maxPageIndex);
            }
        }

        // 원하는 레벨 페이지 불러오기
        public void LoadPage(int pageNumber)
        {
            currentPage = Mathf.Clamp(pageNumber, 0, maxPageIndex);
            int startIndex = pageNumber * buttonsPerPage;

            // 사용되는 것은 활성화, 사용되지 않는 것은 비활성화한다.
            for (int i = 0; i < buttonsPerPage; i++)
            {
                GameObject pooledObject = buttonPool[i];
                if (startIndex + i <= totalLevels - 1)
                {
                    pooledObject.SetActive(true);

                    var button = pooledObject.GetComponent<LevelSelectButton>();
                    button.selectGrid = this;
                    button.SetLoadData(packSave, startIndex + i);
                }
                else
                {
                    pooledObject.SetActive(false);
                }
            }

            UpdatePageButtons();
        }
        #endregion


        #region 이스터에그

        private static readonly bool[] targets =
        {
            true, false, true, true,
            false, true, false, false,
            true, false, true, true,
            false, true, false, false
        };

        private void PushToEasterEgg(bool isRight)
        {
            easterEggClickList.Add(isRight);
            if (easterEggClickList.Count > targets.Length)
            {
                easterEggClickList.RemoveAt(0);
            }

            if (easterEggClickList.Count == targets.Length)
            {
                bool isOrderPassed = true;
                for (int i = 0; i < targets.Length; i++)
                {
                    if (easterEggClickList[i] != targets[i])
                    {
                        isOrderPassed = false;
                        break;
                    }
                }

                if (isOrderPassed)
                {
                    Debug.Log("이스터에그 도달!");

                    ButtonGrid grid = new ButtonGrid();
                    LevelGenerator generator = new LevelGenerator();
                    generator.grid = grid;
                    generator.options = new LevelGenerator.Options
                    {
                        mapSize = new Vector2Int(10, 10),
                        buttonCount = 64,
                        clickCount = 20,
                        fourArrowPlaceLimit = 2,
                        mirrorType = MirrorType.None,
                        shouldClickEssentialType = false,
                        shouldPlaceEssentialType = false,
                        typeFlags = FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow
                    };
                    generator.Generate();

                    ToggleLevel level = new ToggleLevel();
                    grid.CopyToLevel(level);
                    LevelGameMode.levelToLoad = level;
                    GameManager.SetGameMode(typeof(LevelGameMode));
                    SceneManager.LoadScene("PlayScene");
                }
            }
        }

        #endregion
    }
}