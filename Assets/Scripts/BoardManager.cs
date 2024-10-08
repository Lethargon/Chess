using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct Coords
{
    public override bool Equals(object obj)
    {
        if (!(obj is Coords))
            return false;
        else
        {
            Coords c = (Coords)obj;
            return (c.x == x && c.y == y);
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public int DistSquared(Coords other)
    {
        return Mathf.Abs(x - other.x) ^ 2 + Mathf.Abs(y - other.y) ^ 2;
    }

    public int x;
    public int y;

    public string ToText()
    {
        char column = (char)(x + 65);
        string row = "" + (y + 1);
        return column + row;
    }
}
public enum Color
{
    White, Black
}

public struct LogEntry
{
    public Coords from;
    public Coords target;
    public Coords to;
    public string text;

    public void GenerateText()
    {
        text = "Moved piece from " + from.ToText() + " to " + to.ToText() + ", targeting " + target.ToText();
    }
}

public class BoardManager : MonoBehaviour
{
    [Serializable]
    private struct Column
    {
        [SerializeField] public string name;
        [SerializeField] public GameObject[] tiles;
    }

    private enum PieceType
    {
        Pawn, Rook, Knight, Bishop, Queen, King
    }

    [SerializeField] private Column[] board;

    [SerializeField] private GameObject[] whitePieces;
    [SerializeField] private PieceType[] whiteIdentities;

    [SerializeField] private GameObject[] blackPieces;
    [SerializeField] private PieceType[] blackIdentities;

    [SerializeField] private Sprite[] whiteSprites;
    [SerializeField] private Sprite[] blackSprites;

    [SerializeField] private Color32 tileHighlight;

    private GameObject chosenPiece;

    private Color colorToPlay = Color.White;

    private int[] knightMovesX = { 1, 1, -1, -1, 2, 2, -2, -2};
    private int[] knightMovesY = { 2, -2, 2, -2, 1, -1, 1, -1};

    private float lerpDuration = 0.1f;

    private int whiteCaptures = 0;
    private int blackCaptures = 0;

    private bool whiteInCheck = false;
    private bool blackInCheck = false;
    private bool gameOver = false;

    private Piece passantable = null;
    private bool doublestepLastTurn = false;

    public List<LogEntry> log = new List<LogEntry>();

    private AudioSource audioSource;
    private string clipToPlay = "";

    private Tile previousTile = null;

    
    void Start()
    {
        gameOver = false;
        //randomizes the piece types
        for (int i = 0; i < 16; i++)
        {
            PieceType temp = whiteIdentities[i];
            int r = UnityEngine.Random.Range(i, 15);
            whiteIdentities[i] = whiteIdentities[r];
            whiteIdentities[r] = temp;

            temp = blackIdentities[i];
            r = UnityEngine.Random.Range(i, 15);
            blackIdentities[i] = blackIdentities[r];
            blackIdentities[r] = temp;

            whitePieces[i].GetComponent<Piece>().id = i;
            blackPieces[i].GetComponent<Piece>().id = i;
        }

        //hooks tiles to the board object for click handling
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                Tile t = board[i].tiles[j].GetComponent<Tile>();
                t.pos = new() { x = i, y = j };
                t.board = this;
                if (!t.empty) t.piece.GetComponent<Piece>().pos = t.pos;
            }
        }

        for(int i = 0; i < 16; i++)
        {
            if (UpdateLegalMoves(whitePieces[i].GetComponent<Piece>())) blackInCheck = true;
            if (UpdateLegalMoves(blackPieces[i].GetComponent<Piece>())) whiteInCheck = true;
        }

        UIController.Instance().SetCheckLights(whiteInCheck, blackInCheck);

        audioSource = GetComponent<AudioSource>();


    }

    public void TileClicked(Tile t)
    {
        if (chosenPiece == null)
        {
            if (!t.empty && colorToPlay == t.piece.GetComponent<Piece>().color)
            {
                chosenPiece = t.piece;
                t.SetColor(tileHighlight);
                previousTile = t;
            }
        }
        else if (chosenPiece.GetComponent<Piece>().pos.Equals(t.pos))
        {
            chosenPiece = null;
            t.ResetColor();
        }
        else if (!t.empty && colorToPlay == t.piece.GetComponent<Piece>().color)
        {
            chosenPiece= t.piece;
            t.SetColor(tileHighlight);
            previousTile.ResetColor();
            previousTile = t;
        }
        else
        {
            bool moved = AttemptMove(t.pos);
            if (moved)
            {
                clipToPlay = clipToPlay.Length < 1 ? "move" : clipToPlay;

                colorToPlay = colorToPlay == Color.White ? Color.Black : Color.White;
                if(!gameOver)UIController.Instance().SetGameInfo("" + colorToPlay.ToString() + "'s turn");
                blackInCheck = false;
                whiteInCheck = false;
                for (int i = 0; i < 16; i++)
                {
                    if (whitePieces[i].activeInHierarchy && UpdateLegalMoves(whitePieces[i].GetComponent<Piece>())) blackInCheck = true;
                    if (blackPieces[i].activeInHierarchy && UpdateLegalMoves(blackPieces[i].GetComponent<Piece>())) whiteInCheck = true;
                }
                if(!gameOver)UIController.Instance().SetCheckLights(whiteInCheck, blackInCheck);
                //Debug.Log("white in check: " + whiteInCheck);
                //Debug.Log("black in check: " + blackInCheck);

                if (log.Count > 1)
                {
                    GetTile(log[^2].to).ResetColor();
                    GetTile(log[^2].from).ResetColor();
                }
                GetTile(log[^1].to).SetColor(tileHighlight);
                if(blackInCheck || whiteInCheck)
                {
                    clipToPlay = "check";
                }
                
                audioSource.PlayOneShot((AudioClip)Resources.Load(clipToPlay));
                clipToPlay = "";
                
            }
            else
            {
                audioSource.PlayOneShot((AudioClip)Resources.Load("error_move"));
            }
        }
    }

    private bool AttemptMove(Coords target)
    {
        if (chosenPiece == null) return false;

        Piece p = chosenPiece.GetComponent<Piece>();
        UpdateLegalMoves(p);
        chosenPiece = null;

        if (p.legalMoves.Count == 0)
        {
            GetTile(p.pos).ResetColor();
            return false;
        }

        Coords destination = new Coords();

        if (p.legalMoves.Contains(target)) //if equals override doesn't affect contains this won't work
        {
            destination = target;
        }
        else
        {
            int minDistSq = 64;
            for(int i = 0; i < p.legalMoves.Count; i++)
            {
                int d = p.legalMoves[i].DistSquared(target);
                if ( d < minDistSq)
                {
                    minDistSq = d;
                    destination = p.legalMoves[i];
                }
            }
        }

        Tile origin = GetTile(p.pos);
        origin.empty = true;
        origin.piece = null;

        Tile end = GetTile(destination);
        if (!end.empty)
        {
            clipToPlay = "capture";
            // adds captured piece to display zone
            if (colorToPlay == Color.White)
            {
                UIController.Instance().root.Q<VisualElement>("WhiteCapture" + whiteCaptures).style.backgroundImage = new StyleBackground(blackSprites[(int)blackIdentities[end.piece.GetComponent<Piece>().id]]);
                whiteCaptures++;

                // checks if captured piece is king
                if(blackIdentities[end.piece.GetComponent<Piece>().id] == PieceType.King)
                {
                    WinGame(true);
                }
            }
            else
            {
                UIController.Instance().root.Q<VisualElement>("BlackCapture" + blackCaptures).style.backgroundImage = new StyleBackground(whiteSprites[(int)whiteIdentities[end.piece.GetComponent<Piece>().id]]);
                blackCaptures++;

                // checks if captured piece is king
                if (whiteIdentities[end.piece.GetComponent<Piece>().id] == PieceType.King)
                {
                    WinGame(false);
                }
            }
            end.piece.gameObject.SetActive(false);
            
        }
        end.piece = p.gameObject;
        end.empty = false;
        StartCoroutine(LerpMove(p, p.gameObject.transform.position, end.transform.position));
        p.pos = end.pos;
        p.unmoved = false;

        if (doublestepLastTurn)
        {
            doublestepLastTurn = false;
        }
        else
        {
            passantable = null;
        }

        if (p.color == Color.White)
        {
            if(whiteIdentities[p.id] == PieceType.Pawn)
            {
                if (p.pos.y == 7)
                {
                    whiteIdentities[p.id] = PieceType.Queen;
                    p.gameObject.GetComponent<SpriteRenderer>().sprite = whiteSprites[4];
                }
                if (passantable != null && passantable.pos.x == p.pos.x && passantable.pos.y + 1 == p.pos.y)
                {
                    UIController.Instance().root.Q<VisualElement>("WhiteCapture" + whiteCaptures).style.backgroundImage = new StyleBackground(blackSprites[(int)blackIdentities[passantable.id]]);
                    whiteCaptures++;
                    passantable.gameObject.SetActive(false);
                }
                passantable = null;
                if (origin.pos.y + 2 == end.pos.y)
                {
                    passantable = p;
                    doublestepLastTurn = true;
                }
            }
        }
        else
        {
            if (blackIdentities[p.id] == PieceType.Pawn)
            {
                if (p.pos.y == 0)
                {
                    blackIdentities[p.id] = PieceType.Queen;
                    p.gameObject.GetComponent<SpriteRenderer>().sprite = blackSprites[4];
                }
                if (passantable != null && passantable.pos.x == p.pos.x && passantable.pos.y - 1 == p.pos.y)
                {
                    UIController.Instance().root.Q<VisualElement>("BlackCapture" + blackCaptures).style.backgroundImage = new StyleBackground(whiteSprites[(int)whiteIdentities[passantable.id]]);
                    blackCaptures++;
                    passantable.gameObject.SetActive(false);
                }
                passantable = null;
                if (origin.pos.y - 2 == end.pos.y)
                {
                    passantable = p;
                    doublestepLastTurn = true;
                }
            }
        }

        LogEntry e = new() { from = origin.pos, to = end.pos, target = target };
        e.GenerateText();
        Debug.Log(e.text);
        log.Add(e);

        return true;
    }

    //returns true if one of the legal moves checks the opposing king
    private bool UpdateLegalMoves(Piece p)
    {
        p.legalMoves.Clear();

        PieceType type = PieceType.Pawn;
        if (p.color == Color.White)
        {
            type = whiteIdentities[p.id];
        }
        else
        {
            type = blackIdentities[p.id];
        }

        Tile t;

        if (type == PieceType.Pawn)
        {
            int dir = p.color == Color.White ? 1 : -1;
            if (GetTile(p.pos.x, p.pos.y + 1 * dir).empty)
            {
                p.AddLegalMove(p.pos.x, p.pos.y + 1 * dir);
                if (p.unmoved && GetTile(p.pos.x, p.pos.y + 2 * dir).empty)
                {
                    p.AddLegalMove(p.pos.x, p.pos.y + 2 * dir);
                }
            }
            t = GetTile(p.pos.x - 1, p.pos.y + 1 * dir);
            if (t != null && !t.empty && t.piece.GetComponent<Piece>().color != p.color)
            {
                p.AddLegalMove(t.pos);
            }
            else if (t != null && passantable != null && passantable.color != p.color && passantable.pos.x == t.pos.x && passantable.pos.y + dir == t.pos.y)
            {
                p.AddLegalMove(t.pos);
            }
            t = GetTile(p.pos.x + 1, p.pos.y + 1 * dir);
            if (t != null && !t.empty && t.piece.GetComponent<Piece>().color != p.color)
            {
                p.AddLegalMove(t.pos);
            }
            else if (t != null && passantable != null && passantable.color != p.color && passantable.pos.x == t.pos.x && passantable.pos.y + dir == t.pos.y)
            {
                p.AddLegalMove(t.pos);
            }
        }

        if (type == PieceType.Rook || type == PieceType.Queen)
        {
            for (int i = p.pos.x + 1; i < 8; i++)
            {
                t = GetTile(i, p.pos.y);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = p.pos.x - 1; i >= 0; i--)
            {
                t = GetTile(i, p.pos.y);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = p.pos.y + 1; i < 8; i++)
            {
                t = GetTile(p.pos.x, i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = p.pos.y - 1; i >= 0; i--)
            {
                t = GetTile(p.pos.x, i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        if (type == PieceType.Bishop || type == PieceType.Queen)
        {
            for (int i = 1; p.pos.x + i < 8 && p.pos.y + i < 8; i++)
            {
                t = GetTile(p.pos.x + i, p.pos.y + i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = 1; p.pos.x + i < 8 && p.pos.y - i >= 0; i++)
            {
                t = GetTile(p.pos.x + i, p.pos.y - i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = 1; p.pos.x - i >= 0 && p.pos.y + i < 8; i++)
            {
                t = GetTile(p.pos.x - i, p.pos.y + i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
            for (int i = 1; p.pos.x - i >= 0 && p.pos.y - i >= 0; i++)
            {
                t = GetTile(p.pos.x - i, p.pos.y - i);
                if (t.empty)
                {
                    p.AddLegalMove(t.pos);
                }
                else if (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)
                {
                    p.AddLegalMove(t.pos);
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        if (type == PieceType.Knight)
        {
            for (int i = 0; i < 8; i++)
            {
                t = GetTile(p.pos.x + knightMovesX[i], p.pos.y + knightMovesY[i]);
                if (t != null && (t.empty || (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)))
                {
                    p.AddLegalMove(t.pos);
                }
            }
        }

        if (type == PieceType.King)
        {
            for(int i = -1; i < 2; i++)
            {
                for(int j = -1; j < 2; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        t = GetTile(p.pos.x + i, p.pos.y + j);
                        if (t != null && (t.empty || (t.piece != null && t.piece.GetComponent<Piece>().color != p.color)))
                        {
                            p.AddLegalMove(t.pos);
                        }
                    }
                }
            }
        }
        for(int i = 0; i < p.legalMoves.Count; i++)
        {
            t = GetTile(p.legalMoves[i]);
            if (!t.empty)
            {
                Piece op = t.piece.GetComponent<Piece>();
                if ((p.color == Color.White ? blackIdentities[op.id] : whiteIdentities[op.id]) == PieceType.King) return true;
            }
        }
        return false;
    }

    private Tile GetTile(Coords pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x > 7 || pos.y > 7) return null;
        return board[pos.x].tiles[pos.y].GetComponent<Tile>();
    }

    private Tile GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x > 7 || y > 7) return null;
        return board[x].tiles[y].GetComponent<Tile>();
    }

    // this should maybe in in Piece.cs
    private IEnumerator LerpMove(Piece p, Vector3 startPosition, Vector3 endPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            // calculate the progress of the lerp
            float progress = elapsedTime / lerpDuration;

            // interpolate position
            p.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            // increment time
            elapsedTime += Time.deltaTime;

            // wait for next frame
            yield return null;
        }

        // ensure the final position is actually the target position
        p.gameObject.transform.position = endPosition;

        //Debug.Log("Lerp completed");
    }

    private void WinGame(bool whiteWin)
    {
        clipToPlay = "king_captured";

        string winText = whiteWin == true ? "White won!" : "Black won!";

        if(!gameOver)UIController.Instance().SetGameInfo(winText);

        UIController.Instance().SetCheckMateLight(whiteWin);

        gameOver = true;
    }

}
