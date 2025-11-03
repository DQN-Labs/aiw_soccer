using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinCard : MonoBehaviour
{
    [Header("Buttons and texts")]
    [SerializeField] private Image skinImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button selectButton;

    [Header("Skin Data")]
    [SerializeField] private string skinName;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;

    [Header("Unlock Requirement")]
    [SerializeField] private GoalType goalType;
    [SerializeField] private int requiredAmount;

    private bool isUnlocked;

    private void Awake()
    {
        nameText.text = skinName;
        RefreshCard();
    }

    public void RefreshCard()
    {
        isUnlocked = IsUnlocked();

        skinImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        selectButton.gameObject.SetActive(isUnlocked);
        nameText.gameObject.SetActive(isUnlocked);
    }

    private bool IsUnlocked()
    {
        switch (goalType)
        {
            case GoalType.Kai:
                return GoalSaver.KaiGoals >= requiredAmount;
            case GoalType.Albert:
                return GoalSaver.AlbertGoals >= requiredAmount;
            default:
                return false;
        }
    }

    public void OnSelectSkin()
    {
        if (!isUnlocked) return;
        Debug.Log($"Selected skin: {skinName}");
        PlayerPrefs.SetString("SelectedSkin", skinName);
        PlayerPrefs.Save();
    }
}

public enum GoalType
{
    Kai,
    Albert
}
