using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region TankAttributes
    [SerializeField]
    private float tankSpeed, steeringPower, turretRotationSpeed, tankFireDelay, tankBulletSpeed, tankHealth, tankCannonDamage;
    public float TankSpeed { get { return tankSpeed * 10; } }
    public float SteeringPower { get { return steeringPower * 10; } }
    public float TurretRotationSpeed { get { return turretRotationSpeed * 10; } }
    public float TankFireDelay { get { return tankFireDelay; } }
    public float TankBulletSpeed { get { return tankBulletSpeed; } }
    public float TankHealth { get { return tankHealth; } }
    public float TankTotalHealth { get; private set; }
    public float TankCannonDamage { get { return tankCannonDamage; } }



    float startTankSpeed;
    #endregion

    #region HumanAttributes
    [SerializeField]
    private float humanSpeed, humanHealth, axeDamage, pickAxeDamage, humanMeleeDelay, humanMeleeRadius;
    public float HumanSpeed { get { return humanSpeed * 10; } }
    public float HumanHealth { get { return humanHealth; } }
    public float HumanTotalHealth { get; private set; }
    public float AxeDamage { get { return axeDamage; } }
    public float PickAxeDamage { get { return pickAxeDamage; } }
    public float HumanMeleeDelay { get { return humanMeleeDelay; } }
    public float HumanMeleeRadius { get { return humanMeleeRadius; } }

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
    enum HandedTool
    {
        None = 0,
        Axe = 1,
        PickAxe = 2
    }
    HandedTool handedTool;
    #endregion

    #region CommonProperties
    [SerializeField] Transform axeTransform, pickAxeTransform;
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
        handedTool = HandedTool.None;

        if (sceneLoader.GetSceneIndex() == 1)
        {
            isInBunker = true;
            engineOn = false;
            tankTransform.GetComponentInChildren<LocalMessage>().Show();
        }
    }

    void Update()
    {
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
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(GetToolHitPoint(), humanMeleeRadius, entityLayer);
        if (hitColliders != null)
        {
            //Causa dano em todas as entidades atingidas pelo hit
            foreach (var collider in hitColliders)
            {
                var entity = collider.GetComponentInParent<EntityObject>();

                if (entity != null)
                {
                    entity.TakeDamage(GetToolDamage());
                }
            }
        }
    }

    //Retorna a posição da ponta da ferramenta atual na mão
    Vector3 GetToolHitPoint()
    {
        var hitPoint = new Vector3();
        switch (handedTool)
        {
            case HandedTool.Axe:
                hitPoint = axeTransform.Find("HitPoint").position;
                break;
            case HandedTool.PickAxe:
                hitPoint = pickAxeTransform.Find("HitPoint").position;
                break;
        }
        return hitPoint;
    }

    float GetToolDamage()
    {
        float dmg = 0;
        switch (handedTool)
        {
            case HandedTool.Axe:
                dmg = AxeDamage;
                break;
            case HandedTool.PickAxe:
                dmg = pickAxeDamage;
                break;
        }
        return dmg;
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
                axeTransform.gameObject.SetActive(true);
                handedTool = HandedTool.Axe;
            }
        }
        if (collision.CompareTag("Rock") && !isInTank)
        {
            var msg = collision.transform.parent.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Show();

            if (!isInWater)
            {
                entityInRange = true;
                pickAxeTransform.gameObject.SetActive(true);
                handedTool = HandedTool.PickAxe;
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
        if (collision.CompareTag("Tree"))
        {
            var msg = collision.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Hide();

            entityInRange = false;
            var treeSprite = collision.GetComponent<SpriteRenderer>();
            var color = new Color32(255, 255, 255, 255);
            treeSprite.color = color;
            axeTransform.gameObject.SetActive(false);
            handedTool = HandedTool.None;
        }
        if (collision.CompareTag("Rock"))
        {
            var msg = collision.transform.parent.GetComponentInChildren<LocalMessage>();
            if (msg != null) msg.Hide();

            entityInRange = false;
            pickAxeTransform.gameObject.SetActive(false);
            handedTool = HandedTool.None;
        }
    }
}
