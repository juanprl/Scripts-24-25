using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;

public class Item_GamePlay : MonoBehaviour
{
    [Header("Data")]
    public string nameItem = "";
    public Sprite item_sprite;
    public string taskName = "";
    public int moneyPrice_i = 0;

    public string nameCookingNow_str = "";

    [Header("")]
    public Game_Manager game_manager_r;

    [Header("Item Gameplay")] //Solo se puede hacer un plato de ingrediente a la vez por Item.
    public int maxNpcAtSameTime_i = 1;
    public List<GameObject> npc_UsingIt_r = new List<GameObject>(); 
    public List<GameObject> npc_WaitingForIt_r = new List<GameObject>(); //Lista de espera. //Como se procesan las órdenes,se hace de forma líneal o de pila, tal vez un objeto puede aparecer lleno, pero una vez se procesen todas las órdenes puede que acabe vacío.
    public List<GameObject> npc_UsingItRightNow_r = new List<GameObject>();

    [Header("")] //Solo se puede hacer un plato de ingrediente a la vez por Item.
    public List<GameObject> npc_PlaceToBe_r = new List<GameObject>();//Donde se colocan los NPC //Todo:Hacer un Get para el lugar correcto,mientras todos al 1
    public GameObject ingredientsVisual_parents_go;//Simplemente para que se muestre la cantidad que has fabricado.
    public List<GameObject> npc_IngredientsToBe_r = new List<GameObject>();//Donde se colocan los Ingredients //Todo:Hacer un Get para el lugar correcto,mientras todos al 1

    bool enoughIngredients_b = false; //Ver si sigue haciend ofalta //**Todo: Cronometro no puede empezar si es bool false //Todo:Si hay dos ingredientes necesita cambios //Pos:versi solo una maquina un ingrediente.
    public int countMaxProductsMakeEachTime = 6;

    [Header("Kitchen")]
    int countMaxIngredientInItem = 100; //Max Inventario por ingrediente.
    public List<Ingredients_Gameplay> ingredientsInItem = new List<Ingredients_Gameplay>(); //*Solo hay un ingrediente de cada tipo,Ej: si hay 100 cebollas, aquí solo hay una y un int de 100, sino quedan el valor es 0
    public List<GameObject> ingredientsAndProducts_r = new List<GameObject>();

    [Header("Cronometro")]
    public float timeToFinishAction_f = 8;
    float timeToFinishAction_Real_f = -2;//Aquí se aplican los buffos o nerfeos

    public bool timeStart_b = false;
    public float timeRecord_f = 0;

    [Header("Mostrador")]
    public bool esUnMostrador_b = false; //la comida se deja aquí.
    
    [Header("Dispensadores")]
    public bool esUnDispensador_b = false;
    public List<int> countIngredientsInItemAtStart = new List<int>();
    
    [Header("Objetos Ensuciables")]
    public bool seEnsucia_b = false;
    public List<GameObject> rats_go = new List<GameObject>();

    public int porcentajeAumentarPorTiempo = 2; //la idea es que cada 3 segs aumente un 2% es deir en 300seg/5mins saldrá una rata. 
    public float tiempoQueAumentaporcentaje = 3;

    [Header("Visual")]
    public AudioSource source_sound;
    public GameObject partycles_go;

    [Header("Temp - Saber...")] //Todo esto porque los script no son serializables, o no sé como todavía,para saber su contenido.
    public List<string> npcsName_list_str = new List<string>();
    public List<string> ingredientsName_list_str = new List<string>();

