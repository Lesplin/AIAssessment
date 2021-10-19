using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RuleBasedAI : MonoBehaviour
{
    private AIController controller; //stores the controller which holds the need values of the character
    private NavMeshAgent agent; //stores the navmesh agent which allows the character to navigate the navmesh

    //stores the interactible objects in the level
    private GameObject couch;
    private GameObject bed;
    private GameObject toilet;
    private GameObject bath;
    private GameObject fridge;

    //bools that tell the character what they should be doing at any point
    private bool usingToilet = false;
    private bool eating = false;
    private bool bathing = false;
    private bool sleeping = false;
    private bool haveFun = false;

    //bools that tell the character if they are currently using a facility
    private bool arrivedAtToilet = false;
    private bool arrivedAtFridge = false;
    private bool arrivedAtBath = false;
    private bool arrivedAtBed = false;
    private bool arrivedAtCouch = false;

    //shows what the character is currently doing
    public Text actionDescription;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AIController>();
        agent = GetComponent<NavMeshAgent>();

        couch = GameObject.Find("CouchCollider");
        bed = GameObject.Find("BedCollider");
        toilet = GameObject.Find("ToiletCollider");
        bath = GameObject.Find("BathCollider");
        fridge = GameObject.Find("FridgeCollider");
    }

    // Update is called once per frame
    void Update()
    {
        CheckNeedValues();
        PerformActions();
        checkIfCurrentNeedIsOccupied();
    }

    void CheckNeedValues()
    {
        //conditions responsible for deciding if the character is going to the toilet
        if (toilet.GetComponent<ObjectScript>()._isOccupied == false)
        {
            //using the toilet is the most urgent need and should interrupt any other need 
            if (controller._bladder < 30.0f)
            {
                usingToilet = true;
            }
        }

        //conditions responsible for deciding if the character is going to the fridge
        if (fridge.GetComponent<ObjectScript>()._isOccupied == false)
        {
            //using the toilet is more urgent than eating, so if the character is using the toilet, they should not be interrupted
            if (usingToilet == false)
            {
                if (controller._hunger < 30.0f)
                {
                    eating = true;
                }
            }
        }

        //conditions responsible for deciding if the character is going to bed
        //should only sleep if the character does not want to use the toilet or eat since they are more urgent than sleeping
        //using the toilet and eating will interrupt sleeping
        if (usingToilet == false && eating == false)
        {
            if (controller._sleep < 30.0f)
            {
                sleeping = true;
            }
        }

        //conditions responsible for deciding if the character is going to the bath
        if (bath.GetComponent<ObjectScript>()._isOccupied == false)
        {
            //should only bathe if the character is not using the toilet, eating or sleeping since they are more urgent than bathing
            //using the toilet, eating and sleeping will interrupt bathing
            if (usingToilet == false && eating == false && sleeping == false)
            {
                if (controller._clean < 30.0f)
                {
                    bathing = true;
                }
            }
        }

        //the default action is sitting on the couch (having fun)
        //this should only be done if all of the other needs are above a threshold
        if (usingToilet == false && eating == false && bathing == false && sleeping == false)
        {
            haveFun = true;          
        }
    }

    void PerformActions()
    {
        if (usingToilet == true)
        {
            //lets the game know that if a character is at a facility and they need to use the toilet then the facility
            //they were using is no longer occupied
            if (arrivedAtFridge == true)
            {
                fridge.GetComponent<ObjectScript>()._isOccupied = false;
            }
            else if (arrivedAtBath == true)
            {
                bath.GetComponent<ObjectScript>()._isOccupied = false;
            }

            GoToToilet();
        }

        if (bathing == true)
        {
            GoToBath();
        }

        if (eating == true)
        {
            //lets the game know that if a character is at a facility and they need to use the fridge then the facility
            //they were using is no longer occupied
            if (arrivedAtBath == true)
            {
                bath.GetComponent<ObjectScript>()._isOccupied = false;
            }

            GoToFridge();
        }

        if (sleeping == true)
        {
            //lets the game know that if a character is at a facility and they need to use the bed then the facility
            //they were using is no longer occupied
            if (arrivedAtBath == true)
            {
                bath.GetComponent<ObjectScript>()._isOccupied = false;
            }

            GoToBed();
        }

        if (haveFun == true)
        {
            GoToTV();
        }
    }

    void checkIfCurrentNeedIsOccupied()
    {
        //checks if the bath is occupied and if the player wants to bathe
        if (bath.GetComponent<ObjectScript>()._isOccupied == true && bathing == true)
        {
            //if the bath is occupied and the player hasn't arrived at the bath, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtBath == false)
            {
                bathing = false;

                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._bladder, controller._fun, controller._hunger, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._bladder)
                {
                    usingToilet = true;
                }
                else if (lowestValue == controller._fun)
                {
                    haveFun = true;
                }
                else if (lowestValue == controller._hunger)
                {
                    eating = true;
                }
                else if (lowestValue == controller._sleep)
                {
                    sleeping = true;
                }
            }
        }

        //checks if the fridge is occupied and if the player wants to eat food
        if (fridge.GetComponent<ObjectScript>()._isOccupied == true && eating == true)
        {
            //if the fridge is occupied and the player hasn't arrived at the fridge, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtFridge == false)
            {
                eating = false;

                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._bladder, controller._fun, controller._clean, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._bladder)
                {
                    usingToilet = true;
                }
                else if (lowestValue == controller._fun)
                {
                    haveFun = true;
                }
                else if (lowestValue == controller._clean)
                {
                    bathing = true;
                }
                else if (lowestValue == controller._sleep)
                {
                    sleeping = true;
                }
            }
        }

        //checks if the toilet is occupied and if the player wants to use the toilet
        if (toilet.GetComponent<ObjectScript>()._isOccupied == true && usingToilet == true)
        {
            //if the toilet is occupied and the player hasn't arrived at the toilet, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtToilet == false)
            {
                usingToilet = false;

                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._hunger, controller._fun, controller._clean, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._hunger)
                {
                    eating = true;
                }
                else if (lowestValue == controller._fun)
                {
                    haveFun = true;
                }
                else if (lowestValue == controller._clean)
                {
                    bathing = true;
                }
                else if (lowestValue == controller._sleep)
                {
                    sleeping = true;
                }
            }
        }
    }

    void GoToTV()
    {
        //makes the character travel to the couch object's position
        agent.destination = couch.transform.position;

        //increases the fun need value if the character is on the couch
        controller._fun = increaseNeedValues(couch, controller._fun, ref arrivedAtCouch);

        //interrupts sitting on couch if the character needs to do anything else
        if (usingToilet == true || eating == true || sleeping == true || bathing == true)
        {
            haveFun = false;

            return;
        }

        //displays what the character is currently doing on screen
        if (arrivedAtCouch == false)
        {
            actionDescription.text = "GOING TO COUCH";
        }
        else
        {
            actionDescription.text = "WATCHING TV";
        }
    }

    void GoToBed()
    {
        //makes the character travel to the bed object's position
        agent.destination = bed.transform.position;

        //increases the sleep need value if the character is on the bed
        controller._sleep = increaseNeedValues(bed, controller._sleep, ref arrivedAtBed);

        //interrupts sleeping if the character needs to eat or go to the toilet
        if (usingToilet == true || eating == true)
        {
            sleeping = false;

            return;
        }

        //stops using the bed once the character's bar is full
        if (controller._sleep > 99.0f)
        {
            sleeping = false;

            return;
        }

        //displays what the character is currently doing on screen
        if (arrivedAtBed == false)
        {
            actionDescription.text = "GOING TO BED";
        }
        else
        {
            actionDescription.text = "SLEEPING";
        }
    }

    void GoToToilet()
    {
        //makes the character travel to the toilet object's position
        agent.destination = toilet.transform.position;

        //increases the bladder need value if the character is on the toilet
        controller._bladder = increaseNeedValues(toilet, controller._bladder, ref arrivedAtToilet);

        //stops using the toilet and lets the game know it's not occupied once the character's bar is full
        if (controller._bladder > 99.0f)
        {
            usingToilet = false;
            toilet.GetComponent<ObjectScript>()._isOccupied = false;

            return;
        }

        //displays what the character is currently doing on screen
        if (arrivedAtToilet == false)
        {
            actionDescription.text = "GOING TO TOILET";
        }
        else
        {
            actionDescription.text = "TOILETING";
        }
    }

    void GoToBath()
    {
        //makes the character travel to the bath object's position
        agent.destination = bath.transform.position;

        //increases the clean need value if the character is in the bath
        controller._clean = increaseNeedValues(bath, controller._clean, ref arrivedAtBath);

        //interrupts bathing if the character needs to use toilet, eat or sleep
        if (usingToilet == true || eating == true || sleeping == true)
        {
            bathing = false;

            //if the character has been interrupted while they are occupying a facility
            //they tell the game they are not using this facility anymore
            if (bath.GetComponent<ObjectScript>()._isOccupied == true)
            {
                bath.GetComponent<ObjectScript>()._isOccupied = false;
            }

            return;
        }

        //stops using the bath and lets the game know it's not occupied once the character's bar is full
        if (controller._clean > 99.0f)
        {
            bathing = false;
            bath.GetComponent<ObjectScript>()._isOccupied = false;

            return;
        }

        //displays what the character is currently doing on screen
        if (arrivedAtBath == false)
        {
            actionDescription.text = "GOING TO BATH";
        }
        else
        {
            actionDescription.text = "BATHING";
        }
    }

    void GoToFridge()
    {
        //makes the character travel to the fridge object's position
        agent.destination = fridge.transform.position;

        //increases the hunger need value if the character is at the fridge
        controller._hunger = increaseNeedValues(fridge, controller._hunger, ref arrivedAtFridge);

        //interrupts eating if the character needs to use toilet
        if (usingToilet == true)
        {
            eating = false;

            //if the character has been interrupted while they are occupying a facility
            //they tell the game they are not using this facility anymore
            if (fridge.GetComponent<ObjectScript>()._isOccupied == true)
            {
                fridge.GetComponent<ObjectScript>()._isOccupied = false;
            }

            return;
        }

        //stops eating and lets the game know the fridge is not occupied once the character's bar is full
        if (controller._hunger > 99.0f)
        {
            eating = false;
            fridge.GetComponent<ObjectScript>()._isOccupied = false;

            return;
        }

        //displays what the character is currently doing on screen
        if (arrivedAtFridge == false)
        {
            actionDescription.text = "GOING TO FRIDGE";
        }
        else
        {
            actionDescription.text = "EATING";
        }
    }

    //increases a character's need if they are currently occupying a facility
    float increaseNeedValues(GameObject obj, float needValue, ref bool arrivedAtLocation)
    {
        if (obj.GetComponent<BoxCollider>().bounds.Intersects(GetComponent<BoxCollider>().bounds))
        {
            arrivedAtLocation = true;

            //adds a value to the need value
            if (needValue < 99.0f)
            {
                needValue += 20 * Time.deltaTime;
            }
            else
            {
                //keeps the value from going over 100
                needValue = 100.0f;
            }

            //if the character overlaps with the facility object then let the game know that facility is occupied
            obj.GetComponent<ObjectScript>()._isOccupied = true;
        }
        else
        {
            arrivedAtLocation = false;
        }

        return needValue;
    }

    //compares all values to see which is lowest
    float checkLowestValue(float value1, float value2, float value3, float value4)
    {
        float lowestValue = value1;

        if (lowestValue > value2)
        {
            lowestValue = value2;
        }

        if (lowestValue > value3)
        {
            lowestValue = value3;
        }

        if (lowestValue > value4)
        {
            lowestValue = value4;
        }

        return lowestValue;
    }
}