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
        ScreenOrientationManager.SetLandscape();
        runner = NetworkManager.Instance.GetRunner();

        if (runner == null)
        {
            //Debug.LogError("PlayerSpawner: NetworkRunner not found.");
            return;
        }

        //Debug.Log($"PlayerSpawner started. " +$"IsServer={runner.IsServer}, " +$"IsSceneAuthority={runner.IsSceneAuthority}");
        // Only Server/Host should spawn network objects.
        if (!runner.IsServer)
        {
            //Debug.Log("PlayerSpawner: Local player is not Server. " + "Waiting for network-spawned players.");
            return;
        }

        await SpawnPlayers();
    }

    private async System.Threading.Tasks.Task SpawnPlayers()
    {
        if (playersSpawned) return;

        if (runner == null) return;

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

        //Debug.Log($"PlayerSpawner: Active player count = {playerCount}");

        if (playerCount < 2)
        {
            //Debug.LogError($"PlayerSpawner: Expected 2 players, found {playerCount}.");
            return;
        }

        playersSpawned = true;

        //Debug.Log($"PlayerSpawner: " +$"Player1={player1}, " +$"Player2={player2}");
        
        NetworkObject spawnedPlayer1 =
            await runner.SpawnAsync(
                player1Prefab,
                player1SpawnPoint.position,
                player1SpawnPoint.rotation,
                player1
            );

        if (spawnedPlayer1 == null)
        {
            Debug.LogError("PlayerSpawner: Failed to spawn Player 1.");
            playersSpawned = false;
            return;
        }

        //Debug.Log($"PlayerSpawner: Player 1 spawned. " +$"Input Authority = {player1}");

        NetworkObject spawnedPlayer2 =
            await runner.SpawnAsync(
                player2Prefab,
                player2SpawnPoint.position,
                player2SpawnPoint.rotation,
                player2
            );

        if (spawnedPlayer2 == null)
        {
            //Debug.LogError("PlayerSpawner: Failed to spawn Player 2.");
            playersSpawned = false;
            return;
        }

        //Debug.Log($"PlayerSpawner: Player 2 spawned. " + $"Input Authority = {player2}");
        //Debug.Log("PlayerSpawner: Both players spawned successfully.");
    }
}