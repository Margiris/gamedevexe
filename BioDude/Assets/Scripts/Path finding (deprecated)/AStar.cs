using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

    //reikia masyvo kuriame butu saugomi poinetriai i node'us
    private Node[,] nodeMap;
    public InitiateMap init;

    //FOR CODING
    //kai algoritmas baigs darba gausis medis kurio pagrindine saka netures jungciu su sakomis
    //todel duosiu tik pagrindines sakos pradzia ir tai bus path
    ////////
    

    /// <summary>
    /// Finds shortest path to target
    /// </summary>
    /// <param name="start">find path from this position</param>
    /// <param name="finish">find path to this position</param>
    /// <returns>returns path to go to the target node by node. Null if path don't exist</returns>
    public Node FindPath(Vector3 start, Vector3 finish)
    {
        Node path = null;
        Vector2Int MapSize = init.MapSize;
        nodeMap = new Node[MapSize.x, MapSize.y];
        Node begin = new Node(init.GetCoordinates(start), 0);
        Node end = new Node(init.GetCoordinates(finish), int.MaxValue);

        Node cur = begin;
        Node open;
        Node close;

        return path;
    }


    private int CalcNodeValue()
    {

        return 1;
    }

}
