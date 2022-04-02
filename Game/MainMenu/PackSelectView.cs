using System.Collections;
using System.Collections.Generic;
using Toggle.Game.Common;
using Toggle.Game.Data;
using Toggle.Game.MainMenu;
using UnityEngine;

public class PackSelectView : MonoBehaviour
{
    public Transform listRoot;
    public GameObject buttonPrefab;
    public LevelSelectGrid selectGrid;

    private LevelManager levelManager;
    private List<LevelPack> levelPacks;

    void Start()
    {
        levelManager = LevelManager.Instance;

        levelPacks = new List<LevelPack>(levelManager.GetPacks());

        // GeneratePackButtons();
    }

    void GeneratePackButtons()
    {
        foreach (Transform child in listRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (LevelPack pack in levelPacks)
        {
            GameObject newObj = Instantiate(buttonPrefab, listRoot, false);
            PackSelectButton selectButton = newObj.GetComponent<PackSelectButton>();

            LevelPackSave save = DataManager.Instance.GetPackSave(pack.name);

            selectButton.parent = this;
            selectButton.packName = pack.name;
        }
    }

    public void SwitchToLevelSelect(string packName)
    {
        PanelStack.Instance.Push(selectGrid.gameObject);
        selectGrid.SetPackAndUpdate(packName);
    }
}
