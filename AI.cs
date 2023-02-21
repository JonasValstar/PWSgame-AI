using System.Collections.Generic;
using UnityEngine;


public class AI : MonoBehaviour
{
    // variables
    public int Depth;
    public float ValueZeta = 100; // base value of Zeta piece
    public float ZetaMC = 0.1f; // multiplication constant of Zeta piece
    public float ValueEta = 130;// base value of Eta piece
    public float EtaMC = 0.01f; // multiplication constant of Eta piece
    public float ValueTheta = 200;// base value of Theta piece
    public float ThetaMC = 0.03f; // multiplication constant of Theta piece
    public float W_pieceValue = 1; // Weight of pieceValue
    public float W_diffPieceValue = 1; // Weight of enemy's pieceValue
    public float W_averagePosition = 1; // Weight of position
    public float W_diffAveragePosition = 1; // Weight of enemy position
    public float W_coverage = 1; // Weight of covered pieces
    public float W_Values = 1; // Weight of difference in values
    public float W_Positions = 1; // Weight of difference in average positions

    // return Lists
    List<int[,]> R_winFM = new List<int[,]>(); // 2 column list with First Move that leads to win and at which depth
    List<Vector3> R_newPos = new List<Vector3>(); 
    List<Vector3> R_oldPos = new List<Vector3>(); 
    List<int> R_type = new List<int>(); 

    // list for AI
    List<List<Vector3>> boardLayout = new List<List<Vector3>>();
    List<List<Vector3>> boardLayoutBuff = new List<List<Vector3>>(); // buffer
    List<List<Vector3>> boardLayoutDiff = new List<List<Vector3>>();
    List<List<Vector3>> boardLayoutBuffDiff = new List<List<Vector3>>(); // buffer
    List<List<int>> boardType = new List<List<int>>();
    List<List<int>> boardTypeBuff = new List<List<int>>(); // buffer
    List<List<int>> boardTypeDiff = new List<List<int>>();
    List<List<int>> boardTypeBuffDiff = new List<List<int>>(); // buffer
    List<int> firstMove = new List<int>();
    List<int> firstMoveBuff = new List<int>(); // buffer

    // list for surroundings
    Vector3[] surroundingTiles = {  new Vector3(0, 10, 0), 
                                    new Vector3(10, 10, 0),
                                    new Vector3(10, 0, 0),
                                    new Vector3(10, -10, 0),
                                    new Vector3(0, -10, 0),
                                    new Vector3(-10, -10, 0),
                                    new Vector3(-10, 0, 0),
                                    new Vector3(-10, 10, 0) };
    
    // prefabs of Pieces
    public GameObject[] piecePrefabs = new GameObject[6];   /*  0 == ATK Light
                                                                1 == DEF Light
                                                                2 == ALL Light
                                                                3 == ATK Dark
                                                                4 == DEF Dark
                                                                5 == ALL Dark */ 
    
    void getPossibleMoves(List<Vector3> Positions, List<int> Types, int currentDepth) 
    {
        // clearing lists
        R_oldPos.Clear();
        R_newPos.Clear();
        R_type.Clear();

        // calculating all possible moves
        for (int i = 0; i < Positions.Count; i++) { // loops through every piece
            foreach(Vector3 move in piecePrefabs[Types[i]].GetComponent<Piece>().pieceSO.MoveLocations) { // gets all possible moves a piece can make
                Vector3 newLocation = new Vector3(Positions[i].x + move.x, Positions[i].y + move.y, -1); // get the new location
                if ((newLocation.y > 40 && piecePrefabs[Types[i]].tag == "Piece Light") || (newLocation.y < -40 && piecePrefabs[Types[i]].tag == "Piece Dark")) { // check to see if move is legal
                    //! error in code on line 72 (index is negative or bigger than list)
                    R_winFM.Add(new int[firstMove[i], currentDepth]); // adds FM and depth to list
                } else if(newLocation.x < 20 && newLocation.x > -20 && newLocation.y < 40 && newLocation.y > -40 && Positions.Contains(newLocation) == false) {
                    R_oldPos.Add(Positions[i]); 
                    R_newPos.Add(newLocation);
                    R_type.Add(Types[i]);
                }
            }
        }
    }

