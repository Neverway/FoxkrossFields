using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WB_ItemStats : MonoBehaviour
{
    public TMP_Text details;
    public void Show()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SetPosition(Vector3 transformPosition)
    {
        transform.position = transformPosition;
    }

    public void SetTextContent(string assetDisplayName, string assetDescription)
    {
        details.text = $"<color=#FFFF44><b>{assetDisplayName}</b><br><color=#FFFFFF><i>{assetDescription}";
    }

    public void Hide()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
}
