using UnityEngine;

public class Weapon : MonoBehaviour
{

    [SerializeField] Transform hand;

    // Initialize any necessary components or variables here
    void Awake()
    {
        transform.SetParent(hand);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
