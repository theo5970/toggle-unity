using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Toggle.Game;
using UnityEngine.EventSystems;

public class LevelClearDialog : MonoBehaviour
{
    public Color blankStarColor = Color.white;
    public Color fillStarColor = Color.white;
    public Image[] stars;
    public TextMeshProUGUI clearText;
    public Button nextButton;

    public bool canClose = true;


    private void Start()
    {
        nextButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Show(int starCount, string clearMessage, string nextButtonText)
    {
        gameObject.SetActive(true);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = (i < starCount) ? fillStarColor : blankStarColor;
        }
        clearText.text = clearMessage;

        nextButton.SetText(nextButtonText);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CloseIfClickedOutside()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null))
            {
                if (canClose) Hide();
            }
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position, null))
            {
                if (canClose) Hide();
            }
        }
#endif

    }

    void Update()
    {
        CloseIfClickedOutside();
    }
}
