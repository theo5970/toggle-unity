using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Toggle.LevelEditor.Palette
{
    public class PaletteDropdown : MonoBehaviour, IPointerClickHandler
    {
        public EditorTypePalette palette;

        [HideInInspector] public int dropdownIndex;

        public int currentIndex { get; private set; }
        public int itemCount => itemsRoot.childCount;
        
        private RectTransform rectTransform;

        private Image currentIcon;
        private Transform currentIconTransform;
        private Transform itemsRoot;
        private GameObject itemsGO;

        private List<RectTransform> buttonRects;
        private List<Image> buttonIcons;

        // Start is called before the first frame update
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            buttonRects = new List<RectTransform>();
            buttonIcons = new List<Image>();

            currentIcon = transform.Find("CurrentIcon").GetComponent<Image>();
            itemsRoot = transform.Find("Items");
            itemsGO = itemsRoot.gameObject;
            itemsGO.SetActive(false);

            int childCount = itemsRoot.childCount;
            currentIconTransform = currentIcon.transform;

            for (int i = 0; i < childCount; i++)
            {
                Button itemButton = itemsRoot.GetChild(i).GetComponent<Button>();
                Image itemIcon = itemButton.transform.GetChild(0).GetComponent<Image>();

                int index = i;
                itemButton.onClick.AddListener(() =>
                {
                    ApplyCurrentIcon(index);
                    palette.OnDropdownClicked(dropdownIndex);
                });

                buttonRects.Add(itemButton.GetComponent<RectTransform>());
                buttonIcons.Add(itemIcon);
            }

            ApplyCurrentIcon(0);


        }

        private void ApplyCurrentIcon(int index)
        {
            var itemIcon = buttonIcons[index];
            var itemIconTransform = itemIcon.transform;

            currentIcon.sprite = itemIcon.sprite;
            currentIconTransform.rotation = itemIconTransform.rotation;
            currentIconTransform.localScale = itemIconTransform.localScale;

            itemsGO.SetActive(false);
            currentIndex = index;
        }
    
        private void LateUpdate()
        {
            bool hasTouch = false;
            Vector2 touchPosition = Vector2.zero;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;
                hasTouch = true;
            }
        }
#else
            if (Input.GetMouseButtonDown(0)) {
                hasTouch = true;
                touchPosition = Input.mousePosition;
            }
#endif

            if (hasTouch)
            {
                bool isClickedOutside =
                    !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPosition);

                if (isClickedOutside)
                {
                    for (int i = 0; i < buttonRects.Count; i++)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(buttonRects[i], touchPosition))
                        {
                            isClickedOutside = false;
                            break;
                        }
                    }
                }

                if (isClickedOutside)
                {
                    itemsGO.SetActive(false);
                }
            }
        }


        private float lastTimeClick;
        private bool lastDoubleClick;
    
        public void OnPointerClick(PointerEventData eventData)
        {
            float currentTimeClick = eventData.clickTime;
            bool isDoubleClick = false;
            if (!lastDoubleClick)
            {
                isDoubleClick = Mathf.Abs(currentTimeClick - lastTimeClick) < 0.6f;
            }
            palette.OnDropdownClicked(dropdownIndex);

            StartCoroutine(SetItemsGameObjectState(isDoubleClick && !lastDoubleClick));
            lastTimeClick = currentTimeClick;
            lastDoubleClick = isDoubleClick;
        }

        private IEnumerator SetItemsGameObjectState(bool state)
        {
            yield return null;
            itemsGO.SetActive(state);
        }


    }
}