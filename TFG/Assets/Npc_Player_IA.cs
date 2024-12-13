using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public class Npc_Player_IA : MonoBehaviour
{
    Game_Manager game_manager_r;
    UI_Manager ui_manager_r;
    public Npc_Player_Gameplay data_Npc_r;
    public NavMeshAgent agent_r;

    [Header("Gameplay")]
    public bool resting_b = true; //Sin tareas
    public bool working_b = false;
    public bool goTo_b = false;
    Vector3 destination_v3 = Vector3.zero;
    GameObject whereToStandUp_item_go; //Todo: *Cuando ocupa el lugar esta inactivo,y cuando lo deja está en activo y null para que lo active otro.

    //Inventario
    int countMaxIngredientsInNpc = 2; //Max de ingredientes. //**Pos y si se consiguiera con mejora?
    int countMaxIngredientInSlot = 20; //Max Inventario por ingrediente.
    public List<Ingredients_Gameplay> ingredientsInNpc_r = new List<Ingredients_Gameplay>(); //
    public GameObject ingredientsVisual_parent_go;//Simplemente para que se muestre la cantidad que has fabricado. //Solo unap osición para spawn cosas

    [Header("Visual")]
    public AudioSource audio_source;
    
    [Header("Tasks - Tareas")]
    public int countOrdersNow_i = 0;
    int idOrders_i = 0;
    public List<Task_Gameplay> orders_r = new List<Task_Gameplay>();
    
    public bool ordersSecond_b = false;
    public int countOrdersSecondNow_i = 0;

    public float timerNpc_f = 0;

    //Todo:hacer lista secundaria para cuando la cocina arda o este en estado de arreglar,dejando la primera sin continuar.
    
    [Header("Temp - Saber...")] //Saber cuantos ingredientes tiene y que tareas tiene actualmente.
    public List<string> ingredientsCount_list_i = new List<string>();
    public List<string> ordersName_list_i = new List<string>();
    public List<string> ordersSecondName_list_i = new List<string>();

    bool goToResting_b = false;

    void Start()
    {
        game_manager_r = FindAnyObjectByType<Game_Manager>();
        ui_manager_r = FindAnyObjectByType<UI_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!resting_b) 
        {
            if (goTo_b)
            {
                if (Vector3.Distance(agent_r.transform.position, agent_r.destination) < 2)
                {
                    goTo_b = false;
                    working_b = true;

                    audio_source.Stop();

                    agent_r.enabled = false;
                    agent_r.transform.position = destination_v3;//Todo: Hacer un translate. y rot.
                    agent_r.enabled = true;

                    if (!ordersSecond_b)
                    {
                        orders_r[countOrdersNow_i].item_r.Update_ItemWork(true, this.gameObject);
                    }
                    else
                    {
                        orders_r[countOrdersNow_i].ordersSecond_r[countOrdersSecondNow_i].item_r.Update_ItemWork(true, this.gameObject);
                    }
                }
            }
        }

        if (!goTo_b && !working_b && orders_r.Count > 0) //Cada cierto tiempo vea si un npc se ha quedado sin tareas y vea si ya le ha desbloqueado alguna.
        {
            timerNpc_f += Time.deltaTime;

            if (timerNpc_f > 7)
            {
                Finished_Action_Kitchen();

                timerNpc_f = 0;

                Debug.LogError("Reactivar NPC: " + data_Npc_r.nameNpc_str);
            }
        }

        if (resting_b && !goToResting_b)
        {
            resting_b = true;
            goToResting_b = true;
            int x = Random.Range(1, 5);
            agent_r.SetDestination(game_manager_r.restingpositions_parent_kitchen_go.transform.GetChild(x - 1).transform.position);
        }
    }
    
    //Corrige los JSON teniendo en cuenta el lenguaje humano. //Comprueba si es posible la tarea.
    public void Set_Orders(int type, string taskName, string objetiveName, int tono /*si metemos diccionarios cambiar,por ahora sirve, creo que podríamos obtener un int de cada palabra y listo*/)
    {
        Debug.LogError("Tipo:" + type + "Tarea enviada:" + taskName + " Objetivo: " + objetiveName);

        if (type == 1) //Traer //No hagas nada si es 
        {
            Set_Orders_Processed(type, taskName, objetiveName);
        }

       /* if (type == 2) //Limpiar 
        {
            //Todo: Ver si objecName se puede limpiar
            //Por ahora solo hay platos, se podría hacer un for y ver sus tareas y comparar con IF pero si metemos cosas como suelo,mesas y otros como lo haríamos? Otro list de solo cosas limpiables y sacarlos de ahi aunque no sean ingredietnes.
            
            //meter suelos y mesas
            
            if (objetiveName.Contains("Plato"))
            {
                Set_Orders_Processed(type, taskName, objetiveName);
            }
        }*/

        if (type == 0 || type == 2) //Cocinar //Tdo:Para limpiar items hay que cambiar el formato
        {
            string ingredientChecked = null;

            Ingredients_Gameplay go = game_manager_r.Get_Ingredient_By_Name_InGame(objetiveName);

            try
            {             
                bool tryVariant = true;

                //ver si tiene esa tarea,si la tiene hazla. Sino, ver si sus variaciones las tiene,si es así, que ellas sean el objetivo
                for (int j = 0; j < go.taskAffectedIt.Count; j++)
                {
                    if (go.GetComponent<Ingredients_Gameplay>().taskAffectedIt[j] == taskName)
                    {
                        tryVariant = false;

                        ingredientChecked = objetiveName;
                    }
                }

                if (tryVariant) //Ver si sus variantes tienen la tarea
                {
                    ingredientChecked = null;

                    for (int j = 0; j < go.ingredientsToAffectIfIwasSaid_.Count; j++)
                    {
                        Ingredients_Gameplay goChild = go.ingredientsToAffectIfIwasSaid_[j].GetComponent<Ingredients_Gameplay>();

                        for (int ii = 0; ii < goChild.taskAffectedIt.Count; ii++)
                        {
                            if (goChild.taskAffectedIt[ii] == taskName)
                            {
                                ingredientChecked = goChild.nameIngredient_str;
                            }
                        }
                    }
                }
            }
            catch
            {
               Debug.LogError("Esto no debería pasar nunca, ya que en el Manager se asegura que el ingrediente existe o su vatiación.");
            }

            if (ingredientChecked != null)
            {
                Debug.Log("Orden re-procesada, Cocina, tarea: " + taskName + " Ingrediente: " + ingredientChecked);
                Set_Orders_Processed(type, taskName, ingredientChecked);
            }
        }
    }
    

    void Set_Orders_Processed(int type, string taskName, string objetiveName)
        /*si metemos diccionarios cambiar,por ahora sirve, creo que podríamos obtener un int de cada palabra y listo*/ //Ejecuta los comandos
    {        
        //0º Ver si la tarea está repetida, para no añadirla.
        bool itsRepeated = false;

        for (int i = 0; i < orders_r.Count; i++)
        {
            if (orders_r[i].item_r.taskName == taskName && orders_r[i].objective_str == objetiveName)
            {
                itsRepeated = true;
                Debug.Log("Orden re-procesada, REPETIDA");

                break;
            }
            /*else
            {
                Debug.Log("Orden re-procesada, REPETIDA");
                Debug.Log(orders_r[i].item_r.nameItem);
            }*/
        }

        if (!itsRepeated)
        {
            //0º Crea la orden.
            idOrders_i++;

            Task_Gameplay task = new Task_Gameplay();
            task.typeOfTask_i = type;
            task.objective_str = objetiveName;
            task.turnCreation_i = game_manager_r.turnPrompt_i;
            task.idTask_i = idOrders_i;

            if (orders_r.Count > 0)
            {                
                //1º Ver si hay una tarea vieja igual en este npc, y borrala. //Las tareas nuevas sustituyen a las viejas. Es decir, si estabas cortando patatas y te digo luego,"corta jamon", ahora querré que hagas eso,a no ser que diga corta patatas y jamón,para eso es el bool. Es por diseño.
                //**Si fuera necesario como es por diseño se quita por repeticiones y no por antiguedad. Solo quitar el if y el for donde pide destruir.

                int countRepeatTask = 0;
                List<int> deleteTasks = new List<int>(); //por si hay varios.

                for (int i = 0; i < orders_r.Count; i++)
                {
                    if (orders_r[i].item_r.taskName == taskName)
                    {
                        countRepeatTask++;
                    }

                    if (orders_r[i].item_r.taskName == taskName && orders_r[i].turnCreation_i != game_manager_r.turnPrompt_i)
                    {
                        deleteTasks.Add(i);
                        countRepeatTask--;
                    }
                }
                
                //Un npc solo puede usar una maquina del mismo tipo a la vez,no puede tener multiples tareas de cortar en diferentes máquinas,aunque el maximo de tareas sea limitado, pun mismo objeto que lo permita puede cortar varios ingredientes diferentees.Se les asigna una cantidad por defecto para que no sea hacer uno y cambiar,eso se puede predetrminar en opciones para que siempre
                //sea así o ditarlo solo para esa situación en concreto.

                for (int i = 0; i < deleteTasks.Count; i++)
                {
                    task.item_r = orders_r[deleteTasks[i]].item_r; //Para que siga haciendo la tarea donde estaba. 

                    Destroy_Task(deleteTasks[i], false);
                }

                //2º Quita la primera tarea en caso de haber demasiadas.
                if (countRepeatTask + 1 >= game_manager_r.maxRepeatTask)
                {
                    for (int i = 0; i < orders_r.Count; i++)
                    {
                        if (orders_r[i].item_r.taskName == taskName)
                        {
                            task.item_r = orders_r[i].item_r; //Para que siga haciendo la tarea donde ya lo estaba haciendo. 

                            Destroy_Task(i, false); //Quitar el primero repetido

                            break;
                        }
                    }
                }

                //3º
                if (orders_r.Count + 1 >= game_manager_r.maxCountTask)
                {
                    task.item_r = orders_r[0].item_r; //Quitar el primero repetido

                    //ver si la primera tiene repeticiones,si la tiene que no lo quite del item ,sino si quitala. Ej: Das tres órdenes de cortar cosas diferentes, pues si el max son dos quita la primera
                    bool deleteNpcFromItem = true;
                    int count = 0;
                    for (int i = 1; i < orders_r.Count; i++)//Empiezo en 1 para que ignore así mismo.
                    {
                        if (orders_r[0].item_r.nameItem == orders_r[i].item_r.nameItem)
                        {
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        deleteNpcFromItem = false;
                    }
                    Destroy_Task(deleteTasks[0], deleteNpcFromItem);
                }

                //Todo:Revisar creo que esta mal. //4º Añade la orden.
                if (task.item_r != null)
                {
                    orders_r.Add(task);
                }
            }

            if (orders_r.Count == 0 || task.item_r == null)
            {
                if (type == 0)
                {
                    //1º Buscamos el Item a usar, si está ocupado iguamente se le asigna, a ver si se desocupa en el futuro.
                    task.item_r = Get_ItemKitchenClosest_Free_ByTask(taskName);
                }

                if (type == 1 || type == 2)//Todo:Tocar para platos,solo hay platos si limpias mesa no va //Encontrar el item que contiene el ingrediente. 
                {                
                    Ingredients_Gameplay ing = game_manager_r.Get_Ingredient_By_Name_InGame(objetiveName);

                    try
                    {
                        if (ing.productStart_b)
                        {
                            //No funcionaría con Get_ItemKitchenClosest_Free_ByTask("Traer") porque no sé si tienen los ingredientes

                            //Ver los items que produzcan ese objeto.
                            //Todo: Hacer un list elegir el más cercano.
                            for (int j = 0; j < game_manager_r.items_parent_kitchen_go.transform.childCount; j++)
                            {
                                Item_GamePlay item = game_manager_r.items_parent_kitchen_go.transform.GetChild(j).GetComponent<Item_GamePlay>();

                                if (item.esUnDispensador_b)
                                {
                                    for (int ii = 0; ii < item.ingredientsInItem.Count; ii++)
                                    {
                                        Ingredients_Gameplay ing2 = item.ingredientsInItem[ii];

                                        if (ing.nameIngredient_str == ing2.nameIngredient_str)
                                        {
                                            task.item_r = item;
                                            task.item_r.GetComponent<Item_GamePlay>().Npc_AddRemove(true, this.gameObject);

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
                            for (int j = 0; j < game_manager_r.items_parent_kitchen_go.transform.childCount; j++)
                            {
                                Item_GamePlay item = game_manager_r.items_parent_kitchen_go.transform.GetChild(j).GetComponent<Item_GamePlay>();

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
                }

                //3.5º Asignar idTask
                if (orders_r.Count > 0)
                {
                    task.idTask_i = orders_r[orders_r.Count - 1].idTask_i + 1;
                }
                else
                {
                    task.idTask_i = 1;
                }

                //4º Añade la orden.
                orders_r.Add(task);

                data_Npc_r.ui_manager_r.Create_IconTask_UI(this.gameObject, task);
            }      
        }
    }
    
    public Item_GamePlay Get_ItemKitchenClosest_Free_ByTask(string taskName)//TEmp sustituir por int cuando haya diccionario
    {
        GameObject parent = game_manager_r.items_parent_kitchen_go;
        GameObject itemFree = null;
        List<GameObject> items = new List<GameObject>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (taskName == parent.transform.GetChild(i).GetComponent<Item_GamePlay>().taskName) //Todo: y que no estropeada,ardiendo,etc.
            {               
                if (parent.transform.GetChild(i).GetComponent<Item_GamePlay>().npc_UsingIt_r.Count < parent.transform.GetChild(i).GetComponent<Item_GamePlay>().maxNpcAtSameTime_i)
                {
                    items.Add(parent.transform.GetChild(i).gameObject);
                }
            }
        }

        if (items.Count > 0) //Devolver el más cercano.
        {
            float closest = 9999;
            GameObject itemClosest = null;

            for (int i = 0; i < items.Count; i++)
            {
                if (Vector3.Distance(this.transform.position, items[i].transform.position) < closest)
                {
                    closest = Vector3.Distance(this.transform.position, items[i].transform.position);
                    itemClosest = items[i];
                }
            }

            itemFree = itemClosest;
            itemFree.GetComponent<Item_GamePlay>().Npc_AddRemove(true, this.gameObject);
        }
        else //Cogemos el más cercano, y a la lista de espera.
        {
            float closest = 9999;
            GameObject itemClosest = null;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                if (taskName == parent.transform.GetChild(i).GetComponent<Item_GamePlay>().taskName) //Todo: y que no estropeada,ardiendo,etc.
                {
                    if (Vector3.Distance(this.transform.position, parent.transform.GetChild(i).transform.position) < closest)
                    {
                        closest = Vector3.Distance(this.transform.position, parent.transform.GetChild(i).position);
                        itemClosest = parent.transform.GetChild(i).gameObject;
                    }
                }
            }

            itemFree = itemClosest;
            itemFree.GetComponent<Item_GamePlay>().Npc_AddRemove(true, this.gameObject);
        }

        return itemFree.GetComponent<Item_GamePlay>();
    }

    public void Destroy_Task(int positionArray, bool removeNpcFromItem)
    {
        //Eliminar - Npc Del Objeto
        if (removeNpcFromItem) //En las tareas repetidas siempre es false, porque al no cambiar de sitio, no es necesario remover el NPC de la lista.
        {
            orders_r[positionArray].item_r.Npc_AddRemove(false, this.gameObject);
        }

        if (positionArray == countOrdersNow_i) //Para que si la tarea a eliminar se está ejecutando, no de error.
        {
            bool ordSecond = ordersSecond_b;
            
            if (ordSecond)
            {               
                orders_r[countOrdersNow_i].ordersSecond_r[countOrdersSecondNow_i].item_r.Update_ItemWork(false, this.gameObject); //Copiado del método Finish_Action-Kitchen()

                //
                Finished_Action_Kitchen_OrderSecond();
                
                ordSecond = false;
            }        
            
            if (!ordSecond)
            {
                orders_r[countOrdersNow_i].item_r.Update_ItemWork(false, this.gameObject);

                if (orders_r.Count == 1)
                {
                    resting_b = true;
                    countOrdersNow_i = -1;

                    orders_r.RemoveAt(positionArray);

                    //Todo: Iniciar anim de resting
                }

                if (orders_r.Count > 1)
                {
                    countOrdersNow_i = 0; //Por ahora si quitas una tarea y es esa, empieza por la primera.

                    orders_r.RemoveAt(positionArray);
                    StartNext_Action_Kitchen();
                }
            }
        }
        else
        {
            orders_r.RemoveAt(positionArray);
        }

        //Actualizar Animación del NPC
        if (orders_r.Count == 0)
        {
            //Anim Reposo Idle

        }
        else
        {
            //Que inicie siguiente Task
        }
    }

    void StartNext_Action_Kitchen() 
    {
        goToResting_b = false;
        Show_Values_Temp();

        if (!ordersSecond_b)
        {
            if (orders_r[countOrdersNow_i].typeOfTask_i > -10) //**11//Cocinar
            {
                if (orders_r[countOrdersNow_i].item_r.Get_NpcIA_WAITING_By_Name(data_Npc_r.nameNpc_str) == null)
                {
                    bool enoughIngredients = orders_r[countOrdersNow_i].item_r.Check_EnoughIngredients(orders_r[countOrdersNow_i].objective_str, data_Npc_r.nameNpc_str);

                    if (!enoughIngredients)
                    {
                        //Activar Seconds Tasks
                        ordersSecond_b = true;
                        countOrdersSecondNow_i = 0;
                        ui_manager_r.Show_NpcPanel_OrdersSeconds(data_Npc_r.nameNpc_str, true);

                        goTo_b = true;
                        destination_v3 = orders_r[countOrdersNow_i].ordersSecond_r[0].item_r.Check_Npc_WhereHasToStandUp();
                        agent_r.SetDestination(destination_v3);
                    }
                    else
                    {
                        //Ver si ya está ahí, es su última tarea
                        if (Vector3.Distance(agent_r.transform.position, orders_r[countOrdersNow_i].item_r.Check_Npc_WhereHasToStandUp()) > 0.2) //Esta en lugar diferente
                        {
                            orders_r[countOrdersNow_i].targetReached_b = false; //Si ya ha llegado al sitio que no se vaya y solo siga haciendo la actividad,esto es por si se queda solo con una actividad.

                            //Todo:Hacer método
                            goTo_b = true;
                            destination_v3 = orders_r[countOrdersNow_i].item_r.Check_Npc_WhereHasToStandUp();
                            agent_r.SetDestination(destination_v3);

                            //Todo:Activar Anim Correr
                            audio_source.Play();

                        }
                    }

                    timerNpc_f = 0; //Reset
                    Show_Values_Temp();
                }
                else
                {
                    Debug.LogError("No puede hacer esta tarea, Npc:" + data_Npc_r.nameNpc_str);
                }
            }
        }
        else
        {
            //Ver si ya está ahí, es su última tarea
            if (Vector3.Distance(agent_r.transform.position, orders_r[countOrdersNow_i].item_r.Check_Npc_WhereHasToStandUp()) > 0.2) //Esta en lugar diferente
            {
                //Todo:Hacer método
                goTo_b = true;
                destination_v3 = orders_r[countOrdersNow_i].ordersSecond_r[countOrdersSecondNow_i].item_r.Check_Npc_WhereHasToStandUp();
                agent_r.SetDestination(destination_v3);

                //Todo:Activar Anim Correr
                audio_source.Play();

            }
        }
    }
    
    public void Finished_Action_Kitchen() //*Hacer método que calcule el final del array para utilizarlo por otros métodos,se le pasa la longitud del array y te devuleva la posicion,irá en el manager
    {
        working_b = false;

        Show_Values_Temp();
        StartWorking();

        if (!ordersSecond_b) 
        {
            //
            orders_r[countOrdersNow_i].item_r.Update_ItemWork(false, this.gameObject);

            countOrdersNow_i++;

            if (countOrdersNow_i > orders_r.Count - 1)
            {
                countOrdersNow_i = 0;
            }
        }
        else
        {
            //
            orders_r[countOrdersNow_i].ordersSecond_r[countOrdersSecondNow_i].item_r.Update_ItemWork(false, this.gameObject);

            //*Esta es la parte que digo hacer método,repito este codigo mucho
            countOrdersSecondNow_i++;

            if (countOrdersSecondNow_i > countOrdersSecondNow_i - 1)
            {
                Finished_Action_Kitchen_OrderSecond();
            }
        }

        StartNext_Action_Kitchen();
    }

    void Finished_Action_Kitchen_OrderSecond()
    {
        //Terminar
        ordersSecond_b = false;

        for (int i = 0; i < orders_r[countOrdersNow_i].ordersSecond_r.Count; i++) //Sacar los NPC de todos los items.
        {
            if (orders_r[countOrdersNow_i].ordersSecond_r[i].item_r.nameItem != orders_r[countOrdersNow_i].item_r.nameItem) //Si la orden secundaria ocurre en el mismo item que el primero, no se le puede quitar
            {
                bool doIt = false;
                for (int j = 0; j < orders_r.Count; j++)
                {
                    if (orders_r[j].item_r.nameItem == orders_r[countOrdersNow_i].ordersSecond_r[i].item_r.nameItem) //Si alguna orden usa ese item,tampoco se quita.
                    {
                        doIt = true;
                    }
                }

                if (!doIt)
                {
                    orders_r[countOrdersNow_i].ordersSecond_r[i].item_r.Npc_AddRemove(false, this.gameObject);
                }
            }
        }

        orders_r[countOrdersNow_i].ordersSecond_r.Clear();
        ui_manager_r.Show_NpcPanel_OrdersSeconds(data_Npc_r.nameNpc_str, false);
    }

    public void StartWorking()
    {
        if (resting_b && orders_r.Count > 0)
        {
            resting_b = false;
            countOrdersNow_i = 0;
            StartNext_Action_Kitchen();
        }
    }
    
    void Show_Values_Temp()
    {
        ingredientsCount_list_i.Clear();
        ordersSecondName_list_i.Clear();
        ordersName_list_i.Clear();

        for (int i = 0; i < ingredientsInNpc_r.Count; i++)
        {
            ingredientsCount_list_i.Add(ingredientsInNpc_r[i].nameIngredient_str + " - " + ingredientsInNpc_r[i].countInItem_i);
        }
        
        for (int i = 0; i < orders_r.Count; i++)
        {
            ordersName_list_i.Add(orders_r[i].item_r.nameItem + " - " + orders_r[i].objective_str);
        }

        if (ordersSecond_b)
        {
            for (int i = 0; i < orders_r[countOrdersNow_i].ordersSecond_r.Count; i++)
            {
                ordersSecondName_list_i.Add(orders_r[countOrdersNow_i].ordersSecond_r[i].objective_str + " - " + orders_r[countOrdersNow_i].ordersSecond_r[i].item_r.nameItem);
            }
        }
    }

    public Ingredients_Gameplay Get_IngredientsInNpc_By_Name(string nameIng) //*Insisto hacer un id y una nueva clase, idIng - Ingredient que llame al prefab original y Count
    {
        Ingredients_Gameplay ing = null;

        for (int i = 0; i < ingredientsInNpc_r.Count; i++)
        {
            if (ingredientsInNpc_r[i].nameIngredient_str == nameIng)
            {
                ing = ingredientsInNpc_r[i];
            }
        }

        return ing;
    }

    public void DestroyThisNpc()
    {
        Destroy(this.gameObject);   
    }
}
