using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class CameraMgr : MonoBehaviour
{
    
    public NoiseSettings shake6D;
    [HideInInspector]
    public CinemachineBasicMultiChannelPerlin shake;

    [SerializeField]
    public Camera cam;
    [SerializeField]
    public CinemachineVirtualCamera vCam;
    [SerializeField]
    public CinemachineVirtualCamera talkVCam;
    
    [Range(0, 1)] [SerializeField]
    [Header("鼠标移动触发距离")]
    private float triggerFloat = 0.3f;

    [Header("每次旋转角度")]
    [SerializeField] private int roaAngle = 30;
    [SerializeField] private float CD = 0.5f;
    private float count;

    public bool canRoaCamera = true;

    //正反转
    private bool isMinus = false;
    //跟随点
    [SerializeField] 
    private GameObject followPlayerPos;
    //是否在转
    private bool isTurning;

    private Vector3 initRoa;

    private Vector3 lastCamRoa;

    private float MouseX;

    private bool isInHouse;
    private void OnEnable()
    {
        GameMgr.Instance.onReturnHouse += () => { 
            canRoaCamera = false;
            isTurning = false;
            isInHouse = true;
            followPlayerPos.transform.eulerAngles = new Vector3(0,120,0);};
        
        GameMgr.Instance.onOutHouse += () => {
            canRoaCamera = true;
            isInHouse = false;
            followPlayerPos.transform.eulerAngles = lastCamRoa; };
    }

    private void LockCam()
    {
        canRoaCamera = false;
        followPlayerPos.transform.eulerAngles = lastCamRoa;
    }

    private void Start()
    {
        if(vCam != null)
        {
            shake = vCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }        
    }

    private void Update()
    {
        if (isInHouse)
        {
            followPlayerPos.transform.eulerAngles = new Vector3(0,120,0);
        }
        if (SystemMgr.IsPaused) return;
        if (!canRoaCamera || !Input.GetKey(KeyCode.Mouse1))
        {
            MouseX = 0;
        }
        else if(Input.GetKey(KeyCode.Mouse1))
        {
            MouseX = Input.GetAxis("Mouse X");
        }
        if (!isTurning)
        {

            if(followPlayerPos == null) return;
            if (MouseX > triggerFloat)
            {

                initRoa = followPlayerPos.transform.eulerAngles;
                isTurning = true;
                isMinus = false;
            }
            else if (MouseX < -triggerFloat)
            {

                initRoa = followPlayerPos.transform.eulerAngles;
                isTurning = true;
                isMinus = true;
            }
            
        }
        if(isTurning)
        {
            //print("turning");
            GameMgr.Instance.PlayerMgr.transform.eulerAngles = GameMgr.Instance.PlayerMgr.followPlayerPos.eulerAngles;
            if (count < CD)
            {

                count += Time.deltaTime;
                if (!isMinus)
                {
                    followPlayerPos.transform.DORotate(initRoa+new Vector3(0,roaAngle,0),CD * 0.8f);
                }
                else
                {
                    followPlayerPos.transform.DORotate(initRoa-new Vector3(0,roaAngle,0),CD * 0.8f);
                }
            }
            else
            {
                if (!isMinus)
                {
                    lastCamRoa = initRoa + new Vector3(0, roaAngle, 0);
                    followPlayerPos.transform.eulerAngles = initRoa+new Vector3(0,roaAngle,0);
                }
                else
                {
                    lastCamRoa = initRoa + new Vector3(0, roaAngle, 0);
                    followPlayerPos.transform.eulerAngles = initRoa-new Vector3(0,roaAngle,0);
                }
                count = 0;
                isTurning = false;
            }
        }
        
    }
}
