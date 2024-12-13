using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Windows;
using UnityEditor.PackageManager.Requests;

public class Game_Manager : MonoBehaviour
{
    //Prompt
    public string promptWorkInKitchen_Str = "";
    public List<string> tonoMensajes_list = new List<string>();
    public List<string> tasksKitchen_list = new List<string>();
    public List<string> foodKitchen_list = new List<string>();
    public List<string> workersKitchen_list = new List<string>(); //*Todo: No se pueden repetir nombres.

    [Header("Gameplay")]
    public GameObject npcs_parent_kitchen_go;
    public Npc_Player_Gameplay npcSelected_r;

    public GameObject items_parent_kitchen_go;
    public GameObject ingredients_parent_kitchen_go;
    public GameObject restingpositions_parent_kitchen_go;

    //De aquí salen los items para la tienda,el armario,...
    public List<GameObject> ingredients_AllInGame_prefab_list = new List<GameObject>(); //Todo:HAcer otro list con todos los gameobjects de ingredientes,este sea solo para los que se an a usar en este escenario actual, tal vez hacer una lista de recetas y según que ingredientes pida los una aqui
    public List<GameObject> itemsKitchen_AllInGame_prefab_list = new List<GameObject>();
    public List<GameObject> taskUI_AllInGame_prefab_list = new List<GameObject>();

    //Faces //Los NPC usarán las mismas caras para mostrar como están de estado, pos crear una clase para ser fácil de identificar
    [Header("Npc")]
    public Image faceCheff_img;
    public List<Sprite> facesNPC_sprite = new List<Sprite>();

    int npcCreateHire_i = -1; //Temp, sustituir con nuevo método.
    public List<GameObject> npcsPrefabs_go = new List<GameObject>();//Todo: Crear sistema de variación de NPC para no depender de solo listas de Prefabs,meter variaciones y esas cosas

    [Header("")]
    public int maxNpcInGame = 3; //No puedes tener más de X Npc.
    public int maxRepeatTask = 2; //No puedes tener más de dos tareas iguales.
    public int maxCountTask = 3; //Tareas que puede tener un NPC

    [HideInInspector] 
    public int turnPrompt_i = -1; //*Como un id.

    //UI
    [Header("")]
    public TMP_InputField inputText_ui_str;
    public GameObject waiting_go; //Temp

    public TMP_Text waitingTimeIAResponse_txt;
    float waitingTime_f = -99;


    [Header("")]
    public List<string> phrasesTest = new List<string>();
    public List<string> recepcionTest = new List<string>();

    public string messageToIA_str = "";

    //Gameplay - Input Player
    [Header("")]
    public bool waitingAnswer_b = false;
    public bool microphone_b = false;

    void Start()
    {
        //Temp
        Application.targetFrameRate = 60;

        //
        phrasesTest.Add("Cuenta nº enteros del 0 al 100");
        phrasesTest.Add("Tell me a joke about clowns");
        phrasesTest.Add("4 + 5 Dime el resultado");

        for (int i = 0; i < taskUI_AllInGame_prefab_list.Count; i++)
        {
            Task_Data_UI_Prefab task = taskUI_AllInGame_prefab_list[i].GetComponent<Task_Data_UI_Prefab>();

            for (int j = 0; j < task.nameVariants_str.Count; j++)
            {
                tasksKitchen_list.Add(task.nameVariants_str[j]);
            }

            tasksKitchen_list.Add(task.taskName_str);
        }
        //UnityEngine.Debug.Log("Tareas Count: " + taskUI_AllInGame_prefab_list.Count);

        tonoMensajes_list.Add("Positivo");
        tonoMensajes_list.Add("Neutro"); //Pos: Añadir variaciones,Neutral y cosas así.
        tonoMensajes_list.Add("Negativo");

        for (int i = 0; i < ingredients_AllInGame_prefab_list.Count; i++)
        {
            Ingredients_Gameplay ing = ingredients_AllInGame_prefab_list[i].GetComponent<Ingredients_Gameplay>();

            foodKitchen_list.Add(ing.nameIngredient_str);

            for (int j = 0; j < ing.nameIngredient_Variants_str.Count; j++)
            {
                if (ing.nameIngredient_Variants_str[j] != null)
                {
                    foodKitchen_list.Add(ing.nameIngredient_str);
                }
            }
        }

        //LLM_StartProgram();
    }

