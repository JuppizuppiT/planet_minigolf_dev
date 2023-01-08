using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject[] items;
    public Movement movement;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        items = GameObject.FindGameObjectsWithTag("Item");

        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var direction = item.transform.position - transform.position;
            var distance = direction.magnitude;
            CircleCollider2D collider_item =
                item.GetComponent(typeof (CircleCollider2D)) as
                CircleCollider2D;
            float radius = collider_item.bounds.extents[0];
            if (distance < radius + 0.1f)
            {
                OnTouch(item);
            }
    }

    void OnTouch(GameObject item)
    {
        var item_type = item.GetComponent<ItemType>();

        // speed
        if (item_type.type == "speed")
        {
            rb.velocity = rb.velocity * 1.5f;
        }
        // slow down
        else if (item_type.type == "slow")
        {
            rb.velocity = rb.velocity * 0.5f;
        }
        // reverse
        else if (item_type.type == "reverse")
        {
            rb.velocity = rb.velocity * -1f;
        }
        // stopper
        else if (item_type.type == "stopper")
        {
            rb.velocity = new Vector2(0, 0);
        }
        // vaccine
        else if (item_type.type == "vaccine")
        {
            movement.AddMove();
        }

        Destroy(item);
        
    }

    }
}
