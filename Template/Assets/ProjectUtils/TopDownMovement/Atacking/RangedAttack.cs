using ProjectUtils.ObjectPooling;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectUtils.Attacking
{
  public class RangedAttack : MonoBehaviour
  {

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject explosionDisplay;
    [SerializeField] private float attackCoolDown;
    [SerializeField] private float circularAttackCoolDown;
    private float _lastCircularAttack;
    private float _lastAttackTime;

    public void Attack(Vector3 shootPoint, Vector3 direction, int damage, int pierceCount, float pushForce,
      int areaDamage, float damageRadius, float bulletSpeed)
    {
      Attack(shootPoint, direction, damage, pierceCount, pushForce, areaDamage, damageRadius, bulletSpeed, Vector3.one);
    }

    public void Attack(Vector3 shootPoint, Vector3 direction, int damage, int pierceCount, float pushForce,
      int areaDamage, float damageRadius, float bulletSpeed, Vector3 bulletScale)
    {
      Attack(shootPoint, direction, damage, pierceCount, pushForce, areaDamage, damageRadius, bulletSpeed, bulletScale,
        0);
    }

    public void Attack(Vector3 shootPoint, Vector3 direction, int damage, int pierceCount, float pushForce,
      int areaDamage, float damageRadius, float bulletSpeed, Vector3 bulletScale, float imprecision)
    {
      SemiCircularAttack(shootPoint, direction, 1, damage, pushForce, pierceCount, areaDamage, damageRadius, bulletSpeed, bulletScale,  imprecision, 0, 0);
    }

    public void CircularAttack(int bulletNumber, float initialOffset, int damage, int pierceCount, float pushForce,
      int areaDamage, float damageRadius, float bulletSpeed, Vector3 bulletScale, float imprecision)
    {
      if (Time.time - _lastCircularAttack < circularAttackCoolDown) return;
      _lastCircularAttack = Time.time;

      float angle = (360f / bulletNumber) * Mathf.Deg2Rad;

      for (int i = 0; i < bulletNumber; i++)
      {
        //Get new direction and position for the bullet
        Vector3 direction = new Vector3(Mathf.Cos(angle * i), Mathf.Sin(angle * i), 0);
        Vector3 initialPos = transform.position + direction * (transform.localScale.magnitude / 2 * initialOffset);

        Quaternion bulletRotation = Quaternion.Euler(0, 0, (angle * i) * Mathf.Rad2Deg);
        GameObject bullet = ObjectPool.Instance.InstantiateFromPool(bulletPrefab, initialPos, bulletRotation, true);
        var bulletComponent = bullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
          bullet.transform.localScale = bulletScale;

          //Select a random angle to rotate direction
          float rotationAngle = Random.Range(-45f * imprecision, 45f * imprecision) * Mathf.Deg2Rad;
          //Rotate direction the desired angle
          direction = new Vector3(transform.right.x + Mathf.Cos(Mathf.PI / 2 + rotationAngle) * transform.right.y,
            transform.right.y + Mathf.Sin(rotationAngle) * transform.right.x, 0);

          bulletComponent.SetProperties(direction, damage, pushForce, bulletSpeed, pierceCount, areaDamage,
            damageRadius, explosionDisplay, gameObject.GetInstanceID());
        }
      }
    }

    public void SemiCircularAttack(Vector3 shootPoint, Vector3 direction, int bulletNumber, int damage, float pushForce, int pierceCount,
      int areaDamage, float damageRadius, float bulletSpeed, Vector3 bulletScale, float imprecision, float startAngle, float maxAngle)
    {
      if (Time.time - _lastAttackTime < attackCoolDown) return;
      _lastAttackTime = Time.time;

      //initialize angle between bullets
      float angle = 0;
      if (bulletNumber > 1)
      {
        angle = (maxAngle + Mathf.Abs(startAngle)) / (bulletNumber - 1) * Mathf.Deg2Rad;
        if (angle >= 2 * Mathf.PI) angle -= (int)(angle / (2 * Mathf.PI)) * (2 * Mathf.PI);
      }

      //Rotate bullet direction taking into account the fighter direction
      startAngle *= Mathf.Deg2Rad;
      startAngle += Mathf.Atan2(direction.y, direction.x);
      
      for (int i = 0; i < bulletNumber; i++)
      {
        Quaternion bulletRotation = Quaternion.Euler(0, 0, (startAngle + angle * i) * Mathf.Rad2Deg);

        GameObject bullet = ObjectPool.Instance.InstantiateFromPool(bulletPrefab, shootPoint, bulletRotation, true);
        var bulletComponent = bullet.GetComponent<Bullet>();
        
        if (bulletComponent != null)
        {
          bullet.transform.localScale = bulletScale;

          //Select a random angle to rotate direction
          float rotationAngle = Random.Range(-45f * imprecision, 45f * imprecision) * Mathf.Deg2Rad;
          //Rotate direction the desired angle
          direction = new Vector3(transform.right.x + Mathf.Cos(Mathf.PI / 2 + rotationAngle) * transform.right.y,
            transform.right.y + Mathf.Sin(rotationAngle) * transform.right.x, 0);

          bulletComponent.SetProperties(direction, damage, pushForce, bulletSpeed, pierceCount, areaDamage,
            damageRadius, explosionDisplay, gameObject.GetInstanceID());
        }
      }
    }
  }
}
