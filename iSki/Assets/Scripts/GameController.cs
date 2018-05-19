using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

/// <summary>
/// GameController controls player movement, player & trail colors, scores, continuing and ads
/// </summary>
public class GameController : MonoBehaviour {

    public float speed = 10.0f;
    private float horizontalSpeed = 0.0f;
    public float horizontalMaxSpeed = 4.0f;
    public float horizontalAcceleration = 0.15f;
    private bool hasStarted = false;
    private bool ended = false;
    private bool hasContinued = false;

    private Vector3 moveDirection = new Vector3(0.0f, -1.0f, 0.0f);
    private Vector3 moveDown = new Vector3(0.0f, -1.0f, 0.0f);
    private Vector3 moveDownLeft = new Vector3(-1.0f, -1.0f, 0.0f);
    private Vector3 moveDownRight = new Vector3(1.0f, -1.0f, 0.0f);

    public List<Color> colors;
    private SpriteRenderer playerSprite;
    private TrailRenderer playerLine;
    private int colorIndex = 0;

    private Text bestText;
    private Text scoreText;
    private Text complimentText;
    private float score = 0.0f;
    private int bonus = 0;

    public GameObject endObject;
    public GameObject endNoAdsObject;
    private GameObject instructionsObject;

    private string[] complimentsLow = new string[] { "Nice!", "Awesome!", "Wow!", "That's nice!", "Nice job!" };
    private string[] complimentsMedium = new string[] { "What the...", "You're the best!", "Simply amazing!", "Unbelievable!" };
    private string[] complimentsHigh = new string[] { "Next stop: world cup!", "Your mom would be proud!", "Mind shocking skills!", "One of a kind" };

    private void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        playerLine = GetComponent<TrailRenderer>();

        bestText = GameObject.Find("Best").GetComponent<Text>();
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        complimentText = GameObject.Find("Compliment").GetComponent<Text>();
        instructionsObject = GameObject.Find("Instructions");

        // Show best result is there is one
        if (PlayerPrefs.HasKey("best"))
        {
            bestText.text = PlayerPrefs.GetInt("best").ToString();
        }
        else
        {
            bestText.text = "0";
        }
    }

    /// <summary>
    /// In Update we move player and update score view
    /// </summary>
    private void Update()
    {
        if (ended) return;

        // Determinate need for change of direction.
        if (Input.GetMouseButtonDown(0))
        {
            if (moveDirection == moveDown || moveDirection == moveDownLeft)
            {
                moveDirection = moveDownRight;
            }
            else
            {
                moveDirection = moveDownLeft;
            }

            hasStarted = true;
            instructionsObject.SetActive(false);
        }

        if (!hasStarted) return;

        // Check need for acceleration.
        if (moveDirection == moveDownLeft && horizontalSpeed > -horizontalMaxSpeed)
        {
            horizontalSpeed -= (horizontalAcceleration * Time.deltaTime);
        }
        else if (moveDirection == moveDownRight && horizontalSpeed < horizontalMaxSpeed)
        {
            horizontalSpeed += (horizontalAcceleration * Time.deltaTime);
        }

        // Move to desired direction.
        Vector3 tmpMoveDirection = moveDirection;
        tmpMoveDirection.x = horizontalSpeed;
        transform.Translate(tmpMoveDirection * Time.deltaTime * speed);

        // Add score.
        score += (Time.deltaTime * 6);
        scoreText.text = ((int)score).ToString();
    }

    /// <summary>
    /// OnCollisionEnter2D means that the player has hit a collider called "collider-shake"
    /// which means the game is over. If there is an ad available and player haven't continued
    /// yet, we offer chance to continue by watching an ad.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Save best result (if it's larger than the old best score).
        if (!PlayerPrefs.HasKey("best") || PlayerPrefs.GetInt("best") < (int)score) {
            PlayerPrefs.SetInt("best", (int)score);
        }
           
        ended = true;

        // Prevents players color from changing while in "paused" state.
        CancelInvoke();

        Handheld.Vibrate();

        // If player has already continued the game by watching an ad, the only option is to retry.
        if (hasContinued)
        {
            endNoAdsObject.SetActive(true);
            return;
        }
        
        // Check for available ads. If there are none, we show retry option.
        if (Advertisement.IsReady("rewardedVideo"))
        {
            endObject.SetActive(true);

            // There is 5 seconds timer after which scene will be loaded automatically.
            Invoke("Restart", 4.5f);
        }
        else
        {
            endNoAdsObject.SetActive(true);
        }
    }

    /// <summary>
    /// Player hit a trigger on a tree which means we need to give out bonus points.
    /// Also determinate need to change color of the player and the trail following the player.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Handheld.Vibrate();

        // Chance colors
        colorIndex = bonus <= 15 ? 0 : bonus <= 30 ? 1 : 2;
        //colorIndex = colorIndex < colors.Count - 1 ? colorIndex + 1 : colors.Count - 1;
        ChangeColor();
        // Change color again after 3 seconds
        CancelInvoke();
        Invoke("ReduceColorIndex", 3.0f);

        // Count bonus & show score
        bonus += (colorIndex + 2);
        score += bonus;

        // Show compliment
        string[] tmpCompliment = colorIndex == 0 ? complimentsLow : colorIndex == 1 ? complimentsMedium : complimentsHigh;
        complimentText.text = tmpCompliment[Random.Range(0, tmpCompliment.Length)];

        // Shake the tree & show bonus
        collision.GetComponentInParent<TreeController>().GotHit(bonus, colors[colorIndex]);
    }

    /// <summary>
    /// Quite self explanatory
    /// </summary>
    private void ChangeColor()
    {
        Color newColor = colors[colorIndex];
        playerSprite.color = newColor;
        playerLine.startColor = newColor;

        newColor.a = 0.29411f;
        playerLine.endColor = newColor;
    }

    /// <summary>
    /// Quite self explanatory
    /// </summary>
    private void ReduceColorIndex()
    {
        colorIndex = 0;
        bonus = 0;

        ChangeColor();
    }

    /// <summary>
    /// Player clicked the "SECOND CHANCE" button. Show ad. Ad availability has already been
    /// checked when game ended.
    /// </summary>
    public void SecondChance()
    {
        // Cancel the restart in 5 seconds
        CancelInvoke();

        ShowOptions options = new ShowOptions { resultCallback = HandleShowResult };
        Advertisement.Show("rewardedVideo", options);
    }


    /// <summary>
    /// Quite self explanatory
    /// </summary>
    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Continue();
                break;
            case ShowResult.Skipped:
                Restart();
                break;
            case ShowResult.Failed:
                Restart();
                break;
        }
    }

    /// <summary>
    /// Set player back to center and remove nearby trees.
    /// </summary>
    private void Continue()
    {
        hasStarted = false;
        ended = false;
        hasContinued = true;

        instructionsObject.SetActive(true);
        endObject.SetActive(false);

        // Reposition player
        Vector3 newStartPosition = transform.position;
        newStartPosition.x = 0.0f;
        transform.position = newStartPosition;

        // Spherecast to remove nearby trees
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].name == "collider-trunk")
                hits[i].transform.parent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Quite self explanatory
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene("game");
    }
}
