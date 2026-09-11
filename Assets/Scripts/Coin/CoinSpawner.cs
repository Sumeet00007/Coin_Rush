using Fusion;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [Header("Coin")]
    [SerializeField] private NetworkObject coinPrefab;

    [Header("Possible Coin Spawn Locations")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Settings")]
    [SerializeField] private bool preventSamePosition = true;

    [Networked]
    private NetworkObject CoinObject { get; set; }

    [Networked]
    private NetworkBool CoinActive { get; set; }

    
    private bool spawnStarted;
    private Coin coin;
    private int lastSpawnIndex = -1;


    public override void Spawned()
    {
        // Only State Authority creates the networked coin.
        if (!HasStateAuthority)
            return;

        SpawnInitialCoin();
    }


    private async void SpawnInitialCoin()
    {
        if (spawnStarted) return;

        spawnStarted = true;

        if (coinPrefab == null)
        {
            Debug.LogError("CoinSpawner: Coin Prefab is not assigned.");
            spawnStarted = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("CoinSpawner: No spawn points assigned.");
            spawnStarted = false;
            return;
        }

        int spawnIndex = GetRandomSpawnIndex();

        Transform spawnPoint = spawnPoints[spawnIndex];

        lastSpawnIndex = spawnIndex;

        NetworkObject spawnedCoin = await Runner.SpawnAsync
            (
                coinPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                PlayerRef.None
            );

        if (spawnedCoin == null)
        {
            Debug.LogError("CoinSpawner: Failed to spawn coin.");
            spawnStarted = false;
            return;
        }

        CoinObject = spawnedCoin;

        coin = spawnedCoin.GetComponent<Coin>();

        if (coin == null)
        {
            Debug.LogError("CoinSpawner: Coin prefab requires a Coin component.");
            return;
        }

        // Give the coin a reference to this spawner.
        coin.Initialize(this);
        CoinActive = true;
        ApplyCoinState();
    }


    

    public void CollectCoin()
    {
        // --------------------------------------------------------
        // IMPORTANT:
        // Only State Authority changes the network state.
        // --------------------------------------------------------

        if (!HasStateAuthority) return;

        if (!CoinActive)  return;

        if (CoinObject == null)  return;
        
        CoinActive = false;
        ApplyCoinState();

        SpawnCoinAtRandomPosition();
    }

    private void SpawnCoinAtRandomPosition()
    {
        if (CoinObject == null) return;

        int spawnIndex = GetRandomSpawnIndex();

        Transform spawnPoint = spawnPoints[spawnIndex];

        lastSpawnIndex = spawnIndex;

        CoinObject.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        CoinActive = true;

        ApplyCoinState();
    }


    // ============================================================
    // RANDOM SPAWN POINT
    // ============================================================

    private int GetRandomSpawnIndex()
    {
        int count =  spawnPoints.Length;

     
        if (count == 1) return 0;

        int index;

        do
        {
            index = Random.Range(0, count);

        }

        while
        (
            preventSamePosition && index == lastSpawnIndex
        );

        return index;
    }


    private void ApplyCoinState()
    {
        if (CoinObject == null)
            return;

        Coin coinComponent = coin != null? coin: CoinObject.GetComponent<Coin>();

        if (coinComponent != null)
        {
            coinComponent.SetActiveState(CoinActive);
        }
    }

    public bool IsCoinActive()
    {
        return CoinActive;
    }
}