using UnityEngine;

public class WeaponAnim : MonoBehaviour
{
    private float startingY;
    private float floatSpeed;
    private float floatHeight;
    private float timeOffset;

    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1.5f;
    [SerializeField] private float minHeight = 0.2f;
    [SerializeField] private float maxHeight = 0.5f;

    void Start()
    {
        startingY = transform.position.y;

        floatSpeed = Random.Range(minSpeed, maxSpeed);
        floatHeight = Random.Range(minHeight, maxHeight);
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        float newY = startingY + Mathf.Abs(Mathf.Sin((Time.time + timeOffset) * floatSpeed)) * floatHeight;

        Vector3 position = transform.position;
        position.y = newY;
        transform.position = position;
    }
}