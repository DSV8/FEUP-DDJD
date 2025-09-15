using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    public GameObject gameManagerPrefab;

    void Awake()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManagerPrefab);
        }
    }
}
