using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Selection : MonoBehaviour
{
    // turn
    public int turn = 1; // 1 = light -1 = dark
    public Animator turnAnimatorDark;
    public Animator turnAnimatorLight;

    // ai auto-play
    public int storedTurn = 1;
    public bool gameWon = false;

    // score
    public GameObject lightWinText;
    public GameObject DarkWinText;
    bool won = false;

    // total pieces
    public int totalLight = 8;
    public int totalDark = 8;
    public int lightZeta = 0;
    public int lightEta = 0;
    public int lightTheta = 0;
    public int darkZeta = 0;
    public int darkEta = 0;
    public int darkTheta = 0;
    public TMP_Text lightZetaText;
    public TMP_Text lightEtaText;
    public TMP_Text lightThetaText;
    public TMP_Text darkZetaText;
    public TMP_Text darkEtaText;
    public TMP_Text darkThetaText;
    GameObject[] scoringPieces;

    // rule menu and choose menu
    public GameObject ruleMenu;
    public GameObject chooseMenu;

    // rest
    public bool somethingSelected;
    public Transform selectedTransform;
    public AI ai;

    void Start()
    {
        if (PlayerPrefs.HasKey("playMode") == false) {
            PlayerPrefs.SetInt("playMode", 1);
        }
        if (PlayerPrefs.HasKey("aiTurn") == false) {
            PlayerPrefs.SetInt("aiTurn", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            ai.InvokeAI();
        }

        if (PlayerPrefs.GetInt("playMode") == 3) { // checks if AI vs AI is selected
            if (gameWon == false) {
                if (turn == storedTurn) {
                    storedTurn = storedTurn * -1;
                    ai.InvokeAI();
                }
            }
        } else {
            if (PlayerPrefs.GetInt("playMode") == 2) {
                if (turn == storedTurn) {
                    storedTurn = storedTurn * -1;
                    if (turn == PlayerPrefs.GetInt("aiTurn")) {
                        ai.InvokeAI();
                    }
                }
                
            }


            if (Input.GetMouseButtonDown(0)) {
                Vector2 raycastPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(raycastPosition, Vector2.zero);

                if (hit.collider != null) {
                    if (hit.collider.tag == "Piece Light" || hit.collider.tag == "Piece Dark") {
                        if ((hit.collider.tag == "Piece Light" && turn == 1) || (hit.collider.tag == "Piece Dark" && turn == -1))
                        if (hit.collider.gameObject.GetComponent<Piece>().isSelected != true) {
                            hit.collider.gameObject.GetComponent<Piece>().isSelected = true;
                        } else {
                            hit.collider.gameObject.GetComponent<Piece>().isSelected = false;
                        }
                    } else if (hit.collider.tag == "Movement Thing") {
                        hit.collider.gameObject.GetComponent<Mover>().isSelected = true;
                        ChangeTurn();
                    }
                }
            }
        }
        
        

        // check for win
        if (totalDark == 0 && won == false) { // light wins
            WinGame(true);
            won = true;
        } else if (totalLight == 0 && won == true) { // Dark wins
            WinGame(false);
            won = true;
        }
    }

    public void OpenRuleMenu() {
        if (ruleMenu.activeSelf == false) {
            ruleMenu.SetActive(true);
        } else {
            ruleMenu.SetActive(false);
        }
        
    }

    public void ChangeTurn() {
        turn = turn * -1;
        if (turn == 1) {
            turnAnimatorDark.SetTrigger("Disappear");
            turnAnimatorLight.SetTrigger("Appear");
        }
        else {
            turnAnimatorDark.SetTrigger("Appear");
            turnAnimatorLight.SetTrigger("Disappear");
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WinGame(bool light) 
    {
        gameWon = true;
        // deleting all pieces from board
        foreach(GameObject piece in GameObject.FindGameObjectsWithTag("Piece Light")) {
            piece.GetComponent<Piece>().DeletePieceBecauseWon();
        }
        foreach(GameObject piece in GameObject.FindGameObjectsWithTag("Piece Dark")) {
            piece.GetComponent<Piece>().DeletePieceBecauseWon();
        }

        // make win active
        if (light == true) { // light has won
            turnAnimatorDark.SetTrigger("Disappear");
            lightWinText.SetActive(true);
        } else {
            turnAnimatorLight.SetTrigger("Disappear");
            DarkWinText.SetActive(true);
            
        }
    }

    public void changePlayMode(int mode) 
    {
        PlayerPrefs.SetInt("playMode", mode);
        if (mode != 2) {
            Restart();
        }
    }

    public void SetAiTurn(int turn) {
        PlayerPrefs.SetInt("aiTurn", turn);
    }

    public void OpenChooseMenu(bool open) {
        chooseMenu.SetActive(open);
        if (open == false) {
            Restart();
        }
    }

    public void UpdateCaptureTexts()
    {
        lightZetaText.text = " " + lightZeta.ToString();
        lightEtaText.text = " " + lightEta.ToString();
        lightThetaText.text = " " + lightTheta.ToString();
        darkZetaText.text = " " + darkZeta.ToString();
        darkEtaText.text = " " + darkEta.ToString();
        darkThetaText.text = " " + darkTheta.ToString();
    }

    public void ExitGame() {
        Application.Quit();
    }
}
