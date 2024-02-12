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

    private BoxCollider diceCollider;
    public BoxCollider diceCollider2;
	// Use this for initialization
	private void Start () {

        // Assign Renderer component
        rend = GetComponent<SpriteRenderer>();
        diceCollider = GetComponent<BoxCollider>();
        // Load dice sides sprites to array from DiceSides subfolder of Resources folder
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
	}
	
    // If you left click over the dice then RollTheDice coroutine is started
    private void OnMouseDown()
    {
        StartCoroutine("RollTheDice");
    }

    // Coroutine that rolls the dice
    private IEnumerator RollTheDice()
    {
        // Variable to contain random dice side number.
        // It needs to be assigned. Let it be 0 initially
        int randomDiceSide = 0;
        int randomDiceSide2 = 0;
        // Final side or value that dice reads in the end of coroutine
        int finalSide = 0;
        int finalSide2 = 0;
        // Loop to switch dice sides ramdomly
        // before final side appears. 20 itterations here.
        for (int i = 0; i <= 20; i++)
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
        finalSide = randomDiceSide + 1;
        finalSide2 = randomDiceSide2 +1;
        
        OnDiceRolled?.Invoke(finalSide, finalSide2); // Trigger event with final dice results
        //DisableDice();
    }

    public void DisableDice()
    {
        diceCollider.enabled = false; // Disable collider to prevent interaction
        diceCollider2.enabled = false; // Disable collider to prevent interaction
    }

    // Method to re-enable dice, for future use
    public void EnableDice()
    {
        diceCollider.enabled = true; // Re-enable collider to allow interaction
        diceCollider2.enabled = true; // Disable collider to prevent interaction
    }
}
