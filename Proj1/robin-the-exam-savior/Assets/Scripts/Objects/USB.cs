using UnityEngine;

public class USB : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 50f;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 1f;

    public GameObject visualEffect;

    private Vector3 startPosition;
    private float timeOffset;

    void Start() {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        if (visualEffect != null) {
            visualEffect.SetActive(true);
        }
    }

    void Update() {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (GameManager.Instance != null) {
                GameManager.Instance.CollectKey();
                Destroy(gameObject);
            }
        }
    }
}
