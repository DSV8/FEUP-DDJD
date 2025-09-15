using UnityEngine;
using FMODUnity;

public class Pistol : MonoBehaviour
{
    protected int playerMask;
    protected bool canShoot = false;

    [Header("Main")]
    public float damage = 5;
    public float range = 500;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("Firing")]
    public float fireRate = 1f;
    private float fireCooldown = 0f;

    [Header("Recoil")]
    public float recoilTime = 0.1f;
    public float recoilSpeed = 10f;
    public float recoilGun = 5f;
    public float recoilCamera = 2f;

    [Header("Effects")]
    public GameObject bulletHole;
    public ParticleSystem muzzleFlash;

    private float fireRateUP = 0f;

    void Awake()
    {
        playerMask = ~LayerMask.GetMask("Player", "Powerups");

        int FireRateLevel = PlayerPrefs.GetInt("FireRateLevel", 1);
        fireRate = fireRate + (FireRateLevel - 1) * 1.5f + PlayerPrefs.GetFloat("fireRateUP", 0f);
    }

    // Old version
    // void Shoot()
    // {
    //     GameObject newBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Camera.main.transform.rotation);
    // }

    void Shoot()
    {
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, playerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = cam.transform.position + cam.transform.forward * range;
        }

        Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

        GameObject newBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));

        RuntimeManager.PlayOneShot("event:/High Priority/Disparo de Arma", transform.position);
    }

    public void IncreaseFireRate(float value)
    {
        fireRate += value;
        fireRateUP += value;
        PlayerPrefs.SetFloat("fireRateUP", fireRateUP);
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }
}
