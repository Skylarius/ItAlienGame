using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> meleeWeaponPrefabs;
    [SerializeField] private List<GameObject> longWeaponPrefabs;
    [SerializeField] private int numberOfMeleeWeapons = 5;
    [SerializeField] private int numberOfLongWeapons = 3;
    [SerializeField] private float minDistanceBetweenWeapons = 2.0f;
    [SerializeField] private Transform navMeshOrigin;
    [SerializeField] private float spawnRadius = 50.0f;

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        if (meleeWeaponPrefabs.Count == 0 || longWeaponPrefabs.Count == 0)
        {
            Debug.LogError("Weapon lists are empty! Please assign weapon prefabs.");
            return;
        }

        SpawnWeapons(meleeWeaponPrefabs, numberOfMeleeWeapons);
        SpawnWeapons(longWeaponPrefabs, numberOfLongWeapons);
    }

    void SpawnWeapons(List<GameObject> weaponList, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject selectedWeapon = weaponList[Random.Range(0, weaponList.Count)];
            Vector3 spawnPosition = GetRandomNavMeshPosition();

            if (spawnPosition != Vector3.zero)
            {
                GameObject weapon = Instantiate(selectedWeapon, spawnPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
                spawnedPositions.Add(spawnPosition);
            }
            else
            {
                Debug.LogWarning("Could not find valid spawn position for weapon");
            }
        }
    }

    Vector3 GetRandomNavMeshPosition()
    {
        if (navMeshOrigin == null)
        {
            navMeshOrigin = transform;
            Debug.LogWarning("NavMeshOrigin not set, using this GameObject as origin");
        }

        for (int attempts = 0; attempts < 30; attempts++)
        {
            Vector3 randomPoint = navMeshOrigin.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                if (IsPositionFarEnough(hit.position))
                {
                    return hit.position + Vector3.up * 0.5f;
                }
            }
        }

        return Vector3.zero;
    }

    bool IsPositionFarEnough(Vector3 position)
    {
        foreach (Vector3 pos in spawnedPositions)
        {
            if (Vector3.Distance(position, pos) < minDistanceBetweenWeapons)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearWeapons()
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");
        foreach (GameObject weapon in weapons)
        {
            Destroy(weapon);
        }
        spawnedPositions.Clear();
    }

    public void RespawnWeapons()
    {
        ClearWeapons();
        SpawnWeapons(meleeWeaponPrefabs, numberOfMeleeWeapons);
        SpawnWeapons(longWeaponPrefabs, numberOfLongWeapons);
    }
}