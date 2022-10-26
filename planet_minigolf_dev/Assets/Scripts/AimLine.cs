using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLine : MonoBehaviour
{
    public Transform player;
    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void DrawLine(Vector2[] vertexPositions)
    {
        lineRenderer.positionCount = vertexPositions.Length;
        Vector3[] vertexPositions3 = new Vector3[vertexPositions.Length];
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            vertexPositions3[i] = vertexPositions[i];
        }
        lineRenderer.SetPositions (vertexPositions3);
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 playerPos = new Vector2(player.position.x, player.position.y);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetvec = mousePos - playerPos;
        targetvec.Normalize();
        Vector2 target = playerPos + targetvec * 2;
        Debug.Log(targetvec);
        Debug.Log(target);
        Vector2[] positions =
            new Vector2[2]
            {
                playerPos,
                target,
            };
        DrawLine(positions);
    }
}
