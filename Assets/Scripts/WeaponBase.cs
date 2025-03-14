using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] private string weaponName = "Default Weapon";
    public string WeaponName => weaponName;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    
    // Called to pick up the weapon. Re-parents it to the new owner’s hold position.
    public virtual void Pickup(Transform newOwner)
    {
        Owner = newOwner;
        IsEquipped = true;
        transform.SetParent(newOwner);
        
        transform.position = newOwner.position;

        // Optionally disable physics and collider for the equipped weapon.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        transform.Find("GunSprite").localRotation = Quaternion.Euler(0, 0, 0);
    }

    // Called to drop the weapon back into the world.
    public virtual void Drop()
    {
        IsEquipped = false;
        Owner = null;
        transform.SetParent(null);

        // Optionally re-enable physics and collider.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Set rotation to a random angle when dropped.
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        transform.Find("GunSprite").localRotation = Quaternion.Euler(0, 0, randomAngle);
    }

    
    // Virtual aiming method that rotates the weapon to face the target position.
    // Can be overridden by derived classes.
    public virtual void Aim(Vector3 targetPosition)
    {
        Vector3 aimDirection = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip sprite based on angle for natural visuals.
        Vector3 localScale = Vector3.one;
        localScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        transform.localScale = localScale;
    }
    
    // Abstract method to shoot the weapon.
    public abstract void Shoot(Vector3 aimDirection);
}