    void Update()
    {
        if (waitingTime_f > -1)
        {
            waitingTime_f += Time.deltaTime;
            waitingTimeIAResponse_txt.text = "Waiting... " + waitingTime_f.ToString("F2");
        }
    }

    //
    public GameObject Get_NpcPrefab_ByName(string nameNpc)
    {
        GameObject go = null;

        for (int i = 0; i < npcs_parent_kitchen_go.transform.childCount; i++)
        {
            if (npcs_parent_kitchen_go.transform.GetChild(i).GetComponent<Npc_Player_Gameplay>().nameNpc_str == nameNpc)
            {
                go = npcs_parent_kitchen_go.transform.GetChild(i).gameObject;

                break;
            }
        }

        return go;
    }

    //Gameplay
    public void Set_Prompt_Kitchen(string input)
    {
        //Sacar lista de trabajadores //Todo: Mover a contratación cuantdo exista,esto no va a cambiar en mucho tiempo los npc en juego,// otra cosa las comas y el y solo sirve para español,tendrás que hacer métodos para diferentes idiomas
        workersKitchen_list.Clear();

        //Esp
        int countNpc = npcs_parent_kitchen_go.transform.childCount;

        for (int i = 0; i < countNpc; i++) //Todo: Esto va haber que cambiarlo cuando creemos una tienda o contratación,es mejor añadir y quitar antes de instanciarlos en el mapa. Un simple list string añadir el mismo y ya.
        {
            workersKitchen_list.Add(npcs_parent_kitchen_go.transform.GetChild(i).GetComponent<Npc_Player_Gameplay>().nameNpc_str);
        }

        string trabajadores = "";
        countNpc = workersKitchen_list.Count;
        string namePlus = "";

        for (int i = 0; i < countNpc; i++)
        {
            namePlus = "";

            if (i < countNpc)
            {
                if (i == countNpc - 2)
                {
                    namePlus = " y ";
                }

                if (i < countNpc - 2)
                {
                    namePlus = ", ";
                }
            }

            trabajadores += workersKitchen_list[i] + namePlus;
        }


        //-------

        //Tareas //Todo: esto hacerlo método y ver cuando hace falta y cuando no
        string tareas = ""; //*Hacerlo global
        int countTasks = tasksKitchen_list.Count;
        namePlus = "";

        for (int i = 0; i < countTasks; i++)
        {
            namePlus = "";

            if (i < countTasks)
            {
                if (i == countTasks - 2)
                {
                    namePlus = " y ";
                }

                if (i < countTasks - 2)
                {
                    namePlus = ", ";
                }
            }

            tareas += tasksKitchen_list[i] + namePlus;
        }

        //Todo: Hacerlo método al principio de la partida con el primer npc añadido //Hacer método para el ultimo o primer, bool, NPC mencionado en el prompt
        npcSelected_r = npcs_parent_kitchen_go.transform.GetChild(0).GetComponent<Npc_Player_Gameplay>();

        //Todo: hacer variaciones para idiomas
        string temp2 = $"Vamos a realizar un juego de rol en el que yo seré el chef, dando instrucciones a mis trabajadores en un restaurante. Los trabajadores son {trabajadores}. Quiero que analices el siguiente texto:\r\n'{input}'\r\nRealiza el análisis considerando lo siguiente:\r\n1. Tono del mensaje: Indica si el tono es positivo, negativo o neutro hacia los trabajadores. No añadas explicación sobre el tono del mensaje. Usa el formato: Tono: [Positivo/Negativo/Neutro]\r\n2. Tareas asignadas a los trabajadores: Las tareas permitidas deben estar en esta lista: {tareas}. Si se mencionan tareas en el texto con sinónimos o referencias que coincidan con estas, usa el nombre de la tarea correspondiente. Si no se asigna una tarea, responde 'Nada'.\r\nNo menciones tareas que no estén en la lista. \r\nSolo informa de las tareas correspondientes sin explicar cómo llegaste a esa conclusión.\r\nSi no se indica el trabajador es {npcSelected_r.nameNpc_str}, y si la tarea no se indica es {npcSelected_r.lastTask_str}\r\n\r\nUsa este formato:\r\nTrabajador: [Nombre], Tarea: [NombreDeLaTarea], Objetivo: [Nombre del destinatario o acción], Repeticiones: [Número de veces].\r\n";
        string temp3 = $"Vamos a jugar un juego de rol en el que yo soy un chef dando órdenes a mis trabajadores. En el restaurante, los trabajadores son {trabajadores}.  \r\nAnaliza el siguiente texto:  \r\n\"{input}\"\r\n\r\n 1. Tono del mensaje:\r\nIndica si el tono del mensaje es Positivo hacia los trabajadores, negativo hacia los trabajadores o neutro. Usa el siguiente formato:\r\nTono: Tipo;\r\n\r\n2. Tareas asignadas a los trabajadores: \r\nLas tareas permitidas son las de esta lista: {tareas}. Si en el texto se usan sinónimos o referencias a estas tareas, asegúrate de usar uno de los nombres de tarea de esta lista. Si no hay una tarea asignada a un trabajador, responde 'Nada'.\r\n\r\n- Formato:\r\n   Trabajador: NombreDelTrabajador, Tarea: NombreDeLaTarea, Objetivo de la tarea: Nombre, Repeticiones: Número de veces\r\n\r\n*Ejemplo de salida esperada para el texto dado:*\r\n\r\n- *Halagado e Insultado:*  \r\n   Halagado: Nadie; Insultado: Paco;\r\n  \r\n- *Tareas:*  \r\n   Trabajador: Jhon, Tarea: Preparar, Objetivo de la tarea: Hamburguesa, Repeticiones: 1  \r\n   Trabajador: María, Tarea: Nada, Objetivo de la tarea: Nadie, Repeticiones: Nadie  \r\n   Trabajador: Carlos, Tarea: Freir, Objetivo de la tarea: Carne, Repeticiones: 1\r\n\r\nInstrucciones adicionales para la IA:\r\n- No asignes tareas que no estén en la lista proporcionada.\r\n- Para indicar la tarea no añadas detalles, solo el nombre de la tarea.\r\n- Si el texto menciona realizar una acción que no está claramente definida en términos de las tareas permitidas, di \"Nada\" para esa tarea.\r\n-Si no se indica el trabajador, el trabajador es {npcSelected_r.nameNpc_str}\r\n-Si no se indica la tarea a realizar es {npcSelected_r.lastTask_str}";
        
        string temp = $"Vamos a jugar un juego de rol en el que yo soy un chef dando órdenes a mis trabajadores. En el restaurante, hay trabajadores llamados {trabajadores}. \r\nAnaliza el siguiente texto:\r\n\"{input}\" \r\n\r\n1º Tono del mensaje:\r\nIndica si el tono del mensaje es \"Positivo\", \"Negativo\" o \"Neutro\" hacia los trabajadores.\r\nUsa el siguiente formato JSON:\r\n{{\r\n  \"tono\": \"tipo de tono\"\r\n}}\r\n\r\n2º Tareas asignadas a los trabajadores:\r\nLas tareas permitidas son: {tareas}.\r\n\r\nSi en el texto se usan sinónimos o referencias a estas tareas, usa el nombre exacto de la tarea permitida de la lista anterior.\r\nSi un trabajador tiene múltiples tareas, incluye cada tarea de forma independiente con su objetivo correspondiente.\r\nSi no se nombra un trabajador de la lista no escribas sobre él. Expresiones como 'Todo el mundo','cada uno', o frases que hagan referencia a un grupo de personas mayor a dos personas sin indicar nombres hace referencia al trabajador \"Todos\"\r\nUsa el siguiente formato JSON: \r\n\r\n{{ \"trabajadores\": [ {{ \"nombre\": \"NombreDelTrabajador\", \"tareas\": [ {{ \"tarea\": \"NombreDeLaTarea\", \"objetivo\": \"Objetivo de la tarea\" }} ] }} ] }} \r\n\r\nEn caso de tener dos tareas, repite el apartado tarea\r\n\r\nEjemplo de salida esperada para el texto dado:\r\n{{ \"tono\": \"Negativo\", \"trabajadores\": [ {{ \"nombre\": \"Carlos\", \"tareas\": [ {{ \"tarea\": \"Freír\", \"objetivo\": \"Patatas\"}}, {{ \"tarea\": \"Limpiar\", \"objetivo\": \"Platos\"}} ] }}, {{ \"nombre\": \"Jhon\", \"tareas\": [ {{ \"tarea\": \"Nada\", \"objetivo\": \"Nadie\"}} ] }}, {{ \"nombre\": \"Tan\", \"tareas\": [ {{ \"tarea\": \"Nada\", \"objetivo\": \"Nadie\" }} ] }} ]}} \r\n\r\nJSON - Respond only with valid JSON. Do not write an introduction or summary.\r\n";

        UnityEngine.Debug.Log("Prompt send:" + temp);

        //Temp - 
        string rutaArchivo = @"C:\Users\Jp\Desktop\iaPeticion.txt";
        System.IO.File.WriteAllText(rutaArchivo, temp);
    }

