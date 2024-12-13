using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Task_Gameplay : MonoBehaviour
{
    public int typeOfTask_i = -1;
    
    [Header("")]
    public Item_GamePlay item_r;
    public string objective_str; //Aquí sacas que está cocinadno o tienes que buscar en caso de que se quede sin ese ingrediente.
    public bool targetReached_b; //Para el NavMeshAgent.

    [Header("")]
    public bool tasksSecondaryActive_b = false;
    public int countTasksSecondary_i = -1;
    public List<Task_Gameplay> ordersSecond_r = new List<Task_Gameplay>();

    public int idTask_i = -1; //Id de nº interno, los nº se repiten entre NPC.
    public int turnCreation_i = -1; //Es para detectar cuando se crearon.

    //Ej: Cortar Patatas,si te quedas sin ellas, activa acción secundaria tasksSecondaryActive_b = true, esa es Item = armario, objective = patata, al conseguirlo se acaba.
    //Ve si la lista tiene más tareas ,sino pasa a false y debe dirigirse al lugar del item principal y activar su tarea.

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set_tasksSecond(string nameNpc, int type, string objetiveName)
    {
        Game_Manager manager = FindAnyObjectByType<Game_Manager>();

        if (type == 0)
        {
            Task_Gameplay task = new Task_Gameplay();
            task.typeOfTask_i = 1;
            task.objective_str = objetiveName;

            //**Por ahora si no tiene el ING que lo busque, en el futuro veremos eso de construirlos si no hay o lo que ea. 
            /*if (!ing.productFinal_b && !ing.productFinal_WithPlate_b) //Cocinar
            {
                Task_Gameplay task = new Task_Gameplay();
                task.typeOfTask_i = 0;
                task.objective_str = objetiveName;
                tasksSecondary.Add(task);
            }

            if (ing.productFinal_b || ing.productFinal_WithPlate_b) //Preparar
            {
                Task_Gameplay task = new Task_Gameplay();
                task.typeOfTask_i = 0;
                task.objective_str = objetiveName;
                tasksSecondary.Add(task);
            }*/

            //*Este es el código de (IA NPC) Set Order Procesed,cortado, ver si unifacarlo haciendolo método
            Ingredients_Gameplay ing = manager.Get_Ingredient_By_Name_InGame(objetiveName);

            try
            {
                if (ing.productStart_b)
                {
                    for (int j = 0; j < manager.items_parent_kitchen_go.transform.childCount; j++)
                    {
                        Item_GamePlay item = manager.items_parent_kitchen_go.transform.GetChild(j).GetComponent<Item_GamePlay>();

                        if (item.esUnDispensador_b)
                        {
                            for (int ii = 0; ii < item.ingredientsInItem.Count; ii++)
                            {
                                Ingredients_Gameplay ing2 = item.ingredientsInItem[ii];

                                if (ing.nameIngredient_str == ing2.nameIngredient_str)
                                {
                                    GameObject npc = FindAnyObjectByType<Game_Manager>().Get_NpcPrefab_ByName(nameNpc);
                                    item.Npc_AddRemove(true, npc);
                                    task.item_r = item;

                                    break;
                                }
                            }
                        }
                    }
                    if (task.item_r == null) { Debug.Log("Ingrediente no encontrado en dispensador??"); }
                }
                else
                {
                    //Ver los items que produzcan ese objeto.
                    for (int j = 0; j < manager.items_parent_kitchen_go.transform.childCount; j++)
                    {
                        Item_GamePlay item = manager.items_parent_kitchen_go.transform.GetChild(j).GetComponent<Item_GamePlay>();

                        if (!item.esUnDispensador_b)
                        {
                            for (int ii = 0; ii < item.ingredientsAndProducts_r.Count; ii++)
                            {
                                Ingredients_Gameplay ing2 = item.ingredientsAndProducts_r[ii].GetComponent<Item_IngredientsAndProducts_Gameplay>().product[0].GetComponent<Ingredients_Gameplay>();
                                if (ing.nameIngredient_str == ing2.nameIngredient_str)
                                {
                                    task.item_r = item;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Traer Ing no encontrado: " + objetiveName);
            }           

            //-------

            ordersSecond_r.Add(task);
        }
    }

}
