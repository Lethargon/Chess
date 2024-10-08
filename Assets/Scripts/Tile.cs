using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public BoardManager board;
    public Coords pos;
    public bool empty = true;
    public GameObject piece;

    private Color32 baseColor;

    private void Start()
    {
        baseColor = GetComponent<SpriteRenderer>().color;
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked tile at x: " + pos.x + ", y: " + pos.y);
        board.TileClicked(this);
    }

    public void SetColor(Color32 color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void ResetColor()
    {
        GetComponent<SpriteRenderer>().color = baseColor;
    }
}