    void CMD_Start(string comando)
    {
        // Configurar el proceso para ejecutar CMD
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "cmd.exe"; // Ejecutar CMD
        psi.Arguments = "/C " + comando; // "/C" significa ejecutar el comando y luego salir
        psi.RedirectStandardOutput = true; // Redirigir la salida
        psi.UseShellExecute = false; // No usar la shell del sistema
        psi.CreateNoWindow = true; // No mostrar la ventana de CMD

        // Ejecutar el proceso
        Process proc = new Process();
        proc.StartInfo = psi;
        proc.Start();

        // Leer la salida del comando
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        // Mostrar la salida
        Console.WriteLine(output);
    }

    //LLM STUDIO
    
    void LLM_StartProgram()//Iniciar LLM Studios
    {
        
        
        LLM_StartServer();
    }
    
    void LLM_StartServer()
    {
        // Comando a ejecutar
        string comando = "lms server start";
        CMD_Start(comando);

        LLM_StartIA();
    }

    void LLM_StartIA()
    {
        string comando = "";

       // CMD_Start(comando);// Comando a ejecutar
    }
    
    public void LLM_SendMessage_UI()
    {
        turnPrompt_i++;

        if (!microphone_b)
        {
            StartCoroutine(StartPrompt());
        }
        else
        {

        }
    }

