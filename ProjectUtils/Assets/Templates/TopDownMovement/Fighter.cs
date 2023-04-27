using System.Collections;
using Templates.Attacking;
using UnityEngine;

namespace Templates.TopDownMovement
{
  public class Fighter : MonoBehaviour, IDamageable
  {
    public int health = 10;
    public int maxHealth = 10;
    public float pushRecoverySpeed = 1;

    [SerializeField] private float immuneTime = 1;
    private float _lastImmune;

    protected Vector3 pushDirection;
    protected Vector3 dashDirection;

    public void ReceiveDamage(Damage dmg)
    {
      if (Time.time - _lastImmune > immuneTime)
      {
        _lastImmune = Time.time;
        health -= dmg.damageAmount;
        pushDirection = (transform.position - dmg.origin).normalized * dmg.pushForce;

        if (health <= 0)
        {
          health = 0;
          Death();
        }

        StartCoroutine(ImmuneDisplay());
      }
    }

    private IEnumerator ImmuneDisplay()
    {
      float elapsedTime = 0;
      SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
      if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      Color originalColor = spriteRenderer.color;

      while (elapsedTime < immuneTime)
      {
        spriteRenderer.color = new Color(255, 255, 255, 0);
        yield return new WaitForSeconds(immuneTime / 10);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(immuneTime / 10);
        elapsedTime += immuneTime / 5;

      }

      spriteRenderer.color = originalColor;
    }

    protected virtual void Death()
    {
      Destroy(gameObject);
    }
  }
}
