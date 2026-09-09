using Fusion;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private NetworkObject player1Prefab;
    [SerializeField] private NetworkObject player2Prefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    private NetworkRunner runner;

    private bool playersSpawned;

    private async void Start()
    {
        runner = NetworkManager.Instance.GetRunner();

        if (runner == null)
        {
            Debug.LogError("PlayerSpawner: NetworkRunner not found.");
            return;
        }

        // Only Host / State Authority spawns network objects.
        if (!runner.IsServer)
        {
            Debug.Log("PlayerSpawner: Client does not spawn players.");
            return;
        }

        await SpawnPlayers();
    }

    private async System.Threading.Tasks.Task SpawnPlayers()
    {
        if (playersSpawned)
            return;

        PlayerRef player1 = PlayerRef.None;
        PlayerRef player2 = PlayerRef.None;

        int playerCount = 0;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (playerCount == 0)
            {
                player1 = player;
            }
            else if (playerCount == 1)
            {
                player2 = player;
            }

            playerCount++;
        }

        if (playerCount < 2)
        {
            Debug.LogError(
                $"PlayerSpawner: Expected 2 players, but found {playerCount}."
            );

            return;
        }

        playersSpawned = true;

        Debug.Log(
            $"PlayerSpawner: Spawning Player 1 ({player1}) and Player 2 ({player2})."
        );

        // -----------------------------------------
        // Spawn Player 1
        // -----------------------------------------

        NetworkObject spawnedPlayer1 = await runner.SpawnAsync(
            player1Prefab,
            player1SpawnPoint.position,
            player1SpawnPoint.rotation,
            player1
        );

        if (spawnedPlayer1 == null)
        {
            Debug.LogError("Failed to spawn Player 1.");

            playersSpawned = false;
            return;
        }

        Debug.Log(
            $"Player 1 spawned successfully. Owner: {player1}"
        );

        // -----------------------------------------
        // Spawn Player 2
        // -----------------------------------------

        NetworkObject spawnedPlayer2 = await runner.SpawnAsync(
            player2Prefab,
            player2SpawnPoint.position,
            player2SpawnPoint.rotation,
            player2
        );

        if (spawnedPlayer2 == null)
        {
            Debug.LogError("Failed to spawn Player 2.");

            playersSpawned = false;
            return;
        }

        Debug.Log(
            $"Player 2 spawned successfully. Owner: {player2}"
        );

        Debug.Log("PlayerSpawner: Both players spawned successfully.");
    }
}

