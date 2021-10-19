using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;


public class FuzzyStateMachine : AIStateMachine
{
    private AIController controller; //stores the controller which holds the need values of the character
    private NavMeshAgent agent; //stores the navmesh agent which allows the character to navigate the navmesh

    //stores the interactible objects in the level
    private GameObject couch;
    private GameObject bed;
    private GameObject toilet;
    private GameObject bath;
    private GameObject fridge;

    //bools that tell the character if they are currently using a facility
    private bool arrivedAtToilet = false;
    private bool arrivedAtFridge = false;
    private bool arrivedAtBath = false;
    private bool arrivedAtBed = false;
    private bool arrivedAtCouch = false;

    //shows what the character is currently doing
    public Text actionDescription;
    public Text fuzzyResult;

    IFuzzyEngine engine;
    LinguisticVariable fun;
    LinguisticVariable sleep;
    LinguisticVariable bladder;
    LinguisticVariable clean;
    LinguisticVariable hunger;
    LinguisticVariable actOnNeed;

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

        //set membership functions for each variable
        sleep = new LinguisticVariable("sleep");
        var exhausted = sleep.MembershipFunctions.AddTrapezoid("exhausted", 0, 0, 50, 60);
        var rested = sleep.MembershipFunctions.AddTrapezoid("rested", 50, 60, 100, 100);

        clean = new LinguisticVariable("clean");
        var unclean = clean.MembershipFunctions.AddTrapezoid("unclean", 0, 0, 40, 50);
        var cleaned = clean.MembershipFunctions.AddTrapezoid("cleaned", 40, 60, 100, 100);

        hunger = new LinguisticVariable("hunger");
        var hungry = hunger.MembershipFunctions.AddTrapezoid("hungry", 0, 0, 35, 40);
        var full = hunger.MembershipFunctions.AddTrapezoid("full", 35, 60, 100, 100);

        bladder = new LinguisticVariable("bladder");
        var uncomfortable = bladder.MembershipFunctions.AddTrapezoid("uncomfortable", 0, 0, 20, 25);
        var empty = bladder.MembershipFunctions.AddTrapezoid("empty", 20, 50, 100, 100);

        actOnNeed = new LinguisticVariable("actOnNeed");
        var fl_goToilet = actOnNeed.MembershipFunctions.AddTriangle("fl_goToilet", 0, 5, 10);
        var fl_haveFun = actOnNeed.MembershipFunctions.AddTriangle("fl_haveFun", 10, 40, 70);
        var fl_bathe = actOnNeed.MembershipFunctions.AddTriangle("fl_bathe", 70, 75, 80);
        var fl_sleep = actOnNeed.MembershipFunctions.AddTriangle("fl_sleep", 80, 85, 90);
        var fl_eat = actOnNeed.MembershipFunctions.AddTriangle("fl_eat", 90, 95, 100);

        engine = new FuzzyEngineFactory().Default();

