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

    // ============================================================
    // NETWORKED STATE
    // ============================================================

    // -1 means no position has been selected yet.
    [Networked]
    private int SpawnPointIndex { get; set; } = -1;

    [Networked]
    private NetworkBool CoinActive { get; set; }

    // Reference to the ONE coin NetworkObject.
    private NetworkObject coinObject;

    private bool hasSpawnedCoin;

    private int lastSpawnPointIndex = -1;


    // ============================================================
    // SPAWNED
    // ============================================================

    public override void Spawned()
    {
        // --------------------------------------------------------
        // VERY IMPORTANT:
        //
        // Only State Authority creates the coin.
        //
        // In Host/Client mode:
        // Host = State Authority
        // Client = Proxy
        // --------------------------------------------------------

        if (!HasStateAuthority)
            return;

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "CoinSpawner: No spawn points assigned."
            );

            return;
        }

        if (coinPrefab == null)
        {
            Debug.LogError(
                "CoinSpawner: Coin prefab is not assigned."
            );

            return;
        }

        SpawnCoin();
    }


    // ============================================================
    // INITIAL COIN SPAWN
    // ============================================================

    private async void SpawnCoin()
    {
        if (hasSpawnedCoin)
            return;

        hasSpawnedCoin = true;

        // --------------------------------------------------------
        // Select random position ONLY on State Authority.
        // --------------------------------------------------------

        int index =
            GetRandomSpawnPointIndex();

        lastSpawnPointIndex = index;

        Transform spawnPoint =
            spawnPoints[index];

        // --------------------------------------------------------
        // Spawn exactly ONE coin.
        // --------------------------------------------------------

        NetworkObject spawnedCoin =
            await Runner.SpawnAsync(
                coinPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                PlayerRef.None
            );

        if (spawnedCoin == null)
        {
            Debug.LogError(
                "CoinSpawner: Failed to spawn coin."
            );

            hasSpawnedCoin = false;

            return;
        }

        coinObject = spawnedCoin;

        // --------------------------------------------------------
        // IMPORTANT:
        //
        // This is the value that gets synchronized to the Client.
        // --------------------------------------------------------

        SpawnPointIndex = index;

        CoinActive = true;

        ApplyCoinState();
    }


    // ============================================================
    // COIN COLLECTED
    // ============================================================

    public void CollectCoin()
    {
        // --------------------------------------------------------
        // ONLY State Authority is allowed to change game state.
        // --------------------------------------------------------

        if (!HasStateAuthority)
            return;

        if (!CoinActive)
            return;

        if (coinObject == null)
            return;

        // --------------------------------------------------------
        // Select another position.
        // --------------------------------------------------------

        int newIndex =
            GetRandomSpawnPointIndex();

        lastSpawnPointIndex = newIndex;

        // --------------------------------------------------------
        // Update the NETWORKED spawn index FIRST.
        // --------------------------------------------------------

        SpawnPointIndex = newIndex;

        // --------------------------------------------------------
        // Move the existing coin.
        // --------------------------------------------------------

        Transform newSpawnPoint =
            spawnPoints[newIndex];

        coinObject.transform.SetPositionAndRotation(
            newSpawnPoint.position,
            newSpawnPoint.rotation
        );

        // --------------------------------------------------------
        // Make coin available again.
        // --------------------------------------------------------

        CoinActive = true;

        ApplyCoinState();
    }


    // ============================================================
    // RANDOM POSITION
    // ============================================================

    private int GetRandomSpawnPointIndex()
    {
        int count =
            spawnPoints.Length;

        if (count <= 1)
            return 0;

        int index;

        if (!preventSamePosition)
        {
            return Random.Range(
                0,
                count
            );
        }

        do
        {
            index =
                Random.Range(
                    0,
                    count
                );

        } while (index == lastSpawnPointIndex);

        return index;
    }


    // ============================================================
    // NETWORK STATE CHANGE
    // ============================================================

    public override void Render()
    {
        // --------------------------------------------------------
        // Client receives SpawnPointIndex from Host.
        //
        // Make sure the coin is positioned according to the
        // synchronized value.
        // --------------------------------------------------------

        if (coinObject == null)
        {
            FindCoinObject();
        }

        if (coinObject == null)
            return;

        if (SpawnPointIndex < 0 ||
            SpawnPointIndex >= spawnPoints.Length)
        {
            return;
        }

        Transform spawnPoint =
            spawnPoints[SpawnPointIndex];

        // --------------------------------------------------------
        // Do NOT generate a random number here.
        //
        // The Client MUST use the Host's synchronized index.
        // --------------------------------------------------------

        coinObject.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        ApplyCoinState();
    }


    // ============================================================
    // FIND COIN
    // ============================================================

    private void FindCoinObject()
    {
        // --------------------------------------------------------
        // Search only until we find the one spawned coin.
        //
        // This normally happens only during initialization.
        // --------------------------------------------------------

        Coin[] coins =
            FindObjectsByType<Coin>(
                FindObjectsSortMode.None
            );

        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] == null)
                continue;

            NetworkObject networkObject =
                coins[i].GetComponent<NetworkObject>();

            if (networkObject == null)
                continue;

            coinObject =
                networkObject;

            return;
        }
    }


    // ============================================================
    // APPLY COIN ACTIVE STATE
    // ============================================================

    private void ApplyCoinState()
    {
        if (coinObject == null)
            return;

        Coin coin =
            coinObject.GetComponent<Coin>();

        if (coin == null)
            return;

        coin.SetActiveState(
            CoinActive
        );
    }


}