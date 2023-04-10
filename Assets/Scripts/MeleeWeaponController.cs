using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class MeleeWeaponController : MonoBehaviour
{
    public MeleeWeaponType ActiveWeapon { get; set; }
    [SerializeField] Sprite axeSprite, pickaxeSprite;

    private SpriteRenderer WeaponSpriteRenderer;
    private Vector3 RightHandCenter;

    // Start is called before the first frame update
    void Start()
    {
        ActiveWeapon = MeleeWeaponType.Fists;
        WeaponSpriteRenderer = GetComponent<SpriteRenderer>();
        RightHandCenter = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// Get's a melee weapon instance of given type.
    /// </summary>
    /// <typeparam name="T">The type of the melee weapon that implements IMeleeWeapon</typeparam>
    /// <returns></returns>
    public void SetWeapon(MeleeWeaponType weaponType)
    {
        ActiveWeapon = weaponType;
        WeaponSpriteRenderer.enabled = true;
        Debug.Log(weaponType.ToString());
        switch (ActiveWeapon)
        {
            case MeleeWeaponType.Fists:
                WeaponSpriteRenderer.sprite = null;
                WeaponSpriteRenderer.enabled = false;
                break;
            case MeleeWeaponType.Axe:
                WeaponSpriteRenderer.sprite = axeSprite;
                break;
            case MeleeWeaponType.Pickaxe:
                WeaponSpriteRenderer.sprite = pickaxeSprite;
                break;
        }
    }

    /// <summary>
    /// The position in the sprite that represents the contact point, which will generate a hitbox.
    /// </summary>
    /// <returns>The vector3 coordinates of the hitpoint.</returns>
    public Vector3 GetHitPoint()
    {
        switch (ActiveWeapon)
        {
            case MeleeWeaponType.Fists:
                return Vector3.zero;

            case MeleeWeaponType.Axe:
                return transform.localPosition + new Vector3(-0.14f, 0.274f, 0f);

            case MeleeWeaponType.Pickaxe:
                return transform.localPosition + new Vector3(-0.026f, 0.396f, 0f);

            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// The damage this melee weapon/tool causes to a specified target. If none, enemy target will be considered.
    /// </summary>
    /// <returns>The damage value to the target.</returns>
    public float GetDamage(MeleeTargetType target = MeleeTargetType.Enemy)
    {
        switch (ActiveWeapon)
        {
            case MeleeWeaponType.Fists:
                return target switch
                {
                    MeleeTargetType.Wood => 0.5f,
                    MeleeTargetType.Enemy => 1,
                    _ => 0
                };

            case MeleeWeaponType.Axe:
                return target switch
                {
                    MeleeTargetType.Wood => 3,
                    MeleeTargetType.Enemy => 2,
                    _ => 0
                };

            case MeleeWeaponType.Pickaxe:
                return target switch
                {
                    MeleeTargetType.Stone => 3,
                    MeleeTargetType.Enemy => 2,
                    MeleeTargetType.EnemyTank => 1,
                    _ => 0
                };

            default:
                return 0f;
        }
    }
}