        //fuzzy rules
        var rule1 = Rule.If(bladder.Is(empty).And(sleep.Is(rested)).And(hunger.Is(full)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_haveFun));
        var rule2 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(exhausted)).And(hunger.Is(hungry)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_goToilet));

        var rule3 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(rested)).And(hunger.Is(full)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_goToilet));
        var rule4 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(exhausted)).And(hunger.Is(full)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_goToilet));
        var rule5 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(exhausted)).And(hunger.Is(hungry)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_goToilet));
        var rule6 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(rested)).And(hunger.Is(hungry)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_goToilet));
        var rule7 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(rested)).And(hunger.Is(hungry)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_goToilet));
        var rule8 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(exhausted)).And(hunger.Is(full)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_goToilet));
        var rule9 = Rule.If(bladder.Is(uncomfortable).And(sleep.Is(rested)).And(hunger.Is(full)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_goToilet));

        var rule10 = Rule.If(bladder.Is(empty).And(sleep.Is(rested)).And(hunger.Is(full)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_bathe));
        var rule11 = Rule.If(bladder.Is(empty).And(sleep.Is(exhausted)).And(hunger.Is(full)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_sleep));
        var rule12 = Rule.If(bladder.Is(empty).And(sleep.Is(exhausted)).And(hunger.Is(hungry)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_eat));
        var rule13 = Rule.If(bladder.Is(empty).And(sleep.Is(rested)).And(hunger.Is(hungry)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_eat));
        var rule14 = Rule.If(bladder.Is(empty).And(sleep.Is(rested)).And(hunger.Is(hungry)).And(clean.Is(unclean))).Then(actOnNeed.Is(fl_eat));
        var rule15 = Rule.If(bladder.Is(empty).And(sleep.Is(exhausted)).And(hunger.Is(full)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_sleep));
        var rule16 = Rule.If(bladder.Is(empty).And(sleep.Is(exhausted)).And(hunger.Is(hungry)).And(clean.Is(cleaned))).Then(actOnNeed.Is(fl_eat));

        //add rules to fuzzy engine
        engine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10, rule11, rule12, rule13, rule14, rule15, rule16);
    }

    // Update is called once per frame
    void Update()
    {
        //pass need values into defuzzifier
        double result = engine.Defuzzify(new { sleep = (double)controller._sleep, bladder = (double)controller._bladder,
        clean = (double)controller._clean, hunger = (double)controller._hunger });

        //display crisp value on screen
        fuzzyResult.text = result.ToString();

        switch (AIState)
        {
            case State.Toilet:
                GoToToilet(result);
                break;
            case State.Eat:
                GoToFridge(result);
                break;
            case State.Bathe:
                GoToBath(result);
                break;
            case State.Sleep:
                GoToBed(result);
                break;
            case State.WatchTV:
                GoToTV(result);
                break;
            default:
                break;
        }
    }

    void GoToTV(double fuzzyResult)
    {
        //makes the character travel to the couch object's position
        agent.destination = couch.transform.position;

        //increases the fun need value if the character is on the couch
        controller._fun = increaseNeedValues(couch, controller._fun, ref arrivedAtCouch);

        //conditions responsible for deciding if the character is going to the toilet
         if (toilet.GetComponent<ObjectScript>()._isOccupied == false)
         {
            //using the toilet is the most urgent need and should interrupt any other need 
            if (fuzzyResult >= 0 && fuzzyResult <= 10)
            {
                ChangeState(State.Toilet);
            }
        }

         //conditions responsible for deciding if the character is going to the fridge
         if (fridge.GetComponent<ObjectScript>()._isOccupied == false)
        {
            if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                ChangeState(State.Eat);
            }
        }

        //conditions responsible for deciding if the character is going to bed
        if (fuzzyResult > 80 && fuzzyResult <= 90)
        {
            ChangeState(State.Sleep);
        }

        //conditions responsible for deciding if the character is going to the bath
        if (bath.GetComponent<ObjectScript>()._isOccupied == false)
        {
            if (fuzzyResult > 70 && fuzzyResult <= 80)
            {
                ChangeState(State.Bathe);
            }
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

    void GoToBed(double fuzzyResult)
    {
        //makes the character travel to the bed object's position
        agent.destination = bed.transform.position;

        //increases the sleep need value if the character is on the bed
        controller._sleep = increaseNeedValues(bed, controller._sleep, ref arrivedAtBed);

        //conditions responsible for deciding if the character is going to the toilet
        if (toilet.GetComponent<ObjectScript>()._isOccupied == false)
        {
            //using the toilet is the most urgent need and should interrupt any other need 
            if (fuzzyResult >= 0 && fuzzyResult <= 10)
            {
                ChangeState(State.Toilet);
            }
        }

        //conditions responsible for deciding if the character is going to the fridge
        if (fridge.GetComponent<ObjectScript>()._isOccupied == false)
        {
            if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                ChangeState(State.Eat);
            }
        }
        

        //stops using the bed once the character's bar is full
        if (controller._sleep > 99.0f)
        {
            if(fuzzyResult >= 0 && fuzzyResult <= 10)
            {
               ChangeState(State.Toilet);
            }
            else if (fuzzyResult > 10 && fuzzyResult <= 70)
            {
               ChangeState(State.WatchTV);
            }
            else if (fuzzyResult > 70 && fuzzyResult <= 80)
            {
                ChangeState(State.Bathe);
            }
            else if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                ChangeState(State.Eat);
            }
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

    void GoToToilet(double fuzzyResult)
    {
        //makes the character travel to the toilet object's position
        agent.destination = toilet.transform.position;

        //increases the bladder need value if the character is on the toilet
        controller._bladder = increaseNeedValues(toilet, controller._bladder, ref arrivedAtToilet);

        //checks if the toilet is occupied and if the player wants to use the toilet
        if (toilet.GetComponent<ObjectScript>()._isOccupied == true)
        {
            //if the toilet is occupied and the player hasn't arrived at the toilet, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtToilet == false)
            {
                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._hunger, controller._fun, controller._clean, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._clean)
                {
                    ChangeState(State.Bathe);
                }
                else if (lowestValue == controller._fun)
                {
                    ChangeState(State.WatchTV);
                }
                else if (lowestValue == controller._hunger)
                {
                    ChangeState(State.Eat);
                }
                else if (lowestValue == controller._sleep)
                {
                    ChangeState(State.Sleep);
                }
            }
        }

        //stops using the toilet and lets the game know it's not occupied once the character's bar is full
        if (controller._bladder > 99.0f)
        {
            toilet.GetComponent<ObjectScript>()._isOccupied = false;

            if (fuzzyResult > 10 && fuzzyResult <= 70)
            {
                ChangeState(State.WatchTV);
            }
            else if (fuzzyResult > 70 && fuzzyResult <= 80)
            {
                ChangeState(State.Bathe);
            }
            else if (fuzzyResult > 80 && fuzzyResult <= 90)
            {
                ChangeState(State.Sleep);
            }
            else if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                ChangeState(State.Eat);
            }
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

    void GoToBath(double fuzzyResult)
    {
        //makes the character travel to the bath object's position
        agent.destination = bath.transform.position;

        //increases the clean need value if the character is in the bath
        controller._clean = increaseNeedValues(bath, controller._clean, ref arrivedAtBath);

        //checks if the bath is occupied and if the player wants to bathe
        if (bath.GetComponent<ObjectScript>()._isOccupied == true)
        {
            //if the bath is occupied and the player hasn't arrived at the bath, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtBath == false)
            {
                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._bladder, controller._fun, controller._hunger, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._bladder)
                {
                    ChangeState(State.Toilet);
                }
                else if (lowestValue == controller._fun)
                {
                    ChangeState(State.WatchTV);
                }
                else if (lowestValue == controller._hunger)
                {
                    ChangeState(State.Eat);
                }
                else if (lowestValue == controller._sleep)
                {
                    ChangeState(State.Sleep);
                }
            }
        }

            //conditions responsible for deciding if the character is going to the toilet
            if (toilet.GetComponent<ObjectScript>()._isOccupied == false)
         {
             //using the toilet is the most urgent need and should interrupt any other need 
            if (fuzzyResult >= 0 && fuzzyResult <= 20)
            {
                if (bath.GetComponent<ObjectScript>()._isOccupied == true)
                {
                    bath.GetComponent<ObjectScript>()._isOccupied = false;
                }

                ChangeState(State.Toilet);
            }
         }

         //conditions responsible for deciding if the character is going to the fridge
         if (fridge.GetComponent<ObjectScript>()._isOccupied == false)
         {
            if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                if (bath.GetComponent<ObjectScript>()._isOccupied == true)
                {
                    bath.GetComponent<ObjectScript>()._isOccupied = false;
                }

                ChangeState(State.Eat);
            }
        }

        //conditions responsible for deciding if the character is going to bed
        if (fuzzyResult > 80 && fuzzyResult <= 90)
        {
            if (bath.GetComponent<ObjectScript>()._isOccupied == true)
            {
                bath.GetComponent<ObjectScript>()._isOccupied = false;
            }

            ChangeState(State.Sleep);
        }
    

        //stops using the bath and lets the game know it's not occupied once the character's bar is full
        if (controller._clean > 99.0f)
        {
            bath.GetComponent<ObjectScript>()._isOccupied = false;

            if (fuzzyResult >= 0 && fuzzyResult <= 10)
            {
                ChangeState(State.Toilet);
            }
            else if (fuzzyResult > 10 && fuzzyResult <= 70)
            {
                ChangeState(State.WatchTV);
            }
            else if (fuzzyResult > 80 && fuzzyResult <= 90)
            {
                ChangeState(State.Sleep);
            }
            else if (fuzzyResult > 90 && fuzzyResult <= 100)
            {
                ChangeState(State.Eat);
            }
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

    void GoToFridge(double fuzzyResult)
    {
        //makes the character travel to the fridge object's position
        agent.destination = fridge.transform.position;

        //increases the hunger need value if the character is at the fridge
        controller._hunger = increaseNeedValues(fridge, controller._hunger, ref arrivedAtFridge);

        //checks if the fridge is occupied and if the player wants to eat food
        if (fridge.GetComponent<ObjectScript>()._isOccupied == true)
        {
            //if the fridge is occupied and the player hasn't arrived at the fridge, then they are not the
            //one currently occupying it and need to find somewhere else to go until it is unoccupied
            if (arrivedAtFridge == false)
            {
                //finds the lowest value out of all the values except for the need that has its facility occupied
                float lowestValue = checkLowestValue(controller._bladder, controller._fun, controller._clean, controller._sleep);

                //attends to the need that is lowest
                if (lowestValue == controller._bladder)
                {
                    ChangeState(State.Toilet);
                }
                else if (lowestValue == controller._fun)
                {
                    ChangeState(State.WatchTV);
                }
                else if (lowestValue == controller._clean)
                {
                    ChangeState(State.Bathe);
                }
                else if (lowestValue == controller._sleep)
                {
                    ChangeState(State.Sleep);
                }
            }
        }
        
        //conditions responsible for deciding if the character is going to the toilet
        if (toilet.GetComponent<ObjectScript>()._isOccupied == false)
        {
            if (fuzzyResult >= 0 && fuzzyResult <= 10)
            {
                if (fridge.GetComponent<ObjectScript>()._isOccupied == true)
                {
                    fridge.GetComponent<ObjectScript>()._isOccupied = false;
                }

                ChangeState(State.Toilet);
            }
        }

        //stops eating and lets the game know the fridge is not occupied once the character's bar is full
        if (controller._hunger > 99.0f)
        {
            fridge.GetComponent<ObjectScript>()._isOccupied = false;

            if (fuzzyResult >= 0 && fuzzyResult <= 10)
            {
                ChangeState(State.Toilet);
            }
            else if (fuzzyResult > 10 && fuzzyResult <= 70)
            {
                ChangeState(State.WatchTV);
            }
            else if (fuzzyResult > 70 && fuzzyResult <= 80)
            {
                ChangeState(State.Bathe);
            }
            else if (fuzzyResult > 80 && fuzzyResult <= 90)
            {
                ChangeState(State.Sleep);
            }
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
