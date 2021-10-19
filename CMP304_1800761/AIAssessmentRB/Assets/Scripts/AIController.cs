using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private float hunger = 40.0f;
    private float bladder = 40.0f;
    private float clean = 100.0f;
    private float sleep = 100.0f;
    private float fun = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //decreases each need over time
        hunger = decreaseNeedValues(hunger, 2.0f * 1.0f);
        bladder = decreaseNeedValues(bladder, 2.0f * 1.0f);
        clean = decreaseNeedValues(clean, 1.5f * 1.0f);
        sleep = decreaseNeedValues(sleep, 1.0f * 1.0f);
        fun = decreaseNeedValues(fun, 2.0f * 1.0f);
    }

    //slowly decreases the need passed into function
    float decreaseNeedValues(float value, float decreaseSpeed)
    {
        if (value > 0.0f)
        {
            value -= decreaseSpeed * Time.deltaTime;
        }
        else
        {
            value = 0.0f;
        }

        return value;
    }

    public float _hunger { get { return hunger; } set { hunger = value; } }
    public float _bladder { get { return bladder; } set { bladder = value; } }
    public float _clean { get { return clean; } set { clean = value; } }
    public float _sleep { get { return sleep; } set { sleep = value; } }
    public float _fun { get { return fun; } set { fun = value; } }

}
