using System.Collections;
using UnityEngine;
using System;

public class Dice : MonoBehaviour {

    // Array of dice sides sprites to load from Resources folder
    private Sprite[] diceSides;
    public static event Action<int, int> OnDiceRolled;
    // Reference to sprite renderer to change sprites
    private SpriteRenderer rend;
    public SpriteRenderer rend2;
    public int finalSide = 1;
    public int finalSide2 = 1;
    private BoxCollider diceCollider;
    public BoxCollider diceCollider2;
	// Use this for initialization
	private void Start () {

        // Assign Renderer component
        rend = GetComponent<SpriteRenderer>();
        diceCollider = GetComponent<BoxCollider>();
        // Load dice sides sprites to array from DiceSides subfolder of Resources folder
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");

        if(Client.instance.player.turn && Client.instance.player.currentStatus == PlayerStatus.Player)
        {
            EnableDice();
            SetFinalDice(Client.instance.player.dice1, Client.instance.player.dice2);
        }
        else
        {
            if(!Client.instance.player.turn && Client.instance.player.currentStatus == PlayerStatus.Player)
            {
                ChangeOpacity(Client.instance.player.turn);
            }
            DisableDice();
            SetFinalDice(Client.instance.player.dice1, Client.instance.player.dice2);
            StartRollDice();
        }
	}
	
    // If you left click over the dice then RollTheDice coroutine is started
    private void OnMouseDown()
    {
        StartCoroutine(nameof(RollTheDice));
        DisableDice();
    }

    // Coroutine that rolls the dice
    private IEnumerator RollTheDice()
    {
        // Variable to contain random dice side number.
        // It needs to be assigned. Let it be 0 initially
        int randomDiceSide = 0;
        int randomDiceSide2 = 0;
        // Final side or value that dice reads in the end of coroutine
        // Loop to switch dice sides ramdomly
        // before final side appears. 20 itterations here.
        for (int i = 0; i <= 12; i++)
        {
            // Pick up random value from 0 to 5 (All inclusive)
            randomDiceSide = UnityEngine.Random.Range(0, 6);
            randomDiceSide2 = UnityEngine.Random.Range(0, 6);
            // Set sprite to upper face of dice from array according to random value
            rend.sprite = diceSides[randomDiceSide];
            rend2.sprite = diceSides[randomDiceSide2];
            // Pause before next itteration
            yield return new WaitForSeconds(0.05f);
        }

        // Assigning final side so you can use this value later in your game
        // for player movement for example
        
        rend.sprite = diceSides[finalSide-1];
        rend2.sprite = diceSides[finalSide2-1];
        if(Client.instance.player.turn && Client.instance.player.currentStatus == PlayerStatus.Player)
        {
            OnDiceRolled?.Invoke(finalSide, finalSide2); // Trigger event with final dice results
        }
        
        //DisableDice();
    }

    public void DisableDice()
    {
        diceCollider.enabled = false;
        diceCollider2.enabled = false;
    }

    // Method to re-enable dice, for future use
    public void EnableDice()
    {
        diceCollider.enabled = true; 
        diceCollider2.enabled = true;
    }
    public void SetFinalDice(int final1, int final2)
    {
        finalSide = final1; 
        finalSide2 = final2; 
    }
    public void StartRollDice()
    {
        StartCoroutine(nameof(RollTheDice));
    }
    public void Reset()
    {
        rend.sprite = diceSides[0];
        rend2.sprite = diceSides[0];
    }
    public void ChangeOpacity(bool change)
    {
        Color color = rend.color;
        if(change)
        {
            color.a = 1f;
        }
        else
        {
            color.a = 0.8f;
        }
        rend.color = color;
        rend2.color = color;
    }
}
