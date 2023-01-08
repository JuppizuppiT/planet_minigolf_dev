using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform player;
    public Transform black_hole;

    private Camera cam;
    private float minHeight = 10f;
    private float maxHeight = 50f;
    private bool focusGoal = true;
    private float zoomValue = 0.65f;

    void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoomValue -= scroll;
        zoomValue = Mathf.Clamp(zoomValue, 0.0f, 1.0f);

        if (Input.GetKeyUp(KeyCode.C))
        {
            focusGoal = !focusGoal;
        }

        if (focusGoal)
        {
            setGoalCam();
        }
        else
        {
            setPlayerCam();
        }
    }

    void setGoalCam()
    {
        Vector3 player_to_goal_vector = black_hole.transform.position - player.transform.position;
        Vector3 new_camera_pos_xy = player.transform.position + player_to_goal_vector * 0.5f;
        float height = (player_to_goal_vector.magnitude / 2) / Mathf.Tan(30 * (Mathf.PI / 180));
        transform.position = new_camera_pos_xy + new Vector3(0, 0, -5);
        height = (height < minHeight) ? minHeight : height;
        cam.orthographicSize = height * (0.4f + (1.0f - 0.4f) * zoomValue);
    }

    void setPlayerCam()
    {
        transform.position = player.transform.position + new Vector3(0, 0, -5);
        cam.orthographicSize = minHeight + (maxHeight - minHeight) * zoomValue;
    }
}