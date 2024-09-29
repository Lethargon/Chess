using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coords
{
    int x;
    int y;
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
        [SerializeField] string name;
        [SerializeField] GameObject[] tiles;
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
    }

    private bool AttemptMove(Coords target)
    {
        if (chosenPiece == null) return false;

        Piece p = chosenPiece.GetComponent<Piece>();

        return false;
    }
}
