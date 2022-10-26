using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public float speed = 15.0f;
    public float speed_factor = 10.0f;
    public float click_down_timestamp = 0.0f;

    // Update is called once per frame
    void Update()
    {
        var move =
            new Vector3(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0);
        transform.position += move * speed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            click_down_timestamp = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float click_up_timestamp = Time.time;
            float click_duration = click_up_timestamp - click_down_timestamp;

            MoveBall (click_duration);
        }
    }

    void MoveBall(float click_duration)
    {
        float max_duration = 5f;
        if (click_duration < max_duration)
        {
            speed = click_duration * speed_factor;
        }
        else
        {
            speed = max_duration * speed_factor;
        }

        this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        print("Left Mouse button clicked");
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - transform.position;
        Debug.Log (targetvec);
        Debug.Log (mousePos);
        Debug.Log(transform.position);
        targetvec.Normalize();
        this.GetComponent<Rigidbody2D>().velocity = targetvec * speed;
    }
}
