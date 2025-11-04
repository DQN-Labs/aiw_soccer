using UnityEngine;

public class SelectButton : MonoBehaviour
{
    public string itemId;

    public void OnClick()
    {
        FindObjectOfType<SelectionManager>().SelectItem(itemId);
    }
}
