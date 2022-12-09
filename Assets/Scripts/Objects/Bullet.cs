using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject Shooter,Explosion;


    void Start()
    {

    }


    void Update()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Shooter != null && !Shooter.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(Shooter.GetComponent<Enemy>().Damage);
            Explode(collision);
        }
        else if (collision.gameObject.CompareTag("Enemy") && Shooter != null && !Shooter.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(Shooter.GetComponent<PlayerController>().TankCannonDamage);
            Explode(collision);

            if (enemy.Health - Shooter.GetComponent<PlayerController>().TankCannonDamage <= 0)
            {
                enemy.DestroySelf(Explosion);
            }
        }
    }

    void Explode(Collision2D collision)
    {
        Destroy(this.gameObject);
        var explosion = Instantiate(Explosion, transform.position, transform.rotation, WorldBuilder.FXInstances);
        explosion.GetComponent<SelfDestroy>().Begin(2);
    }
}
