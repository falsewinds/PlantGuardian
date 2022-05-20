using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEngineScript : MonoBehaviour
{
    public SpriteRenderer background;
    public CombatantScript player;
    public GameObject combatantPrefab;
    public Sprite enemySprite;
    public List<Sprite> bulletSprites;

    private Camera m_pMainCamera;
    private Vector3 m_vScalar;
    private Rect m_cMapBorder, m_cCameraBorder, m_cPlayerBorder;
    private int m_iCounter;
    private List<CombatantScript> m_vEnemies;

    void Awake()
    {
        m_iCounter = 1000;
        m_vEnemies = new List<CombatantScript>();

        m_pMainCamera = Camera.main;
        float h2 = m_pMainCamera.orthographicSize,
            w2 = h2 * m_pMainCamera.aspect;
        m_vScalar.x = w2 * 2;
        m_vScalar.y = h2 * 2;
        m_vScalar.z = 1;

        Transform bgTfm = background.gameObject.transform;
        Vector3 bound = background.sprite.bounds.extents;
        m_cMapBorder.x = bgTfm.position.x - bound.x * bgTfm.localScale.x;
        m_cMapBorder.y = bgTfm.position.y - bound.y * bgTfm.localScale.y;
        m_cMapBorder.width = bound.x * bgTfm.localScale.x * 2;
        m_cMapBorder.height = bound.y * bgTfm.localScale.y * 2;

        m_cCameraBorder = m_cMapBorder;
        m_cCameraBorder.x += w2;
        m_cCameraBorder.y += h2;
        m_cCameraBorder.width -= w2 * 2;
        m_cCameraBorder.height -= h2 * 2;

        m_cPlayerBorder = m_cMapBorder;

    }

    void Start()
    {
        try
        {
            using (StreamReader reader = File.OpenText("config.txt"))
            {
                string s = "";
                while ((s=reader.ReadLine()) != null)
                {
                    string[] v = s.Split('=');
                    switch (v[0])
                    {
                    case "MinimumCD":
                        CombatantScript.MinimumCooldown = System.Int32.Parse(v[1]);
                        break;
                    case "MoveSpeed":
                        player.moveSpeed = System.Int32.Parse(v[1]);
                        break;
                    case "WeaponCD":
                        player.weaponCooldown = System.Int32.Parse(v[1]);
                        break;
                    case "WeaponMoveFactor":
                        player.weaponMoveFactor = (float)System.Double.Parse(v[1]);
                        break;
                    case "BulletSpeed":
                        player.weaponSpeed = System.Int32.Parse(v[1]);
                        break;
                    case "BulletRange":
                        player.weaponRange = (float)System.Double.Parse(v[1]);
                        break;
                    case "BulletSize":
                        player.weaponBulletSize = (float)System.Double.Parse(v[1]);
                        break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            Debug.Log("config.txt read error, use default setting.");
        }
    }

    void FixedUpdate()
    {
        // Escape
        if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }

        // Keyboard Input
        Vector3 pos = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.A)) { pos.x -= 1; }
        if (Input.GetKey(KeyCode.D)) { pos.x += 1; }
        if (Input.GetKey(KeyCode.W)) { pos.y += 1; }
        if (Input.GetKey(KeyCode.S)) { pos.y -= 1; }
        pos = player.Move(pos);
        player.transform.position = restrictPosition(pos,m_cPlayerBorder);

        // Move Friend & Enemy

        // check Hit & damage
        List<CombatantScript> death = new List<CombatantScript>();
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach(GameObject b in bullets)
        {
            foreach(CombatantScript cb in m_vEnemies)
            {
                if (cb.IsDead())
                {
                    death.Add(cb);
                    continue;
                }
                if (cb.IsHit(b.transform.position))
                {
                    cb.Hit(1);
                    GameObject.Destroy(b);
                }
            }
        }
        foreach(CombatantScript cb in death)
        {
            m_vEnemies.Remove(cb);
        }
    }

    void Update()
    {
        // Mouse Input
        Vector3 mouse = new Vector3(
            (Input.mousePosition.x / Screen.width - 0.5f) * m_vScalar.x,
            (Input.mousePosition.y / Screen.height - 0.5f) * m_vScalar.y,
            0);
        player.Aim(mouse + m_pMainCamera.transform.position);
        if (Input.GetMouseButtonDown(0)) { player.UseWeapon(true); }
        if (Input.GetMouseButtonUp(0)) { player.UseWeapon(false); }

        // Debug : Add Enemy
        if (Input.GetKeyUp(KeyCode.E))
        {
            Vector2 xy = Random.insideUnitCircle * 5;
            Vector3 epos = player.transform.position;
            epos.x += xy.x;
            epos.y += xy.y;
            CombatantScript c = GenerateCombatant("TestEnemy", "TestEnemy", enemySprite, epos);
            //c.gameObject.tag = "Enemy";
            m_vEnemies.Add(c);
        }
    }

    void LateUpdate()
    {
        Vector3 pos = player.transform.position;
        pos.z = -10;
        m_pMainCamera.transform.position = restrictPosition(pos,m_cCameraBorder);
    }

    /******************************\
     * Custom Function
    \******************************/

    private Vector3 restrictPosition(Vector3 pos, Rect bounded)
    {
        if (pos.x < bounded.xMin) { pos.x = bounded.xMin; }
        if (pos.x > bounded.xMax) { pos.x = bounded.xMax; }
        if (pos.y < bounded.yMin) { pos.y = bounded.yMin; }
        if (pos.y > bounded.yMax) { pos.y = bounded.yMax; }
        return pos;
    }

    public CombatantScript GenerateCombatant(string prefix, string name, Sprite img, Vector3 pos)
    {
        m_iCounter++;
        GameObject o = Instantiate(combatantPrefab) as GameObject;
        o.name = prefix + "#" + m_iCounter;
        o.transform.position = pos;
        SpriteRenderer renderer = o.GetComponent<SpriteRenderer>();
        renderer.sprite = img;
        CombatantScript cbt = o.GetComponent<CombatantScript>();
        cbt.displayName = name;
        cbt.id = m_iCounter;
        cbt.hp = 10;
        o.SetActive(true);
        return cbt;
    }

    public BulletScript CreateBullet(Vector3 pos, int type, float scalar)
    {
        if (type>bulletSprites.Count)
        {
            Debug.LogError("Bullet type is not define!");
        }
        GameObject b = new GameObject("Bullet");
        b.tag = "Bullet";
        b.transform.position = pos;
        b.transform.localScale = new Vector3(scalar,scalar,1.0f);
        SpriteRenderer r = b.AddComponent<SpriteRenderer>() as SpriteRenderer;
        r.sprite = bulletSprites[type];
        Vector3 size = r.sprite.bounds.extents;
        /*Rigidbody2D rb = b.AddComponent<Rigidbody2D>() as Rigidbody2D;
        rb.bodyType = RigidbodyType2D.Kinematic;*/
        CircleCollider2D c = b.AddComponent<CircleCollider2D>() as CircleCollider2D;
        c.radius = Mathf.Max(size.x,size.y);
        return b.AddComponent<BulletScript>() as BulletScript;
    }
}
