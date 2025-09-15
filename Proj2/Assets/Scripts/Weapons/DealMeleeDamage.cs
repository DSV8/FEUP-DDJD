using System;
using UnityEngine;

public class DealMeleeDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 35;

    void Start()
    {

    }

    void Update()
    {

    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies") || other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = other.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                Debug.Log("Dealt " + damageAmount + " damage to " + enemyHealth.gameObject.name);

                // Increase the total damage in player stats
                PlayerRunStats.playerTotalDmg += damageAmount;
            }
        }
    }
}
