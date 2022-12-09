using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed, steeringPower, turretRotationSpeed, fireDelay, bulletSpeed, health, damage, spotDistance, fireDistance;
    public float MovementSpeed { get { return movementSpeed * 10; } }
    public float SteeringPower { get { return steeringPower * 10; } }
    public float TurretRotationSpeed { get { return turretRotationSpeed * 10; } }
    public float FireDelay { get { return fireDelay; } }
    public float BulletSpeed { get { return bulletSpeed; } }
    public float Health { get { return health; } }
    public float Damage { get { return damage; } }

    [SerializeField] private GameController gameController;
    [SerializeField] GameObject explosionPrefab, cannonBulletPrefab;
    [SerializeField] AudioClip cannonAudio;

    Transform explosionPoint;

    float steeringAmount, tankSpeed, direction;
    DateTime lastTimeShot;
    Transform Wheel_FL, Wheel_FR, tankTransform, turretTransform;
    GameObject Player;
    Rigidbody2D rb;
    AudioSource cannonAudioSource;
    Collider2D tankCollider;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        Player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody2D>();
        tankTransform = transform.Find("Tank");
        tankCollider = tankTransform.GetComponent<Collider2D>();
        turretTransform = tankTransform.Find("Turret");
        explosionPoint = turretTransform.Find("Explosion_Point");
        Wheel_FL = tankTransform.Find("Wheels").Find("Wheel.FL");
        Wheel_FR = tankTransform.Find("Wheels").Find("Wheel.FR");
        cannonAudioSource = turretTransform.Find("Cannon").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(Player.transform.position, transform.position) < spotDistance)
        {
            Move();
            RotateTurret();

            if (Vector3.Distance(Player.transform.position, transform.position) < fireDistance)
            {
                Shoot();
            }
            var cameraPos = Camera.main.WorldToScreenPoint(transform.position);
            if (cameraPos.y < Screen.height && cameraPos.y > 0) fireDistance = 12;
            else fireDistance = 6;
        }
    }

    void Move()
    {
        Vector3 playerDirection = Player.transform.position - transform.position;
        playerDirection.Normalize();

        steeringAmount = -GetRelativeDirection(transform.forward, playerDirection, transform.up);

        //Velocidade
        tankSpeed = MovementSpeed * Time.deltaTime;

        //Direção em que o tanque está indo
        direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));

        //Rotaciona o tanque de acordo com a velocidade e quantidade de giro
        rb.rotation += steeringAmount * SteeringPower * rb.velocity.magnitude * direction * Time.deltaTime;

        //Move o tanque
        rb.AddRelativeForce(Vector2.up * tankSpeed);

        //Gira as rodas em até 40° de acordo com a quantidade de giro
        Wheel_FL.localRotation = Quaternion.Euler(0, 0, steeringAmount * 40);
        Wheel_FR.localRotation = Quaternion.Euler(0, 0, steeringAmount * 40);
    }

    void RotateTurret()
    {
        var playerPos = Player.transform.position;

        //Calcula a direção entre a torreta e o player
        Vector3 direction = playerPos - transform.position;
        direction.Normalize();

        //Calcula o angulo necessário para girar
        Quaternion angle = Quaternion.LookRotation(forward: Vector3.forward, upwards: direction);

        //Rotaciona a torreta a torreta até esse ângulo
        turretTransform.rotation = Quaternion.RotateTowards(turretTransform.rotation, angle, TurretRotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (DateTime.Now < lastTimeShot.AddSeconds(FireDelay))
        {
            return;
        }

        //Grava o tempo que atirou
        lastTimeShot = DateTime.Now;

        //Instancia explosão
        var exp = Instantiate(explosionPrefab, explosionPoint.position, transform.rotation, WorldBuilder.FXInstances);
        exp.GetComponent<SelfDestroy>().Begin(1.5f);

        //Instancia bala de canhão
        var bullet = Instantiate(cannonBulletPrefab, explosionPoint.position, transform.rotation, WorldBuilder.FXInstances);
        bullet.GetComponent<SelfDestroy>().Begin(5);
        bullet.GetComponent<MoveForward>().SetSpeed(BulletSpeed);
        bullet.GetComponent<Bullet>().Shooter = gameObject;
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), tankCollider);

        //Reproduz audio de tiro
        cannonAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.3f);
        cannonAudioSource.PlayOneShot(cannonAudio);
    }

    float GetRelativeDirection(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        return Mathf.Clamp(dir, -1, 1);
    }

    public void TakeDamage(float damage)
    {
        if (damage >= 0)
        {
            if (damage >= health)
            {
                health = 0;
            }
            else
            {
                health -= damage;
            }
        }
    }

    public void DestroySelf(GameObject explosion)
    {
        Destroy(this.gameObject);
        var exp = Instantiate(explosion, transform.position, transform.rotation, WorldBuilder.FXInstances);
        exp.GetComponent<SelfDestroy>().Begin(2);
        gameController.KillEnemy();
    }
}
