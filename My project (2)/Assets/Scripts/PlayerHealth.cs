using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's health ("Vida"). The player dies when Vida reaches 0.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int Vida = 100;
    public int maxVida = 100;

    [Header("Debug")]
    [Tooltip("Press K in Play mode to take 10 damage (for testing).")]
    public bool enableDebugKeys = true;

    public bool IsDead { get; private set; }

    /// <summary>Fired once when the player dies. Other scripts (UI, game manager) can subscribe.</summary>
    public event Action OnDeath;

    /// <summary>Fired whenever Vida changes. Useful for updating a health bar.</summary>
    public event Action<int> OnVidaChanged;

    private void Start()
    {
        Vida = maxVida;
        OnVidaChanged?.Invoke(Vida);
    }

    private void Update()
    {
        if (enableDebugKeys && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            TakeDamage(10);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        Vida = Mathf.Max(Vida - amount, 0);
        OnVidaChanged?.Invoke(Vida);
        Debug.Log($"Player took {amount} damage. Vida: {Vida}");

        if (Vida <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        Vida = Mathf.Min(Vida + amount, maxVida);
        OnVidaChanged?.Invoke(Vida);
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("Player has died.");

        // Stop the player from moving or looking around.
        var movement = GetComponent<FirstPersonController>();
        if (movement != null) movement.enabled = false;

        // Release the cursor so menus can be used.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnDeath?.Invoke();

        // Optional: reload the scene, show a Game Over screen, etc.
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
