using Fusion;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [Header("Coin Prefab")]
    [SerializeField] private NetworkObject coinPrefab;

    [Header("Possible Coin Spawn Locations")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Settings")]
    [SerializeField] private bool preventSamePosition = true;

    [Networked] private int SpawnPointIndex { get; set; } = -1;
    [Networked] private NetworkBool CoinActive { get; set; }

    // Reference to the ONE coin NetworkObject.
    private NetworkObject coinObject;
    private bool hasSpawnedCoin;
    private int lastSpawnPointIndex = -1;
    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError( "CoinSpawner: No spawn points assigned.");
            return;
        }

        if (coinPrefab == null)
        {
            Debug.LogError("CoinSpawner: Coin prefab is not assigned.");
            return;
        }

        SpawnCoin();
    }



    private async void SpawnCoin()
    {
        if (hasSpawnedCoin) return;
        hasSpawnedCoin = true;

        int index = GetRandomSpawnPointIndex();
        lastSpawnPointIndex = index;
        Transform spawnPoint = spawnPoints[index];

        NetworkObject spawnedCoin =
            await Runner.SpawnAsync(
                coinPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                PlayerRef.None
            );

        if (spawnedCoin == null)
        {
            Debug.LogError("CoinSpawner: Failed to spawn coin.");
            hasSpawnedCoin = false;
            return;
        }

        coinObject = spawnedCoin;
        SpawnPointIndex = index;
        CoinActive = true;

        ApplyCoinState();
    }


    public void CollectCoin()
    {
        if (!HasStateAuthority) return;

        if (!CoinActive) return;

        if (coinObject == null) return;

        int newIndex = GetRandomSpawnPointIndex();
        lastSpawnPointIndex = newIndex;

        SpawnPointIndex = newIndex;

        Transform newSpawnPoint = spawnPoints[newIndex];

        coinObject.transform.SetPositionAndRotation(
            newSpawnPoint.position,
            newSpawnPoint.rotation
        );

        CoinActive = true;
        ApplyCoinState();
    }

    private int GetRandomSpawnPointIndex()
    {
        int count = spawnPoints.Length;

        if (count <= 1) return 0;

        int index;

        if (!preventSamePosition)
        {
            return Random.Range(0,count);
        }

        do
        {
            index = Random.Range(0,count);

        } 
        while (index == lastSpawnPointIndex);

        return index;
    }


    public override void Render()
    {
        if (coinObject == null)
        {
            FindCoinObject();
        }

        if (coinObject == null) return;

        if (SpawnPointIndex < 0 || SpawnPointIndex >= spawnPoints.Length)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[SpawnPointIndex];

        coinObject.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        ApplyCoinState();
    }

    private void FindCoinObject()
    {
        
        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);

        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] == null) continue;

            NetworkObject networkObject = coins[i].GetComponent<NetworkObject>();

            if (networkObject == null)  continue;
            coinObject = networkObject;
            return;
        }
    }

    private void ApplyCoinState()
    {
        if (coinObject == null) return;

        Coin coin = coinObject.GetComponent<Coin>();

        if (coin == null)  return;

        coin.SetActiveState(CoinActive);
    }


}