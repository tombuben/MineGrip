using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFaces
{
    public static readonly Vector3[] Vertices = {
        //front
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
        //back
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
    };

    public static readonly int[,] FaceIndices = {
        // front
        {0, 1, 2,
        2, 3, 0},
        // right
        {1, 5, 6,
        6, 2, 1},
        // back
        {5, 4, 7,
         7, 6, 5},
        // left
        {4, 0, 3,
        3, 7, 4},
        // bottom
        {4, 5, 1,
        1, 0, 4},
        // top
        {3, 2, 6,
        6, 7, 3}
    };

    public static readonly Vector2[] UVs =
    {
        new Vector2(1.0f, 0.0f),
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
    };
}
