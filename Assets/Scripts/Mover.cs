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
                        Debug.Log("1" + toBeDeletedPiece.gameObject.GetComponent<Piece>().type);
                        switch(toBeDeletedPiece.gameObject.GetComponent<Piece>().type) {
                            case 0:
                                selection.lightZeta++;
                                break;
                            case 1:
                                selection.lightEta++;
                                break;
                            case 2:
                                selection.lightTheta++;
                                break;
                        }
                        selection.totalLight -= 1;
                    } else {
                        Debug.Log("2" + toBeDeletedPiece.gameObject.GetComponent<Piece>().type);
                        switch(toBeDeletedPiece.gameObject.GetComponent<Piece>().type) {
                            case 3:
                                selection.darkZeta++;
                                break;
                            case 4:
                                selection.darkEta++;
                                break;
                            case 5:
                                selection.darkTheta++;
                                break;
                        }
                        selection.totalDark -= 1;
                    }
                    selection.UpdateCaptureTexts();
                    Destroy(toBeDeletedPiece);
                }
                pieceScript.MovePiece(movePosition);
                isSelected = false;
            }
        }
    }
}
