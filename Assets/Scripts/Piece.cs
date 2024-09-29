using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Piece : MonoBehaviour
{
    public Coords pos;
    public int id;
    public List<Coords> legalMoves = new List<Coords>();
    public Color color;
    public bool unmoved = true;

    public void AddLegalMove(int x, int y)
    {
        Coords c = new ();
        c.x = x;
        c.y = y;
        legalMoves.Add(c);
    }

    public void AddLegalMove(Coords pos)
    {
        legalMoves.Add(pos);
    }
}
