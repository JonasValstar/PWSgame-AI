using UnityEngine;

public class Mover : MonoBehaviour
{
    bool score;
    public bool isSelected = false;
    public GameObject toBeDeletedPiece;
    Selection selection;
    Piece pieceScript;
    public string pieceTag;

    // Start is called before the first frame update
    void Start()
    {
        selection = GameObject.Find("Main Camera").GetComponent<Selection>();

        if (transform.position.x > 15 || transform.position.x < -15 || transform.position.y > 35 || transform.position.y < -35) {
            if (transform.position.y > 35 && transform.position.y < 55 && pieceTag == "Piece Light") {
                this.transform.position = new Vector3(0, 45, 0);
                score = true;
            } else if (transform.position.y < -35 && transform.position.y > -55 && pieceTag == "Piece Dark") {
                this.transform.position = new Vector3(0, -45, 0);
                score = true;
            } else {
                Destroy(gameObject);
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected == true) {
            if (score == true) {
                Vector3 movePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                pieceScript = GetComponentInParent<Piece>();
                pieceScript.isSelected = false;
                pieceScript.ScorePiece(movePosition);
                isSelected = false;
            } else {
                Vector3 movePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                pieceScript = GetComponentInParent<Piece>();
                pieceScript.isSelected = false;
                if (name == "Deletion") {
                    if (toBeDeletedPiece.transform.tag == "Piece Light") {
                        selection.totalLight -= 1;
                    } else {
                        selection.totalDark -= 1;
                    }
                    Destroy(toBeDeletedPiece);
                }
                pieceScript.MovePiece(movePosition);
                isSelected = false;
            }
        }
    }
}
