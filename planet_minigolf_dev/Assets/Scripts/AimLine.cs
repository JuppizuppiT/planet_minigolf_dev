using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLine : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D player_rb;
    public LineRenderer lineRenderer;
    public float lineLength = 2f;
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
        playerPos = playerPos + targetvec * 0.6f;
        Vector2 target;
        if (Input.GetMouseButton(0))
        {
            Movement movement_player = player.GetComponent(typeof(Movement)) as Movement;
            float length = (2 * (Time.time - movement_player.click_down_timestamp)/movement_player.max_duration);
            if (length > lineLength)
            {
                length = lineLength;
            }
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            target = playerPos + targetvec * length;
        }  
        else
        {
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            target = playerPos + targetvec * lineLength;
        }
        if (player_rb.velocity.magnitude > 0.1f)
        {
            target = playerPos + player_rb.velocity * 0f;
        }


        Vector2[] positions =
        new Vector2[2]
        {
            playerPos,
            target,
        };
        DrawLine(positions);
         
    }
}
