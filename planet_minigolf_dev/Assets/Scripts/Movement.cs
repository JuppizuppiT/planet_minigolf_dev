using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public GameObject[] planets;
    public float[] lastHit;

    public float speed = 15.0f;

    public float speed_factor = 10.0f;

    public float click_down_timestamp = 0.0f;
    public float click_up_timestamp = 0.0f;

    public float max_duration = 5f;
    
    public int hit_counter = 0;

    int charge_cancelled = 0;
    
    public TMPro.TextMeshProUGUI score;

    void Start()
    {
        score = gameObject.GetComponent(typeof(TMPro.TextMeshProUGUI)) as TMPro.TextMeshProUGUI;
        planets = GameObject.FindGameObjectsWithTag("Planet");
        lastHit = new float[planets.Length];
        for (int i = 0; i < planets.Length; i++)
        {
            lastHit[i] = 0;
        }
        //collider_player = transform.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
    }
    void Update()
    {
        var move =
            new Vector3(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0);
        transform.position += move * speed * Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && charge_cancelled == 0)
        {
            click_down_timestamp = Time.time;
        }
        if (Input.GetMouseButtonDown(1))
        {
            charge_cancelled = 1;
        }
        if (Input.GetMouseButtonUp(0) && charge_cancelled == 0)
        {
            click_up_timestamp = Time.time;
            float click_duration = click_up_timestamp - click_down_timestamp;

            MoveBall (click_duration);
        }
        if (Input.GetMouseButtonUp(1))
        {
            charge_cancelled = 0;
        }
        
        CalculateGravity();
    }

    void CalculateGravity()
    {
        Vector3[] forces = new Vector3[planets.Length];
        for (int i = 0; i < planets.Length; i++)
        {
            var planet = planets[i];
            var direction = planet.transform.position - transform.position;
            var distance = direction.magnitude;
            CircleCollider2D collider_planet = planet.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
            float radius = collider_planet.bounds.extents[0];
            if (planet.tag == "Planet")
            {
                var force = radius * direction.normalized * 2500 * Time.deltaTime/ (distance * distance);
                GetComponent<Rigidbody2D>().AddForce(force);
            }
        }
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
        hit_counter++;
        // int to string
        score.text = hit_counter.ToString();
        this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - transform.position;
        var distance = targetvec.magnitude;
        targetvec.Normalize();
        var force = targetvec.normalized * 10 / (distance * distance);
        //forces.Append(force);
        GetComponent<Rigidbody2D>().AddForce(force);
        this.GetComponent<Rigidbody2D>().velocity = targetvec * speed;
        //ChangeMaterialToBounce();
    }
}
