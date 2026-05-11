
using RivenFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WB_FileSelect : MonoBehaviour
{
    public TMP_Text[] NewGameText, FileNameText, FilePlayTimeText;
    public Image[] FileSlotImages;
    public GI_SaveManager saveManager;
    public UnityEvent OnFileSelected;
    public UnityEvent OnNewFileSelected;
    public TMP_Text deleteFileSlotText, deleteFileNameText, deleteFilePlayTimeText;
    public Color selectedColor, normalColor;

    public void Start()
    {
        saveManager = GameInstance.Get<GI_SaveManager>();
        
        for (int i = 0; i < NewGameText.Length; i++)
        {
            if (GI_SaveManager.HasSaveFile($"slot_{i+1}"))
            {
                var (_, playtimeStr, _, _) = GI_SaveManager.LoadFileInfo($"slot_{i+1}");
                NewGameText[i].gameObject.SetActive(false);
                FileNameText[i].gameObject.SetActive(true);
                FilePlayTimeText[i].gameObject.SetActive(true);
                FileNameText[i].text = $"File {i+1}";
                FilePlayTimeText[i].text = playtimeStr;
            }
            else
            {
                NewGameText[i].gameObject.SetActive(true);
                FileNameText[i].gameObject.SetActive(false);
                FilePlayTimeText[i].gameObject.SetActive(false);
                NewGameText[i].text = "New Game";
            }
        }
    }

    private void SetDeleteFileText(int fileIndex)
    {
        deleteFileSlotText.text = (fileIndex).ToString();
        deleteFileNameText.text = $"File {fileIndex}";
        var (_, playtimeStr, _, _) = GI_SaveManager.LoadFileInfo($"slot_{fileIndex}");
        deleteFilePlayTimeText.text = playtimeStr;
    }

    public void SetSlotSelectedColor(int fileIndex)
    {
        for (int i = 0; i < FileSlotImages.Length; i++)
        {
            if (i == fileIndex - 1)
            {
                FileSlotImages[i].color = selectedColor;
            }
            else
            {
                FileSlotImages[i].color = normalColor;
            }
        }
    }
    
    public void SelectFile(int fileIndex)
    {
        if (GI_SaveManager.HasSaveFile($"slot_{fileIndex}"))
        {
            saveManager.LoadSaveFile($"slot_{fileIndex}");
            OnFileSelected.Invoke();
            SetDeleteFileText(fileIndex);
            SetSlotSelectedColor(fileIndex);
        }
        else
        {
            saveManager.LoadSaveFile($"slot_{fileIndex}");
            OnNewFileSelected.Invoke();
        }
    }

    public void LoadSelectedFile()
    {
        SceneManager.LoadScene("World");
    }
    
    public void DeleteSelectedFile()
    {
        saveManager.DeleteSaveFile(saveManager.saveSlot);
        Start();
    }

    public void CloseWidget()
    {
        FindObjectOfType<WB_Title>().GetComponentInChildren<WidgetNavigator>().SetIsNavigating(true);
        Destroy(gameObject);
    }
}
