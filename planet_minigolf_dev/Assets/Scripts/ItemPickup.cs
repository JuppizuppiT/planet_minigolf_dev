using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject[] items;

    // Start is called before the first frame update
    void Start()
    {
        items = GameObject.FindGameObjectsWithTag("Item");
    }

    // Update is called once per frame
    void Update()
    {
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

    void OnTouch()
    {
        print("Item touched");
    }
}
}
