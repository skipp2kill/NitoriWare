﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RemiCover_RemiBehaviour : MonoBehaviour {


    [SerializeField]
    private float HP = 1;                           // Remilia's Health Points.
    public float burnSpeed;                         // How much will HP decrease when Remilia's collider is exposed to sunlight?




    public float walkingSpeed;                      // Speed of Remilia's movement (Walking)
    public float runningSpeed;                      // Speed of Remilia's movement (Running)
    private float currentSpeed;                     // CurrentSpeed (Only useful for Running movement)
    public float accelerationFactor;                // How much will CurrentSpeed increase until reaching runningSpeed?


    public float leftLimit, rightLimit;             // Minimum and Maximum value of Remilia's X position that she can take

    // Probabilities for choosing, randomly, different movements for Remilia (Walking, Standing and Running)
    public int walkProbability;                     // Must be between 0 and 100.
    public int standProbability;                    // Must be between 0 and 100.
                                                    // Running probabilty will be the remaining percentage

    // Movement actions
    private const int NONE = -1;                    // None selection
    private const int WALK = 0;                     // Walk movement selection
    private const int STAND = 1;                    // Standing movement selection 
    private const int RUN = 2;                      // Run movement selection
    private int lastMovementSelection = NONE;       // Last movement selected
    private int previousMovementSelection = NONE;   // Previous movement selected
    private bool isMoving = false;                  // Boolean to check if character is moving or not.

    private float selectionTimer = 0;               // How long will the selected movement be performed? (Will be assigned Randomly).
    public float min_selectionTimer;                // Minimum value of selectionTimer (on Initialization)
    public float max_SelectionTimer;                // Maximum value of selectionTimer

    private int movementDirection = 0;              // To specify where Remilia is moving (Left by default).
    private const int LEFT = 0;                     // Left direction
    private const int RIGHT = 1;                    // Right direction
  
    private GameObject shadowObj = null;            // List of Gameobjects that in-game represents a Shadow
    private GameObject remiSprite = null;           // Sprite of Remi

    private bool facingRight = true;
    private bool stopMovement = false;              // To stop movement


    [SerializeField]                                // Delete later
    private int collidersOutside = 3;

    void Start () {
        Vector2 mousePosition = CameraHelper.getCursorPosition();
        this.transform.position = new Vector2(mousePosition.x, this.transform.position.y);
        this.remiSprite = transform.Find("RemiSprite").gameObject;
        this.shadowObj = GameObject.Find("Player/UmbrellaShadow");
        this.lastMovementSelection = STAND;
        this.selectionTimer = 1.0f;

        this.HP = 1;                                // Delete later
        this.collidersOutside = 3;                  // Delete later
    }
	
    // Update is called once per frame
    void Update(){
        if (!stopMovement){ 
            moveCharacter();
            burnCharacter();
            if ( HP <= 0 ) GameOver();
        }
    }

    private void burnCharacter()
    {
        this.HP -= burnSpeed * Time.deltaTime * collidersOutside;
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "UmbrellaShadow")
        {
            collidersOutside += 1;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "UmbrellaShadow")
        {
            collidersOutside -= 1;
        }
    }


    // The things that happens when Player loses.
    private void GameOver(){
        this.stopMovement = true;
        MicrogameController.instance.setVictory(false, true);
        changeSpriteColor(Color.red);                                   // ONLY FOR DEBUGING
    }


    // Move character (Remilia)
    private void moveCharacter()
    {
        if (lastMovementSelection == NONE) {
            chooseMovement();
        }
 
        else {
            continueMovement();
        }
    }


    // Select one of the three options of movement (Walk, Stand or Run)
    private void chooseMovement()
    {

        int rnd_number = Random.Range(1, 101);                                  // Random, number between 1 and 100 (For probabilities)

        if (hasWalkBeenSelected(rnd_number))
        {
            chooseMovementDirection();
            walkMovement();
            lastMovementSelection = WALK;
        }

        else if (standHasBeenSelected(rnd_number))
        {
            standing();
            lastMovementSelection = STAND;
        }

        else
        {
            chooseMovementDirection();
            runMovement();
            lastMovementSelection = RUN;
        }

        selectionTimer = Random.Range(min_selectionTimer, max_SelectionTimer);
    }


    // Continue movement that has been chosen previously.
    private void continueMovement()
    {
        switch (lastMovementSelection)
        {
            case WALK:
                walkMovement();
                break;

            case STAND:
                standing();
                break;

            case RUN:
                runMovement();
                break;
        }

        selectionTimer = selectionTimer - Time.deltaTime;
        if (selectionTimer <= 0) { resetMovementSelectionParameters(); }
    }


    public void resetMovementSelectionParameters()
    {
        this.previousMovementSelection = this.lastMovementSelection;
        this.lastMovementSelection = NONE;
        this.isMoving = false;
    }


    // Check if Walk movement has been selected according to a number between 1 and 100.
    private bool hasWalkBeenSelected(int number)
    {
        int temp_walkProbability = 0;

        if(previousMovementSelection == STAND)  // Standing move will not be selected twice in a row.
        {
            temp_walkProbability = this.walkProbability * 100 / (100 - this.standProbability);
        }

        else
        {
            temp_walkProbability = this.walkProbability;
        }

        if (number >= 1 && number <= temp_walkProbability){
            return true;
        }
        
        return false;
    }

    // Check if Standing movement has been selected according to a number between 1 and 100
    private bool standHasBeenSelected(int number)
    {

        if (previousMovementSelection == STAND) // Standing move will not be selected twice in a row.
        {
            return false;
        }

        if(number > this.walkProbability && number <= this.standProbability + this.walkProbability)
        {
            return true;
        }

        return false;
    }


    // Make character walk
    private void walkMovement()
    {
        var move = obtainMovementVector3();
        Bounds remiBounds = this.GetComponent<BoxCollider2D>().bounds;
        this.transform.position = this.transform.position + (move * this.walkingSpeed * Time.deltaTime);
        this.isMoving = true;
        this.currentSpeed = walkingSpeed;
        changeDirectionOnLimit();
    }


    // Make character stand
    private void standing()
    {
        this.isMoving = false;
        this.currentSpeed = 0;
    }
    

    // Make caharacter run
    private void runMovement()
    {
        var move = obtainMovementVector3();
        Bounds remiBounds = this.GetComponent<BoxCollider2D>().bounds;
        if (this.currentSpeed == 0) this.currentSpeed = walkingSpeed;
        this.transform.position = this.transform.position + (move * this.currentSpeed * Time.deltaTime);
        if (this.currentSpeed < this.runningSpeed) this.currentSpeed += this.accelerationFactor;
        this.isMoving = true;
        changeDirectionOnLimit();
    }


    // Choose Randomly a direction which the character will follow (LEFT or RIGHT, 0 or 1). Also, if character was walking or running previously, then it won't change direction.
    private void chooseMovementDirection()
    {
        if (!isMoving) {
            if (!(previousMovementSelection == WALK || previousMovementSelection == RUN))
            {
                this.movementDirection = Random.Range(0, 2);
                if (this.movementDirection == RIGHT && facingRight == false){ flipHorizontally(); }
                else if (this.movementDirection == LEFT && facingRight == true) { flipHorizontally(); }
            }
        }
    }


    // Change direction of movement if character reach left or right limit
    private void changeDirectionOnLimit()
    {

        if (this.transform.position.x <= leftLimit){ this.movementDirection = RIGHT; }
        else if(this.transform.position.x >= rightLimit){ this.movementDirection = LEFT; }

        if (this.movementDirection == RIGHT && facingRight == false) { flipHorizontally(); }
        else if (this.movementDirection == LEFT && facingRight == true) { flipHorizontally(); }
    }


    // Obtain movement vector according to direction
    private Vector3 obtainMovementVector3()
    {
        var move = new Vector3(0, 0, 0);           
        switch (this.movementDirection)
        {
            case LEFT:
                move = new Vector3(-1, 0, 0);
                break;

            case RIGHT:
                move = new Vector3(1, 0, 0);
                break;
        }
        return move;
    }

    private void flipHorizontally()
    {
        if (facingRight)
        {
            facingRight = false;
            remiSprite.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            facingRight = true;
            remiSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // Limit character movement to left and right limit
    private void limitMovement()
    {
        this.transform.position = new Vector2(Mathf.Clamp(leftLimit, this.transform.position.x, rightLimit), this.transform.position.y);
    }


    // Check if character is under shadow or not
    private bool checkIfUnderShadow()
    {
        Bounds remiBounds = this.GetComponent<BoxCollider2D>().bounds;
        float left = remiBounds.center.x - remiBounds.extents.x;
        float right = remiBounds.center.x + remiBounds.extents.x;

        Bounds shadowBounds = shadowObj.GetComponent<BoxCollider2D>().bounds;

        if (shadowBounds.Contains(new Vector2(left, shadowBounds.center.y)) && shadowBounds.Contains(new Vector2(right, shadowBounds.center.y)))
        {   
            return true;
        }

        return false;      
    }


    private void changeSpriteColor(Color color)
    {
        remiSprite.GetComponent<SpriteRenderer>().color = color;
    }

}
