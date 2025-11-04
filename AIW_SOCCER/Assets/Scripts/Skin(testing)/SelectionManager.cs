using UnityEngine;
using System;

public class SelectionManager : MonoBehaviour
{
    public static Action<string> OnItemSelected;

    public void SelectItem(string id)
    {
        PlayerPrefs.SetString("SelectedItem", id);
        PlayerPrefs.Save();
        OnItemSelected?.Invoke(id);
        Debug.Log($"Item selected: {id}");
    }

    public string GetSelectedId()
    {
        return PlayerPrefs.GetString("SelectedItem", "DefaultSkin");
    }
}