using UnityEngine;

public class WeaponLookAt : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private bool lookOnlyY = true;
    [SerializeField] private float rotationSpeed = 5f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 targetPosition = mainCamera.transform.position;

            if (lookOnlyY)
            {
                targetPosition.y = transform.position.y;
            }

            Vector3 direction = targetPosition - transform.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}