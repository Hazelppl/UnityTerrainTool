using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class TerrainCallPrototype : MonoBehaviour
{
    Terrain m_Terrain;
    TerrainData m_TerrainData;
    TreeInstance[] m_TreeInstances; //原始Instance 用于保存数据
    public float[] Range;
    void Start()
    {
        m_Terrain = GetComponent<Terrain>();
        m_TerrainData = m_Terrain.terrainData;
        m_TreeInstances = m_TerrainData.treeInstances;
        GenerateTree();
    }
    private void GenerateTree()
    {
        GameObject prefabItems = new GameObject();
        prefabItems.name = "prefabItem";
        prefabItems.transform.parent = gameObject.transform.parent;
        for(int index = 0; index < m_TerrainData.treeInstances.Length; index++)
        {
            //物体空间树位置
            var treePosOS = m_TerrainData.GetTreeInstance(index).position;
            //世界空间树位置
            var treePosWS = new Vector3(treePosOS.x * m_TerrainData.size.x, treePosOS.y * m_TerrainData.size.y, treePosOS.z * m_TerrainData.size.z) + this.gameObject.transform.position;
            //预制体index
            var treePrototypeIndex = m_TerrainData.treeInstances[index].prototypeIndex;
            var terrainNormal = m_TerrainData.GetInterpolatedNormal(treePosWS.x, treePosWS.y);

            //实例化
            GameObject obj = GameObject.Instantiate(m_TerrainData.treePrototypes[treePrototypeIndex].prefab, treePosWS, Quaternion.Euler(terrainNormal * (30 * Random.Range(0, 11) % 360)));
            //设置父物体
            obj.transform.parent = prefabItems.transform;
            //缩放
            obj.transform.localScale *= Random.Range(Range[0], Range[1]);

            //删除树实例
            if(index == m_TerrainData.treeInstances.Length - 1)
            {
                GameObject terrain = Instantiate(gameObject);
                gameObject.SetActive(false);
            }
        }
    }
}
