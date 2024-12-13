using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Task_UI_Prefab : MonoBehaviour
{
    public Sprite task_sprite;
    public Image task_image;

    public Sprite taskObjetive_sprite;
    public Image taskObjetive_image;
    
    public int idTask_i = -1;

    [HideInInspector]public string taskName_str;
    public TextMeshPro taskName_txt;
    
    public List<string> taskName_Variants_str = new List<string>();
    public GameObject destroy_btt; //Destruir tarea

    Npc_Player_AllTasks_Prefabs_UI npcUI_r;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Load_Data()
    {
        task_image.sprite = task_sprite;
        taskObjetive_image.sprite = taskObjetive_sprite;

        npcUI_r = this.transform.parent.parent.GetComponent<Npc_Player_AllTasks_Prefabs_UI>();
    }

    public void Destroy_Task()
    {
        int posTask = -1;
        bool removeIt = true;

        //Saber la posición de la tarea. //Todo: Ver si hacer método
        for (int i = 0; i < npcUI_r.npcIA_r.orders_r.Count; i++)
        {
            if (npcUI_r.npcIA_r.orders_r[i].idTask_i == idTask_i)
            {
                posTask = i; 
                break;
            }
        }

        //Ver si hay que sacarle del item
        for (int i = 0; i < npcUI_r.npcIA_r.orders_r.Count; i++)
        {
            if (npcUI_r.npcIA_r.orders_r[posTask].item_r.taskName == npcUI_r.npcIA_r.orders_r[i].item_r.taskName && i != posTask /*Para que se ignore asi mismo al comparar datos.*/)
            {
                removeIt = false;
                break;
            }
        }

        npcUI_r.npcIA_r.Destroy_Task(posTask, removeIt);
        Debug.Log("Destruir tarea desde UI, posTask: " + posTask + " Quitarla del objeto: " + removeIt);

        Destroy(this.gameObject); //Eliminar - Icono UI

    }
}
