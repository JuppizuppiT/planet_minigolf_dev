using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private SceneLoader sceneLoader;

    public GameObject ScriptSlave;

    public uint defaultMoveCount = 5;

    private uint moveCount;

    private int levelNum = 0;

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

        moveCount = defaultMoveCount;
    }

    void Update()
    {
        score.text = moveCount.ToString();
        float gravity_threshold = 0.1f;
        if (rb.velocity.magnitude < gravity_threshold)
        {
            if (moveCount == 0)
            {
                GameOver("You have no more moves left");
            }
            else
            {
                StandingStillHandler();
            }
        }
        else
        {
            CalculateGravity();
        }
    }

    void StandingStillHandler()
    {
        rb.velocity = new Vector2(0, 0);
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
            MoveBall(click_duration);
        }
        if (Input.GetMouseButtonUp(1))
        {
            charge_cancelled = 0;
        }
        if (Input.GetKeyUp("space"))
        {
            GameObject touchedPlanet = null;
            for (int i = 0; i < celestials.Length; i++)
            {
                var celestial = celestials[i];
                var direction = celestial.transform.position - transform.position;
                var distance = direction.magnitude;
                CircleCollider2D collider_planet = celestial.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
                float radius = collider_planet.bounds.extents[0];

                if (celestial.tag == "Planet")
                {
                    if (distance < radius + 1f)
                    {
                        touchedPlanet = celestial;
                        break;
                    }
                }
            }
            if (touchedPlanet)
            {
                PlanetScript planetScript = touchedPlanet.GetComponent(typeof(PlanetScript)) as PlanetScript;
                if (planetScript.infectionStatus > 0)
                {
                    removeMove();
                    planetScript.infectionStatus--;
                    print(planetScript.infectionStatus);
                }
            }
        }
    }

    void CalculateGravity()
    {
        
        float distance_dingsbums = 1.3f;
        GameObject touchedPlanet = null;
        for (int i = 0; i < celestials.Length; i++)
        {
            var celestial = celestials[i];
            var direction = celestial.transform.position - transform.position;
            var distance = direction.magnitude;
            CircleCollider2D collider_planet =
                celestial.GetComponent(typeof (CircleCollider2D)) as
                CircleCollider2D;
            float radius = collider_planet.bounds.extents[0];

            if (celestial.tag == "Planet")
            {
                var force =
                    radius *
                    direction.normalized *
                    gravity_factor *
                    Time.deltaTime /
                    Mathf.Pow(distance, distance_dingsbums);
                GetComponent<Rigidbody2D>().AddForce(force);
                if (distance < radius + 1f)
                {
                    touchedPlanet = celestial;
                }
            }
            if (celestial.tag == "Sun")
            {
                var force =
                    radius *
                    direction.normalized *
                    gravity_factor *
                    Time.deltaTime /
                    Mathf.Pow(distance, distance_dingsbums);
                GetComponent<Rigidbody2D>().AddForce(force);
                if (distance < radius + 1)
                {
                    GameOver("You flew to close to the sun");
                    break;
                }
            }
            if (celestial.tag == "Goal")
            {
                var force =
                    radius *
                    direction.normalized *
                    gravity_factor *
                    10 *
                    Time.deltaTime /
                    Mathf.Pow(distance, distance_dingsbums);
                GetComponent<Rigidbody2D>().AddForce(force);
                if (distance < radius + 0.6f)
                {
                    if (CheckIfPlanetsCleared())
                    {
                        Debug.Log("You Win");
                        ResetBallAfterGoal();
                        break;
                    }
                    else
                    {
                        GameOver("You didn't clear all planets from their Planetary Pneumonia :(");
                        break;
                    }
                }
            }
        }
        if (touchedPlanet != null)
        {
            rb.drag = 0.3f;
        }
        else
        {
            rb.drag = 2f;
        }
    }

    void GameOver(string gameOverReason)
    {
        Debug.Log("Game Over" + gameOverReason);
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
        moveCount = defaultMoveCount;
    }

    

    void ResetBallAfterGoal()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
        sceneLoader = ScriptSlave.GetComponent<SceneLoader>();
        Scene scene = SceneManager.GetActiveScene();
        switch (scene.name)
        {
            case "Tutorial_01":
                sceneLoader.LoadScene("Tutorial_02");
                break;
            case "Tutorial_02":
                sceneLoader.LoadScene("Tutorial_03");
                break;
            default:
                sceneLoader.LoadScene("LevelGoal");
                break;
        }
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
        moveCount--;
        

        this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - transform.position;
        targetvec.Normalize();
        var force = targetvec.normalized * speed;
        GetComponent<Rigidbody2D>().AddForce(force);
    }

    public void addMove()
    {
        moveCount++;
    }

    public void removeMove()
    {
        moveCount--;
    }

    private bool CheckIfPlanetsCleared()
    {
        uint sum = 0;
        for (int i = 0; i < planets.Length; i++)
        {
            PlanetScript planetScript = planets[i].GetComponent(typeof(PlanetScript)) as PlanetScript;
            sum += planetScript.infectionStatus;
            if (sum > 0)
            {
                return false;
            }
        }
        return true;
    }
}
