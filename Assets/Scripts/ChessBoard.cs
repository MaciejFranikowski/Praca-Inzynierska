using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine.UI;

public enum SpecialMove { 
    None = 0,
    EnPassant,
    Castle,
    Promotion
}

public class ChessBoard : MonoBehaviour
{
    [Header("Materia³y")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffest = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;
    [SerializeField] private float deadPieceSize = 0.5f;
    [SerializeField] private float deadPieceSpacing = 0.55f;
    [SerializeField] private float deadPieceAboveBoard = 0.05f;
    [SerializeField] private float dragOffset = 1.6f;

    [Header("Prefaby")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logika
    private Piece[,] chessPieces;
    private Piece currentlyHolding;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Piece> beatenWhitePieces = new List<Piece>();
    private List<Piece> beatenBlackPieces = new List<Piece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHoveredTile;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    // Multi logika
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool localGame = true;
    private bool[] playerRematch = new bool[2];

    private void Awake()
    {
        isWhiteTurn = true;
        TileGeneration(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        CreatePieces();
        SetAllPieces();

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera) {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Indeksy tam gdzie dosz³o do 'zderzenia'
            Vector2Int hitPos = GetTileIndex(info.transform.gameObject);

            // Nic podœwietlone albo pierwszy hover
            if (currentHoveredTile == -Vector2Int.one)
            {
                currentHoveredTile = hitPos;
                tiles[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
            }

            // Coœ ju¿ podœwietlone
            if (currentHoveredTile != hitPos)
            {
                // Przywracanie starej warstwy
                tiles[currentHoveredTile.x, currentHoveredTile.y].layer = (ContainsCorrectMove(ref availableMoves, currentHoveredTile)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHoveredTile = hitPos;
                // Zmiana warstwy podœwietlonego
                tiles[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");

            }
        
            // LMB w dó³
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPos.x, hitPos.y] != null)
                {
                    // Sprawdzenie czyja akutalnie tura
                    if ((chessPieces[hitPos.x, hitPos.y].team == 0 && isWhiteTurn && currentTeam == 0) || (chessPieces[hitPos.x, hitPos.y].team == 1 && !isWhiteTurn && currentTeam == 1))
                    {
                        currentlyHolding = chessPieces[hitPos.x, hitPos.y];
                        // Sprawdzenie dostepnych ruchów i podœwietlenie
                        availableMoves = currentlyHolding.GetPossibleMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        // Ruchy specjalnie
                        // Referencja po to ¿eby wci¹æ albo modyfikowaæ od razu
                        specialMove = currentlyHolding.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
                        // Wy³¹czenie ruchów które zakoñczy³yby rozgrywkrê gracza samemu robi¹c szach mat dla siebie
                        PreventCheck();
                        HighlightTiles();
                    }
                }
            }
            // LMB w górê
            if (currentlyHolding != null && Input.GetMouseButtonUp(0))
            {

                Vector2Int prevPosition = new Vector2Int(currentlyHolding.currentX, currentlyHolding.currentY);

                if (ContainsCorrectMove(ref availableMoves, new Vector2Int(hitPos.x, hitPos.y)))
                {
                    // wykonananie ruchu
                    MoveTo(prevPosition.x, prevPosition.y, hitPos.x, hitPos.y);

                    // Wys³anie info do serwera o ruchu
                    NetMakeMove mm = new NetMakeMove();
                    mm.originalX = prevPosition.x;
                    mm.originalY = prevPosition.y;
                    mm.destinationX = hitPos.x;
                    mm.destinationY = hitPos.y;
                    mm.teamId = currentTeam;
                    Client.Instance.SendToServer(mm);

                }
                else {
                    currentlyHolding.SetPosition(GetTileCenter(prevPosition.x, prevPosition.y));
                    currentlyHolding = null;
                    RemoveHighlightTiles();
                }
                
            }
        }
        else
        {
            // Jeœli ray poza szachownic¹
            if (currentHoveredTile != -Vector2Int.one)
            {
                // Cofniêcie wartsw
                tiles[currentHoveredTile.x, currentHoveredTile.y].layer = (ContainsCorrectMove(ref availableMoves, currentHoveredTile))?LayerMask.NameToLayer("Highlight"): LayerMask.NameToLayer("Tile");
                currentHoveredTile = -Vector2Int.one;
            }

            if (currentlyHolding && Input.GetMouseButtonUp(0)) {
                // cofniêcie bierki
                currentlyHolding.SetPosition(GetTileCenter(currentlyHolding.currentX, currentlyHolding.currentY));
                currentlyHolding = null;
                RemoveHighlightTiles();
            }
        }

        // Poruszanie siê bierki
        if (currentlyHolding) {
            // Tworzenie p³aszczyzny
            Plane horPlane = new Plane(Vector3.up, Vector3.up * yOffest);
            float distance = 0.0f;
            // Rzucenie promienie i otrzymanie dystansu od kamery
            if (horPlane.Raycast(ray, out distance)) {
                // Zmiania polozenie za podstawie dystansu
                currentlyHolding.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    // Generacja szachownicy
    private void TileGeneration(float tileSize, int tileCountX, int tileCountY)
    {
        yOffest += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffest, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffest, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffest, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffest, (y + 1) * tileSize) - bounds;

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Tworzenie bierek
    private void CreatePieces() {
        chessPieces = new Piece[TILE_COUNT_X, TILE_COUNT_Y];
        int white = 0;
        int black = 1;

        // bia³e
        chessPieces[0, 0] = CreateSinglePiece(ChessPieceType.Rook, white);
        chessPieces[1, 0] = CreateSinglePiece(ChessPieceType.Knight, white);
        chessPieces[2, 0] = CreateSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[3, 0] = CreateSinglePiece(ChessPieceType.Queen, white);
        chessPieces[4, 0] = CreateSinglePiece(ChessPieceType.King, white);
        chessPieces[5, 0] = CreateSinglePiece(ChessPieceType.Bishop, white);
        chessPieces[6, 0] = CreateSinglePiece(ChessPieceType.Knight, white);
        chessPieces[7, 0] = CreateSinglePiece(ChessPieceType.Rook, white);

        
        chessPieces[0, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[1, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[2, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[3, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[4, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[5, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[6, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);
        chessPieces[7, 1] = CreateSinglePiece(ChessPieceType.Pawn, white);

        // czarne
        chessPieces[0, 7] = CreateSinglePiece(ChessPieceType.Rook, black);
        chessPieces[1, 7] = CreateSinglePiece(ChessPieceType.Knight, black);
        chessPieces[2, 7] = CreateSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[3, 7] = CreateSinglePiece(ChessPieceType.Queen, black);
        chessPieces[4, 7] = CreateSinglePiece(ChessPieceType.King, black);
        chessPieces[5, 7] = CreateSinglePiece(ChessPieceType.Bishop, black);
        chessPieces[6, 7] = CreateSinglePiece(ChessPieceType.Knight, black);
        chessPieces[7, 7] = CreateSinglePiece(ChessPieceType.Rook, black);

        chessPieces[0, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[1, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[2, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[3, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[4, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[5, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[6, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);
        chessPieces[7, 6] = CreateSinglePiece(ChessPieceType.Pawn, black);

    }
    private Piece CreateSinglePiece(ChessPieceType type, int team) {
        // Tworzenie piona uzywajac instantiate a potem wziêcie komponentu piece w celu ustalenia mu wartosci
        Piece chessPiece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Piece>();
        chessPiece.type = type;
        chessPiece.team = team;
        // Odpowniedni material (odpowiada materialom dodanym w UI, kolejnosc wazna bo typy)
        chessPiece.GetComponent<MeshRenderer>().material = teamMaterials[((team==0)? 0 : 6 ) + ((int)type -1)];
        return chessPiece;
    }

    // Pozycjonowanie bierek
    private void SetAllPieces() {
        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++ ){
                // Sprawdzenie czy jest na tej pocyzji bierka
                if (chessPieces[x, y] != null) {
                    SetSinglePiece(x, y, true);
                }
            }
        } 
    }
    private void SetSinglePiece(int x, int y, bool force = false) {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y) { 
        return new Vector3(x * tileSize, yOffest, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Podswietlanie
    private void HighlightTiles() {
        for (int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

    // Logika konca gry
    private void CheckMate(int team) {
        DisplayEndScreen(team);
    }
    private void DisplayEndScreen(int team){
        endScreen.SetActive(true);
        // W zaleznosci od wygranej druzyny (aktualnej), wlaczony jest tekst ogladaszajacy zwycieztwo odpowiedniej druzyny
        endScreen.transform.GetChild(team).gameObject.SetActive(true);
    }
    public void OnRematchButton() {
        if (localGame)
        {
            // Wiadomosci o rematchu
            NetRematch wrm = new NetRematch();
            wrm.teamId = 0;
            wrm.wantRematch = 1;
            Client.Instance.SendToServer(wrm);

            NetRematch brm = new NetRematch();
            brm.teamId = 1;
            brm.wantRematch = 1;
            Client.Instance.SendToServer(brm);
        } else
        {
            NetRematch rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 1;
            Client.Instance.SendToServer(rm);
        }
        
    }
    public void gameRestart() {
        // Naprawa UI
        rematchButton.interactable = true;

        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        endScreen.transform.GetChild(0).gameObject.SetActive(false);
        endScreen.transform.GetChild(1).gameObject.SetActive(false);
        endScreen.SetActive(false);

        // Reset odpowiednich pol
        currentlyHolding = null;
        availableMoves.Clear();
        moveList.Clear();
        playerRematch[0] = playerRematch[1] = false;

        // Usuniêcie bierek
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }
                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < beatenWhitePieces.Count; i++)
        {
            Destroy(beatenWhitePieces[i].gameObject);
        }

        for (int i = 0; i < beatenBlackPieces.Count; i++)
        {
            Destroy(beatenBlackPieces[i].gameObject);
        }
        beatenWhitePieces.Clear();
        beatenBlackPieces.Clear();

        // Stworzenie nowych bierek
        CreatePieces();
        SetAllPieces();
        // Ustawienie tury
        isWhiteTurn = true;
    }
    public void OnMenuButton(){
        NetRematch rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instance.SendToServer(rm);

        gameRestart();
        GameUI.Instance.OnLeaveFromGameMenu();

        Invoke("ShodownRelay", 1.0f);

        playerCount = -1;
        currentTeam = -1;
    }

    // Dodatkowa obs³uga ruchów specjalnych
    private void ProcessSpecialMove() {
        // Usuwanie odpowiednich bierek
        // En Passant
        if (specialMove == SpecialMove.EnPassant) {
            var newMove = moveList[moveList.Count - 1];
            Piece ourPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            Piece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(ourPawn.currentX == enemyPawn.currentX){
                if(ourPawn.currentY == enemyPawn.currentY - 1 || ourPawn.currentY == enemyPawn.currentY + 1){
                    if (enemyPawn.team == 0)
                    {
                        beatenWhitePieces.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadPieceSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffest, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, deadPieceAboveBoard, tileSize / 2)
                            + (Vector3.back * -deadPieceSpacing) * beatenWhitePieces.Count);
                    }
                    else {
                        beatenBlackPieces.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadPieceSize);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffest, 8 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, deadPieceAboveBoard, tileSize / 2)
                            + (Vector3.forward * -deadPieceSpacing) * beatenBlackPieces.Count);
                    }
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }

            }
        }
        // Promocja
        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            Piece pawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if(pawn.type == ChessPieceType.Pawn)
            {
                if(pawn.team == 0 && lastMove[1].y == 7)
                {
                    Piece newQueen = CreateSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    SetSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                if (pawn.team == 1 && lastMove[1].y == 0)
                {
                    Piece newQueen = CreateSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    SetSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }
        // Roszada
        if (specialMove == SpecialMove.Castle){
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            // Lewa wie¿a
            if(lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0)
                {
                    Piece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    SetSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    Piece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    SetSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0)
                {
                    Piece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    SetSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    Piece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    SetSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck() {
        // Zapobieganie zakoñczeniu przez gracza gry swoj¹ przegran¹
        Piece targetKing = null;
        // Zanlezenie króla akutalnej dru¿yny
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(chessPieces[x,y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyHolding.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }
        // Usuniêcie ruchów z dostêpnych, która zakoñczy³y by grê
        SimulateMoveForSinglePiece(currentlyHolding, ref availableMoves, targetKing);
    }
    private void SimulateMoveForSinglePiece(Piece currentPiece, ref List<Vector2Int> moves, Piece targetKing){
        int currentX = currentPiece.currentX;
        int currentY = currentPiece.currentY;
        List<Vector2Int> wrongMoves = new List<Vector2Int>();

        // Dla ka¿dego ruchu sprawdzamy czy jest szach
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPos = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Czy symulowana bierka to król
            if(currentPiece.type == ChessPieceType.King)
            {
                // Jeœli tak, to poz króla musi siê zmieniaæ wraz z symulowan¹
                kingPos = new Vector2Int(simX, simY);
            }
            Piece[,] simBoard = new Piece[TILE_COUNT_X, TILE_COUNT_Y];
            // Lista wrogich bierek 
            List<Piece> simEnemyPieces = new List<Piece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if(chessPieces[x,y] != null)
                    {
                        simBoard[x, y] = chessPieces[x, y];
                        if(simBoard[x,y].team != currentPiece.team)
                        {
                            simEnemyPieces.Add(simBoard[x, y]);
                        }
                    }
                }
            }

            // Symulacja ruchu, przeniesienie bierki na pozycjê ruchu
            simBoard[currentX, currentY] = null;
            currentPiece.currentX = simX;
            currentPiece.currentY = simY;
            simBoard[simX, simY] = currentPiece;

            // Czy zabita zosta³a bierka podczas tego ruchu
            var deadPiece = simEnemyPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null) {
                simEnemyPieces.Remove(deadPiece);
            }

            // Sprawdzenie ruchów wrogich bierek ¿eby zobaczyæ czy jest szach
            List<Vector2Int> simAttackingMoves = new List<Vector2Int>();
            for (int j = 0; j < simEnemyPieces.Count; j++)
            {
                var attackingMoves = simEnemyPieces[j].GetPossibleMoves(ref simBoard, TILE_COUNT_X, TILE_COUNT_Y);
                for (int a = 0; a < attackingMoves.Count; a++)
                {
                    simAttackingMoves.Add(attackingMoves[a]);
                }
            }

            // Jeœli ruch koñcz¹cy siê pozycj¹ króla jest w tych ruchach, ususwamy odpowiedni ruch z listy dostpenych
            if(ContainsCorrectMove(ref simAttackingMoves, kingPos))
            {
                wrongMoves.Add(moves[i]);
            }

            currentPiece.currentX = currentX;
            currentPiece.currentY = currentY;

        }

        for (int i = 0; i < wrongMoves.Count; i++)
        {
            moves.Remove(wrongMoves[i]);

        }

    }
    private bool CheckForCheckmate(){
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        // Wyznaczenie odpowiednich zmiennych, takich jak król etc
        Piece targetKing = null;
        List<Piece> attackingPieces = new List<Piece>();
        List<Piece> defendingPieces = new List<Piece>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if(chessPieces[x,y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        // Czy król jest zagro¿ony
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var attackingMoves = attackingPieces[i].GetPossibleMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int a = 0; a < attackingMoves.Count; a++)
            {
                currentAvailableMoves.Add(attackingMoves[a]);
            }
        }

        // Szach, sprawdzany jest szach mat
        if(ContainsCorrectMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            //Czy mo¿na wykonaæ ruch ¿eby pomóc królowi
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                // Dostêpne ruchy dla broni¹cych siê bierek
                List<Vector2Int> defendingMoves = defendingPieces[i].GetPossibleMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                // Symulacja ruchów
                // Usuwane ruchy które nie pomog¹ królowi
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                // Jeœli jest jakikolwiek to nie ma szach mat
                if(defendingMoves.Count != 0)
                {
                    return false;
                }
            }

            // Szach mat bo nie ma zadnych ruchów broni¹cych króla
            return true;
        }

        return false;
    }
    

    private bool ContainsCorrectMove(ref List<Vector2Int> moves, Vector2Int pos) {
        for (int i = 0; i < moves.Count; i++) {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    private Vector2Int GetTileIndex(GameObject hitInfo) {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo) {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; // <-1, -1>
    }
    private void MoveTo(int originalX, int originalY,  int x, int y)
    {
        // Poruszanie bierek
        Piece ch = chessPieces[originalX, originalY];
        Vector2Int prevPosition = new Vector2Int(ch.currentX, ch.currentY);
        if (chessPieces[x, y] != null) {
            Piece otherCp = chessPieces[x, y];
            // Jeœli próbujemy polozyc bierke nasza na nasz¹
            if (otherCp.team == ch.team) {
                return;
            }
            // Jak na przeciwn¹ dru¿ynê to zbijamy
            if (otherCp.team == 0) {
                if (otherCp.type == ChessPieceType.King) {
                    CheckMate(1);
                }
                beatenWhitePieces.Add(otherCp);
                otherCp.SetScale(Vector3.one * deadPieceSize);
                otherCp.SetPosition(new Vector3(8 * tileSize, yOffest, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize/2, deadPieceAboveBoard, tileSize/2)
                    + (Vector3.back * -deadPieceSpacing) * beatenWhitePieces.Count);
            }
            else {
                if (otherCp.type == ChessPieceType.King){
                    CheckMate(0);
                }
                beatenBlackPieces.Add(otherCp);
                otherCp.SetScale(Vector3.one * deadPieceSize);
                otherCp.SetPosition(new Vector3(-1 * tileSize, yOffest, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, deadPieceAboveBoard, tileSize / 2)
                    + (Vector3.forward * -deadPieceSpacing) * beatenBlackPieces.Count);
            }

        }
        chessPieces[x, y] = ch;
        chessPieces[prevPosition.x, prevPosition.y] = null;

        SetSinglePiece(x, y);
        isWhiteTurn = !isWhiteTurn;
        if (localGame)
        {
            currentTeam = (currentTeam == 0) ? 1 : 0;
        }
        moveList.Add(new Vector2Int[] { prevPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();
        if (currentlyHolding)
        {
            currentlyHolding = null;
        }
        RemoveHighlightTiles();
        
        if (CheckForCheckmate())
        {
            CheckMate(ch.team);
        }

        return;
    }
    // Wydarzenia
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;


        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }
    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        // Klient siê po³¹czyæ, przypisaæ mu team i wyslaæ mu wiadomoœæ
        NetWelcome nw = msg as NetWelcome;
        // Przypisanie dur¿yny
        nw.AssignedTeam = ++playerCount;
        // Zwrócenie do klienta
        Server.Instance.SendToClient(cnn, msg);
        
        // Liczymy od -1 wiêc gra siê zaczyna od 1
        if(playerCount == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }
    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        NetMakeMove mm = msg as NetMakeMove;
        Server.Instance.Broadcast(msg);
    }
    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.Broadcast(msg);
    }
    // klient
    private void OnWelcomeClient(NetMessage msg)
    {
        NetWelcome nw = msg as NetWelcome;
        // Przypisanie dru¿ny
        currentTeam = nw.AssignedTeam;
        Debug.Log($"Moja przypisana druzyna to {nw.AssignedTeam}");
        if(localGame && currentTeam == 0)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
        
    }
    private void OnStartGameClient(NetMessage msg)
    {
        GameUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.white : CameraAngle.black);
    }
    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;
        if(mm.teamId != currentTeam)
        {
            Piece target = chessPieces[mm.originalX, mm.originalY];
            availableMoves = target.GetPossibleMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            specialMove = target.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

            MoveTo(mm.originalX, mm.originalY, mm.destinationX, mm.destinationY);

        }

    }
    private void OnRematchClient(NetMessage msg)
    {
        NetRematch rm = msg as NetRematch;
        playerRematch[rm.teamId] = rm.wantRematch == 1;

        if (rm.teamId != currentTeam)
        {
            rematchIndicator.transform.GetChild((rm.wantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            if(rm.wantRematch != 1)
            {
                rematchButton.interactable = false;
            }
        }
            

        if (playerRematch[0] && playerRematch[1])
        {
            gameRestart();
        }
    }

    private void OnSetLocalGame(bool value)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = value;
    }
    private void ShutDownRelay()
    {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
    }

}
