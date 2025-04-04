using UnityEngine;

public class LasersController : MonoBehaviour
{
    public GameObject laserTop;
    public GameObject laserBottom;
    
    public float switchInterval = 3.0f;

    private bool isTopActive = false;
    private float timer;

    void Start()
    {
        laserTop = transform.Find("LaserTop").gameObject;
        laserBottom = transform.Find("LaserBottom").gameObject;

        SwitchLaser(isTopActive);
        timer = switchInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isTopActive = !isTopActive;
            SwitchLaser(isTopActive);
            timer = switchInterval;
        }   
    }

    void SwitchLaser(bool isTopActive)
    {
        laserTop.SetActive(isTopActive);
        laserBottom.SetActive(!isTopActive);
    }
}
