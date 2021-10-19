using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIUI : MonoBehaviour
{
    private AIController controller;

    //UI elements to show player values
    public Text needValues;
    public Slider hungerSlider;
    public Slider bladderSlider;
    public Slider cleanSlider;
    public Slider sleepSlider;
    public Slider funSlider;

    // Start is called before the first frame update
    void Start()
    {
        //gets the AI's controller which holds all need values of the character
        controller = GetComponent<AIController>();
    }

    // Update is called once per frame
    void Update()
    {
        //displays need value names 
        needValues.text = "HUNGER "  + "\n" + "BLADDER "  + "\n" + "CLEAN " + "\n" + "SLEEP " + "\n" + "FUN ";

        //displays the bars showing how full a need is
        hungerSlider.value = controller._hunger / 100.0f;
        bladderSlider.value = controller._bladder / 100.0f;
        cleanSlider.value = controller._clean / 100.0f;
        sleepSlider.value = controller._sleep / 100.0f;
        funSlider.value = controller._fun / 100.0f;
    }
}
