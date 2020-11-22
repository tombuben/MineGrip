using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Helper class containing the vertices and UV's of a cube
/// in a way we can pick and choose separate faces for generation
/// </summary>
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

    public static readonly Vector3[] WallDirection =
    {
        new Vector3(0.0f, 0.0f, 1.0f), // front
        new Vector3(1.0f, 0.0f, 0.0f), // right
        new Vector3(0.0f, 0.0f, -1.0f), // back
        new Vector3(-1.0f, 0.0f, 0.0f), // left
        new Vector3(0.0f, -1.0f, 0.0f), // bottom
        new Vector3(0.0f, 1.0f, 0.0f), // top
    };

    public static readonly Vector2[] UVs =
    {
        new Vector2(1.0f, 0.0f), // vertex (0,0)
        new Vector2(0.0f, 0.0f), // vertex (1,0)
        new Vector2(0.0f, 1.0f), // vertex (1,1)
        new Vector2(0.0f, 1.0f), // vertex (1,1)
        new Vector2(1.0f, 1.0f), // vertex (0,1)
        new Vector2(1.0f, 0.0f), // vertex (0,0)
    };

    /// <summary>
    /// Returns a UV array which maps a specific rect onto the face the UVs represent
    /// </summary>
    /// <param name="rect">A rect of UVs</param>
    /// <returns></returns>
    public static Vector2[] getUVsOfRect(Rect rect)
    {
        return new Vector2[]
        {
            new Vector2(rect.xMax, rect.yMin),
            rect.min,
            new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMin, rect.yMax),
            rect.max,
            new Vector2(rect.xMax, rect.yMin),
        };
    }

    public static readonly List<int>[] FaceMap = GenerateFaceMap();

    private static List<int>[] GenerateFaceMap()
    {
        var faces = new List<int>[63];
        for (var i = 0; i < 0b111111; i++)
        {
            faces[i] = GetFacesFromMask(i);
        }

        return faces;
    }
    
    private static List<int> GetFacesFromMask(int mask)
    {
        var faces = new List<int>();
        for (var i = 0; i < 6; i++)
        {
            if (( mask >> i & 1) == 1)
            {
                faces.AddRange(Enumerable.Range(0, 6).Select(x => FaceIndices[0, x]));
            }
        }

        return faces;
    } 

}
