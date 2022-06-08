using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using TreeEditor;

public class PlaceObjs : EditorWindow
{
    public Transform parent;
    public Terrain terrain;
    
    private string targetName = string.Empty;
    [MenuItem("CatHoYoTool/放置物体")]
    private static void OpenWindow()
    {
        GetWindow<PlaceObjs>("放置工具").Show();
    }

    private void OnGUI()
    {    
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("需要放置的地形: ");
            GUILayout.FlexibleSpace();
            terrain = (Terrain)EditorGUILayout.ObjectField(terrain, typeof(Terrain),true);
        }  
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("生成的父物体: ");
            GUILayout.FlexibleSpace();
            parent = (Transform)EditorGUILayout.ObjectField(parent, typeof(Transform),true);
        }   
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("");
            GUILayout.FlexibleSpace();

            if (terrain == null)
            {
                terrain = Terrain.activeTerrain;
            }

            if (GUILayout.Button("放置", GUILayout.MaxWidth(90)))
            {
                //地形数据获取
                TerrainData terrainData = terrain.terrainData;
                TreeInstance[] treeInstances = terrainData.treeInstances;
                
                for (int i = 0; i < treeInstances.Length; i++)
                {
                    //如果不是资源则不考虑转换
                    var prototypeIndex = treeInstances[i].prototypeIndex;
                    //资源
                    if (terrainData.treePrototypes[prototypeIndex].prefab.CompareTag("Res"))
                    {
                        //清空
                        GameMgr.Instance.ObjectPool.rpPoolObjects.Clear();
                        //克隆
                        //GameObject tmpObj = GameMgr.Instance.ObjectPool.GetRPPoolObject
                        //(terrainData.treePrototypes[prototypeIndex].prefab.GetComponent<ResourcePointBase>().RPID);
                        GameObject tmpObj = (GameObject)PrefabUtility.InstantiatePrefab(terrainData.treePrototypes[prototypeIndex].prefab);
                        GameMgr.Instance.ObjectPool.rpPoolObjects.Add(tmpObj);
                        //位置
                        //获取树位置
                        var treePosOS = treeInstances[i].position;
                        var treePosWS = new Vector3(treePosOS.x * terrainData.size.x, treePosOS.y * terrainData.size.y, treePosOS.z 
                        * terrainData.size.z) + Terrain.activeTerrain.transform.position;

                        //放置
                        tmpObj.transform.position = treePosWS;
                        
                        //设置旋转和缩放
                        tmpObj.transform.localScale *= UnityEngine.Random.Range(0.7f, 1.3f);
                        tmpObj.transform.rotation = Quaternion.Euler(Vector3.up * (30 * UnityEngine.Random.Range(0, 11) % 360));

                        //设置父物体
                        if (parent != null)
                        {
                            tmpObj.transform.parent = parent;
                        }
                        tmpObj.SetActive(true);                        
                    }
                }
            }
        }
    }
}
