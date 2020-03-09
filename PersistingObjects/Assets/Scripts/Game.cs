using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Game : Shape
{
    public ShapeFactory shapeFactory;

    public KeyCode createkey = KeyCode.C;
    public KeyCode destroyKey = KeyCode.X;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    public PersistentStorage storage;

    public SpawnZone spawnZoneOfLevel { get; set; }

    public static Game Instance { get; private set; }

    public int levelCount;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    float creationProgress;
    float destructionProgress;

    int loadedLevelBuildIndex;

    const int saveVersion = 2;

    List<Shape> shapes;

    void OnEnable()
    {
        Instance = this;
    }

    void Start()
    {
        shapes = new List<Shape>();

        if(Application.isEditor)
        {
            for (int i = 0; i < SceneManager.sceneCount;i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Scene 1"))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
            StartCoroutine(LoadLevel(1));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(createkey))
        {
            CreateShape();
        }

       else if(Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
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

        else
        {
            for (int i = 1; i <= levelCount; i++)
            {
                if(Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    beginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }

        creationProgress += Time.deltaTime * CreationSpeed;

        while(creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;

        while(destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = spawnZoneOfLevel.SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(.1f, 1f);
        instance.SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.25f, 1f, 1f, 1f));
        shapes.Add(instance);
    }

    void DestroyShape()
    {
        if(shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count); //Pick random index of shapes to destroy
            shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex); 
        }
    }

    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;

        if(loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }

        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive); //Async loading
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        enabled = true;
    }

    void beginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            shapeFactory.Reclaim(shapes[i]);
        }

        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        writer.Write(loadedLevelBuildIndex);

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
        StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));

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
