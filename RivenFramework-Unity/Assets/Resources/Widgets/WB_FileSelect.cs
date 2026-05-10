using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WB_FileSelect : MonoBehaviour
{
    public TMP_Text[] NewGameText, FileNameText, FilePlayTimeText;
    public GI_SaveManager saveManager;

    public void Start()
    {
        saveManager = GameInstance.Get<GI_SaveManager>();
        
        // Check for files
        for (int i = 0; i < NewGameText.Length; i++)
        {
            if (GI_SaveManager.HasSaveFile($"slot_{i+1}"))
            {
                NewGameText[i].text = $"File {i+1}";
            }
            else
            {
                NewGameText[i].text = "New Game";
            }
        }
    }

    public void SelectFile(int fileIndex)
    {
        saveManager.LoadSaveFile($"slot_{fileIndex}");
        SceneManager.LoadScene("World");
    }

    public void CloseWidget()
    {
        FindObjectOfType<WB_Title>().GetComponentInChildren<WidgetNavigator>().SetIsNavigating(true);
        Destroy(gameObject);
    }
}
