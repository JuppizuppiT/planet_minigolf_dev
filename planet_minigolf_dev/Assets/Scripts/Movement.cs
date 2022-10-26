using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public GameObject[] planets;

    public float speed = 15.0f;

    public float speed_factor = 10.0f;

    public float click_down_timestamp = 0.0f;

    public float max_duration = 5f;

    public PhysicsMaterial2D bounce; //assigned in the editor

    public PhysicsMaterial2D no_bounce; //assigned in the editor

    public float last_hit_time = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");
    }

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

        CalculateGravity();
    }

    void CalculateGravity()
    {
        Vector3[] forces = new Vector3[planets.Length];

        foreach (var planet in planets)
        {
            var direction = planet.transform.position - transform.position;
            var distance = direction.magnitude;
            var force = direction.normalized * 10 / (distance * distance);

            //forces.Append(force);
            GetComponent<Rigidbody2D>().AddForce(force);
        }
        foreach (var force in forces)
        {
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // check if hit was planet
        Debug.Log(other.transform.parent);
        if (other.transform.parent.gameObject.tag == "Planet")
        {
            Debug.Log(Time.time - last_hit_time);
            if (Time.time - last_hit_time < 0.8f)
            {
                ChangeMaterialToNotBounce();
            }
            last_hit_time = Time.time;
        }

    }

    private void OnTriggerEnter(Collider2D other)
    {
        // check if hit was planet
        // Debug.Log(other.transform.parent);
        // if (other.transform.parent.gameObject.tag == "Planet")
        // {
        //     Debug.Log(Time.time - last_hit_time);
        //     if (Time.time - last_hit_time < 0.8f)
        //     {
        //         ChangeMaterialToNotBounce();
        //     }
        //     last_hit_time = Time.time;
        // }
    }

    public void ChangeMaterialToBounce()
    {
        var collider = GetComponent<Collider2D>();
        Debug.Log(collider);
        collider.sharedMaterial = bounce;
        Debug.Log("Bounce");
    }

    public void ChangeMaterialToNotBounce()
    {
        var collider = GetComponent<Collider2D>();
        collider.sharedMaterial = no_bounce;
        Debug.Log("Changed material to no bounce");
    }

    void MoveBall(float click_duration)
    {
        if (click_duration < max_duration)
        {
            speed = click_duration * speed_factor;
        }
        else
        {
            speed = max_duration * speed_factor;
        }

        this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - transform.position;
        var distance = targetvec.magnitude;
        targetvec.Normalize();
        var force = targetvec.normalized * 10 / (distance * distance);
        //forces.Append(force);
        GetComponent<Rigidbody2D>().AddForce(force);
        this.GetComponent<Rigidbody2D>().velocity = targetvec * speed;
        ChangeMaterialToBounce();
    }
}
