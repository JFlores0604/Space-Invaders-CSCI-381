using UnityEngine;
using TMPro;

public class AlienCounter : MonoBehaviour
{
    private TextMeshProUGUI uiText;

    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    // Only rebuilds the text when the count actually changes, instead of every frame.
    public void SetCount(int count)
    {
        uiText.text = "Remaining Aliens: " + count.ToString("#,0");
    }
}
