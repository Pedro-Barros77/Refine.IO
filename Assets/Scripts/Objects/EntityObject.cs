using UnityEngine;
using UnityEngine.UI;
using static Enums;

public class EntityObject : MonoBehaviour
{
    public WorldBuilder.EntityType type;
    public MeleeTargetType MeleeTarget => meleeTarget;
    public float Health = 10;
    [SerializeField] bool hasLocalMessage;
    [SerializeField] AudioClip[] HitClips;
    [SerializeField] Sprite[] Sprites;
    [SerializeField] MeleeTargetType meleeTarget;

    float totalHealth, colorTime = 1;
    bool isAlive = true;
    Slider slider;
    Transform healthBar;
    AudioSource audioSource;
    SpriteRenderer spriteRenderer;
    LocalMessage localMessage;

    private void Start()
    {
        totalHealth = Health;
        healthBar = transform.GetChild(0).transform.GetChild(0);
        slider = healthBar.transform.GetChild(0).GetComponent<Slider>();
        healthBar.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = Sprites[Random.Range(0, Sprites.Length)];

        if (hasLocalMessage)
        {
            localMessage = transform.GetComponentInChildren<LocalMessage>();
        }

        if (type == WorldBuilder.EntityType.Rock)
        {
            gameObject.AddComponent<PolygonCollider2D>();
            var hitRangeCollider = transform.GetChild(1);
            hitRangeCollider.gameObject.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite;
            hitRangeCollider.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
            hitRangeCollider.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    private void Update()
    {
        if (!isAlive)
        {
            //Efeito de transparência (fade out) ao zerar a vida
            spriteRenderer.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), colorTime);
            colorTime -= Time.deltaTime / 2;

            if (colorTime < 0 && !audioSource.isPlaying)
            {
                WorldBuilder.WorldEntities.Remove(this);
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage >= Health)
        {
            Die();
            return;
        }
        else Health -= damage;
        PlayRandomHit();

        healthBar.gameObject.SetActive(true);
        slider.value = (Health * 100 / totalHealth) / 100;
    }

    void PlayRandomHit()
    {
        audioSource.clip = HitClips[Random.Range(0, HitClips.Length - 1)];
        audioSource.Play();
    }

    void Die()
    {
        Health = 0;
        isAlive = false;
        healthBar.gameObject.SetActive(false);
        audioSource.clip = HitClips[HitClips.Length - 1];
        audioSource.Play();
        GetComponent<Collider2D>().enabled = false;

        if (hasLocalMessage) Destroy(localMessage.gameObject);
        

        switch (type)
        {
            case WorldBuilder.EntityType.Tree:
                transform.Find("TreeLog").gameObject.SetActive(false);
                break;
        }
    }
}
