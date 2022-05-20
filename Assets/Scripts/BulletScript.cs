using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public Vector3 direction;
    public float speed, range, radius;

    private float m_fCurrentRange;

    void Awake()
    {
        Vector3 size = GetComponent<SpriteRenderer>().sprite.bounds.extents;
        radius = Mathf.Max(size.x,size.y);
        m_fCurrentRange = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_fCurrentRange>=range)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += direction * speed;
        m_fCurrentRange += speed;
    }
}
