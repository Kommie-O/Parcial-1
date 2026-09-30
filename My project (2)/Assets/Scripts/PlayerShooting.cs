using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hitscan shooting using Physics.Raycast from the center of the camera.
/// Hold the left mouse button to fire (automatic fire, limited by Cadencia).
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera used to aim. If empty, Camera.main is used.")]
    public Camera playerCamera;

    [Header("Weapon Stats")]
    [Tooltip("Maximum distance of the shot.")]
    public float Rango = 20f;

    [Tooltip("Damage dealt per hit.")]
    public int Dano = 25;

    [Tooltip("Shots per second.")]
    public float Cadencia = 1.5f;


    [Header("Raycast")]
    [Tooltip("Layers the shot can hit.")]
    public LayerMask hitMask = ~0;

    [Header("Debug")]
    public bool drawDebugRay = true;

    /// <summary>Fired whenever Balas changes. Useful for an ammo UI.</summary>
   

    private float nextFireTime;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

       
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
            TryShoot();
    }

    private void TryShoot()
    {
       

        if (playerCamera == null || Cadencia <= 0f) return;

        // Consume ammo and set the time of the next allowed shot.
      
       
        nextFireTime = Time.time + 1f / Cadencia;

        Shoot();
    }

    private void Shoot()
    {
        // Ray from the exact center of the screen / camera.
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // RaycastAll so we can ignore the player's own colliders and take the closest valid hit.
        RaycastHit[] hits = Physics.RaycastAll(ray, Rango, hitMask, QueryTriggerInteraction.Ignore);

        RaycastHit? validHit = hits
            .Where(h => !h.collider.transform.IsChildOf(transform))
            .OrderBy(h => h.distance)
            .Select(h => (RaycastHit?)h)
            .FirstOrDefault();

        if (drawDebugRay)
        {
            Vector3 end = validHit.HasValue ? validHit.Value.point : ray.origin + ray.direction * Rango;
            Debug.DrawLine(ray.origin, end, Color.red, 0.5f);
        }

        if (!validHit.HasValue) return;

        RaycastHit hit = validHit.Value;
        Debug.Log($"Hit {hit.collider.name} at {hit.distance:F1}m");

        // Damage the enemy if the object (or one of its parents) has an EnemyHealth component.
        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(Dano);
    }

    /// <summary>Call this from pickups to give the player more ammo.</summary>
  
}
