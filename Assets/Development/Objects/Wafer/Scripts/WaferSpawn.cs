using UnityEngine;

public class WaferSpawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject waferUncoatedPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.02f, 0f);

    public void SpawnWafer()
    {
        if (spawnPoint == null || waferUncoatedPrefab == null) return;
        Instantiate(waferUncoatedPrefab, spawnPoint.position + offset, spawnPoint.rotation);
    }
}