using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Shooting : MonoBehaviour
{
    [SerializeField] GameObject explosionPrefab, cannonBulletPrefab;
    [SerializeField] AudioClip cannonAudio;

    Transform explosionPoint;
    GameObject Player;
    PlayerController playerCTRL;
    UIController UI;
    Animator playerAnimator;

    bool isAimOnCursor = false, isTurretReset = false;
    DateTime lastTimeShot;
    AudioSource turretAudioSource, cannonAudioSource;
    float turretRotationVolume = 0, turretRotationPitch = 0.7f;

    void Start()
    {
        Player = transform.parent.parent.gameObject;
        playerCTRL = Player.GetComponent<PlayerController>();
        playerAnimator = Player.GetComponent<Animator>();
        UI = GameObject.Find("UIController").GetComponent<UIController>();
        explosionPoint = transform.Find("Explosion_Point");
        lastTimeShot = DateTime.MinValue;
        turretAudioSource = GetComponent<AudioSource>();
        cannonAudioSource = transform.Find("Cannon").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCTRL.isInTank && !playerCTRL.isInBunker)
        {
            RotateTurret();
            isTurretReset = false;
        }
        else if (!isTurretReset)
        {
            ResetTurret();
            isTurretReset = true;
        }
    }

    void RotateTurret()
    {
        //Posição do mouse
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Calcula a direção entre a torreta e o mouse
        Vector3 direction = mousePos - transform.position;
        direction.Normalize();

        //Calcula o angulo necessário para girar
        Quaternion angle = Quaternion.LookRotation(forward: Vector3.forward, upwards: direction);

        //Rotaciona a torreta a torreta até esse ângulo
        transform.rotation = Quaternion.RotateTowards(transform.rotation, angle, playerCTRL.TurretRotationSpeed * Time.deltaTime);

        //Verifica se a torreta chegou ao destino (se está alinhada com o mouse)
        isAimOnCursor = transform.rotation.z <= angle.z + 0.1 && transform.rotation.z >= angle.z - 0.1;

        //Altera valores do som da torreta se estiver em rotação
        if (!isAimOnCursor)
        {
            turretRotationVolume = Mathf.Clamp(turretRotationVolume + Time.deltaTime * 4, 0, 0.5f);
            turretRotationPitch = Mathf.Clamp(turretRotationPitch + Time.deltaTime * 4, 0.7f, 1f);
        }
        else
        {
            turretRotationVolume = Mathf.Clamp(turretRotationVolume - Time.deltaTime * 4, 0, 0.5f);
            turretRotationPitch = Mathf.Clamp(turretRotationPitch - Time.deltaTime * 4, 0.7f, 1f);
        }
        turretAudioSource.volume = turretRotationVolume;
        turretAudioSource.pitch = turretRotationPitch;

        //Lógica para trocar o ícone do mouse
        if (Input.GetMouseButtonDown(0) && DateTime.Now > lastTimeShot.AddSeconds(Player.GetComponent<PlayerController>().TankFireDelay))
        {
            Shoot();
        }
        else if (UI.currentCursor.currentIndex == UI.currentCursor.cursorTextures.Length - 1)
        {
            if (isAimOnCursor)
            {
                UI.SetCustomCursor(UIController.CursorType.AimReady);
            }
            else
            {
                UI.SetCustomCursor(UIController.CursorType.AimMoving);
            }
        }
    }

    void Shoot()
    {
        //Grava o tempo que atirou
        lastTimeShot = DateTime.Now;

        //Troca o ícone
        UI.SetCustomCursor(UIController.CursorType.AimReloading);

        //Dispara animação de tiro do canhão
        playerAnimator.SetTrigger("Shoot");

        //Instancia explosão
        var exp = Instantiate(explosionPrefab, explosionPoint.position, transform.rotation, WorldBuilder.FXInstances);
        exp.GetComponent<SelfDestroy>().Begin(1.5f);

        //Instancia bala de canhão
        var bullet = Instantiate(cannonBulletPrefab, explosionPoint.position, transform.rotation, WorldBuilder.FXInstances);
        bullet.GetComponent<SelfDestroy>().Begin(5);
        bullet.GetComponent<MoveForward>().SetSpeed(playerCTRL.TankBulletSpeed);
        bullet.GetComponent<Bullet>().Shooter = Player;
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), Player.transform.Find("Tank").GetComponent<Collider2D>());

        //Reproduz audio de tiro
        cannonAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.3f);
        cannonAudioSource.PlayOneShot(cannonAudio);
    }

    void ResetTurret()
    {
        turretRotationVolume = 0;
        turretRotationPitch = 0.7f;
        turretAudioSource.volume = turretRotationVolume;
        turretAudioSource.pitch = turretRotationPitch;
        UI.SetCustomCursor(UIController.CursorType.AimReady);
    }
}
