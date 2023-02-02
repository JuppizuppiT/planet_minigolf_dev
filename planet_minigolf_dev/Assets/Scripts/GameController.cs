using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using System.Linq;

public class GameController : MonoBehaviour
{
    public GameObject AimDotsParent;
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public GameObject GameOverReason;
    public int TotalMoves;
    private GameObject[] Celestials;
    private GameObject Ball;
    private GameObject RemainingMovesText;
    private GameObject[] AimDots;
    private GameObject AimLine;
    private GameLogic Game;
    private bool Paused;
    private float TimeDeltaRemainder;
    private float? ClickDownTimestamp;
    private float AimCharge;
    private bool HealActive;
    private int HealCounter;
    private int MaxInfectionLevel;

    void Start()
    {
        AimDots = new GameObject[AimDotsParent.transform.childCount];
        for (int i = 0; i < AimDotsParent.transform.childCount; i++)
        {
            AimDots[i] = AimDotsParent.transform.GetChild(i).gameObject;
        }
        AimLine = GameObject.Find("AimLine");

        Ball = GameObject.Find("Ball");
        RemainingMovesText = GameObject.Find("RemainingMovesText");

        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        GameObject[] suns = GameObject.FindGameObjectsWithTag("Sun");
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        GameObject[] deadlyFog = GameObject.FindGameObjectsWithTag("DeadlyFog");
        GameObject[] pills = GameObject.FindGameObjectsWithTag("Pill");

        // Combine planets and suns to one array celestials
        Celestials = new GameObject[goals.Length + suns.Length + planets.Length + deadlyFog.Length + pills.Length];
        goals.CopyTo(Celestials, 0);
        suns.CopyTo(Celestials, goals.Length);
        planets.CopyTo(Celestials, goals.Length + suns.Length);
        deadlyFog.CopyTo(Celestials, goals.Length + suns.Length + planets.Length);
        pills.CopyTo(Celestials, goals.Length + suns.Length + planets.Length + deadlyFog.Length);

        MaxInfectionLevel = 1;

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
                int infectionLevel = Celestials[i].GetComponent<Planet>().InfectionLevel;
                if (infectionLevel > MaxInfectionLevel)
                {
                    MaxInfectionLevel = infectionLevel;
                }
                abc[i] = new GameLogic.Planet((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                              Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x,
                                              infectionLevel);
            }
            else if (Celestials[i].tag == "DeadlyFog")
            {
                abc[i] = new GameLogic.DeadlyFog((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                                 Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x, 30);
            }
            else if (Celestials[i].tag == "Pill")
            {
                abc[i] = new GameLogic.Pill((Vector2)Celestials[i].transform.position + Celestials[i].GetComponent<CircleCollider2D>().offset,
                                            Celestials[i].GetComponent<CircleCollider2D>().radius * Celestials[i].transform.localScale.x);
            }
        }
        Game = new GameLogic(abc, Ball.transform.position, Ball.GetComponent<CircleCollider2D>().radius * Ball.transform.localScale.x, TotalMoves);
        //Game.TickAdvance(500 * Vector2.left, false, 1);

        Paused = false;

        TimeDeltaRemainder = 0;
        ClickDownTimestamp = null;
        AimCharge = 0;

        HealActive = false;
        HealCounter = 0;
    }

    void Update()
    {
        if (Game.State.Result != null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Paused)
            {
                PausePanel.SetActive(true);
                HealActive = false;
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

        if (Input.GetMouseButtonDown(0) && Game.State.Stopped)
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
                FindObjectOfType<AudioManager>().Play("PlayerShoot");
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 targetVec = (mousePos - Game.State.BallPosition).normalized;
                shotVelocity = targetVec * speed;
            }
            ClickDownTimestamp = null;
            AimCharge = 0;
        }

        if (Input.GetKeyDown("space"))
        {
            HealActive = true;
            HealCounter++;
        }
        if (Input.GetKeyUp("space"))
        {
            HealActive = false;
        }

        Game.TickAdvance(shotVelocity, HealActive, HealCounter, ticksDelta);

        Ball.transform.position = Game.State.BallPosition;
        Ball.transform.rotation = Quaternion.Euler(0, 0, Game.State.BallRotation * 180 / Mathf.PI);

        for (int i = 0; i < Celestials.Length; i++)
        {
            float colorValue = 1.0f - ((float)Game.Celestials[i].InfectionLevel / (float)MaxInfectionLevel);
            Celestials[i].GetComponent<SpriteRenderer>().color = new Color(1.0f, colorValue, colorValue);

            Celestials[i].SetActive(Game.State.CelestialVisible[i]);
        }

        UpdateAimDots();
        RemainingMovesText.GetComponent<TextMeshProUGUI>().text = Game.State.RemainingMoves.ToString();

        if (Game.State.Result == GameLogic.GameResult.GameOverFail)
        {
            Debug.Log("Game Over Fail!");
            FindObjectOfType<AudioManager>().Play("PlayerDeath");
            GameOverPanel.SetActive(true);
            GameOverReason.GetComponent<TextMeshProUGUI>().text = Game.State.GameOverMsg;
        }
        else if (Game.State.Result == GameLogic.GameResult.GameOverWin)
        {
            ResetBallAfterGoal();
            Debug.Log("Game Over Win!");
        }
    }

    void ResetBallAfterGoal()
    {
        Scene scene = SceneManager.GetActiveScene();
        switch (scene.name)
        {
            case "Tutorial_01":
                FindObjectOfType<AudioManager>().Play("BlackTravel");
                SceneManager.LoadScene("Tutorial_02");
                break;
            case "Tutorial_02":
                FindObjectOfType<AudioManager>().Play("BlackTravel");
                SceneManager.LoadScene("Tutorial_03");
                break;
            case "Tutorial_03":
                FindObjectOfType<AudioManager>().Play("BlackTravel");
                SceneManager.LoadScene("LevelA");
                break;
            default:
                FindObjectOfType<AudioManager>().Play("BlackTravel");
                SceneManager.LoadScene("LevelGoal");
                break;
        }
    }

    void UpdateAimDots()
    {
        if (AimCharge > 0)
        {
            GameLogic.GameState stateBackup = Game.State;
            bool[] celestialVisibleBackup = new bool[Game.State.CelestialVisible.Length];
            Game.State.CelestialVisible.CopyTo(celestialVisibleBackup, 0);

            AimDotsParent.SetActive(true);

            float speed = 1000 * AimCharge;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 targetVec = (mousePos - Game.State.BallPosition).normalized;
            Vector2 shotVelocity = targetVec * speed;

            Game.TickAdvance(shotVelocity, false, 0);

            uint ticksPerDot = 1 * (uint)GameLogic.TickHz / (uint)AimDots.Length;
            foreach (GameObject aimDot in AimDots)
            {
                Game.TickAdvance(Vector2.zero, false, 0, ticksPerDot);
                aimDot.transform.position = Game.State.BallPosition;
            }

            Game.State = stateBackup;
            Game.State.CelestialVisible = celestialVisibleBackup;
        }
        else
        {
            AimDotsParent.SetActive(false);
        }
    }
}
