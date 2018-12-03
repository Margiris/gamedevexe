using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node : MonoBehaviour {

    //FOR CODING 
    // jei reiks skirtigiem algoritmams skirtingu node reiks padaryt abstraccia klase node su posicija ir algoritmai grazins ta bazine klase
    private Vector2Int coordinates;
    //private int H; //atstumas iki pabaigos
    //private int G; //atstumas nuo start
    private int F;
    public Node next;
    public Node parent;
    
    public Node(Vector2Int coordinates, int value, Node parent = null, Node next = null)
    {

    }
    
}
