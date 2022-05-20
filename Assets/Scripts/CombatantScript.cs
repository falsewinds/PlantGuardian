using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantScript : MonoBehaviour
{
    static float timeFactor = 0.001f;
    static BattleEngineScript battleEngine;
    public static int MinimumCooldown = 5;
    public enum WeaponMode
    {
        MODE_CONTINUS,
        MODE_SINGLESHOT
    }

    public int id = 0;
    public string displayName;
    public int moveSpeed = 50;
    public int hp = 100;

    public int weaponCooldown = 15;
    public int weaponSpeed = 100;
    public float weaponRange = 20.0f;
    public float weaponMoveFactor = 0.5f;
    public float weaponBulletSize = 0.1f;
    public WeaponMode weaponMode;
    public Vector3 moveTarget;

    private SpriteRenderer m_pRenderer;
    private BoxCollider2D m_pCollider2D;
    private int m_iCooldown;
    private bool m_bUseWeapon;
    private float m_fMoveSpeed;
    private Vector3 m_vAimDriection, m_vSpriteBounds;
    private Rect m_cBoxCollider;
    private Color m_cRenderColor;

    void Awake()
    {
        m_vAimDriection = Vector3.zero;
        moveTarget = Vector3.zero;
        m_iCooldown = 0;
        m_bUseWeapon = false;
        m_fMoveSpeed = moveSpeed;
        m_pRenderer = GetComponent<SpriteRenderer>();
        /*m_pCollider2D = GetComponent<BoxCollider2D>();
        if (m_pCollider2D==null)
        {
            m_pCollider2D = gameObject.AddComponent<BoxCollider2D>() as BoxCollider2D;
        }*/
    }

    void Start()
    {
        m_cRenderColor = m_pRenderer.color;
        m_vSpriteBounds = m_pRenderer.sprite.bounds.extents;
        Vector3 bound = m_pRenderer.sprite.bounds.extents;
        m_cBoxCollider = new Rect(-bound.x,-bound.y,bound.x*2,bound.y*2);
        /*m_pCollider2D.size = new Vector2(
            bound.x * transform.localScale.x * 2,
            bound.y * transform.localScale.y * 2
        );*/
        if (battleEngine==null)
        {
            GameObject e = GameObject.Find("BattleEngine");
            if (e!=null)
            {
                battleEngine = e.GetComponent<BattleEngineScript>();
            }
        }
    }

    void FixedUpdate()
    {
        if (m_bUseWeapon)
        {
            if (weaponMode==WeaponMode.MODE_CONTINUS)
            {
                m_iCooldown %= weaponCooldown;
            }
            else
            {
                m_bUseWeapon = false;
            }
            if (m_iCooldown==0)
            {
                // CreateBullet with aimTarget
                BulletScript s = battleEngine.CreateBullet(
                    transform.position + m_vAimDriection,
                    0, weaponBulletSize);
                s.direction = m_vAimDriection;
                s.range = weaponRange;
                s.speed = weaponSpeed * timeFactor;
            }
        }
        else if (m_iCooldown>=MinimumCooldown)
        {
            m_fMoveSpeed = moveSpeed;
        }
        m_iCooldown++;

        m_cBoxCollider.x = transform.position.x - m_vSpriteBounds.x;
        m_cBoxCollider.y = transform.position.y - m_vSpriteBounds.y;

        if (hp<=0)
        {
            hp--;
            m_cRenderColor.a -= 0.1f;
            m_pRenderer.color = m_cRenderColor;
            if (hp<-10)
            {
                GameObject.Destroy(gameObject);
            }
        }
        else
        {
            if (m_cRenderColor.g<1) { m_cRenderColor.g += 0.1f; }
            if (m_cRenderColor.b<1) { m_cRenderColor.b += 0.1f; }
            m_pRenderer.color = m_cRenderColor;
        }
    }

    void Update()
    {
        m_pRenderer.flipX = m_vAimDriection.x<0;
    }


    /******************************\
     * Control Function
    \******************************/

    public bool IsHit(Vector3 pos)
    {
        return m_cBoxCollider.Contains(pos);
    }
    public void Hit(int damage)
    {
        hp -= damage;
        m_cRenderColor.g = 0.5f;
        m_cRenderColor.b = 0.5f;
        m_pRenderer.color = m_cRenderColor;
        if (hp<0) { hp = 0; }
    }
    public bool IsDead() { return hp<=0; }

    /*public void MoveToward(Vector3 direction)
    {
        moveTarget = direct.normalized;
    }*/
    public void Aim(Vector3 aim)
    {
        m_vAimDriection = aim - transform.position;
        m_vAimDriection.z = 0.0f;
        m_vAimDriection.Normalize();
    }

    public Vector3 Move(Vector3 direct)
    {
        direct.Normalize();
        direct *= timeFactor * m_fMoveSpeed;
        return transform.position + direct;
    }

    public void UseWeapon(bool onoff)
    {
        if (onoff)
        {
            if (m_bUseWeapon || m_iCooldown<weaponCooldown) { return; }
            m_bUseWeapon = true;
            m_iCooldown = 0;
            m_fMoveSpeed = moveSpeed * weaponMoveFactor;
        }
        else
        {
            m_bUseWeapon = false;
            if (m_iCooldown>=MinimumCooldown)
            {
                m_fMoveSpeed = moveSpeed;
            }
        }
    }

}
