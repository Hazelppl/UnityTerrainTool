using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//若使用该脚本 必须在结束play前刷新一次恢复地形数据 否则已经采摘（删除）的植被将永久丢失
public class Tree2Prefabs : MonoBehaviour
{
    [SerializeField]
    [Header("是否在删除后生成预制体")]
    [Tooltip("目前没有清理生成的预制体的逻辑")]
    bool isGenerate;
    [SerializeField]
    [Header("全局刷新时间间隔")]
    private float refreshGap;
    private float timer;
    [SerializeField]
    [Header("拾取范围")]
    private float pickRange;
    //内建实例表
    private List<TreeInstance> TreeInstances;
    //存储要删除的index
    private List<int> deleteList;

    //原始地形信息
    private Terrain oTerrain;
    private TerrainData oData;
    private List<TreeInstance> oInstances;

    //玩家引用
    private GameObject player;

    void Start()
    {
        //自建内容 列表支持remove
        TreeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances);

        //原始信息
        oTerrain = Terrain.activeTerrain;
        oData = oTerrain.terrainData;
        oInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances);

        //玩家
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > refreshGap)
        {
            Debug.Log("刷新");
            RecoverData();
            timer = 0;
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            Generate();
        }
    }

    void Generate()
    {
        //现在所处的位置
        float sampleHeight = Terrain.activeTerrain.SampleHeight(player.transform.position);
        //玩家位置在地表以下的情况 保护措施
        if (player.transform.position.y < sampleHeight + 0.01f) return;

        //替换内容
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        TreeInstance[] treeInstances = terrainData.treeInstances;

        //获取离玩家最近的资源
        Vector3 closestTreePos = new Vector3();
        int closestTreeIndex = 0;
        int closestTreePrototypeIndex = 0;
        float maxDistance = float.MaxValue;
        for(int i = 0; i < treeInstances.Length; i++)
        {
            //如果不是资源则不考虑转换
            var prototypeIndex = treeInstances[i].prototypeIndex;
            if (!oData.treePrototypes[prototypeIndex].prefab.CompareTag("Res")) continue;

            //获取树位置
            var treePosOS = treeInstances[i].position;
            var treePosWS = new Vector3(treePosOS.x * oData.size.x, treePosOS.y * oData.size.y, treePosOS.z * oData.size.z) + Terrain.activeTerrain.transform.position;

            //比较位置
            float distance = Vector3.Distance(player.transform.position, treePosWS);
            if(distance < maxDistance && distance < pickRange)
            {
                maxDistance = distance;
                closestTreeIndex = i;
                closestTreePos = treePosWS;
                closestTreePrototypeIndex = prototypeIndex;
            }
        }

        TreeInstances.RemoveAt(closestTreeIndex);
        terrainData.treeInstances = TreeInstances.ToArray();

        float[,] heights = terrainData.GetHeights(0, 0, 0, 0);
        terrainData.SetHeights(0, 0, heights);

        if(isGenerate)
        {
            //对应位置生成预制体
            GameObject obj = Instantiate(oData.treePrototypes[closestTreePrototypeIndex].prefab);
            obj.transform.position = closestTreePos;
            obj.transform.parent = gameObject.transform;
            obj.name = "生成的最近预制体" + closestTreeIndex;

        }

        Debug.Log("捡了一个" + oData.treePrototypes[closestTreePrototypeIndex].prefab.name);
    }


    //将资源预制体放到场景中
    private void setPrefabs()
    {
        //容器
        GameObject parent = new GameObject();
        parent.transform.parent = gameObject.transform;
        parent.name = "生成的预制体";

        //遍历地形得到要删除的对象
        for (int index = 0; index < oData.treeInstances.Length; index++)
        {
            //预制体index
            var treePrototypeIndex = oData.GetTreeInstance(index).prototypeIndex;
            //如果不是可交互的类型 跳至下一个
            if (oData.treePrototypes[treePrototypeIndex].prefab.CompareTag("Res"))
            {
                //获得要新建的预制体的位置
                var treePosOS = oData.GetTreeInstance(index).position;
                var treePosWS = new Vector3(treePosOS.x * oData.size.x, treePosOS.y * oData.size.y, treePosOS.z * oData.size.z) + Terrain.activeTerrain.transform.position;
                //地形方向
                var terrainNormal = oData.GetInterpolatedNormal(treePosWS.x, treePosWS.y);

                //实例化
                GameObject obj = GameObject.Instantiate(oData.treePrototypes[treePrototypeIndex].prefab, treePosWS, Quaternion.Euler(terrainNormal * (30 * Random.Range(0, 11) % 360)));
                //设置父物体
                obj.transform.parent = parent.transform;
                //缩放
                obj.transform.localScale *= Random.Range(0.7f, 1.3f);

                //记录要删除的index
                deleteList.Add(index);
            }
        }
    }
    private void RecoverData()
    {
        Terrain.activeTerrain.terrainData = oData;

        TreeInstances = new List<TreeInstance>(oInstances);
        oData.treeInstances = oInstances.ToArray();
    }
}
