using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class UI_Manager_Rol : MonoBehaviour
{
    public ConnectToApi connectApi_r;

    [Header("")]
    public TMP_InputField prompt_txt;
    public TMP_Text titleTest_txt;
    public TMP_Text response_txt;
    public TMP_Text errorMessage_txt;

    public GameObject test_parent_go;
    public int testNow_i = 0;

    //Reloj
    public bool waiting_b = false;
    float waitingTime_f = 0;

    [Header("Test")]
    public GameObject uploadFile_go;
    [HideInInspector] public string pathFileToUpload_str;
    [HideInInspector] public string fileName_str;

    [Header("Chat - Test")]
    public GameObject chatsFile_go;
    public GameObject buttonSelecChat_Prefab_go;

    public GameObject infor_parent_go;
    public GameObject pdfFusion_parent_go;
    public GameObject cocina_parent_go;

    public GameObject textareaChat_go;
    public TMP_Text chatIA;

    List<ThreadNewData> roots_r = new List<ThreadNewData>();
    public string workspaceChat_str = "";
    public string threadChat_str = "";

    [Header("Save File")]
    public GameObject newThread_go;
    public string savedThings_pathFile_str;
    
    [Header("SQL")]
    public GameObject sql_go;
    public List<TMP_Dropdown> dropdownIng_list = new List<TMP_Dropdown>();
    
    public GameObject textareaSql_go;
    public TMP_Text chatAa;

    public ScrollRect scrollRect;

    void Start()
    {
        prompt_txt.text = "qué hacer para añadir un nuevo buzón?";
        NextTest(true);
    }

    void Update()
    {
        if (waiting_b)
        {
            waitingTime_f += Time.deltaTime;
            errorMessage_txt.text = "Waiting... " + waitingTime_f.ToString("F2");
        }
    }
    
    public void SendPrompt()
    {
        errorMessage_txt.text = "";

        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();

        if (test.testName.Contains("SQL"))
        {
            if (!test.query.Contains("*"))
            {
                SendPrompt_Query();
            }
            else
            {
                test.workspaceSelect_txt.text = "No has seleccionado acción";
            }
        }
        
        if (test.testName.Contains("Subir"))
        {
            if (pathFileToUpload_str.Length > 1)
            {
                SendPrompt_QueryUpload();
            }
            else
            {
                test.workspaceSelect_txt.text = "No has seleccionado ARCHIVO a subir";
            }
        }

        if (test.testName.Contains("Conversar"))
        {
            SendPrompt_Chat();
        }

        if (test.query.Contains("thread/new"))
        {
            SendPrompt_NewThread(); 
        }
    }
    
    public void SendPrompt_Query()
    {
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
        string workspace = test.idWorkspace;
        string query = test.query;

        if (test.infoExtra.Count > 0)
        {
            string type = null;
            string json = null;

            if (test.query.Contains("GET"))
            {
                type = "GET";
                json = "{}";
            }
            if (test.query.Contains("Create"))
            {
                type = "POST";
                json = "{ 'nameIng' :" + $"'{test.infoExtra[0]}'" + ", 'typeIng' :" + $"'{ test.infoExtra[1]}'" + "}";
            }
            if (test.query.Contains("Borrar"))
            {
                type = "DELETE";
                json = "{ 'nameIng' :" + $"'{test.infoExtra[0]}'" + "}";
            }
            if (test.query.Contains("Modificar"))
            {
                type = "PUT";
                json = "{ 'nameIng' :" + $"'{test.infoExtra[0]}'" + ", 'cantidadGr' :" + $"{ test.infoExtra[1]}" + "}";
            }
            
            json = json.Replace("'", "\"");
            connectApi_r.QueryModel_SQL(type, json);
        }
    }

    public void SendPrompt_Chat()
    {
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
        string workspace = test.idWorkspace;
        string query = test.query;

        if (workspaceChat_str.Length > 1)
        {
            if (prompt_txt.text.Length > 1)
            {
                connectApi_r.QueryModel(workspaceChat_str, threadChat_str, prompt_txt.text, query, null);
            }
            else
            {
                errorMessage_txt.text = "No hay text";
            }
        }
        else
        {
            errorMessage_txt.text = "No se ha seleccionado workspace";
        }
    }
    
    public void SendPrompt_NewThread()
    {
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
        string workspace = test.idWorkspace;
        string query = test.query;

        connectApi_r.QueryModel(test.workspaceSelect_txt.text, null, null, query, null);
    }
  
    public void SendPrompt_QueryUpload()
    {
        Debug.Log("Subir archivo");
        connectApi_r.QueryModel(null, null, null, null, pathFileToUpload_str);
    }
    
    public void ShowPrompt(string response)
    {
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
        
        if (test.testName.Contains("Crear conversación nueva"))
        {
            if (!response.Contains("Error al realizar la consulta"))
            {
                Debug.LogError(response);
                //Crear JSON - modifico el que me devuelve la api de Nothing
                string[] sad = response.Split('{');
                response = sad[2];
                string[] sad2 = response.Split('}');
                response = '{' + sad2[0] + '}';

                ThreadNewData root = JsonUtility.FromJson<ThreadNewData>(response);
                root.workspace_Name = test.idWorkspace;
                roots_r.Add(root);
                response = "Operación realizada, hilo creado";
            }
        }
        
        //
        response_txt.text = response;
        errorMessage_txt.text = response;
        waiting_b = false;
    }

    public void CleanScene()
    {
        prompt_txt.text = "";
        response_txt.text = "";
        errorMessage_txt.text = "";

        //Reset
        pathFileToUpload_str = "";

        threadChat_str = "";
        workspaceChat_str = "";
        

    }

    public void NextTest(bool forward)
    {
        if (forward)
        {
            testNow_i++;
            if (testNow_i > test_parent_go.transform.childCount - 1) { testNow_i = 0; }
        }
        else
        {
            testNow_i--;
            if (testNow_i < 0) { testNow_i = test_parent_go.transform.childCount - 1; }
        }

        //
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
        titleTest_txt.text = test.testName;
        CheckThings(test.testName);

        CleanScene();
    }

    void CheckThings(string nameTest)
    {        
        //Desactivamos
        prompt_txt.gameObject.SetActive(true);
        uploadFile_go.SetActive(false);

        chatsFile_go.SetActive(false);
        textareaChat_go.SetActive(false);

        newThread_go.SetActive(false);
        
        sql_go.SetActive(false);
        textareaSql_go.SetActive(false);

        //
        if (nameTest.Contains("Subir archivo"))
        {
            uploadFile_go.SetActive(true);

            prompt_txt.gameObject.SetActive(false);
        }
        
        if (nameTest.Contains("Conversar"))
        {
            chatsFile_go.SetActive(true);
            Create_ChatOptions(true);
        }
        
        if (nameTest.Contains("Crear conversación nueva"))
        {
            newThread_go.SetActive(true);
            prompt_txt.gameObject.SetActive(false);

            TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
            test.Set_Workspace(99);
        }
        
        if (nameTest.Contains("SQL"))
        {
            sql_go.SetActive(true);
            prompt_txt.gameObject.SetActive(false);

            TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();
            test.Set_Workspace(11);
        }
    }

    public void OpenFolderExplorer() //Abre un panel para seleccionar un archivo
    {      
        pathFileToUpload_str = "";
        fileName_str = "";
        pathFileToUpload_str = EditorUtility.OpenFilePanel("Selecciona un archivo", "", "*");
        if (!string.IsNullOrEmpty(pathFileToUpload_str))
        {
            Debug.Log("Ruta seleccionada: " + pathFileToUpload_str);
            fileName_str = Path.GetFileName(pathFileToUpload_str);
        }
    }

    //

    public void SelectChat(GameObject button)
    {
        ChatInfo infoChat = button.GetComponent<ChatInfo>();

        workspaceChat_str = infoChat.workspace;
        threadChat_str = infoChat.thread;
    }
    
    public void ShowChats()
    {
        TestRol test = test_parent_go.transform.GetChild(testNow_i).GetComponent<TestRol>();

        connectApi_r.QueryModel_GETCHATAI(workspaceChat_str, threadChat_str, test.query);
    }
    
    public void Create_ChatOptions(bool create)
    {
        //Borrar los antiguos
        /*int xcount = infor_parent_go.transform.childCount;
        for (int i = xcount; i > 0; i--)
        {
            Destroy(infor_parent_go.transform.GetChild(i));
        }

        xcount = pdfFusion_parent_go.transform.childCount;
        for (int i = xcount; i > 0; i--)
        {
            Destroy(pdfFusion_parent_go.transform.GetChild(i));
        }

        xcount = cocina_parent_go.transform.childCount;
        for (int i = xcount; i > 1; i--)
        {
            Destroy(cocina_parent_go.transform.GetChild(i));
        }*/

        //Crearlos
        for (int i = 0; i < roots_r.Count; i++)
        {
            GameObject parent = null;

            if (roots_r[i].workspace_Name == "tfg-receta-sql")
            {
                parent = cocina_parent_go;
            }
            if (roots_r[i].workspace_Name == "tfg-pdf-informatico")
            {
                parent = infor_parent_go;
            }
            if (roots_r[i].workspace_Name == "tfg-pdf-fusion")
            {
                parent = pdfFusion_parent_go;
            }

            if (!roots_r[i].createdIt_b)
            {
                GameObject go = Instantiate(buttonSelecChat_Prefab_go, parent.transform);
                ChatInfo chat = go.GetComponent<ChatInfo>();
                chat.workspace = roots_r[i].workspace_Name;
                chat.thread = roots_r[i].slug;
                chat.Set_Info(chat.workspace, chat.thread);

                roots_r[i].createdIt_b = true;
            }
        }
    }

    public void Set_Waiting()
    {
        waiting_b = true;
        waitingTime_f = 0;
    }
    
    public void ScrollUp()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f; // 1f = Parte superior
        }
    }

    //JSONS

    //Crear nuevo hilo.

    public class ThreadNewData
    {
        public string id = "";
        public string name = "";
        public string slug = "";
        public string workspace_id = "";
        public string workspace_Name = "";
        public string user_id = "";
        public string createdAt = "";
        public string lastUpdatedAt = "";

        //
        public bool createdIt_b = false;
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Start", LoadSceneMode.Single);
    }
}
