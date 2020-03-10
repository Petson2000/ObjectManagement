using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Game : Shape
{
    [SerializeField]ShapeFactory shapeFactory;

    public KeyCode createkey = KeyCode.C;
    public KeyCode destroyKey = KeyCode.X;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    [SerializeField]PersistentStorage storage;

    [SerializeField] Slider creationSpeedSlider;
    [SerializeField] Slider destructionSpeedSlider;

    [SerializeField] bool reseedOnLoad;

    [SerializeField] int levelCount;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    float creationProgress;
    float destructionProgress;

    int loadedLevelBuildIndex;

    Random.State mainRandomState;

    [SerializeField]const int saveVersion = 3;

    List<Shape> shapes;

    void Start()
    {
        mainRandomState = Random.state;

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

            beginNewGame();

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
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
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
    }

    private void FixedUpdate()
    {
        creationProgress += Time.deltaTime * CreationSpeed;

        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;

        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = GameLevel.Current.SpawnPoint;
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
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledDeltaTime; //Randomization
        mainRandomState = Random.state;
        Random.InitState(seed);

        CreationSpeed = 0;
        creationSpeedSlider.value = 0;
        DestructionSpeed = 0;
        destructionSpeedSlider.value = 0;

        for (int i = 0; i < shapes.Count; i++)
        {
            shapeFactory.Reclaim(shapes[i]);
        }

        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);

        GameLevel.Current.Save(writer); 

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

        if (version > saveVersion) //If save version is not supported, throw error.
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
    }

    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;

        int count = version <= 0 ? -version : reader.ReadInt();

        if (version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad)
            {
                Random.state = state;
            }
        }

        CreationSpeed = reader.ReadFloat();
        creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
        creationProgress = reader.ReadFloat();
        destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
        DestructionSpeed = reader.ReadFloat();
        destructionProgress = reader.ReadFloat();

        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());

        if(version >= 3)
        {
            GameLevel.Current.Load(reader);
        }

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