    private IEnumerator StartPrompt()
    {
        waitingTime_f = 0;

        // Configura el entorno inicial
        Set_Prompt_Kitchen(inputText_ui_str.text.ToString());

        // Ejecuta el .exe de manera no bloqueante
        yield return StartCoroutine(RunExternalProcessAsync(@"C:\Users\Jp\Projectos\StartIa.exe"));

        // UI
        inputText_ui_str.text = "";

        // Lee la respuesta generada por el .exe
        string response = System.IO.File.ReadAllText(@"C:\Users\Jp\Desktop\iaRespuesta.txt");
        if (response.Length < 190)
        {
            response = System.IO.File.ReadAllText(@"C:\Users\Jp\Desktop\isTest.txt");
            faceCheff_img.sprite = facesNPC_sprite[3];
            UnityEngine.Debug.LogError("La IA ha devuelto algo incorrecto, pongamos cosas en movimiento.");
        }
        else
        {
            UnityEngine.Debug.LogError("Respuesta: " + response.Length);
        }
        Set_Npcs_Orders(response);

        waitingTime_f = -99;
        waitingTimeIAResponse_txt.text = "";

        yield return null; // Finaliza la corutina
    }

    private IEnumerator RunExternalProcessAsync(string exePath)
    {
        // Flag para saber si el proceso terminó
        bool processCompleted = false;

        // Configura el proceso
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.EnableRaisingEvents = true;

        // Evento que se dispara cuando el proceso finaliza
        process.Exited += (sender, args) =>
        {
            processCompleted = true;
            process.Dispose(); // Limpia el proceso al terminar
        };

        // Inicia el proceso
        process.Start();

        // Espera hasta que el proceso termine (sin bloquear el hilo principal)
        while (!processCompleted)
        {
            yield return null; // Esto permite que el juego siga ejecutándose mientras espera
        }
    }