    void GetBoardPositions(bool AI, List<Vector3> oldLocations, List<int> pieceType, List<Vector3> newPositions, List<Vector3> I_layout, List<int> I_layoutType, List<Vector3> I_diffLayout, List<int> I_diffLayoutType, int firstMove)
    {
        // booleans
        bool moveCheckedAndOk = true;
        bool foundEnemy = false;
        bool foundAlly = false;

        for (int i = 0; i < newPositions.Count; i++) { // loops through all moves

            // creating lists
            List<Vector3> layout = new List<Vector3>();
            List<int> layoutType = new List<int>();
            layout.AddRange(I_layout);
            layoutType.AddRange(I_layoutType);
            List<Vector3> diffLayout = new List<Vector3>();
            List<int> diffLayoutType = new List<int>();
            diffLayout.AddRange(I_diffLayout); //* this one
            diffLayoutType.AddRange(I_diffLayoutType);

            // checking giving away piece
            for (int p = 0; p < surroundingTiles.Length; p++) { // looping every surrounding tile
                if (oldLocations.Contains(newPositions[i] + surroundingTiles[p])) { // checking if 
                    foreach(Vector3 move in piecePrefabs[layoutType[layout.IndexOf(newPositions[i] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                        if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check if ally is able to cover piece
                            foundAlly = true;
                        }
                    }
                } else if(diffLayout.Contains(newPositions[i] + surroundingTiles[p])) {
                    foreach(Vector3 move in piecePrefabs[diffLayoutType[diffLayout.IndexOf(newPositions[i] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                        if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check is enemy is able to attack piece
                            foundEnemy = true;
                        }
                    }
                }
            }

            // checking if move is bad
            if (foundEnemy == true && foundAlly == false) {
                moveCheckedAndOk = false;
            }

            if (moveCheckedAndOk == true) {
                // check if enemy piece is captured
                if (diffLayout.Contains(newPositions[i])) { 
                    diffLayoutType.RemoveAt(diffLayout.IndexOf(newPositions[i])); // removes enemy type from list
                    diffLayout.Remove(newPositions[i]); // removes enemy from list
                }

                // moving pieces
                layoutType.RemoveAt(layout.IndexOf(oldLocations[i]));
                layoutType.Add(pieceType[i]);
                layout.Remove(oldLocations[i]);
                layout.Add(newPositions[i]);

                // replaces the old location with the new one
                if (AI == true) { // checks wether it is resolving for self or enemy
                    boardLayoutBuff.Add(layout); // adds positions to list
                    boardTypeBuff.Add(layoutType); // adds type of pieces to list
                    boardLayoutBuffDiff.Add(diffLayout); // adds enemy positions to list
                    boardTypeBuffDiff.Add(diffLayoutType); // adds enemy types to list
                    firstMoveBuff.Add(firstMove); // adds first move to list
                } else {
                    boardLayoutBuff.Add(diffLayout); // adds positions to list
                    boardTypeBuff.Add(diffLayoutType); // adds type of pieces to list
                    boardLayoutBuffDiff.Add(layout); // adds enemy positions to list
                    boardTypeBuffDiff.Add(layoutType); // adds enemy types to list
                    firstMoveBuff.Add(firstMove); // adds first move to list
                }
            }

            // resetting booleans
            foundAlly = false;
            foundEnemy = false;
            moveCheckedAndOk = true;
        }
    }

    public void InvokeAI() 
    {
        // booleans
        bool moveCheckedAndOk = true;
        bool foundEnemy = false;
        bool foundAlly = false;
        bool freePiece = true;

        // creating lists
        List<Vector3> FM_oldPosition = new List<Vector3>(); // FM = first moves
        List<Vector3> FM_newPosition = new List<Vector3>(); 
        List<int> FM_type = new List<int>(); 
        List<Vector3> currentPositions = new List<Vector3>();
        List<Vector3> diffCurrentPositions = new List<Vector3>();
        List<int> currentPositionsType = new List<int>();
        List<int> diffCurrentPositionsType = new List<int>();
        List<bool> turned = new List<bool>();
        List<int> freePieceMove = new List<int>();

        // back-up lists for bad moves
        List<int> BadMoves = new List<int>();

        // for calculating
        float totalPieceValue = new float();
        float averageBoardPosition = new float();
        float diffTotalPieceValue = new float();
        float diffAverageBoardPosition = new float();
        float covered = new float();
        List<float> totalBoardValue = new List<float>();
        List<float> bestScore = new List<float>();
        bestScore.Add(0);
        List<int> bestScoreIndex = new List<int>();
        bestScoreIndex.Add(0);
        

        // clearing all lists
        R_winFM.Clear();
        boardLayout.Clear();
        boardLayoutBuff.Clear();
        boardLayoutDiff.Clear();
        boardLayoutBuffDiff.Clear();
        boardType.Clear();
        boardTypeBuff.Clear();
        boardTypeDiff.Clear();
        boardTypeBuffDiff.Clear();
        firstMove.Clear();
        firstMoveBuff.Clear();
        
        // getting starting positions
        if (gameObject.GetComponent<Selection>().turn == -1) { // darks turn
            foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Dark")) { // gets all light pieces and puts them in correct list
                currentPositions.Add(piece.transform.position); // location of piece
                currentPositionsType.Add(piece.GetComponent<Piece>().type); // type of piece
            }
            foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Light")) { // gets all dark pieces and puts them in correct list
                diffCurrentPositions.Add(piece.transform.position); // location of piece
                diffCurrentPositionsType.Add(piece.GetComponent<Piece>().type); // type of piece
            }
        } else if (gameObject.GetComponent<Selection>().turn == 1) { // lights turn
            foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Light")) { // gets all light pieces and puts them in correct list
                currentPositions.Add(piece.transform.position); // location of piece
                currentPositionsType.Add(piece.GetComponent<Piece>().type); // type of piece
            }
            foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Dark")) { // gets all dark pieces and puts them in correct list
                diffCurrentPositions.Add(piece.transform.position); // location of piece
                diffCurrentPositionsType.Add(piece.GetComponent<Piece>().type); // type of piece
            }
        }

        // getting all first positions and moves
        for (int i = 0; i < currentPositions.Count; i++) { // loops through every piece
            foreach(Vector3 move in piecePrefabs[currentPositionsType[i]].GetComponent<Piece>().pieceSO.MoveLocations) { // gets all possible moves a piece can make
                Vector3 newLocation = new Vector3(currentPositions[i].x + move.x, currentPositions[i].y + move.y, -1); // get the new location
                if ((newLocation.y > 40 && piecePrefabs[currentPositionsType[i]].tag == "Piece Light")) { // check to see if AI can win as Light
                    gameObject.GetComponent<Selection>().WinGame(true); // winning game
                    freePieceMove.Add(-1); // setting to -1 to stop AI from calculating further
                } else if ((newLocation.y < -40 && piecePrefabs[currentPositionsType[i]].tag == "Piece Dark")) { // check to see if AI can win as Dark
                    gameObject.GetComponent<Selection>().WinGame(false); // winning game
                    freePieceMove.Add(-1); // setting to -1 to stop AI from calculating further
                } else if(newLocation.x < 20 && newLocation.x > -20 && newLocation.y < 40 && newLocation.y > -40 && currentPositions.Contains(newLocation) == false) {
                    // remember first moves
                    FM_oldPosition.Add(currentPositions[i]); 
                    FM_newPosition.Add(newLocation);
                    FM_type.Add(currentPositionsType[i]);
                }
            }
        }

        // adding to list
        for (int i = 0; i < FM_newPosition.Count; i++) { // loops through all moves            
            
            // checking giving away pieces
            for (int p = 0; p < surroundingTiles.Length; p++) { // looping every surrounding tile
                if (currentPositions.Contains(FM_newPosition[i] + surroundingTiles[p]) && currentPositions[currentPositions.IndexOf(FM_newPosition[i] + surroundingTiles[p])] != FM_oldPosition[i]) { // checking if 
                    foreach(Vector3 move in piecePrefabs[currentPositionsType[currentPositions.IndexOf(FM_newPosition[i] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                        if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check if ally is able to cover piece
                            foundAlly = true;
                        }
                    }
                } else if(diffCurrentPositions.Contains(FM_newPosition[i] + surroundingTiles[p])) { // checks if enemy is present
                    foreach(Vector3 move in piecePrefabs[diffCurrentPositionsType[diffCurrentPositions.IndexOf(FM_newPosition[i] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                        if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check is enemy is able to attack piece
                            foundEnemy = true;
                        }
                    }
                }
            }

            // checking if move is bad
            if (foundEnemy == true && foundAlly == false) {
                if (diffCurrentPositions.Contains(FM_newPosition[i])) {
                    moveCheckedAndOk = true;
                } else {
                    moveCheckedAndOk = false;
                }
            }

            // checking for free pieces
            if (diffCurrentPositions.Contains(FM_newPosition[i])) {
                for (int p = 0; p < surroundingTiles.Length; p++) { // looping every surrounding tile
                    if (diffCurrentPositions.Contains(FM_newPosition[i] + surroundingTiles[p])) {
                        foreach(Vector3 move in piecePrefabs[diffCurrentPositionsType[diffCurrentPositions.IndexOf(FM_newPosition[i] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                            if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check is enemy is able to attack piece
                                freePiece = false;
                            }
                        }
                    }
                }
                if (freePiece == true) {
                    freePieceMove.Add(i);
                }
            }

            // something
            if (moveCheckedAndOk == true) {
                // creating lists
                List<Vector3> diffPositions = new List<Vector3>();
                List<int> diffPositionsType = new List<int>();
                diffPositions.AddRange(diffCurrentPositions);
                diffPositionsType.AddRange(diffCurrentPositionsType);
                List<Vector3> Positions = new List<Vector3>();
                List<int> PositionsType = new List<int>();
                Positions.AddRange(currentPositions);
                PositionsType.AddRange(currentPositionsType);

                // check if enemy piece is captured
                if (diffCurrentPositions.Contains(FM_newPosition[i])) { 
                    diffPositions.Remove(FM_newPosition[i]); // removes enemy from list
                    diffPositionsType.Remove(FM_type[i]); // removes enemy type from list
                }
                
                // moving piece 
                PositionsType.RemoveAt(Positions.IndexOf(FM_oldPosition[i]));
                PositionsType.Add(FM_type[i]);
                Positions.Remove(FM_oldPosition[i]);
                Positions.Add(FM_newPosition[i]);

                // adding to the list
                boardLayout.Add(Positions); // adds positions to list
                boardType.Add(PositionsType); // adds type of pieces to list
                boardLayoutDiff.Add(diffPositions); // adds enemy positions to list
                boardTypeDiff.Add(diffPositionsType); // adds enemy types to list
                firstMove.Add(i); // adds first move to list
            } else {
                BadMoves.Add(i);
            }

            moveCheckedAndOk = true;
            foundEnemy = false;
            foundAlly = false;
            freePiece = true;
        }  

        // looping until depth reached
        if (freePieceMove.Count == 0) {
            for (int currentDepth = 1; currentDepth < Depth; currentDepth++) {
                if (currentDepth%2 == 0) { // AI's turn
                    for (int i = 0; i < boardLayout.Count; i++) { // loops through every board configuration
                        getPossibleMoves(boardLayout[i], boardType[i], currentDepth);
                        GetBoardPositions(true, R_oldPos, R_type, R_newPos, boardLayout[i], boardType[i], boardLayoutDiff[i], boardTypeDiff[i], firstMove[i]);
                    }
                } else { // other player's turn
                    for (int i = 0; i < boardLayout.Count; i++) { // loops through every board configuration
                        getPossibleMoves(boardLayoutDiff[i], boardTypeDiff[i], currentDepth);
                        GetBoardPositions(false, R_oldPos, R_type, R_newPos, boardLayoutDiff[i], boardTypeDiff[i], boardLayout[i], boardType[i], firstMove[i]);
                    }                
                }

                // sorting lists
                boardLayout.Clear();
                boardLayoutDiff.Clear();
                boardType.Clear();
                boardTypeDiff.Clear();
                firstMove.Clear();
                boardLayout.AddRange(boardLayoutBuff);
                boardLayoutDiff.AddRange(boardLayoutBuffDiff);
                boardType.AddRange(boardTypeBuff);
                boardTypeDiff.AddRange(boardTypeBuffDiff);
                firstMove.AddRange(firstMoveBuff);
                boardLayoutBuff.Clear();
                boardLayoutBuffDiff.Clear();
                boardTypeBuff.Clear();
                boardTypeBuffDiff.Clear();
                firstMoveBuff.Clear();
            }   

            // calculation
            if (gameObject.GetComponent<Selection>().turn == 1) { // Light's turn
                for (int i = 0; i < boardLayout.Count; i++) {

                    // Value of pieces //? doesn't work
                    for (int k = 0; k < boardLayout[i].Count; k++) {
                        switch(boardType[i][k]) {
                            case 0: // Zeta Light
                                totalPieceValue += (ValueZeta + ValueZeta * ZetaMC * ((boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 1: // Eta Light
                                totalPieceValue += (ValueEta + ValueEta * EtaMC * ((boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 2: // Theta Light
                                totalPieceValue += (ValueTheta + ValueTheta * ThetaMC * ((boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                        }
                    }

                    // value of opponent pieces
                    for (int k = 0; k < boardLayoutDiff[i].Count; k++) {
                        switch(boardTypeDiff[i][k]) {
                            case 3: // Zeta Dark
                                diffTotalPieceValue += (ValueZeta + ValueZeta * ZetaMC * ((-boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 4: // Eta Dark
                                diffTotalPieceValue += (ValueEta + ValueEta * EtaMC * ((-boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 5: // Theta Dark
                                diffTotalPieceValue += (ValueTheta + ValueTheta * ThetaMC * ((-boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                        }
                    }

                    // position on board
                    foreach(Vector3 piece in boardLayout[i]) { averageBoardPosition += piece.y; } // getting sum of all //? invert for light
                    averageBoardPosition = averageBoardPosition / boardLayout[i].Count;  // getting the average

                    // position of opponent
                    foreach(Vector3 piece in boardLayoutDiff[i]) { diffAverageBoardPosition += -piece.y; } // getting sum of all
                    diffAverageBoardPosition = diffAverageBoardPosition / boardLayout[i].Count;  // getting the average

                    // cover thingy
                    for (int k = 0; k < boardLayout[i].Count; k++) {
                        for (int p = 0; p < surroundingTiles.Length; p++) { // looping every surrounding tile
                            if (boardLayout[i].Contains(boardLayout[i][k] + surroundingTiles[p])) { // check if piece is surrounded by other piece
                                foreach(Vector3 move in piecePrefabs[boardType[i][boardLayout[i].IndexOf(boardLayout[i][k] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                                    if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check if possible move is piece
                                        covered++;
                                    }
                                }
                            }
                        }
                    }

                    // calculating total
                    totalBoardValue.Add((((totalPieceValue * W_pieceValue) - (diffTotalPieceValue * W_diffPieceValue)) * W_Values) + (((averageBoardPosition * W_averagePosition) - (diffAverageBoardPosition * W_diffAveragePosition)) * W_Positions) + (covered * W_coverage));

                    // resetting pieces
                    totalPieceValue = 0;
                    diffTotalPieceValue = 0;
                    averageBoardPosition = 0;
                    diffAverageBoardPosition = 0;
                    covered = 0;
                }
            } else {
                for (int i = 0; i < boardLayout.Count; i++) {

                    // Value of pieces //? doesn't work
                    for (int k = 0; k < boardLayout[i].Count; k++) {
                        switch(boardType[i][k]) {
                            case 0: // Zeta Light
                                totalPieceValue += (ValueZeta + ValueZeta * ZetaMC * ((-boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 1: // Eta Light
                                totalPieceValue += (ValueEta + ValueEta * EtaMC * ((-boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 2: // Theta Light
                                totalPieceValue += (ValueTheta + ValueTheta * ThetaMC * ((-boardLayout[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                        }
                    }

                    // value of opponent pieces
                    for (int k = 0; k < boardLayoutDiff[i].Count; k++) {
                        switch(boardTypeDiff[i][k]) {
                            case 3: // Zeta Dark
                                diffTotalPieceValue += (ValueZeta + ValueZeta * ZetaMC * ((boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 4: // Eta Dark
                                diffTotalPieceValue += (ValueEta + ValueEta * EtaMC * ((boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                            case 5: // Theta Dark
                                diffTotalPieceValue += (ValueTheta + ValueTheta * ThetaMC * ((boardLayoutDiff[i][k].y + 35) / 10)); // multiplying the base value of the piece with a constant per tile moved
                                break;
                        }
                    }

                    // position on board
                    foreach(Vector3 piece in boardLayout[i]) { averageBoardPosition += -piece.y; } // getting sum of all //? invert for light
                    averageBoardPosition = averageBoardPosition / boardLayout[i].Count;  // getting the average

                    // position of opponent
                    foreach(Vector3 piece in boardLayoutDiff[i]) { diffAverageBoardPosition += piece.y; } // getting sum of all
                    diffAverageBoardPosition = diffAverageBoardPosition / boardLayout[i].Count;  // getting the average

                    // cover thingy
                    for (int k = 0; k < boardLayout[i].Count; k++) {
                        for (int p = 0; p < surroundingTiles.Length; p++) { // looping every surrounding tile
                            if (boardLayout[i].Contains(boardLayout[i][k] + surroundingTiles[p])) { // check if piece is surrounded by other piece
                                foreach(Vector3 move in piecePrefabs[boardType[i][boardLayout[i].IndexOf(boardLayout[i][k] + surroundingTiles[p])]].GetComponent<Piece>().pieceSO.MoveLocations) { // gather all possible moves for surrounding piece
                                    if (surroundingTiles[p].x == -move.x && surroundingTiles[p].y == -move.y) { // check if possible move is piece
                                        covered++;
                                    }
                                }
                            }
                        }
                    }

                    // calculating total
                    totalBoardValue.Add((((totalPieceValue * W_pieceValue) - (diffTotalPieceValue * W_diffPieceValue)) * W_Values) + (((averageBoardPosition * W_averagePosition) - (diffAverageBoardPosition * W_diffAveragePosition)) * W_Positions) + (covered * W_coverage));

                    // resetting pieces
                    totalPieceValue = 0;
                    diffTotalPieceValue = 0;
                    averageBoardPosition = 0;
                    diffAverageBoardPosition = 0;
                    covered = 0;
                }
            }

            // calculating 
            for (int i = 0; i < totalBoardValue.Count; i++) {
                if (totalBoardValue[i] > bestScore[0]) {
                    bestScore.Clear();
                    bestScoreIndex.Clear();
                    bestScore.Add(totalBoardValue[i]);
                    bestScoreIndex.Add(i);
                } else if(totalBoardValue[i] == bestScore[0]) {
                    bestScore.Add(totalBoardValue[i]);
                    bestScoreIndex.Add(i);
                }
            }

            int choice;

            // check if any good moves are even found
            if (firstMove.Count > 0) { // yes
                choice = firstMove[bestScoreIndex[Random.Range(0, bestScoreIndex.Count)]]; // pick a good move
            } else { // no
                choice = BadMoves[bestScoreIndex[Random.Range(0, BadMoves.Count)]]; // pick a bad move
            }
            
            if (gameObject.GetComponent<Selection>().turn == 1) { // Light's turn
                foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Light")) {
                    if (piece.transform.position == FM_oldPosition[choice]) {
                        piece.transform.Translate(FM_newPosition[choice] - piece.transform.position);
                        foreach (GameObject diffPiece in GameObject.FindGameObjectsWithTag("Piece Dark")) {
                            if (diffPiece.transform.position == piece.transform.position) {
                                switch(diffPiece.gameObject.GetComponent<Piece>().type) {
                                    case 3:
                                    Debug.Log("0");
                                        gameObject.GetComponent<Selection>().darkZeta++;
                                        break;
                                    case 4:
                                    Debug.Log("1");
                                        gameObject.GetComponent<Selection>().darkEta++;
                                        break;
                                    case 5:
                                    Debug.Log("2");
                                        gameObject.GetComponent<Selection>().darkTheta++;
                                        break;
                                }
                                gameObject.GetComponent<Selection>().totalLight -= 1;
                                gameObject.GetComponent<Selection>().UpdateCaptureTexts();
                                GameObject.Destroy(diffPiece);
                                gameObject.GetComponent<Selection>().somethingSelected = false;
                            }
                        }
                    }
                }
            } else {
                foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Dark")) {
                    if (piece.transform.position == FM_oldPosition[choice]) {
                        piece.transform.Translate(FM_newPosition[choice] - piece.transform.position);
                        foreach (GameObject diffPiece in GameObject.FindGameObjectsWithTag("Piece Light")) {
                            if (diffPiece.transform.position == piece.transform.position) {
                                switch(diffPiece.gameObject.GetComponent<Piece>().type) {
                                    case 0:
                                    Debug.Log("3");
                                        gameObject.GetComponent<Selection>().lightZeta++;
                                        break;
                                    case 1:
                                    Debug.Log("4");
                                        gameObject.GetComponent<Selection>().lightEta++;
                                        break;
                                    case 2:
                                    Debug.Log("5");
                                        gameObject.GetComponent<Selection>().lightTheta++;
                                        break;
                                }
                                gameObject.GetComponent<Selection>().totalDark -= 1;
                                gameObject.GetComponent<Selection>().UpdateCaptureTexts();
                                GameObject.Destroy(diffPiece);
                                gameObject.GetComponent<Selection>().somethingSelected = false;
                            }
                        }
                    }
                }
            }
        } else { // free piece can be taken
            if (freePieceMove[0] != -1) {
                if (gameObject.GetComponent<Selection>().turn == 1) { // Light's turn
                    foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Light")) {
                        if (piece.transform.position == FM_oldPosition[freePieceMove[0]]) {
                            piece.transform.Translate(FM_newPosition[freePieceMove[0]] - piece.transform.position);
                            foreach (GameObject diffPiece in GameObject.FindGameObjectsWithTag("Piece Dark")) {
                                if (diffPiece.transform.position == piece.transform.position) {
                                    switch(diffPiece.gameObject.GetComponent<Piece>().type) {
                                        case 3:
                                            gameObject.GetComponent<Selection>().darkZeta++;
                                            break;
                                        case 4:
                                            gameObject.GetComponent<Selection>().darkEta++;
                                            break;
                                        case 5:
                                            gameObject.GetComponent<Selection>().darkTheta++;
                                            break;
                                    }
                                    gameObject.GetComponent<Selection>().totalLight -= 1;
                                    gameObject.GetComponent<Selection>().UpdateCaptureTexts();
                                    GameObject.Destroy(diffPiece);
                                    gameObject.GetComponent<Selection>().somethingSelected = false;
                                }
                            }
                        }
                    }
                } else {
                    foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece Dark")) {
                        if (piece.transform.position == FM_oldPosition[freePieceMove[0]]) {
                            piece.transform.Translate(FM_newPosition[freePieceMove[0]] - piece.transform.position);
                            foreach (GameObject diffPiece in GameObject.FindGameObjectsWithTag("Piece Light")) {
                                if (diffPiece.transform.position == piece.transform.position) {
                                    switch(diffPiece.gameObject.GetComponent<Piece>().type) {
                                        case 0:
                                            gameObject.GetComponent<Selection>().lightZeta++;
                                            break;
                                        case 1:
                                            gameObject.GetComponent<Selection>().lightEta++;
                                            break;
                                        case 2:
                                            gameObject.GetComponent<Selection>().lightTheta++;
                                            break;
                                    }
                                    gameObject.GetComponent<Selection>().totalDark -= 1;
                                    gameObject.GetComponent<Selection>().UpdateCaptureTexts();
                                    GameObject.Destroy(diffPiece);
                                    gameObject.GetComponent<Selection>().somethingSelected = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        // changing turn back to player
        gameObject.GetComponent<Selection>().ChangeTurn();
    }
}