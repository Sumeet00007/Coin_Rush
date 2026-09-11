using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider2D))]
public class Coin : NetworkBehaviour
{
    private CoinSpawner coinSpawner;
    private Collider2D coinCollider;
    private SpriteRenderer spriteRenderer;

    private bool isInitialized;

    private void Awake()
    {
        coinCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        coinCollider.isTrigger = true;
    }


    public void Initialize(CoinSpawner spawner)
    {
        coinSpawner = spawner;
        isInitialized = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority)  return;

        if (!isInitialized) return;

        if (!other.CompareTag("Player")) return;

        if (coinSpawner != null)
        {
            coinSpawner.CollectCoin();
        }
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