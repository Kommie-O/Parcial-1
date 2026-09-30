using System;
using UnityEngine;

/// <summary>
/// Attach to any enemy. It needs a Collider so the raycast can hit it.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    public int Vida = 100;

    public bool IsDead { get; private set; }

    /// <summary>Fired once when the enemy dies.</summary>
    public event Action OnDeath;

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        Vida = Mathf.Max(Vida - amount, 0);
        Debug.Log($"{name} took {amount} damage. Vida: {Vida}");

        if (Vida <= 0)
            Die();
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log($"{name} died.");
        OnDeath?.Invoke();

        // Replace with a death animation, loot drop, etc. if needed.
        Destroy(gameObject);
    }
}