    //Microphone

    void Microphone_Start()// Si necesita una conf va aquí
    {
        
    }
    
    void Microphone_Set(bool state)//Grabar o no
    {
        if (state) 
        {

        }
        else //Stop
        {

        }
    }

    //Procesar Texto
    //!!!De aqui tiene que salir la tarea e ingredientes finales, aqui se debe procesar la palabra variane para no darle el trabaja a otros metodos

    private void Set_Npcs_Orders(string message)
    {        
        //Tener solo el JSON. //*Tengo que hacer esto porque aunque digo que no añada nada más aparte del JSON lo puede hacer, mejor prevenir.
        char[] array = message.ToCharArray();
        string txt = "";
        bool start = false;

        int startPosition = -1;
        int endPosition = -1;

        for (int i = 0; i < array.Length; i++)
        {
            if (!start) { if (array[i] == '{') { start = true; startPosition = i; } }
            
            if (array[i] == '}') { endPosition = i; }
        }
        
        for (int i = startPosition; i < endPosition; i++)
        {
            if (array[i] != ',' && array[i] != '{' && array[i] != '}' && array[i] != ')' && array[i] != '(' && array[i] != ']' && array[i] != '[' && array[i] != ':')
            {
                txt += array[i];
            }
        }

        UnityEngine.Debug.Log("t: \n" + txt);

        //No me deja procesar el JSON, je , con SPLIT será
        int typeOfTask = -99;
        int state = -99;
        string[] listTono = txt.Split("trabajadores");        
        if (listTono[0].Contains("Neutro")) { state = 0; }
        if (listTono[0].Contains("Positivo")) { state = 1; }
        if (listTono[0].Contains("Negativo")) { state = 2; }
        //Todo: Hacer que afecte al gameplay
        faceCheff_img.sprite = facesNPC_sprite[state];

        try
        {
            string[] listNombresTareas = listTono[1].Split("nombre");
            for (int i = 0; i < listNombresTareas.Length; i++)
            {
                GameObject npc = null;
                typeOfTask = -99; //Reset

                string[] listNombres = listNombresTareas[i].Split("tareas");

                for (int j = 0; j < workersKitchen_list.Count; j++)
                {
                    if (listNombres[0].Contains(workersKitchen_list[j]))
                    {
                        npc = npcs_parent_kitchen_go.transform.GetChild(j).gameObject;
                    }
                }

                bool todos = false;
                if (npc == null && listNombres[0].Contains("Todos")) //*No es la manera más correcta, debería ir en un for, pero tendría que tocar el codigo moviendo esto a un método, pero antes debería tocar el json para evitar liarla tanto y me ahorraría este codigo
                {
                    npc = npcs_parent_kitchen_go.transform.GetChild(0).gameObject;
                    todos = true;
                }

                if (npc)
                {
                    string[] listTareas = listNombres[1].Split("tarea");

                    for (int j = 0; j < listTareas.Length; j++)
                    {
                        for (int ii = 0; ii < tasksKitchen_list.Count; ii++)
                        {
                            if (listTareas[j].ToLower().Contains(tasksKitchen_list[ii].ToLower()))
                            {
                                string taskName = Get_Task_By_Name_InGame(tasksKitchen_list[ii]).GetComponent<Task_Data_UI_Prefab>().taskName_str; //Si se ha activado por una variable,devolver la tare principal.

                                //UnityEngine.Debug.LogError("Tarea a dividir" + j + " -: " + listTareas[j]);
                                string objective_str = null;

                                //Sacar el objetivo
                                string[] objetivo = listTareas[j].Split("objetivo");

                                //Limpiar Objetivo
                                string objNew = "";
                                int count = 0;
                                char[] objetivoNew = objetivo[1].ToArray();
                                for (int jj = 0; jj < objetivoNew.Length; jj++)
                                {
                                    if (objetivoNew[jj] == '"')
                                    {
                                        count++;
                                    }

                                    if (count == 2 && objetivoNew[jj] != '"')
                                    {
                                        objNew += objetivoNew[jj];
                                    }
                                }

                                objNew = objNew.ToLower().Trim().Replace(" ", ""); //*Revisar

                                //
                                for (int jj = 0; jj < ingredients_AllInGame_prefab_list.Count; jj++)
                                {
                                    Ingredients_Gameplay ing = ingredients_AllInGame_prefab_list[jj].GetComponent<Ingredients_Gameplay>();
                                    if (objNew.ToLower().Contains(ing.nameIngredient_str.ToLower()) && !objNew.ToLower().Contains("cort"))//**Las palabras con espacios me dan problemas
                                    {
                                        objective_str = ing.nameIngredient_str;

                                        break;
                                    }
                                    else
                                    {
                                        for (int jjj = 0; jjj < ing.nameIngredient_Variants_str.Count; jjj++)
                                        {
                                            if (objNew.Contains(ing.nameIngredient_Variants_str[jjj].ToLower().Replace(" ", "")))
                                            {
                                                objective_str = ing.nameIngredient_str;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (objective_str != null) //Llamar a NPC
                                {
                                    //Indicar tipo //Todo: Modficar si hay diccionarios o clases
                                    if (tasksKitchen_list[ii].ToLower() == "Traer".ToLower())
                                    {
                                        typeOfTask = 1;
                                    }
                                    if (tasksKitchen_list[ii].ToLower() == "Limpiar".ToLower())
                                    {
                                        typeOfTask = 2;
                                    }
                                    if (typeOfTask == -99)
                                    {
                                        typeOfTask = 0;
                                    }

                                    //
                                    npc.GetComponent<Npc_Player_IA>().Set_Orders(typeOfTask, taskName, objective_str, state);

                                    if (todos && npcs_parent_kitchen_go.transform.childCount > 1)
                                    {
                                        for (int z = 1; z < npcs_parent_kitchen_go.transform.childCount; z++)
                                        {
                                            npc = npcs_parent_kitchen_go.transform.GetChild(z).gameObject;
                                            npc.GetComponent<Npc_Player_IA>().Set_Orders(typeOfTask, taskName, objective_str, state);
                                        }
                                    }

                                    typeOfTask = -99; //Reset
                                }
                            }
                            else
                            {
                                /*UnityEngine.Debug.Log("Tarea fallida: " + tasksKitchen_list[ii]);
                                UnityEngine.Debug.Log("Tarea comparada:" + listTareas[j]);*/
                            }
                        }
                    }
                }
            }

            //Indicar a los NPC que empiecen a trabajar.
            for (int i = 0; i < npcs_parent_kitchen_go.transform.childCount; i++)
            {
                npcs_parent_kitchen_go.transform.GetChild(i).gameObject.GetComponent<Npc_Player_IA>().StartWorking();
            }

        }
        catch (Exception)
        {
            Set_Npcs_Orders(System.IO.File.ReadAllText(@"C:\Users\Jp\Desktop\isTest.txt"));
            faceCheff_img.sprite = facesNPC_sprite[3];
            UnityEngine.Debug.LogError("La IA ha devuelto algo incorrecto, pongamos cosas en movimiento.");

            throw;
        }
        
    }

    public bool Set_HireNewNpc()
    {
        bool createdIt = false;

        if (npcs_parent_kitchen_go.transform.childCount < maxNpcInGame) 
        { 
            createdIt = true; 
        }

        if (createdIt && npcCreateHire_i >= 4) //Temp,hasta que añada la nueva forma
        {
            createdIt = false; //Nos hemos quedado sin prefabs a fabricar,meter método de crear NPC aleatorios o prodecudare
        }

        //Instanciarlo
        if (createdIt)
        {
            npcCreateHire_i++;
            GameObject go = Instantiate(npcsPrefabs_go[npcCreateHire_i], npcs_parent_kitchen_go.transform);
            go.transform.position = new Vector3(go.transform.position.x - (npcCreateHire_i + 1), go.transform.position.y, go.transform.position.z);
        }

        return createdIt;
    }

    //Métodos para Refactorizar 
    public Item_GamePlay Get_Item_By_TaskName_InGame(string taskName)
    {
        Item_GamePlay item = null;

        for (int i = 0; i < items_parent_kitchen_go.transform.childCount; i++)
        {
            Item_GamePlay itemTemp = items_parent_kitchen_go.transform.GetChild(i).GetComponent<Item_GamePlay>();

            if (itemTemp.taskName == taskName)
            {
                item = items_parent_kitchen_go.transform.GetChild(i).GetComponent<Item_GamePlay>();

                break;
            }
        }

        return item;
    }
    
    public Ingredients_Gameplay Get_Ingredient_By_Name_InGame(string nameIng) //Los nombres va haber que cambiarlo.
    {
        Ingredients_Gameplay ing = null;

        for (int i = 0; i < ingredients_AllInGame_prefab_list.Count; i++)
        {
            Ingredients_Gameplay ingTemp = ingredients_AllInGame_prefab_list[i].GetComponent<Ingredients_Gameplay>();

            if (ingTemp.nameIngredient_str == nameIng)
            {
                ing = ingredients_AllInGame_prefab_list[i].GetComponent<Ingredients_Gameplay>();

                break;
            }
            else
            {

            }
        }

        return ing;
    }
    
    public GameObject Get_Task_By_Name_InGame(string nameTask) //Los nombres va haber que cambiarlo.
    {
        GameObject go = null;

        for (int i = 0; i < taskUI_AllInGame_prefab_list.Count; i++)
        {
            Task_Data_UI_Prefab taskData = taskUI_AllInGame_prefab_list[i].GetComponent<Task_Data_UI_Prefab>();
            if (taskData.taskName_str.ToLower() == nameTask)
            {
                go = taskUI_AllInGame_prefab_list[i];
                break;
            }
            else //Buscar en las variables
            {
                for (int j = 0; j < taskData.nameVariants_str.Count; j++)
                {
                    if (taskData.nameVariants_str[j] != "-" && taskData.nameVariants_str[j].ToLower() == nameTask.ToLower())
                    {
                        go = taskUI_AllInGame_prefab_list[i];
                        break;
                    }
                }
            }
        }

        if (go == null)
        {
            UnityEngine.Debug.Log("Tarea no encontrada: " + nameTask);
        }

        return go;
    }
    
    //Es necesario? No se puede hacer .gameobject y ya?
    public GameObject Get_IngredientGameobject_By_Name_InGame(string nameIng) //Los nombres va haber que cambiarlo.
    {
        GameObject ingGo = null;

        for (int i = 0; i < ingredients_AllInGame_prefab_list.Count; i++)
        {
            Ingredients_Gameplay ingTemp = ingredients_AllInGame_prefab_list[i].GetComponent<Ingredients_Gameplay>();

            if (ingTemp.nameIngredient_str == nameIng)
            {
                ingGo = ingredients_AllInGame_prefab_list[i];

                break;
            }
        }

        return ingGo;
    }

}
