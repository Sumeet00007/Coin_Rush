using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider2D))]
public class Coin : NetworkBehaviour
{
    private Collider2D coinCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        coinCollider = GetComponent<Collider2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (coinCollider != null)
        {
            coinCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority)  return;

        if (!other.CompareTag("Player")) return;

        NetworkObject playerObject = other.GetComponentInParent<NetworkObject>();

        if (playerObject == null)
        {
            Debug.LogWarning("Coin: Player NetworkObject not found." );
            return;
        }

        PlayerRef collectingPlayer = playerObject.InputAuthority;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoin(collectingPlayer);
        }

        CoinSpawner spawner = FindFirstObjectByType<CoinSpawner>();

        if (spawner == null)
        {
            Debug.LogError("Coin: CoinSpawner not found.");
            return;
        }

        spawner.CollectCoin();
    }


    public void SetActiveState(bool active)
    {
        if (coinCollider != null)
        {
            coinCollider.enabled = active;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
    }
}