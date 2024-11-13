using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBeam : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform beamTip;
    [SerializeField] private LayerMask Scalable;
    [SerializeField] private LineRenderer lr;

    [Header("Beam Variables")]
    [SerializeField] private float beamRange;

    private Vector2 beamPoint;

    [Header("Input")]
    public KeyCode beamUpKey = KeyCode.Mouse0;
    public KeyCode beamDownKey = KeyCode.Mouse1;
    public KeyCode swapKey = KeyCode.Q;
    private bool isBeam = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(swapKey) && !isBeam)
        {
            isBeam = true;
        }
        else if (Input.GetKeyDown(swapKey) && isBeam)
        {
            isBeam = false;
        }


        if (Input.GetKey(beamUpKey) && isBeam)
        {
            FireBeam(true);
        }
        else if (Input.GetKey(beamDownKey) && isBeam)
        {
            FireBeam(false);
        }
        else
        {
            lr.enabled = false;
        }
    }

    void FireBeam(bool up)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(beamTip.position, new Vector3(mousePosition.x - beamTip.position.x, mousePosition.y - beamTip.position.y, mousePosition.z - beamTip.position.z).normalized, beamRange, Scalable);
        if (hit.collider != null)
        {
            beamPoint = hit.point;
            if (hit.collider.gameObject.tag == "Scalable" && up)
            {
                ScaleObject(hit);
            }
            else if (hit.collider.gameObject.tag == "Scalable" && !up)
            {
                ScaleObjectDown(hit);
            }
            
           
        }
        else
        {
            beamPoint = mousePosition;

            if (Vector2.Distance(new Vector2 (beamTip.position.x, beamTip.position.y), new Vector2(mousePosition.x, mousePosition.y)) > beamRange)
            {
                Debug.Log(Vector2.Distance(new Vector2(beamTip.position.x, beamTip.position.y), new Vector2(mousePosition.x, mousePosition.y)) );
                Vector3 temppt = new Vector3(mousePosition.x - beamTip.position.x, mousePosition.y - beamTip.position.y);
                float x = Vector3.Distance(Vector3.zero, temppt);
                Vector3 unit = temppt / x;
                Vector3 shortbeam = beamTip.position + unit * beamRange;
                beamPoint = shortbeam;
            }

            
        }
        Debug.DrawLine(beamTip.position, beamPoint, Color.white, 20.0f);
        Vector3[] pathpoints = { beamTip.position, beamPoint};
        lr.enabled = true;
        lr.positionCount = 2;
        lr.SetPositions(pathpoints);
        //lr.SetPosition(1, beamPoint);
    }

    void ScaleObject(RaycastHit2D h)
    {
        GameObject g = h.collider.gameObject;
        float topyval = g.transform.position.y + (0.5f * g.transform.localScale.y) - 0.01f;
        Vector3 topycoord = new Vector3(g.transform.position.x, topyval, g.transform.position.z);
        float bottomyval = g.transform.position.y - (0.5f * g.transform.localScale.y) + 0.01f;
        Vector3 bottomycoord = new Vector3(g.transform.position.x, bottomyval, g.transform.position.z);
        float leftxval = g.transform.position.x - (0.5f * g.transform.localScale.x) + 0.01f;
        Vector3 leftxcoord = new Vector3(leftxval, g.transform.position.y, g.transform.position.z);
        float rightxval = g.transform.position.x + (0.5f * g.transform.localScale.x) - 0.01f;
        Vector3 rightxcoord = new Vector3(rightxval, g.transform.position.y, g.transform.position.z);

        float mindistvert = Mathf.Min(Vector3.Distance(bottomycoord, h.point), Vector3.Distance(topycoord, h.point));
        float mindisthorz = Mathf.Min(Vector3.Distance(leftxcoord, h.point), Vector3.Distance(rightxcoord, h.point));
        
        if (mindistvert == Vector3.Distance(topycoord, h.point) && (leftxval<h.point.x && h.point.x<rightxval)) //top hit
        {
            Debug.Log("Top Hit");
            g.SendMessage("ScaleUpTop");

        } else if (mindistvert == Vector3.Distance(bottomycoord, h.point) && (leftxval < h.point.x && h.point.x < rightxval)) //bottom hit
        {
            Debug.Log("Bottom Hit");
            g.SendMessage("ScaleUpBottom");
        }

        if (mindisthorz == Vector3.Distance(rightxcoord, h.point) && (bottomyval < h.point.y && h.point.y < topyval)) //right hit
        {
            Debug.Log("Right Hit");
            g.SendMessage("ScaleUpRight");
        }
        else if (mindisthorz == Vector3.Distance(leftxcoord, h.point) && (bottomyval < h.point.y && h.point.y < topyval)) //left hit
        {
            Debug.Log("Left Hit");
            g.SendMessage("ScaleUpLeft");
        }

        
    }

    void ScaleObjectDown(RaycastHit2D h)
    {
        GameObject g = h.collider.gameObject;
        float topyval = g.transform.position.y + (0.5f * g.transform.localScale.y) - 0.01f;
        Vector3 topycoord = new Vector3(g.transform.position.x, topyval, g.transform.position.z);
        float bottomyval = g.transform.position.y - (0.5f * g.transform.localScale.y) + 0.01f;
        Vector3 bottomycoord = new Vector3(g.transform.position.x, bottomyval, g.transform.position.z);
        float leftxval = g.transform.position.x - (0.5f * g.transform.localScale.x) + 0.01f;
        Vector3 leftxcoord = new Vector3(leftxval, g.transform.position.y, g.transform.position.z);
        float rightxval = g.transform.position.x + (0.5f * g.transform.localScale.x) - 0.01f;
        Vector3 rightxcoord = new Vector3(rightxval, g.transform.position.y, g.transform.position.z);

        float mindistvert = Mathf.Min(Vector3.Distance(bottomycoord, h.point), Vector3.Distance(topycoord, h.point));
        float mindisthorz = Mathf.Min(Vector3.Distance(leftxcoord, h.point), Vector3.Distance(rightxcoord, h.point));

        if (mindistvert == Vector3.Distance(topycoord, h.point) && (leftxval < h.point.x && h.point.x < rightxval)) //top hit
        {
            Debug.Log("Top Hit");
            g.SendMessage("ScaleDownTop");

        }
        else if (mindistvert == Vector3.Distance(bottomycoord, h.point) && (leftxval < h.point.x && h.point.x < rightxval)) //bottom hit
        {
            Debug.Log("Bottom Hit");
            g.SendMessage("ScaleDownBottom");
        }

        if (mindisthorz == Vector3.Distance(rightxcoord, h.point) && (bottomyval < h.point.y && h.point.y < topyval)) //right hit
        {
            Debug.Log("Right Hit");
            g.SendMessage("ScaleDownRight");
        }
        else if (mindisthorz == Vector3.Distance(leftxcoord, h.point) && (bottomyval < h.point.y && h.point.y < topyval)) //left hit
        {
            Debug.Log("Left Hit");
            g.SendMessage("ScaleDownLeft");
        }
    }
}