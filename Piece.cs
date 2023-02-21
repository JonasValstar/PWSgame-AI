using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int type;
    public List<Vector3> MoveLocations = new List<Vector3>();
    public bool isSelected = false;
    public bool turned = false;
    public PieceSO pieceSO;
    public Selection selection;
    public GameObject circle;
    public GameObject circleEnemy;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        selection = GameObject.Find("Main Camera").GetComponent<Selection>();
        animator = GetComponent<Animator>();
        type = pieceSO.type;
        MoveLocations = pieceSO.MoveLocations;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected == true) {
            if (transform.childCount == 0) { // check for already spawned circles

                /* --> making unselectable piece actually unselectable
                foreach(Vector3 location in pieceSO.MoveLocations) {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + location, Vector2.zero);
                    if (hit.collider == null || hit.collider.tag != transform.tag) {
                        isSelected = false;
                        break;
                    }
                }
                */

                //! maybe finished / not tested

                // unselecting other pieces
                if (selection.somethingSelected == true) { // checking is something is selected
                    selection.selectedTransform.GetComponent<Piece>().isSelected = false; // making piece unselected, stopping spawning of circles
                    foreach (Transform child in selection.selectedTransform) {
                        GameObject.Destroy(child.gameObject); // delete movement circles
                    }
                } else { // letting the main script know something has been selected
                    selection.somethingSelected = true;
                }

                // making piece selected
                selection.selectedTransform = transform;

                //! until here

                // spawn circles
                foreach(Vector3 location in pieceSO.MoveLocations) {
                    // checking occupancy of tiles
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + location, Vector2.zero);
                    if (hit.collider != null && hit.collider.tag != transform.tag) { // spawns RED circle if piece with different tag already on target tile
                        GameObject redCircle = Instantiate(circleEnemy, transform.position + location, Quaternion.identity, this.gameObject.transform);
                        redCircle.GetComponent<Mover>().toBeDeletedPiece = hit.collider.gameObject;
                        redCircle.GetComponent<Mover>().pieceTag = transform.tag.ToString();
                        redCircle.name = "Deletion";
                    } else if (hit.collider == null) { // creates a GREEN circle is tile is empty
                        GameObject greenCircle = Instantiate(circle, transform.position + location, Quaternion.identity, this.gameObject.transform);
                        greenCircle.GetComponent<Mover>().pieceTag = transform.tag.ToString();
                    } // does not spawn a circle if piece of same tag already on target tile
                }
            }
        }
    }

    public void MovePiece(Vector3 movePosition)
    {
        transform.position = new Vector3(movePosition.x, movePosition.y, -1);
        foreach (Transform child in this.transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void ScorePiece(Vector3 movePosition)
    {
        transform.Translate(movePosition - transform.position);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        foreach (Transform child in this.transform) {
            GameObject.Destroy(child.gameObject);
        }
        animator.SetTrigger("Disappear");
        if (transform.tag == "Piece Light") { // Light wins
             selection.WinGame(true);
        } else { // Dark wins
            selection.WinGame(false);
        }
        selection.somethingSelected = false;
    }

    public void DeletePieceBecauseWon() 
    {
        animator.SetTrigger("Disappear");
        selection.somethingSelected = false;
    }


    public void DeletePiece() {
        Destroy(gameObject);
    }
}