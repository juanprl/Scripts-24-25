using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Npc_Player_AllTasks_Prefabs_UI : MonoBehaviour
{
    public Image npcIcon_img;
    public Sprite npcIcon_sprite;

    public TMP_Text nameNpc_txt;
    public TMP_Text timeToFinishOrder_txt;
    public int idNpc_i = -1;

    [Header("")]
    public GameObject tasksIcons_parent_go;
    public Game_Manager game_manager_r;
    public UI_Manager ui_manager_r;
    public Npc_Player_IA npcIA_r;

    [Header("Fire")]
    public GameObject firePanel_go;
    
    [Header("Camera")]
    public CameraControl cameraControl_r;
    
    [Header("Orders Seconds")]
    public GameObject ordersSeconds_go;
    public TMP_Text ordersSeconds_txt;


    void Start()
    {
        cameraControl_r = FindAnyObjectByType<CameraControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (npcIA_r.working_b && !npcIA_r.ordersSecond_b)
        {
            float maxTime = npcIA_r.orders_r[npcIA_r.countOrdersNow_i].item_r.timeToFinishAction_f;
            float total = (maxTime - npcIA_r.orders_r[npcIA_r.countOrdersNow_i].item_r.timeRecord_f);
            timeToFinishOrder_txt.text = total.ToString("F2");
        }
        if (npcIA_r.working_b && npcIA_r.ordersSecond_b)
        {
            float maxTime = npcIA_r.orders_r[npcIA_r.countOrdersNow_i].ordersSecond_r[npcIA_r.countOrdersSecondNow_i].item_r.timeToFinishAction_f;
            float total = (maxTime - npcIA_r.orders_r[npcIA_r.countOrdersNow_i].ordersSecond_r[npcIA_r.countOrdersSecondNow_i].item_r.timeRecord_f);
            timeToFinishOrder_txt.text = total.ToString("F2");
        }
    }
    
    public void LoadData(Npc_Player_IA npc, UI_Manager uiManager, Game_Manager game_Manager)
    {
        npcIA_r = npc;
        nameNpc_txt.text = npcIA_r.data_Npc_r.nameNpc_str;
        npcIcon_sprite = npc.data_Npc_r.iconUser_sprite;
        npcIcon_img.sprite = npcIcon_sprite;

        ui_manager_r = uiManager;
        game_manager_r = game_Manager;
    }

    public void Show_Panel()
    {
        if (!tasksIcons_parent_go.activeSelf)
        {
            ui_manager_r.Show_NpcPanel();

            tasksIcons_parent_go.SetActive(true);
        }
        else
        {
            tasksIcons_parent_go.SetActive(false);
        }
    }
    
    public void Show_Panel_Fire()
    {
        if (!firePanel_go.activeSelf)
        {
            ui_manager_r.Show_NpcPanel_Fire();

            firePanel_go.SetActive(true);
        }
        else
        {
            firePanel_go.SetActive(false);
        }
    }
    
    public void Fire_NPC()
    {
        int count = npcIA_r.orders_r.Count;

        for (int i = count - 1; i > -1; i--)//*0 o -1? 
        {
            npcIA_r.Destroy_Task(i, true);
        }

        //
        npcIA_r.DestroyThisNpc();
        Destroy(this.gameObject);
    }

    public void CameraControl()
    {
       cameraControl_r.FollowNpc(npcIA_r.gameObject);
    }
}
