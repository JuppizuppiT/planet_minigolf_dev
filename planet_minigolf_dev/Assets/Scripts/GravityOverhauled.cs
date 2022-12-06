using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOverhauled : MonoBehaviour
{
    public GameObject[] planets;

    public GameObject[] suns;

    public GameObject[] goals;

    private float gravity_factor = 9000f;

    GameObject[] celestials;
    // Start is called before the first frame update
    void Start()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");
        suns = GameObject.FindGameObjectsWithTag("Sun");
        goals = GameObject.FindGameObjectsWithTag("Goal");

        // combine planets and suns to one array celestials
        celestials = new GameObject[planets.Length + suns.Length + goals.Length];
        planets.CopyTo(celestials, 0);
        suns.CopyTo(celestials, planets.Length);
        goals.CopyTo(celestials, planets.Length + suns.Length);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vec = new Vector2(0, 0);
        CalculateGravity(vec);
    }

    public Vector2 CalculateGravity(Vector2 point)
    {
        float distance_dingsbums = 1.2f;
        bool touched = false;
        Vector2 force_final = new Vector2(0, 0);
        Vector3[] forces = new Vector3[celestials.Length];

        for (int i = 0; i < celestials.Length; i++)
        {
            var celestial = celestials[i];
            var direction = (Vector2)celestial.transform.position - point;
            var distance = direction.magnitude;
            CircleCollider2D collider_planet =
                celestial.GetComponent(typeof (CircleCollider2D)) as
                CircleCollider2D;
            float radius = collider_planet.bounds.extents[0];

            if (celestial.tag == "Planet")
            {
                var force = radius * direction.normalized * gravity_factor * Time.deltaTime / Mathf.Pow(distance, distance_dingsbums);
                forces[i] = force;
            }
            if (celestial.tag == "Sun")
            {
                var force = radius * direction.normalized * gravity_factor * Time.deltaTime / Mathf.Pow(distance, distance_dingsbums);
                forces[i] = force;
            }
            if (celestial.tag == "Goal")
            {
                var force = radius * direction.normalized * gravity_factor * Time.deltaTime / Mathf.Pow(distance, distance_dingsbums);
                forces[i] = force;
            }
            force_final += (Vector2) forces[i];
        }
        return force_final.normalized;
    }
}
