using Fusion;
using UnityEngine;

public class PlayerRespawner : NetworkBehaviour
{
    [Header("Respawn Points")]
    [SerializeField] private Transform player1RespawnPoint;
    [SerializeField] private Transform player2RespawnPoint;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only State Authority handles the respawn.
        // This prevents Host and Client from independently
        if (!HasStateAuthority)
            return;

        if (!other.CompareTag("Player")) return;

        NetworkObject playerObject =  other.GetComponentInParent<NetworkObject>();

        if (playerObject == null)
        {
           // Debug.LogWarning("PlayerRespawner: Player has no NetworkObject.");
            return;
        }

        PlayerRef playerRef = playerObject.InputAuthority;
        Transform respawnPoint = GetRespawnPoint(playerRef);

        if (respawnPoint == null)
        {
            //Debug.LogError($"PlayerRespawner: No respawn point found for {playerRef}.");
            return;
        }

        RespawnPlayer(playerObject, respawnPoint);
    }

    private Transform GetRespawnPoint(PlayerRef playerRef)
    {
        if (player1RespawnPoint == null || player2RespawnPoint == null)
        {
           // Debug.LogError("PlayerRespawner: Respawn points are not assigned.");
            return null;
        }

        NetworkRunner runner = Runner;

        if (runner == null)  return null;

        int playerIndex = 0;

        foreach (PlayerRef activePlayer in runner.ActivePlayers)
        {
            if (activePlayer == playerRef)
            {
                if (playerIndex == 0) return player1RespawnPoint;

                if (playerIndex == 1)  return player2RespawnPoint;

                break;
            }

            playerIndex++;
        }

        return null;
    }

    private void RespawnPlayer(NetworkObject playerObject,Transform respawnPoint)
    {
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        playerObject.transform.SetPositionAndRotation(respawnPoint.position,respawnPoint.rotation);

        //Debug.Log($"PlayerRespawner: Player {playerObject.InputAuthority} " +$"respawned at {respawnPoint.position}");
    }
}