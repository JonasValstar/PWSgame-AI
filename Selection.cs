using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Selection : MonoBehaviour
{
    // turn
    public int turn = 1; // 1 = light -1 = dark
    public Animator turnAnimatorDark;
    public Animator turnAnimatorLight;

    // score
    public int lightScore = 0;
    public int darkScore = 0;
    public TMP_Text scoreLight;
    public TMP_Text scoreDark;
    public GameObject lightWins;
    public GameObject darkWins;

    // total pieces
    public int totalLight = 8;
    public int totalDark = 8;
    GameObject[] scoringPieces;

    // undo
    public GameObject targetPiece;
    public GameObject DestroyedPiece;
    public Vector3 oldPosition;

    // rest
    public bool somethingSelected;
    public Transform selectedTransform;
    public AI ai;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            ai.InvokeAI();
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

        if (totalDark == 0) {
            //! Light wins
            //? something with this code
            
            scoringPieces = GameObject.FindGameObjectsWithTag("Piece Light");
            foreach (GameObject piece in scoringPieces) {
                piece.GetComponent<Piece>().ScorePiece(piece.transform.position);
            }
            totalDark = -1;
            totalLight = -1;
        } else if (totalLight == 0) {
            //! Dark wins
            //? something with this code

            scoringPieces = GameObject.FindGameObjectsWithTag("Piece Dark");
            foreach (GameObject piece in scoringPieces) {
                piece.GetComponent<Piece>().ScorePiece(piece.transform.position);
            }
            totalDark = -1;
            totalLight = -1;
        }
    }

    //TODO - add animation for turn change and indicator

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

    public void AddScore(bool lightPiece) 
    {
        if (lightPiece == true) {
            scoreLight.text = lightScore.ToString();
        } else {
            scoreDark.text = darkScore.ToString();
        }
    }

    public void EndGame()
    {
        
    }

    public void Restart()
    {
        SceneManager.LoadScene("Standard pvp");
    }
}
