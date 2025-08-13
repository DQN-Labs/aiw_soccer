using UnityEngine;
using UnityEngine.UI;
public class CardHandler : MonoBehaviour
{
    public Sprite[] cardSkins;
    private Image cardImage;
    void Awake()
    {
        cardImage = GetComponent<Image>();
    }
    void Start()
    {
        UpdateCard();
    }

    public void UpdateCard()
    {
        int selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
        if (selectedSkinIndex >= 0 && selectedSkinIndex < cardSkins.Length)
        {
            cardImage.sprite = cardSkins[selectedSkinIndex];  
        }
    }
  
}
