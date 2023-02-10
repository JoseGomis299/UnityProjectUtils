using System;
using System.Collections;
using ProjectUtils.Attacking;
using UnityEngine;

namespace ProjectUtils.Movemet2D
{
  public class Fighter : MonoBehaviour, IDamageable
  {
    [Header("Fighting")]
    [SerializeField] private int health = 10;
    [SerializeField] private int maxHealth = 10;

    [SerializeField] private float immuneTime = 1;
    protected float lastImmune;

    protected Vector3 pushDirection;
    protected GameObject fighterGFX;

    protected virtual void Start()
    {
      fighterGFX = transform.GetChild(0).gameObject;
    }

    public void ReceiveDamage(Damage dmg)
    {
      if (Time.time - lastImmune > immuneTime)
      {
        lastImmune = Time.time;
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
      var spriteRenderer = fighterGFX.GetComponent<SpriteRenderer>();
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
      
    }
  }
}