    void Start()
    {
        if (esUnDispensador_b)
        {
            List<Ingredients_Gameplay> ingredientsNew = new List<Ingredients_Gameplay>();

            for (int i = 0; i < ingredientsInItem.Count; i++)
            {
                Ingredients_Gameplay ingTemp = new Ingredients_Gameplay();
                ingTemp.countInItem_i = countIngredientsInItemAtStart[0];

                ingTemp.nameIngredient_str = ingredientsInItem[i].nameIngredient_str;
                ingTemp.productStart_b = ingredientsInItem[i].productStart_b;
                ingTemp.productFinal_b = ingredientsInItem[i].productFinal_b;
                ingTemp.productFinal_WithPlate_b = ingredientsInItem[i].productFinal_WithPlate_b;
                ingTemp.thisGameObject_go = ingredientsInItem[i].thisGameObject_go;
                ingredientsNew.Add(ingTemp);
            }

            ingredientsInItem = ingredientsNew; //Sino hacemos esto, modificaremos el Prefab.
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (timeStart_b)
        {
            timeRecord_f += Time.deltaTime;

            if (timeRecord_f > timeToFinishAction_Real_f)
            {
                Set_Products();

                timeStart_b = false;
                source_sound.Stop();
                if (partycles_go)
                {
                    partycles_go.SetActive(false);
                }
            }
        }
    }

    public void Npc_AddRemove(bool addNpc, GameObject prefabNpc)
    {
        string npcName = prefabNpc.GetComponent<Npc_Player_Gameplay>().nameNpc_str;

        if (addNpc)
        {
            try //Para no repetir,si puede hacer ya está en item 
            {
                Get_NpcIA_By_Name(npcName).GetComponent<Npc_Player_Gameplay>();
                Get_NpcIA_WAITING_By_Name(npcName).GetComponent<Npc_Player_Gameplay>();
            }
            catch
            {
                if (npc_UsingIt_r.Count < maxNpcAtSameTime_i)
                {
                    npc_UsingIt_r.Add(prefabNpc);
                }
                else
                {
                    npc_WaitingForIt_r.Add(prefabNpc);
                }
            }
        }
        else
        {
            int count = -999;
            for (int i = 0; i < npc_UsingIt_r.Count; i++)
            {
                if (npc_UsingIt_r[i].GetComponent<Npc_Player_Gameplay>().nameNpc_str == prefabNpc.GetComponent<Npc_Player_Gameplay>().nameNpc_str)
                {
                    npc_UsingIt_r.RemoveAt(i);
                    count = 999;

                    break;
                }
            }

            if (count < 0)
            {
                for (int i = 0; i < npc_WaitingForIt_r.Count; i++)
                {
                    if (npc_WaitingForIt_r[i].GetComponent<Npc_Player_Gameplay>().nameNpc_str == prefabNpc.GetComponent<Npc_Player_Gameplay>().nameNpc_str)
                    {
                        npc_WaitingForIt_r.RemoveAt(i);
                        count = 999;

                        break;
                    }
                }

                if (count < 0)
                {
                    Debug.LogError("Mmmm, creo que no debería pasar");
                }
            }

            //Añadir a la lista principal
            if (npc_UsingIt_r.Count < maxNpcAtSameTime_i && npc_WaitingForIt_r.Count > 0)
            {
                GameObject go = npc_WaitingForIt_r[0];
                npc_UsingIt_r.Add(go);
                npc_WaitingForIt_r.RemoveAt(0);
            }
        }

        Show_NameNpcs();
    }

    public void Update_ItemWork(bool usingIt, GameObject npc) 
    {
        if (usingIt) { npc_UsingItRightNow_r.Add(npc); }
        if (!usingIt) 
        {
            for (int i = 0; i < npc_UsingItRightNow_r.Count; i++)
            {
                if (npc.GetComponent<Npc_Player_Gameplay>().nameNpc_str == npc_UsingItRightNow_r[i].GetComponent<Npc_Player_Gameplay>().nameNpc_str)
                {
                    npc_UsingItRightNow_r.RemoveAt(i);
                    break;
                }
            } 
        }

        if (npc_UsingItRightNow_r.Count == 0)//Parar Cronometro
        {
            timeStart_b = false;

            //Parar Anim

            //*Para no ser hijos de puta al quedarse sin nadie apagan la cocina? O eso que vaya por nivel del objeto o habilidades del NPC?

            //Visuals
            source_sound.Stop();
            if (partycles_go)
            {
                partycles_go.SetActive(false);
            }
        }
        else
        {
            if (npc_UsingItRightNow_r.Count == 1) //Iniciar Cronometro.
            {
                timeStart_b = true;
                timeRecord_f = 0;

                timeToFinishAction_Real_f = timeToFinishAction_f;
                source_sound.Play();
                if (partycles_go)
                {
                    partycles_go.SetActive(true);
                }
            }
            else //Ajustar tiempo para terinar tareas
            {
                timeToFinishAction_Real_f = timeToFinishAction_f / npc_UsingIt_r.Count; //Recalcular
            }
        }
    }

    public void AddRemove_Ingredients(int remove, int count, string ingProduct) //Todo:Si esto llega a la versión final,tenemos que cambiar las clases de ingr y crear otra que sea 2 cosas, un count que solo afecte al ing en este item y un public ing para poner el prefab para sacr la info pero no afectarla
    {       
        for (int iii = 0; iii < npc_UsingItRightNow_r.Count; iii++)
        {
            Npc_Player_IA npc = npc_UsingItRightNow_r[iii].GetComponent<Npc_Player_IA>();
            string ingredientPrefab_Name = npc.orders_r[npc.countOrdersNow_i].objective_str; //Todo:Sustituir por CookingNow - Global

            if (remove == 1) //Del item al NPC
            {
                Ingredients_Gameplay ing = Get_IngredientInItem_By_Name(ingredientPrefab_Name);

                try                
                {
                    if (count == -99) {  count = countMaxProductsMakeEachTime; }
                    if (count > ing.countInItem_i) { count = ing.countInItem_i; }

                    ing.countInItem_i -= count;

                    //Añadirlo al NPC
                    bool haveIt = false;
                    for (int j = 0; j < npc.ingredientsInNpc_r.Count; j++) //Ver si tiene ese ingrediente
                    {
                        if (npc.ingredientsInNpc_r[j].nameIngredient_str == ingredientPrefab_Name)
                        {
                            npc.ingredientsInNpc_r[j].countInItem_i += count;
                            haveIt = true;

                            break;
                        }
                    }

                    if (!haveIt)
                    {
                        Ingredients_Gameplay ingredient = new Ingredients_Gameplay();
                        ingredient.nameIngredient_str = ingredientPrefab_Name;
                        ingredient.countInItem_i = count;

                        npc.ingredientsInNpc_r.Add(ingredient);
                    }

                    //este método tiene un error o algo revisar
                    //Ingredients_Anim(ing.nameIngredient_str, iii, true);
                }
                catch
                {
                    Debug.Log(nameItem + " . No ha encontrado en el dispensador: " + ingredientPrefab_Name);
                }          
            }

            if(remove == 2) //Del NPC al objeto.
            {
                for (int i = 0; i < npc.ingredientsInNpc_r.Count; i++) //Quitar al NPC
                {
                    if (npc.ingredientsInNpc_r[i].nameIngredient_str == ingredientPrefab_Name)
                    {
                        if (count == -99) { count = countMaxProductsMakeEachTime; }
                        if (count > npc.ingredientsInNpc_r[i].countInItem_i) { count = npc.ingredientsInNpc_r[i].countInItem_i; }

                        npc.ingredientsInNpc_r[i].countInItem_i -= count;
                        break;
                    }
                }

                //Añadirselo a item
                bool doIt = false;//Buscar si está ese ingrediente en esta lista, sino añadelo.

                for (int i = 0; i < ingredientsInItem.Count; i++)
                {
                    if (ingredientsInItem[i].nameIngredient_str == ingredientPrefab_Name)//si está, cambia el nº de este
                    {
                        ingredientsInItem[i].countInItem_i += count;
                        doIt = true;
                        break;
                    }
                }

                if (!doIt)
                {
                    Ingredients_Gameplay ingredient = new Ingredients_Gameplay();
                    ingredient.nameIngredient_str = ingredientPrefab_Name;
                    ingredient.countInItem_i = count;

                    ingredientsInItem.Add(ingredient);
                }

                //Ver si iniciar animación
                //Ingredients_Anim(ingredientsInItem[i].thisGameObject_go, count, true);

            }
            
            if(remove == 3) //Del item al item, cocinar.
            {
                //Es un poco desastre,pero le hemos descontado los ingredientes ya antesen Set_order. Ahora hay que ver si el producto se ha cocinado
                //antes,aunque se solucionaría si el Item tuviera ese ingrediente desde el principio.

                //Añadirselo a item
                bool doIt = false;//Buscar si está ese ingrediente en esta lista, sino añadelo.

                for (int i = 0; i < ingredientsInItem.Count; i++)
                {
                    if (ingredientsInItem[i].nameIngredient_str == ingProduct)//si está, cambia el nº de este
                    {
                        ingredientsInItem[i].countInItem_i += count;
                        doIt = true;
                        break;
                    }
                }

                if (!doIt)
                {
                    Ingredients_Gameplay ingredient = new Ingredients_Gameplay();
                    ingredient.nameIngredient_str = ingProduct;
                    ingredient.countInItem_i = count;

                    ingredientsInItem.Add(ingredient);
                }

                //Ver si iniciar animación
                //Ingredients_Anim(ingredientsInItem[i].thisGameObject_go, count, true);

            }
        }

        Show_NameNpcs();
    }

    void Set_Products()
    {
        Npc_Player_IA npc = npc_UsingItRightNow_r[0].GetComponent<Npc_Player_IA>();//*Lo uso como referencia ya que cada item en teoría solo puede hacer una cosa a la vez.

        Task_Gameplay order = npc.orders_r[npc.countOrdersNow_i];
        if (npc.ordersSecond_b)
        {
            order = npc.orders_r[npc.countOrdersNow_i].ordersSecond_r[npc.countOrdersSecondNow_i];
        }

        if (order.typeOfTask_i == 0)
        {
            AddRemove_Ingredients(2, -99, null);
            Set_Products_Cooking(false, "");
        }

        if (order.typeOfTask_i == 1)
        {
            AddRemove_Ingredients(1, -99, null);
        }

        for (int i = 0; i < npc_UsingItRightNow_r.Count; i++)
        {
            npc_UsingItRightNow_r[i].GetComponent<Npc_Player_IA>().Finished_Action_Kitchen();
        }
    }

    bool Set_Products_Cooking(bool onlyCheckEnoughIngredients, string npcName) //**¿Hacerlo hilo para que lo miren varios?Si es online si debería
    {
        bool enoughIng = false;

        //todo: en teoría cada item solo puede tener un objetivo a la vez,hay que IMPLEMENTARLO pero se sacaía del objetivo del 
        //primer NPC using it.HAY QUE hacer cuando se busca item libre vea si su objetico coincide con lo que se esta haciendo.
        //temp - Hacer método,implementar
        int x = 0;
        try
        {
            x = npc_UsingIt_r[0].GetComponent<Npc_Player_IA>().countOrdersNow_i;
            nameCookingNow_str = npc_UsingIt_r[0].GetComponent<Npc_Player_IA>().orders_r[x].objective_str;
        }
        catch (System.Exception)
        {
            Debug.LogError("No debería pasar en: " + nameItem + " Estado: " + onlyCheckEnoughIngredients);
            throw;
        }

        //0º Encontrar la receta a hacer
        Item_IngredientsAndProducts_Gameplay recipe = null;

        for (int i = 0; i < ingredientsAndProducts_r.Count; i++)
        {
            Item_IngredientsAndProducts_Gameplay recipeTemp = ingredientsAndProducts_r[i].GetComponent<Item_IngredientsAndProducts_Gameplay>();

            if (recipeTemp.ingredientsToMakeAProduct.Count == 1 && nameCookingNow_str == recipeTemp.ingredientsToMakeAProduct[0].GetComponent<Ingredients_Gameplay>().nameIngredient_str) 
            {
                recipe = recipeTemp;
                break;
            }
            
            //Preparar-Juntar ingredientes la orden debe indicar el nombre de la receta. //Saber que quieren cocinar es lo dificil, tendrías que comparar qe ing tiene el item,cuales tiene el NPC confirmar
            if (recipeTemp.ingredientsToMakeAProduct.Count > 1) 
            {
                if (nameCookingNow_str == recipeTemp.product[0].GetComponent<Ingredients_Gameplay>().nameIngredient_str)
                {
                    recipe = recipeTemp;
                    break;
                }
            }                     
        }

        //Calcular cantidades a fabricar.
        if (recipe != null) //Sino haz un try catch
        {
            if (recipe.ingredientsToMakeAProduct.Count == 1) //Productos que se consiguen con 1 = 1 (Patata = Patata Cortada)
            {
                //Debug.LogError("uwu " + recipe.description_str + " - " + recipe.ingredientsToMakeAProduct.Count);

                //Ver si tiene ingredientes suficentes. //*No es perfecto, si compruebas pero otro NPC lo gasta mientras tanto,pues vas para nada,pero se queda así,no merece la pena el esfuerzo solucionarlo aquí,poner algún if para cuando llegue de veras.                
                for (int i = 0; i < ingredientsInItem.Count; i++)
                {
                    if (ingredientsInItem[i].nameIngredient_str == nameCookingNow_str)
                    {
                        int countMake = countMaxProductsMakeEachTime;

                        if (onlyCheckEnoughIngredients)
                        {
                            if (ingredientsInItem[i].countInItem_i > 0)
                            {
                                enoughIng = true;
                            }
                            else
                            {
                                enoughIng = false;
                            }
                        }
                        else
                        {
                            if (ingredientsInItem[i].countInItem_i > 0)
                            {
                                if (ingredientsInItem[i].countInItem_i >= countMake)
                                {
                                    ingredientsInItem[i].countInItem_i -= countMake;
                                }
                                else
                                {
                                    countMake = ingredientsInItem[i].countInItem_i;
                                    ingredientsInItem[i].countInItem_i = 0;
                                }

                                AddRemove_Ingredients(3,countMake, recipe.product[0].GetComponent<Ingredients_Gameplay>().nameIngredient_str);

                                //Ingredients_Anim(recipe.product[0], countMake, false);
                            }
                        }

                        break;
                    }
                }

                //Ver si el NPC tiene ingredientes suficientes.
                if (onlyCheckEnoughIngredients && !enoughIng) //*No hay ELSE porque solo se supone que el NPC ya ha entregado sus ingredientes al cocinar,por lo que solo se comprueba al ver cantidades.
                {
                    Npc_Player_IA npc = Get_NpcIA_By_Name(npcName);

                    for (int j = 0; j < npc.ingredientsInNpc_r.Count; j++)
                    {
                        if (npc.ingredientsInNpc_r[j].nameIngredient_str == nameCookingNow_str && npc.ingredientsInNpc_r[j].countInItem_i > 0)
                        {
                            enoughIng = true;
                        }
                    }

                    if (!enoughIng)//Crear orden secundaria
                    {
                        CreateSecondaryActions(npcName, nameCookingNow_str);
                    }
                }
            }
            else //Products X + Y = 1 (Carne,Pan... = Hamburguesa)
            {
                //Todo:
                //Ver si LA RECETA tiene ingredientes repetidos
                List<int> countIngredientsRecipe = new List<int>(); //Así no tengo que hacer otra clase
                List<string> nameIngredientsRecipe = new List<string>();

                for (int i = 0; i < recipe.ingredientsToMakeAProduct.Count; i++)
                {
                    int ingredientsRepetidos = 1;

                    if (i + 1 < recipe.ingredientsToMakeAProduct.Count) //Para que no se salga del array
                    {
                        for (int j = i + 1; j < recipe.ingredientsToMakeAProduct.Count; j++) //Revisamos el resto del for para ver si hay repeticiones del ingredientes
                        {
                            if (recipe.ingredientsToMakeAProduct[j].GetComponent<Ingredients_Gameplay>().nameIngredient_str == recipe.ingredientsToMakeAProduct[i].GetComponent<Ingredients_Gameplay>().nameIngredient_str)
                            {
                                ingredientsRepetidos++;
                            }
                        }
                    }

                    countIngredientsRecipe.Add(ingredientsRepetidos);
                    nameIngredientsRecipe.Add(recipe.ingredientsToMakeAProduct[i].GetComponent<Ingredients_Gameplay>().nameIngredient_str);
                }

                //Calcular la cantidad que puede fabricar
                Npc_Player_IA npc = Get_NpcIA_By_Name(npcName);
                List<int> productsItCouldDo = new List<int>();

                for (int i = 0; i < ingredientsInItem.Count; i++)
                {
                    if (ingredientsInItem[i].nameIngredient_str == nameIngredientsRecipe[i])
                    {
                        if (ingredientsInItem[i].countInItem_i > 0)
                        {
                            productsItCouldDo.Add(ingredientsInItem[i].countInItem_i / countIngredientsRecipe[i]);
                        }

                        //Ver si el NPC tiene suficientes ingredientes.
                        if (onlyCheckEnoughIngredients)
                        {
                            if (ingredientsInItem[i].countInItem_i <= 0)
                            {                                
                                Ingredients_Gameplay ing = npc.Get_IngredientsInNpc_By_Name(ingredientsInItem[i].nameIngredient_str);
                                if (ing.countInItem_i >= countIngredientsRecipe[i])
                                {
                                    productsItCouldDo.Add(ingredientsInItem[i].countInItem_i / countIngredientsRecipe[i]);
                                }
                                else
                                {
                                    enoughIng = false;
                                    productsItCouldDo.Add(0);

                                    CreateSecondaryActions(npcName, ingredientsInItem[i].nameIngredient_str);
                                }
                            }
                        }

                        if (!onlyCheckEnoughIngredients && ingredientsInItem[i].countInItem_i <= 0)
                        {
                            productsItCouldDo.Add(0);

                            //No sé si dejarlo porque si vas a cocinar pero faltan los ing al final es que otro npc los ha usado ates de que llegase,mejor que no  pase nada? Y una anim de sin ing
                            //CreateSecondaryActions("all", ingredientsInItem[i].nameIngredient_str);
                        }
                    }
                }

                if (onlyCheckEnoughIngredients)
                {
                    if (!productsItCouldDo.Contains(0) && productsItCouldDo.Count == countIngredientsRecipe.Count) //Si el item tiene menos ingredientes que los necesitados, no se puede hacer
                    {
                        enoughIng = true;
                    }
                }

                //Ver si se tiene suficientes ingredientes
                if (!onlyCheckEnoughIngredients)
                {
                    if (!productsItCouldDo.Contains(0) && productsItCouldDo.Count == countIngredientsRecipe.Count) //Si el item tiene menos ingredientes que los necesitados, no se puede hacer
                    {
                        //Sacar el min,porque si solo tienes dos panes no puedes hacer más de dos hamburguesas
                        int countMake = productsItCouldDo.Min();

                        if (countMake > countMaxProductsMakeEachTime) { countMake = countMaxProductsMakeEachTime; }

                        //Fabricar - restar cantidades,instanciar...
                        for (int i = 0; i < ingredientsInItem.Count; i++)
                        {
                            ingredientsInItem[i].countInItem_i -= countMake;
                        }

                        AddRemove_Ingredients(3, countMake, recipe.product[0].GetComponent<Ingredients_Gameplay>().nameIngredient_str);
                        //Ingredients_Anim(recipe.product[0], countMake, false);
                    }
                    else
                    {
                        //Debería saltar anim de sin ingredientes
                    }
                }
            }
        }
        
        return enoughIng;
    }

    //Visual 

    public void Ingredients_Anim(string nameIngredient, int posArrayNpc, bool decreseIngredients) //Si añades es false.
    {
        //Todo: Necesito terminar cosas
        GameObject ingGo = null; 

        for (int i = 0; i < game_manager_r.ingredients_AllInGame_prefab_list.Count; i++) //Todo: Hacer método, lo uso demasiado.
        {
            if (game_manager_r.ingredients_AllInGame_prefab_list[i].GetComponent<Ingredients_Gameplay>().nameIngredient_str == nameIngredient)
            {
                ingGo = game_manager_r.ingredients_AllInGame_prefab_list[i];
            }
        }

        if (ingGo == null) { Debug.LogError("No debería pasar"); }

        if (!decreseIngredients) //Añadir a Item
        {
            //Item
            //Iniciar Anim
            //Ver si tiene
            //Instanciar Objeto
            GameObject go = Instantiate(ingGo, ingredientsVisual_parents_go.transform.parent);//Todo: Cambiar el parent por el hijo 0 y darle a este una anim de subir y bajaar para indicar que se está cocinando.
            go.transform.position = ingredientsVisual_parents_go.transform.GetChild(0).transform.position; 
            
            //Npc
            //Quitar objeto visual.

        }
        else //Solo NPC?
        {
            
        }
    }

    //NPC IA

    public bool Check_EnoughIngredients(string objective, string npcName) //Todo:tal vez necesita un retoque
    {
        bool enough = false;

        if (esUnDispensador_b) 
        {
            if (Get_IngredientInItem_By_Name(objective).countInItem_i > 0) 
            {
                enough = true;
            }
        }
        else
        {
            enough = Set_Products_Cooking(true, npcName);
        }

        return enough;
    }
    
    public Vector3 Check_Npc_WhereHasToStandUp()
    {
        Vector3 v3 = Vector3.zero;
       
        //Todo:Que no coja el sitio de otro NPC
        for (int i = 0; i < npc_PlaceToBe_r.Count; i++) //Solo revisa los gameobjects activo
        {
            if (npc_PlaceToBe_r[i].activeSelf) //El primer hueco vale.
            {
                v3 = new Vector3(npc_PlaceToBe_r[i].transform.position.x, -1.7f, npc_PlaceToBe_r[i].transform.position.z);

                //Todo:Coger el npc o dar algo pra el gameobject del sitio.

                break;
            }
        }

        return v3;
    }

    //Refactorizado---------------------------------------------------------------
    
    public Ingredients_Gameplay Get_IngredientInItem_By_Name(string name)
    {
        Ingredients_Gameplay ing = null;

        for (int i = 0; i < ingredientsInItem.Count; i++)
        {
            if (ingredientsInItem[i].nameIngredient_str == name)
            {
                ing = ingredientsInItem[i];
                break;
            }
        }

        return ing;
    }
    
    public Npc_Player_IA Get_NpcIA_By_Name(string nameNpc)
    {
        Npc_Player_IA npc = null;

        for (int i = 0; i < npc_UsingIt_r.Count; i++)
        {
            if (npc_UsingIt_r[i].GetComponent<Npc_Player_IA>().data_Npc_r.nameNpc_str == nameNpc) //*Posiblemente hay que hacer cambios,por si hay nombre parecidos?? Ni idea
            {
                npc = npc_UsingIt_r[i].GetComponent<Npc_Player_IA>();
                break;
            }
        }

        return npc;
    }
    
    public Npc_Player_IA Get_NpcIA_WAITING_By_Name(string nameNpc)
    {
        Npc_Player_IA npc = null;

        for (int i = 0; i < npc_WaitingForIt_r.Count; i++)
        {
            if (npc_WaitingForIt_r[i].GetComponent<Npc_Player_IA>().data_Npc_r.nameNpc_str == nameNpc) //*Posiblemente hay que hacer cambios,por si hay nombre parecidos?? Ni idea
            {
                npc = npc_UsingIt_r[i].GetComponent<Npc_Player_IA>();
                break;
            }
        }

        return npc;
    }
    
    public void CreateSecondaryActions(string nameNpc, string nameIng)
    {
        if (nameNpc.Contains("all"))//**Por ahora esto no se usa,pero mientras estas cocinando te faltan deberúia sdañtar
        {
            for (int i = 0; i < npc_UsingItRightNow_r.Count; i++)
            {
                if (npc_UsingIt_r[i].GetComponent<Npc_Player_IA>().data_Npc_r.nameNpc_str == nameNpc) //*Posiblemente hay que hacer cambios,por si hay nombre parecidos?? Ni idea
                {
                    Npc_Player_IA npc = npc_UsingIt_r[i].GetComponent<Npc_Player_IA>();

                    //Para evitar que las tareas secundarias tambiénn se vean afectadas y al final acabe haciendo toda la receta sin decir nada
                    if (!npc.ordersSecond_b)
                    {
                        npc.orders_r[npc.countOrdersNow_i].Set_tasksSecond(npc.data_Npc_r.nameNpc_str, 0, nameIng);
                    }
                }
            }
        }
        else
        {
            Npc_Player_IA npc = Get_NpcIA_By_Name(nameNpc);

            if (!npc.ordersSecond_b)
            {
                npc.orders_r[npc.countOrdersNow_i].Set_tasksSecond(npc.data_Npc_r.nameNpc_str, 0, nameIng);
            }
        }
    }
    
    //****************

    void Show_NameNpcs()
    {
        npcsName_list_str.Clear();
        ingredientsName_list_str.Clear();

        for (int i = 0; i < npc_UsingIt_r.Count; i++)
        {
            npcsName_list_str.Add(npc_UsingIt_r[i].GetComponent<Npc_Player_Gameplay>().nameNpc_str);
        }
        
        for (int i = 0; i < ingredientsInItem.Count; i++)
        {
            ingredientsName_list_str.Add(ingredientsInItem[i].nameIngredient_str + " - " + ingredientsInItem[i].countInItem_i);
        }
    }
}
