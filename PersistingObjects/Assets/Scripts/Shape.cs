using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    private Color color;

    static int colorPropertyId = Shader.PropertyToID("_Color"); //Compare ints instead of strings

    static MaterialPropertyBlock sharedPropertyBlock;

    MeshRenderer meshRenderer;

    public Vector3 angularVelocity { get; set; }

    public Vector3 velocity { get; set; }
    
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();    
    }

    public void GameUpdate()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
        transform.localPosition += velocity * Time.deltaTime;
    }

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

    int shapeId = int.MinValue;

    public int MaterialId
    {
        get; private set;
    }
    public void SetColor(Color color)
    {
        this.color = color;
        
        if(sharedPropertyBlock == null) //Only create new if current one is null to improve performance
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        meshRenderer.SetPropertyBlock(sharedPropertyBlock);
    }

    public void SetMaterial(Material material, int materialId)
    {
        meshRenderer.material = material;
        MaterialId = materialId;
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
        writer.Write(angularVelocity);
        writer.Write(velocity);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        angularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }

}
