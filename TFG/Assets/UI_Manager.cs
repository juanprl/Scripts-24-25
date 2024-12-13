using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Game_Manager gameplay_manager_r;

    [Header("UI")]
    public GameObject notMoreNpc_panel_go;
    
    [Header("Ncp")]
    public GameObject npcsIcon_parent_go;
    public GameObject npcInteractionUI_prefab_go;
    public GameObject npcsIconUI_prefab_go;

    public Image imgTest;

    void Start()
    {
        //Temp
        //Siempre empezamos con un NPC //Le he asignado de manera manual la IA npc
        Npc_Player_IA npc = gameplay_manager_r.npcs_parent_kitchen_go.transform.GetChild(0).GetComponent<Npc_Player_IA>();
        npcsIcon_parent_go.transform.GetChild(0).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().LoadData(npc, this, gameplay_manager_r);

    }

    void Update()
    {
        
    }

    //Botones Game
     
    public void ReloadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    /// </summary>

    public void Set_HireNpcUI()
    {
        if (gameplay_manager_r.Set_HireNewNpc()) //Crear NPC - Prefab
        {
            GameObject go = Instantiate(npcInteractionUI_prefab_go, npcsIcon_parent_go.transform); //Crear Panel UI - Prefab

            Npc_Player_IA npc = gameplay_manager_r.npcs_parent_kitchen_go.transform.GetChild(gameplay_manager_r.npcs_parent_kitchen_go.transform.childCount - 1).GetComponent<Npc_Player_IA>(); //Como es el ultimo de la pila lo podemos coger así.
            go.GetComponent<Npc_Player_AllTasks_Prefabs_UI>().LoadData(npc, this, gameplay_manager_r);
        }
        else
        {
            notMoreNpc_panel_go.SetActive(true); //Todo: Que se desactive tras X segs.
        }    
    }
    
    public void Show_NpcPanel()
    {
        for (int i = 0; i < npcsIcon_parent_go.transform.childCount; i++)
        {
            npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().tasksIcons_parent_go.SetActive(false);
        }
    }
    
    public void Show_NpcPanel_Fire()
    {
        for (int i = 0; i < npcsIcon_parent_go.transform.childCount; i++)
        {
            npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().firePanel_go.SetActive(false);
        }
    }
    
    public void Show_NpcPanel_OrdersSeconds(string nameNpc, bool onOff)
    {
        for (int i = 0; i < npcsIcon_parent_go.transform.childCount; i++)
        {
            if (npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().npcIA_r.data_Npc_r.nameNpc_str == nameNpc)
            {
                npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().ordersSeconds_go.SetActive(onOff);

                //Temp - Sustuir por anim? Por ahora texto plano para diferenciarlo de actividades normales
                npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>().ordersSeconds_txt.text = "Tareas secundarias";
            }
        }
    }
    
    public void Create_IconTask_UI(GameObject npc, Task_Gameplay task)
    {
        for (int i = 0; i < npcsIcon_parent_go.transform.childCount; i++)
        {
            Npc_Player_AllTasks_Prefabs_UI npcUI = npcsIcon_parent_go.transform.GetChild(i).GetComponent<Npc_Player_AllTasks_Prefabs_UI>();

            if (npcUI.nameNpc_txt.text.ToString() == npc.GetComponent<Npc_Player_Gameplay>().nameNpc_str)
            {
                GameObject go = Instantiate(npcsIconUI_prefab_go, npcUI.tasksIcons_parent_go.transform);

                try
                {
                    Task_UI_Prefab taskUI = go.GetComponent<Task_UI_Prefab>();
                    taskUI.task_sprite = gameplay_manager_r.Get_Task_By_Name_InGame(task.item_r.taskName).GetComponent<Task_Data_UI_Prefab>().taskIcon_sprite;

                    taskUI.taskObjetive_sprite = gameplay_manager_r.Get_IngredientGameobject_By_Name_InGame(task.objective_str).GetComponent<Ingredients_Gameplay>().ingredient_sprite;
                    taskUI.idTask_i = task.idTask_i;

                    taskUI.Load_Data();
                }
                catch (System.Exception)
                {
                    Destroy(go);

                    if (task.item_r.taskName == "Limpiar")
                    {
                        Task_UI_Prefab taskUI = go.GetComponent<Task_UI_Prefab>();
                        taskUI.task_sprite = gameplay_manager_r.Get_Task_By_Name_InGame("Limpiar").GetComponent<Task_Data_UI_Prefab>().taskIcon_sprite;

                        taskUI.taskObjetive_sprite = gameplay_manager_r.Get_IngredientGameobject_By_Name_InGame("Plato").GetComponent<Ingredients_Gameplay>().ingredient_sprite;
                        taskUI.idTask_i = gameplay_manager_r.Get_Task_By_Name_InGame("Limpiar").GetComponent<Task_Data_UI_Prefab>().idTask_i;

                        taskUI.Load_Data();
                    }

                    throw;
                }

                break;
            }
            else
            {

            }
        }
    }
    
    public void ExitScene()
    {
        SceneManager.LoadScene("Start", LoadSceneMode.Single);
    }
}
