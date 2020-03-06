using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    public int ShapeId
    {
        get
        {
            return shapeId;
        }
        set
        {
            if(shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }

            else //Since the shape identifier has to be set once per instance
            {
                Debug.LogError("Not allowed to change shapeId");
            }
        }
    }

    int shapeId;

}
