using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public GameObject[] planets;
    public GameObject[] suns;
    public GameObject[] goals;
    GameObject[] celestials;

    public Rigidbody2D rb;

    public float[] lastHit;

    private float gravity_factor = 9000f;
    private float speed_factor = 2000f;

    public float click_down_timestamp = 0.0f;
    public float click_up_timestamp = 0.0f;

    public float max_duration = 2f;
    
    public int hit_counter = 0;

    int charge_cancelled = 0;
    
    public TMPro.TextMeshProUGUI score;

    void Start()
    {
        //score = gameObject.GetComponent(typeof(TMPro.TextMeshProUGUI)) as TMPro.TextMeshProUGUI;
        planets = GameObject.FindGameObjectsWithTag("Planet");
        suns = GameObject.FindGameObjectsWithTag("Sun");
        goals = GameObject.FindGameObjectsWithTag("Goal");
        // combine planets and suns to one array celestials
        celestials = new GameObject[planets.Length + suns.Length + goals.Length];
        planets.CopyTo(celestials, 0);
        suns.CopyTo(celestials, planets.Length);
        goals.CopyTo(celestials, planets.Length + suns.Length);


        // lastHit = new float[planets.Length];
        // for (int i = 0; i < planets.Length; i++)
        // {
        //     lastHit[i] = 0;
        // }
        //collider_player = transform.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
    }
    void Update()
    {
        var move =
            new Vector3(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0);
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
        //UNSTABLE AND UNTESTED BEHAVIOUR
        if(rb.velocity.magnitude > 0.1f)
        {
            CalculateGravity();
        }else{
            rb.velocity = new Vector2(0,0);
        }
        //UNSTABLE AND UNTESTED BEHAVIOUR
    }

    void CalculateGravity()
    {
        Vector3[] forces = new Vector3[celestials.Length];
        for (int i = 0; i < celestials.Length; i++)
        {
            var celestial = celestials[i];
            var direction = celestial.transform.position - transform.position;
            var distance = direction.magnitude;
            CircleCollider2D collider_planet = celestial.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
            float radius = collider_planet.bounds.extents[0];
            Debug.Log(radius, celestial);

            if (celestial.tag == "Planet")
            {
                var force = radius * direction.normalized * gravity_factor * Time.deltaTime/ (distance * distance);
                GetComponent<Rigidbody2D>().AddForce(force);
            }
            if (celestial.tag == "Sun")
            {
                var force = radius * direction.normalized * gravity_factor * Time.deltaTime/ (distance * distance);
                GetComponent<Rigidbody2D>().AddForce(force);
                if (distance < radius + 1){
                    Debug.Log("Game Over");
                    ResetBallAfterGameOver();
                }
            }
            if (celestial.tag == "Goal")
            {
                var force = radius * direction.normalized * gravity_factor * 5 * Time.deltaTime/ (distance * distance);
                GetComponent<Rigidbody2D>().AddForce(force);
                if (distance < radius + 0.6f){
                    Debug.Log("You Win");
                    ResetBallAfterGoal();
                }
            }
        }
    }

    void ResetBallAfterGameOver()
    {
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
    }

    void ResetBallAfterGoal()
    {
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
    }

    void MoveBall(float click_duration)
    {
        float speed;

        if (click_duration < max_duration)
        {
            speed = click_duration * speed_factor;
        }
        else
        {
            speed = max_duration * speed_factor;
        }
        hit_counter++;
        score.text = hit_counter.ToString();

        this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - transform.position;
        targetvec.Normalize();
        var force = targetvec.normalized * speed;
        GetComponent<Rigidbody2D>().AddForce(force);
    }
}
