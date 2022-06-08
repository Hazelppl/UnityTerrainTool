using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ʹ�øýű� �����ڽ���playǰˢ��һ�λָ��������� �����Ѿ���ժ��ɾ������ֲ�������ö�ʧ
public class Tree2Prefabs : MonoBehaviour
{
    [SerializeField]
    [Header("�Ƿ���ɾ��������Ԥ����")]
    [Tooltip("Ŀǰû���������ɵ�Ԥ������߼�")]
    bool isGenerate;
    [SerializeField]
    [Header("ȫ��ˢ��ʱ����")]
    private float refreshGap;
    private float timer;
    [SerializeField]
    [Header("ʰȡ��Χ")]
    private float pickRange;
    //�ڽ�ʵ����
    private List<TreeInstance> TreeInstances;
    //�洢Ҫɾ����index
    private List<int> deleteList;

    //ԭʼ������Ϣ
    private Terrain oTerrain;
    private TerrainData oData;
    private List<TreeInstance> oInstances;

    //�������
    private GameObject player;

    void Start()
    {
        //�Խ����� �б�֧��remove
        TreeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances);

        //ԭʼ��Ϣ
        oTerrain = Terrain.activeTerrain;
        oData = oTerrain.terrainData;
        oInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances);

        //���
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > refreshGap)
        {
            Debug.Log("ˢ��");
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
        //����������λ��
        float sampleHeight = Terrain.activeTerrain.SampleHeight(player.transform.position);
        //���λ���ڵر����µ���� ������ʩ
        if (player.transform.position.y < sampleHeight + 0.01f) return;

        //�滻����
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        TreeInstance[] treeInstances = terrainData.treeInstances;

        //��ȡ������������Դ
        Vector3 closestTreePos = new Vector3();
        int closestTreeIndex = 0;
        int closestTreePrototypeIndex = 0;
        float maxDistance = float.MaxValue;
        for(int i = 0; i < treeInstances.Length; i++)
        {
            //���������Դ�򲻿���ת��
            var prototypeIndex = treeInstances[i].prototypeIndex;
            if (!oData.treePrototypes[prototypeIndex].prefab.CompareTag("Res")) continue;

            //��ȡ��λ��
            var treePosOS = treeInstances[i].position;
            var treePosWS = new Vector3(treePosOS.x * oData.size.x, treePosOS.y * oData.size.y, treePosOS.z * oData.size.z) + Terrain.activeTerrain.transform.position;

            //�Ƚ�λ��
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
            //��Ӧλ������Ԥ����
            GameObject obj = Instantiate(oData.treePrototypes[closestTreePrototypeIndex].prefab);
            obj.transform.position = closestTreePos;
            obj.transform.parent = gameObject.transform;
            obj.name = "���ɵ����Ԥ����" + closestTreeIndex;

        }

        Debug.Log("����һ��" + oData.treePrototypes[closestTreePrototypeIndex].prefab.name);
    }


    //����ԴԤ����ŵ�������
    private void setPrefabs()
    {
        //����
        GameObject parent = new GameObject();
        parent.transform.parent = gameObject.transform;
        parent.name = "���ɵ�Ԥ����";

        //�������εõ�Ҫɾ���Ķ���
        for (int index = 0; index < oData.treeInstances.Length; index++)
        {
            //Ԥ����index
            var treePrototypeIndex = oData.GetTreeInstance(index).prototypeIndex;
            //������ǿɽ��������� ������һ��
            if (oData.treePrototypes[treePrototypeIndex].prefab.CompareTag("Res"))
            {
                //���Ҫ�½���Ԥ�����λ��
                var treePosOS = oData.GetTreeInstance(index).position;
                var treePosWS = new Vector3(treePosOS.x * oData.size.x, treePosOS.y * oData.size.y, treePosOS.z * oData.size.z) + Terrain.activeTerrain.transform.position;
                //���η���
                var terrainNormal = oData.GetInterpolatedNormal(treePosWS.x, treePosWS.y);

                //ʵ����
                GameObject obj = GameObject.Instantiate(oData.treePrototypes[treePrototypeIndex].prefab, treePosWS, Quaternion.Euler(terrainNormal * (30 * Random.Range(0, 11) % 360)));
                //���ø�����
                obj.transform.parent = parent.transform;
                //����
                obj.transform.localScale *= Random.Range(0.7f, 1.3f);

                //��¼Ҫɾ����index
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
