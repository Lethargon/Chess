using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
public enum Color
{
    White, Black
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

    private GameObject chosenPiece;

    private Color colorToPlay = Color.White;

    private int[] knightMovesX = { 1, 1, -1, -1, 2, 2, -2, -2};
    private int[] knightMovesY = { 2, -2, 2, -2, 1, -1, 1, -1};

    
    void Start()
    {
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
            UpdateLegalMoves(whitePieces[i].GetComponent<Piece>());
            UpdateLegalMoves(blackPieces[i].GetComponent<Piece>());
        }
    }

    public void TileClicked(Tile t)
    {
        if (chosenPiece == null)
        {
            if (!t.empty && colorToPlay == t.piece.GetComponent<Piece>().color)
            {
                chosenPiece = t.piece;
            }
        }
        else
        {
            bool moved = AttemptMove(t.pos);
            if (moved)
            {
                colorToPlay = colorToPlay == Color.White ? Color.Black : Color.White;
            }
            else
            {
                //sound/visual feedback that move failed
            }
        }
    }

    private bool AttemptMove(Coords target)
    {
        if (chosenPiece == null) return false;

        Piece p = chosenPiece.GetComponent<Piece>();
        UpdateLegalMoves(p);
        chosenPiece = null;

        if (p.legalMoves.Count == 0) return false;

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
            end.piece.gameObject.SetActive(false); //change this to move to display zone instead
        }
        end.piece = p.gameObject;
        end.empty = false;
        p.gameObject.transform.position = end.transform.position;
        p.pos = end.pos;
        p.unmoved = false;

        return true;
    }

    private void UpdateLegalMoves(Piece p)
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
            t = GetTile(p.pos.x + 1, p.pos.y + 1 * dir);
            if (t != null && !t.empty && t.piece.GetComponent<Piece>().color != p.color)
            {
                p.AddLegalMove(t.pos);
            }
        }

        else if (type == PieceType.Rook || type == PieceType.Queen)
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

        else if (type == PieceType.Bishop || type == PieceType.Queen)
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

        else if (type == PieceType.Knight)
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

        else if (type == PieceType.King)
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

}
