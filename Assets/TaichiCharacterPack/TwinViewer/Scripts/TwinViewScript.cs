using UnityEngine;
using System.Collections;

public class TwinViewScript : MonoBehaviour
{
    public Transform target;
    public Transform initTarget;
    public string mode = "rote";
    public float x = 180.0f;
    public float y = 30.0f;

    public float distance = 1;

    public float xSpeed = 500.0f;
    public float ySpeed = 250.0f;
    public float movSpeed = 250.0f;

    public float yMinLimit = -90;
    public float yMaxLimit = 90;

    public float zoomSpeed = 0.5f;
    public float zoomWheelBias = 5;
    public float zoomMin = 0.1f;
    public float zoomMax = 5;

    public Transform curTarget;
    private float xBk;
    private float yBk;
    private float movX;
    private float movY;
    private float wheel;
    private float distanceBk;
    private Vector3 cameraBk;

    private bool isMouseLocked = false;
    public bool isFixTarget = true;
    private Transform localTarget;

    void Start()
    {
        xBk = x;
        yBk = y;
        distanceBk = distance;
        //GameObject.Find("Directional light").GetComponent.<Light>().intensity = 0.5ff;
    }


    void LateUpdate()
    {
        movX = Input.GetAxis("Mouse X");
        movY = Input.GetAxis("Mouse Y");
        wheel = Input.GetAxis("Mouse ScrollWheel");
        //Input.GetButton("Click");
        if (!isMouseLocked && Input.GetMouseButton(0))
        {
            //if ( !isMouseLocked && Input.GetButton("Click")){

            switch (mode)
            {
                case "move": TargetMove(movX, movY); break;
                case "rote": CameraRote(movX, movY); break;
                case "zoom": CameraZoom(movX, movY); break;
            }
        }

        if (Input.GetMouseButton(2))
        {
            TargetMove(movX, movY);

        }

        if (Input.GetMouseButton(1))
        {
            CameraZoom(movX, movY);

        }
        CameraZoom(wheel * zoomWheelBias, 0);
        if (isFixTarget && curTarget)
        {
            localTarget = curTarget;
        }
        else
        {
            localTarget = target;
            localTarget = curTarget;
        }


        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * new Vector3(0, 0, -distance) + localTarget.position;
        //FIXME_VAR_TYPE position= rotation * Vector3(0, 1, -distance);

        //transform.rotation = rotation;
        transform.position = position;

        if (isFixTarget && curTarget)
        {
            localTarget = curTarget;
        }
        else
        {
            localTarget = target;
        }

        // lookPos = localTarget.position;
        // lookPos.x = 0;

        this.transform.LookAt(localTarget.position, Vector3.up);
    }

    public void CameraRote(float _x, float _y)
    {
        x += _x * xSpeed * 0.01f;
        y -= _y * ySpeed * 0.01f;
        y = ClampAngle(y, yMinLimit, yMaxLimit);
    }

    public void CameraZoom(float _x, float _y)
    {
        distance += (-_y * 10) * zoomSpeed * 0.02f;
        distance += (-_x * 10) * zoomSpeed * 0.02f;
        if (distance < zoomMin) distance = zoomMin;
        if (distance > zoomMax) distance = zoomMax;
    }

    public void TargetMove(float _x, float _y)
    {
        if (isFixTarget) return;
        var movX = -_x * movSpeed * 0.055f * Time.deltaTime;
        var movY = -_y * movSpeed * 0.055f * Time.deltaTime;

        Vector3 camMove = new Vector3(movX, movY);
        camMove = GetComponent<Camera>().cameraToWorldMatrix.MultiplyVector(camMove);
        target.Translate(camMove);
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    public void ModeMove()
    {
        mode = "move";
        //print("move");
    }

    public void ModeRote()
    {
        mode = "rote";
        //print("rote");
    }

    public void ModeZoom()
    {
        mode = "zoom";
        //print("zoom");
    }

    public void Reset()
    {
        distance = distanceBk;
        x = xBk;
        y = yBk;
        isFixTarget = true;
        transform.position = new Vector3(0, 0, 0);
        target.transform.position = new Vector3(0, 1, 0);
    }

    public void FixTarget(bool _flag)
    {
        isFixTarget = _flag;
        //transform.position = Vector3 .zero;
        if (curTarget)
        {
            //target.position= curTarget.position;
        }
    }

    public void ModelTarget(Transform _transform)
    {
        curTarget = _transform;
        curTarget = initTarget;
    }

    public void MouseLock(bool _flag)
    {
        if (!_flag && Input.GetMouseButton(0)) return;
        isMouseLocked = _flag;
    }
}