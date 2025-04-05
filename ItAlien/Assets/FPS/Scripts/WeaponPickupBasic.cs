using UnityEngine;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

public class WeaponPickupBasic : MonoBehaviour
{
    [SerializeField] private float pickupRange = 2.0f;
    [SerializeField] private int maxWeapons = 2;

    private GameObject player;
    private WeaponController weaponController;
    private bool isPickedUp = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        weaponController = GetComponent<WeaponController>();
        if (weaponController == null)
        {
            Debug.LogError("WeaponController component not found on weapon: " + gameObject.name);
        }
    }

    void Update()
    {
        if (isPickedUp || player == null || weaponController == null)
            return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= pickupRange && Input.GetKeyDown(KeyCode.E))
        {
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        PickupWeapon();
                    }
                }
            }
            else
            {
                PickupWeapon();
            }
        }
    }

    void PickupWeapon()
    {
        PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();

        if (weaponsManager != null)
        {
            bool weaponAdded = weaponsManager.AddWeapon(weaponController);

            if (weaponAdded)
            {
                isPickedUp = true;
                Destroy(gameObject);
                return;
            }

            int weaponCount = CountWeapons(weaponsManager);

            if (weaponCount >= maxWeapons)
            {
                WeaponController activeWeapon = weaponsManager.GetActiveWeapon();

                if (activeWeapon != null)
                {
                    weaponsManager.RemoveWeapon(activeWeapon);

                    weaponAdded = weaponsManager.AddWeapon(weaponController);

                    if (weaponAdded)
                    {
                        isPickedUp = true;
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    private int CountWeapons(PlayerWeaponsManager weaponsManager)
    {
        int count = 0;

        for (int i = 0; i < 2; i++)
        {
            if (weaponsManager.GetWeaponAtSlotIndex(i) != null)
            {
                count++;
            }
        }

        return count;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}