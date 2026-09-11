using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider2D))]
public class Coin : NetworkBehaviour
{
    private Collider2D coinCollider;
    private SpriteRenderer spriteRenderer;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        coinCollider =
            GetComponent<Collider2D>();

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (coinCollider != null)
        {
            coinCollider.isTrigger = true;
        }
    }


    // ============================================================
    // PLAYER COLLECTS COIN
    // ============================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        // --------------------------------------------------------
        // ONLY State Authority handles collection.
        // --------------------------------------------------------

        if (!HasStateAuthority)
            return;

        // --------------------------------------------------------
        // Existing Player tag.
        // --------------------------------------------------------

        if (!other.CompareTag("Player"))
            return;

        // --------------------------------------------------------
        // Find CoinSpawner in the scene.
        //
        // This happens only when the coin is collected, so this
        // is not part of the normal per-frame path.
        // --------------------------------------------------------

        CoinSpawner spawner =
            FindFirstObjectByType<CoinSpawner>();

        if (spawner == null)
        {
            Debug.LogError(
                "Coin: CoinSpawner not found."
            );

            return;
        }

        spawner.CollectCoin();
    }


    // ============================================================
    // ENABLE / DISABLE
    // ============================================================

    public void SetActiveState(
        bool active)
    {
        if (coinCollider != null)
        {
            coinCollider.enabled =
                active;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                active;
        }
    }
}