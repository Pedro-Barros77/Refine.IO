using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform;
    [SerializeField] AudioSource waterAudioSource;

    Transform Wheel_FL, Wheel_FR, cameraPoint, tankTransform, humanTransform;
    Rigidbody2D rb;
    PlayerController playerCTRL;
    Animator playerAnimator;

    //Tank
    float steeringAmount, tankSpeed, direction;

    //Human
    float humanSpeed;

    void Start()
    {
        playerCTRL = GetComponent<PlayerController>();
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        tankTransform = transform.Find("Tank");
        humanTransform = transform.Find("Human");
        Wheel_FL = tankTransform.Find("Wheels").Find("Wheel.FL");
        Wheel_FR = tankTransform.Find("Wheels").Find("Wheel.FR");
        cameraPoint = transform.Find("CameraPoint");
        waterAudioSource.volume = 0;
    }

    void Update()
    {
        var yInput = Input.GetAxis("Vertical");
        var xInput = Input.GetAxis("Horizontal");

        if (playerCTRL.isInTank)
        {
            tankTransform.parent = playerCTRL.transform;
            MoveTank(xInput, yInput);
        }
        else
        {
            tankTransform.parent = null;
            MoveHuman(xInput, yInput);
        }
        //Faz a câmera seguir o player
        cameraTransform.position = new Vector3(cameraPoint.transform.position.x, cameraPoint.transform.position.y, -10);
    }

    void MoveTank(float xInput, float yInput)
    {
        playerAnimator.SetBool("isInTank", true);
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetBool("isSwimming", false);

        //Quantidade em que a direção está virada
        steeringAmount = -xInput;

        //Velocidade
        tankSpeed = yInput * playerCTRL.TankSpeed * Time.deltaTime;
        //Direção em que o tanque está indo
        direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
        //Rotaciona o tanque de acordo com a velocidade e quantidade de giro
        rb.rotation += steeringAmount * playerCTRL.SteeringPower * rb.velocity.magnitude * direction * Time.deltaTime;

        //Move o tanque
        if (!playerCTRL.isInBunker)
            rb.AddRelativeForce(Vector2.up * tankSpeed);

        //Gira as rodas em até 40° de acordo com a quantidade de giro
        Wheel_FL.localRotation = Quaternion.Euler(0, 0, steeringAmount * 40);
        Wheel_FR.localRotation = Quaternion.Euler(0, 0, steeringAmount * 40);

        if (xInput != 0 || yInput != 0)
        {
            if (playerCTRL.isInWater)
                waterAudioSource.volume = Mathf.Clamp(waterAudioSource.volume + Time.deltaTime, 0, 0.5f);
            else
                waterAudioSource.volume = Mathf.Clamp(waterAudioSource.volume - Time.deltaTime, 0, 0.5f);
        }
    }

    void MoveHuman(float xInput, float yInput)
    {
        //Velocidade
        humanSpeed = playerCTRL.HumanSpeed;

        //Posição do mouse
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Calcula a direção entre o player e o mouse
        Vector3 direction = mousePos - humanTransform.position;
        direction.Normalize();

        //Calcula o angulo necessário para girar
        Quaternion angle = Quaternion.LookRotation(forward: Vector3.forward, upwards: direction);

        humanTransform.rotation = Quaternion.RotateTowards(humanTransform.rotation, angle, 1);

        rb.AddForce(new Vector2(xInput * humanSpeed * Time.deltaTime, yInput * humanSpeed * Time.deltaTime));

        playerAnimator.SetBool("isInTank", false);

        if (xInput != 0 || yInput != 0)
        {
            if (playerCTRL.isInWater)
            {
                waterAudioSource.volume = Mathf.Clamp(waterAudioSource.volume + Time.deltaTime, 0, 0.5f);
            }
            else
            {
                playerAnimator.SetBool("isWalking", true);
                playerAnimator.SetBool("isSwimming", false);
                waterAudioSource.volume = Mathf.Clamp(waterAudioSource.volume - Time.deltaTime, 0, 0.5f);
            }
        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
            waterAudioSource.volume = Mathf.Clamp(waterAudioSource.volume - Time.deltaTime, 0, 0.5f);
        }

        if (playerCTRL.isInWater)
        {
            playerAnimator.SetBool("isWalking", false);
            playerAnimator.SetBool("isSwimming", true);
        }
        else
        {
            playerAnimator.SetBool("isSwimming", false);
        }
    }
}
