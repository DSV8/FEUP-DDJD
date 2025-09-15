using UnityEngine;

public class EMPAttack : MonoBehaviour
{
    public GameObject empPrefab;
    public float cooldown = 3f;
    private float lastEMPTime = -Mathf.Infinity;

    private float powerUPDown = 0f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastEMPTime + cooldown - PlayerPrefs.GetFloat("powerUPDown", 0f))
        {
            Instantiate(empPrefab, transform.position, Quaternion.identity, transform);
            lastEMPTime = Time.time;
        }
    }

    public void ReduceCooldown(float value)
    {
        if (cooldown > value)
        {
            cooldown -= value;
            powerUPDown += value;

            PlayerPrefs.SetFloat("powerUPDown", powerUPDown);
        }
    }
}