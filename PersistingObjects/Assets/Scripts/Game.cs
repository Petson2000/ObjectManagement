using System.Collections.Generic;
using UnityEngine;

public class Game : Shape
{
    public ShapeFactory shapeFactory;

    public KeyCode createkey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    const int saveVersion = 1;

    List<Shape> shapes;

    public PersistentStorage storage;

    void Awake()
    {
        shapes = new List<Shape>();
    }

    void Update()
    {
        if (Input.GetKeyDown(createkey))
        {
            CreateShape();
        }

        else if(Input.GetKeyDown(newGameKey))
        {
            beginNewGame();
        }

        else if(Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }

        else if(Input.GetKeyDown(loadKey))
        {
            beginNewGame();
            storage.Load(this);
        }
    }

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(.1f, 1f);
        instance.SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.25f, 1f, 1f, 1f));
        shapes.Add(instance);
    }

    void beginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            Destroy(shapes[i].gameObject);
        }

        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);

        for (int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].ShapeId); //Store shapeId 
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        /*
         * Save version can now either be 0 or negative, if negative means we are dealing with an old save file from before shape factory support.
         */

        int version = reader.Version;

        if(version > saveVersion) //If save version is not supported, throw error.
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        int count = version <= 0 ? -version : reader.ReadInt();

        for (int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }
}
