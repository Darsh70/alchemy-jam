using UnityEngine;

public class SpellMenu : MonoBehaviour
{
    [Header("Bomb Button")]
    public GameObject buttonBomb;

    [Header("Water Buttons")]
    public GameObject buttonWaterBall;
    public GameObject buttonWaterBlast;
    public GameObject buttonRain;

    [Header("Electricity Buttons")]
    public GameObject buttonZap;
    public GameObject buttonLightning;
    public GameObject buttonCharge;

    public void HideAll()
    {
        buttonBomb.SetActive(false);

        buttonWaterBall.SetActive(false);
        buttonWaterBlast.SetActive(false);
        buttonRain.SetActive(false);

        buttonZap.SetActive(false);
        buttonLightning.SetActive(false);
        buttonCharge.SetActive(false);
    }

    public void ShowButtonsForElement(ElementType element)
    {
        HideAll();

        switch (element)
        {
            case ElementType.Bomb:
                buttonBomb.SetActive(true);
                break;
            
            case ElementType.Water:
                buttonWaterBall.SetActive(true);
                buttonWaterBlast.SetActive(true);
                buttonRain.SetActive(true);
                break;
            
            case ElementType.Electricity:
                buttonZap.SetActive(true);
                buttonLightning.SetActive(true);
                buttonCharge.SetActive(true);
                break;

        }
    }
}
