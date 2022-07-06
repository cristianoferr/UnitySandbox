using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 10;

    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask = FaceRenderMask.All;

    private Material material;


    [HideInInspector]
    public bool shapeSettingsFoldout;
    public ShapeSettings shapeSettings;

    [HideInInspector]
    public bool colourSettingsFoldout;
    public ColourSettings colourSettings;


    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    #region LOD
    public static Transform player;
    public static float size = 10;

    public static Dictionary<int, float> detailLevelDistances = new Dictionary<int, float>(){
        { 0,Mathf.Infinity },
        { 1,60f },
        { 2,25f },
        { 3,10f },
        { 4,4f },
        { 5,1.5f },
        { 6,0.7f },
        { 7,0.3f },
        { 8,0.1f }
    };

#endregion



    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);    

        if (meshFilters==null || meshFilters.Length == 0) { 
        meshFilters=new MeshFilter[6];
        }
        terrainFaces=new TerrainFace[6];

        Vector3[] directions = {Vector3.up,Vector3.down,Vector3.left,Vector3.right, Vector3.forward, Vector3.back};
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null) {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();

            }
            if (material == null)
            {
                material=new Material(colourSettings.planetMaterial);
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = material;
           terrainFaces[i] = new TerrainFace(shapeGenerator,meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }

    }

    private void Start()
    {
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
    }

    public void OnColourSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateColours();
        }
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
        }
    }

    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }

        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColours()
    {
        colourGenerator.UpdateColours();

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colourGenerator);
            }
        }

    }

}
