using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class PlayerController : MonoBehaviour
{
    #region TankAttributes
    [SerializeField]
    private float tankSpeed, steeringPower, turretRotationSpeed, tankFireDelay, tankBulletSpeed, tankHealth, tankCannonDamage;
    /// <summary>
    /// The current movement speed of player's tank.
    /// </summary>
    public float TankSpeed => tankSpeed * 10;
    /// <summary>
    /// The current force that the player's tank can steer (rotate force)
    /// </summary>
    public float SteeringPower => steeringPower * 10;
    /// <summary>
    /// The current rotation speed of the tank's turret.
    /// </summary>
    public float TurretRotationSpeed => turretRotationSpeed * 10;
    /// <summary>
    /// The current time in seconds to wait before another fire from the tank's main turret.
    /// </summary>
    public float TankFireDelay => tankFireDelay;
    /// <summary>
    /// The current movement speed of the turret's bullet.
    /// </summary>
    public float TankBulletSpeed => tankBulletSpeed;
    /// <summary>
    /// The current health points of the player's tank.
    /// </summary>
    public float TankHealth => tankHealth;
    /// <summary>
    /// The total health points of the player's tank. It's equal to the health at the start of the game.
    /// </summary>
    public float TankTotalHealth { get; private set; }
    /// <summary>
    /// The current damage of the bullet from the tank's main turret.
    /// </summary>
    public float TankCannonDamage => tankCannonDamage;



    float startTankSpeed;
    #endregion

    #region HumanAttributes
    [SerializeField]
    private float humanSpeed, humanHealth, axeDamage, pickAxeDamage, humanMeleeDelay, humanMeleeRadius;
    /// <summary>
    /// The player's current movement speed, when on foot.
    /// </summary>
    public float HumanSpeed => humanSpeed * 10;
    /// <summary>
    /// The player's current health points, when on foot.
    /// </summary>
    public float HumanHealth => humanHealth;
    /// <summary>
    /// The player's total health points, when on foot.
    /// </summary>
    public float HumanTotalHealth { get; private set; }
    public float AxeDamage => axeDamage;
    public float PickAxeDamage => pickAxeDamage;
    /// <summary>
    /// The current time in seconds to wait before another melee hit from the player, when on foot.
    /// </summary>
    public float HumanMeleeDelay => humanMeleeDelay;
    /// <summary>
    /// The current radius of the melee hit from the player, when on foot.
    /// </summary>
    public float HumanMeleeRadius => humanMeleeRadius;

    public MeleeWeaponController MeleeController => meleeWeaponController;

    float startHumanSpeed;
    #endregion

    #region ControlVariables
    float tankEnterDistance = 2;
    public bool isInWater { get; private set; }
    public bool isInTank { get; private set; } = true;
    public bool entityInRange { get; private set; } = false;
    public bool isInBunker { get; private set; } = false;
    public bool engineOn { get; private set; } = true;
    bool baseDoorRange;
    DateTime lastHitTime;
    #endregion

    #region CommonProperties
    [SerializeField] MeleeWeaponController meleeWeaponController;
    Transform tankTransform, humanTransform;
    Rigidbody2D RB;
    Animator animator;
    LayerMask entityLayer;
    SceneLoader sceneLoader;
    #endregion

    void Start()
    {
        TankTotalHealth = tankHealth;
        HumanTotalHealth = humanHealth;
        startTankSpeed = tankSpeed;
        startHumanSpeed = humanSpeed;
        tankTransform = transform.Find("Tank");
        humanTransform = transform.Find("Human");
        RB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        entityLayer = LayerMask.GetMask("EntityInteractable");
        sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
        tankTransform.gameObject.SetActive(true);
        humanTransform.gameObject.SetActive(false);

        if (sceneLoader.GetSceneIndex() == 1)
        {
            isInBunker = true;
            engineOn = false;
            tankTransform.GetComponentInChildren<LocalMessage>().Show();
        }
    }

    void Update()
    {
        Debug.Log(isInTank);
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isInTank && Vector2.Distance(humanTransform.position, tankTransform.position) < tankEnterDistance)
            {
                EnterTank();
            }
            else if (isInTank)
            {
                ExitTank();
            }
        }

        if (Input.GetKey(KeyCode.Mouse0) && !isInTank && entityInRange)
        {
            if (lastHitTime.AddSeconds(humanMeleeDelay) < DateTime.Now)
            {
                animator.SetTrigger("meleeHit");
                lastHitTime = DateTime.Now;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isInBunker && isInTank && baseDoorRange)
            {
                sceneLoader.LoadScene(1);
            }
            else if (isInBunker && isInTank)
            {
                sceneLoader.LoadScene(0);
            }
        }
    }

    //Função chamada pela animação ao chegar ao frame que atinge o alvo
    void GetHitColliders()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(MeleeController.GetHitPoint(), humanMeleeRadius, entityLayer);
        if (hitColliders != null)
        {
            //Causa dano em todas as entidades atingidas pelo hit
            foreach (var collider in hitColliders)
            {
                var entity = collider.GetComponentInParent<EntityObject>();

                if (entity != null)
                {
                    entity.TakeDamage(MeleeController.GetDamage(entity.MeleeTarget));
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage >= 0)
        {
            if (isInTank)
            {
                if (damage >= TankHealth) tankHealth = 0;
                else tankHealth -= damage;
            }
            else
            {
                if (damage >= HumanHealth) humanHealth = 0;
                else humanHealth -= damage;
            }
        }
    }

    void EnterTank()
    {
        if (!isInBunker)
            Camera.main.orthographicSize = 6;
        RB.velocity = Vector3.zero;
        RB.freezeRotation = false;
        humanTransform.rotation = tankTransform.rotation;
        humanTransform.gameObject.SetActive(false);
        transform.position = tankTransform.position;
        isInTank = true;

        if (isInBunker) tankTransform.GetComponentInChildren<LocalMessage>().Show();
    }

    void ExitTank()
    {
        if (isInWater) return;

        tankTransform.SetParent(null);
        Camera.main.orthographicSize = 4;
        RB.velocity = Vector3.zero;
        RB.freezeRotation = true;
        humanTransform.gameObject.SetActive(true);
        humanTransform.SetParent(null);
        humanTransform.position = tankTransform.TransformPoint(-0.6f, 0.3f, 0);
        transform.position = humanTransform.position;
        humanTransform.SetParent(transform);
        isInTank = false;

        if (isInBunker) tankTransform.GetComponentInChildren<LocalMessage>().Hide();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Tilemap_0")
        {
            tankSpeed = startTankSpeed / 2;
            humanSpeed = startHumanSpeed / 1.5f;
            isInWater = true;
        }

        if (collision.CompareTag("LocalMessage"))
        {
            LocalMessage localMessage = collision.GetComponent<LocalMessage>();

            if (collision.transform.parent.name == "Door")
            {
                if (isInTank) localMessage.ActivePreset = 0;
                else localMessage.ActivePreset = 1;
                baseDoorRange = true;
            }

            localMessage.Show();
        }
        if (collision.CompareTag("Tree") && !isInTank)
        {
            var treeSprite = collision.GetComponent<SpriteRenderer>();
            var color = new Color32(255, 255, 255, 100);
            treeSprite.color = color;

            var msg = collision.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Show();

            if (!isInWater)
            {
                entityInRange = true;
                MeleeController.gameObject.SetActive(true);
                MeleeController.SetWeapon(MeleeWeaponType.Axe);
            }
        }
        if (collision.CompareTag("Rock") && !isInTank)
        {
            var msg = collision.transform.parent.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Show();

            if (!isInWater)
            {
                entityInRange = true;
                MeleeController.SetWeapon(MeleeWeaponType.Pickaxe);
            }
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Tilemap_0")
        {
            tankSpeed = startTankSpeed;
            humanSpeed = startHumanSpeed;
            isInWater = false;
        }

        if (collision.CompareTag("LocalMessage"))
        {
            var localMessage = collision.GetComponent<LocalMessage>();
            localMessage.Hide();

            if (collision.transform.parent.name == "Base")
            {
                baseDoorRange = false;
            }
        }
        if (collision.CompareTag("Tree") && !isInTank)
        {
            var msg = collision.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Hide();

            entityInRange = false;
            var treeSprite = collision.GetComponent<SpriteRenderer>();
            var color = new Color32(255, 255, 255, 255);
            treeSprite.color = color;
            MeleeController.SetWeapon(MeleeWeaponType.Fists);
        }
        if (collision.CompareTag("Rock") && !isInTank)
        {
            var msg = collision.transform.parent.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Hide();

            entityInRange = false;
            MeleeController.SetWeapon(MeleeWeaponType.Fists);
        }
    }
}
