using UnityEngine;

public class SwitchModeIcon : MonoBehaviour
{

    private GameObject rangedIcon;
    private GameObject meleeIcon;
    private PlayerMovement playerMovement;

    void Start()
    {
        rangedIcon = transform.Find("Ranged Icon")?.gameObject;
        meleeIcon = transform.Find("Melee Icon")?.gameObject;
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (playerMovement != null)
        {
            bool isInMeleeMode = playerMovement.GetIsInMeleeMode();
            if (meleeIcon != null ) {
                meleeIcon.SetActive(isInMeleeMode);
            }

            if (rangedIcon != null) {
                rangedIcon.SetActive(!isInMeleeMode);
            }
        }
    }
}
