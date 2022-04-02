using System.Collections.Generic;
using Toggle.Core;
using Toggle.Core.Function;
using Toggle.Game.Common.FunctionShape;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Toggle.Game.Common
{
    public class GameMap : Singleton<GameMap>
    {
        private List<GameObject> buttonPool;
        private List<BaseButtonView> viewList;

        public GameObject defaultPrefab;

        public ButtonGrid grid => _grid;

        private static ButtonGrid _grid = new ButtonGrid();

        public Vector3 center => new Vector3((_grid.width - 1) * 0.5f, (_grid.height - 1) * 0.5f);
        public UnityEvent onLevelLoaded;
        public Transform behindBackground;

        private const float border = 0.25f;

        void Awake()
        {
            if (defaultPrefab != null)
            {
                defaultPrefab.SetActive(false);
            }

            buttonPool = new List<GameObject>();
            viewList = new List<BaseButtonView>();
        }

        public GameObject GetPooledButton()
        {
            for (int i = 0; i < buttonPool.Count; i++)
            {
                var buttonObj = buttonPool[i];
                if (!buttonObj.activeInHierarchy)
                {
                    return buttonObj;
                }
            }

            var newObj = Instantiate(defaultPrefab);
            newObj.SetActive(false);
            buttonPool.Add(newObj);

            return newObj;
        }

        private void Update()
        {
            if (behindBackground)
            {
                behindBackground.localScale = new Vector3(_grid.width + border, _grid.height + border, 1);
            }
        }


        public void LoadLevel(ToggleLevel toggleLevel)
        {
            _grid.Resize(toggleLevel.width, toggleLevel.height);

            for (int y = 0; y < _grid.height; y++)
            {
                for (int x = 0; x < _grid.width; x++)
                {
                    int charIndex = y * _grid.width + x;
                    BaseButton button = _grid[x, y];
                    button.functionSubType = toggleLevel.buttons[charIndex];
                }
            }
        
            _grid.RestoreStates(toggleLevel.states);

            RefreshButtons();
            onLevelLoaded.Invoke();
        }

        public void GenerateEmptyMap()
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                var obj = viewList[i].gameObject;
                obj.SetActive(false);
            }

            viewList.Clear();

            for (int y = 0; y < _grid.height; y++)
            {
                for (int x = 0; x < _grid.width; x++)
                {
                    GenerateButton(false, FunctionSubType.NOP, x, y);
                }
            }

            RefreshButtonsOutside();
        }

        private readonly string buttonName = "MapButton";

        private void GenerateButton(bool state, FunctionSubType subType, int x, int y)
        {
            GameObject buttonObj = GetPooledButton();
            buttonObj.name = buttonName;
            buttonObj.SetActive(true);

            Vector3 position = new Vector3(x, y, 0);
            buttonObj.transform.SetParent(transform);
            buttonObj.transform.localPosition = position - center;

            BaseButtonView view = buttonObj.GetComponent<BaseButtonView>();

            BaseButton data = _grid[x, y];
            data.coordinate = new Vector2Int(x, y);
            data.isOn = state;
            view.SetData(data);

            data.functionSubType = subType;

            viewList.Add(view);
        }

        public void RefreshButtonsOutside()
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                var view = viewList[i];

                var shapeDecorator = FunctionShapeDecoratorMap.Get(view.button.functionSubType);
                shapeDecorator.UpdateOutside(view);
            }
        }

        public void RefreshButtons()
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                var obj = viewList[i].gameObject;
                obj.SetActive(false);
            }

            viewList.Clear();

            for (int y = 0; y < _grid.height; y++)
            {
                for (int x = 0; x < _grid.width; x++)
                {
                    BaseButton data = _grid[x, y];
                    bool state = data.isOn;
                    GenerateButton(state, data.functionSubType, x, y);
                }
            }

            RefreshButtonsOutside();
        }

        public BaseButtonView GetButtonView(Vector2Int coord)
        {
            return viewList[coord.y * _grid.width + coord.x];
        }

        public IEnumerable<BaseButtonView> GetButtonViews()
        {
            return viewList;
        }
    
    }
}