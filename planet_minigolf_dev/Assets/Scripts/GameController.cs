using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public GameObject GameOverReason;
    private GameObject[] Celestials;
    private GameObject Ball;
    private GameObject AimLine;
    private GameLogic Game;
    private bool Paused;
    private float TimeDeltaRemainder;
    private float? ClickDownTimestamp;
    private float AimCharge;

    void Start()
    {
        AimLine = GameObject.Find("AimLine");
        Ball = GameObject.Find("Ball");

        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        GameObject[] suns = GameObject.FindGameObjectsWithTag("Sun");
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        GameObject[] deadlyFog = GameObject.FindGameObjectsWithTag("DeadlyFog");

        // combine planets and suns to one array celestials
        Celestials = new GameObject[goals.Length + suns.Length + planets.Length + deadlyFog.Length];
        goals.CopyTo(Celestials, 0);
        suns.CopyTo(Celestials, goals.Length);
        planets.CopyTo(Celestials, goals.Length + suns.Length);
        deadlyFog.CopyTo(Celestials, goals.Length + suns.Length + planets.Length);

        GameLogic.ICelestial[] abc = new GameLogic.ICelestial[Celestials.Length];
        for (int i = 0; i < Celestials.Length; i++)
        {
            if (Celestials[i].tag == "Goal")
            {
                abc[i] = new GameLogic.Goal((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                            Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x);
            }
            else if (Celestials[i].tag == "Sun")
            {
                abc[i] = new GameLogic.Sun((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                           Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x);
            }
            else if (Celestials[i].tag == "Planet")
            {
                abc[i] = new GameLogic.Planet((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                              Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x);
            }
            else if (Celestials[i].tag == "DeadlyFog")
            {
                abc[i] = new GameLogic.DeadlyFog((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                                 Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x, 30);
            }
        }
        Game = new GameLogic(abc, Ball.transform.position, Ball.GetComponent<CircleCollider2D>().radius * Ball.transform.localScale.x);
        //Game.TickAdvance(500 * Vector2.left, false, 1);

        Paused = false;

        TimeDeltaRemainder = 0;
        ClickDownTimestamp = null;
        AimCharge = 0;
    }

    void Update()
    {
        if (Game.State != GameLogic.GameState.Ongoing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Paused)
            {
                PausePanel.SetActive(true);
            }
            else
            {
                PausePanel.SetActive(false);
            }

            Paused = !Paused;
        }

        if (Paused)
        {
            return;
        }

        float totalTimeDelta = Time.deltaTime + TimeDeltaRemainder;
        uint ticksDelta = (uint)(totalTimeDelta / 0.001f);
        TimeDeltaRemainder = totalTimeDelta - ticksDelta * 0.001f;
        //Debug.Log(ticksDelta);

        Vector2 shotVelocity = Vector2.zero;

        if (Input.GetMouseButtonDown(0) && Game.Stopped)
        {
            ClickDownTimestamp = Time.time;
        }
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
        {
            ClickDownTimestamp = null;
        }

        if (ClickDownTimestamp != null)
        {
            float maxDuration = 2;
            float clickDuration = Time.time - (float)ClickDownTimestamp;
            AimCharge = System.Math.Min(clickDuration, maxDuration) / maxDuration;
            Debug.Log(AimCharge);
        }
        else
        {
            AimCharge = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (ClickDownTimestamp != null)
            {
                float speed = 1000 * AimCharge;

                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 targetVec = (mousePos - Game.BallPosition).normalized;
                shotVelocity = targetVec * speed;
            }
            ClickDownTimestamp = null;
            AimCharge = 0;
        }

        Game.TickAdvance(shotVelocity, false, ticksDelta);
        Ball.transform.position = Game.BallPosition;
        Ball.transform.rotation = Quaternion.Euler(0, 0, Game.BallRotation * 180 / Mathf.PI);

        UpdateAimLine();

        if (Game.State == GameLogic.GameState.GameOverFail)
        {
            Debug.Log("Game Over Fail!");
            GameOverPanel.SetActive(true);
            GameOverReason.GetComponent<TextMeshProUGUI>().text = "You died!";
        }
        else if (Game.State == GameLogic.GameState.GameOverWin)
        {
            Debug.Log("Game Over Win!");
        }
    }

    void UpdateAimLine()
    {
        LineRenderer lineRenderer = AimLine.GetComponent<LineRenderer>();
        float lineLength = 20;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetVec = (mousePos - Game.BallPosition).normalized;

        Vector2 aimLineOrigin = Game.BallPosition + targetVec * 6;
        Vector2 aimLinePeak;
        if (Game.Stopped)
        {
            if (AimCharge > 0)
            {
                float length = lineLength * AimCharge;

                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                aimLinePeak = aimLineOrigin + targetVec * length;
            }
            else
            {
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;
                aimLinePeak = aimLineOrigin + targetVec * lineLength;
            }
        }
        else
        {
            aimLinePeak = aimLineOrigin;
        }


        DrawAimLine(new Vector2[] { aimLineOrigin, aimLinePeak });
    }

    void DrawAimLine(Vector2[] vertexPositions)
    {
        LineRenderer lineRenderer = AimLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = vertexPositions.Length;
        Vector3[] vertexPositions3 = new Vector3[vertexPositions.Length];
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            vertexPositions3[i] = vertexPositions[i];
        }
        lineRenderer.SetPositions(vertexPositions3);
    }
}
