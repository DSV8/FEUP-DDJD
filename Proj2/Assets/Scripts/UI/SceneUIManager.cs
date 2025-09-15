using UnityEngine;
using UnityEngine.UI;

public class SceneUIManager : MonoBehaviour
{
    public Text coinTextInScene;

    void Start()
    {
        if (GameManager.Instance != null && coinTextInScene != null)
        {
            GameManager.Instance.SetCoinText(coinTextInScene);
        }
    }
}
