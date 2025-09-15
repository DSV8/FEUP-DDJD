using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{
    public Slider dashSlider;
    public PlayerMovement playerMovement;

    void Update()
    {
        dashSlider.value = playerMovement.GetCurrentDashPercentage();
    }
}
