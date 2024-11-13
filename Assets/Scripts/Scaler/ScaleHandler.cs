using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleHandler : MonoBehaviour
{
    private bool mouseover;
    private float scaleadder = 1.025f;
    private Vector3 initscale;
    private float initdiffx;
    private float initdiffy;
    [SerializeField] private float maxXscale = 2.0f;
    [SerializeField] private float maxYscale = 2.0f;
    [SerializeField] private float minXscale = 0.5f;
    [SerializeField] private float minYscale = 0.5f;

    private void Start()
    {
        initscale = transform.localScale;
        initdiffx = (initscale.x * scaleadder) - initscale.x;
        initdiffy = (initscale.y * scaleadder) - initscale.y;
    }

    // Update is called once per frame
    



    public void ScaleUpRight()
    {

        if (transform.localScale.x < initscale.x * maxXscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.x + initdiffx) / tempScale.x;
            tempScale.x *= scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x + (initdiffx / 2.0f), transform.position.y, transform.position.z);
        }

    }

    public void ScaleUpLeft()
    {
        if (transform.localScale.x < initscale.x * maxXscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.x + initdiffx) / tempScale.x;
            tempScale.x *= scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x - (initdiffx / 2.0f), transform.position.y, transform.position.z);
        }
    }

    public void ScaleUpTop()
    {
        if (transform.localScale.y < initscale.y * maxYscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.y + initdiffy) / tempScale.y;
            tempScale.y *= scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + (initdiffy / 2.0f), transform.position.z);
        }

    }

    public void ScaleUpBottom()
    {
        if (transform.localScale.y < initscale.y * maxYscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.y + initdiffy) / tempScale.y;
            tempScale.y *= scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - (initdiffy / 2.0f), transform.position.z);
        }
    }

    public void ScaleDownRight()
    {
        if (transform.localScale.x > initscale.x * minXscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.x + initdiffx) / tempScale.x;
            tempScale.x *= 2.0f - scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x - (initdiffx / 2.0f), transform.position.y, transform.position.z);
        }
    }

    public void ScaleDownLeft()
    {
        if (transform.localScale.x > initscale.x * minXscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.x + initdiffx) / tempScale.x;
            tempScale.x *= 2.0f - scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x + (initdiffx / 2.0f), transform.position.y, transform.position.z);
        }
    }

    public void ScaleDownTop()
    {
        if (transform.localScale.y > initscale.y * minYscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.y + initdiffy) / tempScale.y;
            tempScale.y *= 2.0f - scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - (initdiffy / 2.0f), transform.position.z);
        }

    }

    public void ScaleDownBottom()
    {
        if (transform.localScale.y > initscale.y * minYscale)
        {
            Vector3 tempScale = transform.localScale;
            scaleadder = (tempScale.y + initdiffy) / tempScale.y;
            tempScale.y *= 2.0f - scaleadder;
            transform.localScale = tempScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + (initdiffy / 2.0f), transform.position.z);
        }
    }

}