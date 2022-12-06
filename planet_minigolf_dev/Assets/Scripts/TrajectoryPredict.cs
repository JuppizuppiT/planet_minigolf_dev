using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredict : MonoBehaviour
{
    private int a = 100;
    private int b = 100;
    private LineRenderer[] lineRenderers;
    public LineRenderer lineRenderer;
    public GameObject ScriptSlave;
    public GameObject me;
    protected GravityOverhauled grav;
    private bool shitHasBeenDrawn = false;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderers = new LineRenderer[a*b];
        for (int i = 0; i < a*b; i++)
        {
            GameObject gObject = new GameObject("MyGameObject");
            lineRenderers[i] = gObject.AddComponent<LineRenderer>();
            lineRenderers[i].SetWidth(0.1f, 0f);
            lineRenderers[i].SetColors(Color.white, Color.white);
            Material whiteDiffuseMat = new Material(Shader.Find("Unlit/Texture"));
            lineRenderers[i].material = whiteDiffuseMat;
        }
        lineRenderer = GetComponent<LineRenderer>();
        grav = ScriptSlave.GetComponent<GravityOverhauled>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!shitHasBeenDrawn)
        {
            drawTheShit();
            shitHasBeenDrawn = true;
        }
    }


    void drawTheShit()
    {
        Vector2[,] all_points = new Vector2[a*b, 2];
        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                Vector2 vec_dir;
                Vector2 vecnew;
                Vector2 vec_calc = new Vector2(i - 50, j - 50);
                vec_dir = grav.CalculateGravity(vec_calc); 
                vecnew = vec_dir + vec_calc;
                all_points[i * a + j, 0] = vec_calc;
                all_points[i * a + j, 1] = vecnew;
            }
        }
        for (int i = 0; i < a * b; i++)
        {
            Vector2[] positions = {all_points[i, 0], all_points[i, 1]};

            DrawLine(lineRenderers[i], positions);
        }
    }



    void DrawLine(LineRenderer lr, Vector2[] vertexPositions)
    {
        Debug.Log(lr);
        lr.positionCount = vertexPositions.Length;
        Vector3[] vertexPositions3 = new Vector3[vertexPositions.Length];
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            vertexPositions3[i] = vertexPositions[i];
        }
        lr.SetPositions (vertexPositions3);
    }
}
