using System;
using System.Collections.Generic;
using Toggle.Game.Common;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelStack : Singleton<PanelStack>
{
    public Button backButton;
    public bool disableBackButtonWhenEmpty;
    public bool backSceneWhenEmpty;
    public string backSceneName;

    public event Action onBackToScene;

    public List<GameObject> prePushList;
    private Stack<GameObject> stack = new Stack<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < prePushList.Count; i++)
        {
            stack.Push(prePushList[i]);
        }
        backButton.onClick.AddListener(Pop);
        if (disableBackButtonWhenEmpty) backButton.gameObject.SetActive(false);
    }

    public void Push(GameObject objToPush)
    {
        if (stack.Count > 0)
        {
            stack.Peek().SetActive(false);
        }

        objToPush.SetActive(true);
        stack.Push(objToPush);

        if (disableBackButtonWhenEmpty) backButton.gameObject.SetActive(stack.Count > 1);
    }

    public void Pop()
    {
        if (stack.Count > 1)
        {
            GameObject obj = stack.Pop();
            obj.SetActive(false);

            if (stack.Count > 0)
            {
                stack.Peek().SetActive(true);
            }

            if (disableBackButtonWhenEmpty) backButton.gameObject.SetActive(stack.Count > 1);
        }
        else
        {
            if (backSceneWhenEmpty)
            {
                onBackToScene?.Invoke();
                SceneManager.LoadScene(backSceneName);
            }
        }
    }

    public void ResetStack()
    {
        while (stack.Count > 0)
        {
            GameObject obj = stack.Pop();
            obj.SetActive(false);
        }

        for (int i = 0; i < prePushList.Count; i++)
        {
            stack.Push(prePushList[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pop();
        }
    }
}