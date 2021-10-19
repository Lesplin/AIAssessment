using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    //stores whether the object is occupied by a character
    private bool isOccupied = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool _isOccupied { get { return isOccupied; } set { isOccupied = value; } }
}
