using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ConnectToApi : MonoBehaviour
{
    public UI_Manager_Rol ui_manager_r;
    public string baseUrl = "http://localhost:3001/api/workspace/";
    public string apiKey = "JHW2CS8-81BM7TW-H331DVV-YS4MGYW";

    public string listaIngredientes = "";

    void Start()
    {

    }

    public void QueryModel(string workspaceId,string thread, string prompt, string query, string pathFile)
    {
        if (!ui_manager_r.waiting_b)
        {
            if (pathFile == null)
            {
                if (thread != null)
                {
                    //StartCoroutine(SendQuery_Chat(workspaceId, prompt, query, thread));
                    StartCoroutine(SendQuery_SQLCHAT(workspaceId, prompt, query, thread));
                }
                else
                {
                    StartCoroutine(SendQuery(workspaceId, prompt, query));
                }
            }
            else
            {
                StartCoroutine(SendQuery_UploadFile(pathFile));
            }

            ui_manager_r.Set_Waiting();
        }
        else
        {
            ui_manager_r.errorMessage_txt.text = "Actualmente ya hay una petición en curso,espera por favor";
        }      
    }
    
    public void QueryModel_SQL(string type, string input)
    {
        if (ui_manager_r.waiting_b)
        {
            ui_manager_r.errorMessage_txt.text = "Actualmente ya hay una petición en curso,espera por favor";
        }
        else
        {
            StartCoroutine(SendQuery_SQL(type, input));
            ui_manager_r.Set_Waiting();
        }

    }
    
    public void QueryModel_GETCHATAI(string workspaceId, string threadChat, string query)
    {
        StartCoroutine(GetQuery_Chat(workspaceId, threadChat, query));
        ui_manager_r.Set_Waiting();
    }

    //------------

    private IEnumerator SendQuery(string workspaceId, string prompt, string query) //Método para hacer una consulta al modelo
    {
        string url = baseUrl + $"{workspaceId}/{query}";

        Debug.Log(url);

        // Crear el objeto JSON para enviar
        var requestData = new
        {
            //prompt = prompt // Consulta al modelo
            title = "looooooooo" // Consulta al modelo
        };

        string json = JsonUtility.ToJson(requestData);

        // Crear una solicitud POST
        using (UnityWebRequest request = UnityWebRequest.Post(url, json))
        {
            // Agregar headers necesarios
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // Adjuntar el JSON al cuerpo de la solicitud
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Enviar solicitud
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ui_manager_r.ShowPrompt(request.downloadHandler.text);
            }
            else
            {
                ui_manager_r.ShowPrompt($"Error al realizar la consulta: {request.error}");
            }
        }

        ui_manager_r.waiting_b = false;
    }

    private IEnumerator GetQuery_Chat(string workspaceId, string thread, string query) 
    {
        query = query + "s";
        string url = baseUrl + $"{workspaceId}/{query.Replace("*",thread)}";

        Debug.Log(url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Agregar headers necesarios
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // Adjuntar el JSON al cuerpo de la solicitud
            request.downloadHandler = new DownloadHandlerBuffer();

            // Enviar solicitud
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ui_manager_r.textareaChat_go.SetActive(true);
                
                string x = request.downloadHandler.text;
                x = x.Replace("},{","\n\n");
                x = x.Replace(",","\n");
                ui_manager_r.chatIA.text = x;
            }
            else
            {
                ui_manager_r.textareaChat_go.SetActive(false); 
                ui_manager_r.chatIA.text = "";

                ui_manager_r.ShowPrompt($"Error al realizar la consulta: {request.error}");
            }
        }

        ui_manager_r.waiting_b = false;
    }


    private IEnumerator SendQuery_UploadFile(string filePath)
    {
        string url = "http://localhost:3001/api/v1/document/upload";

        // Crear el formulario
        WWWForm form = new WWWForm();
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        string fileName = System.IO.Path.GetFileName(filePath);
        form.AddBinaryData("file", fileData, fileName, "application/octet-stream");

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            // Agregar encabezado de autorización
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // Configurar el DownloadHandler para capturar la respuesta
            request.downloadHandler = new DownloadHandlerBuffer();

            // Enviar la solicitud
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ui_manager_r.ShowPrompt(request.downloadHandler.text);
            }
            else
            {
                string errorResponse = request.downloadHandler.text;
                ui_manager_r.ShowPrompt("Error al subir el archivo: " + errorResponse);
            }
        }

        //Añadir el documento al Workspace //Por ser un test solo elegiremos un área de trabajo.

        url = "http://localhost:3001/api/v1/workspace/tfg-pdf-fusion/update-embeddings";
        string json = "{'adds': [" + $"'custom-documents/{ui_manager_r.fileName_str}.json' ] }}";
        json = json.Replace("'", "\"");
        Debug.LogError(json + url);
        UnityWebRequest request2;

        request2 = UnityWebRequest.Post(url, json);
        request2.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        // Agregar headers necesarios
        request2.SetRequestHeader("Content-Type", "application/json");
        request2.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request2.downloadHandler = new DownloadHandlerBuffer();

        // Enviar solicitud
        yield return request2.SendWebRequest();

        // Manejar la respuesta
        if (request2.result == UnityWebRequest.Result.Success)
        {
            ui_manager_r.ShowPrompt(request2.downloadHandler.text);
        }
        else
        {
            string errorResponse = request2.downloadHandler.text;
            ui_manager_r.ShowPrompt("Error al subir el archivo: " + errorResponse);
        }

        ui_manager_r.waiting_b = false;
    }



    //SQL
    private IEnumerator SendQuery_SQL(string type, string input) 
    {
        string url = $"http://localhost/tfg/Api_MySQL_TFG.php";
        UnityWebRequest request;

        Debug.LogError("SQL Petition:" + input + " Type: " + type); 

        switch (type.ToUpper())
        {
            case "GET":
                url += $"?{input}"; // Añadir parámetros a la URL
                request = UnityWebRequest.Get(url);
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(input));
                break;

            case "POST":
                request = UnityWebRequest.Post(url, string.Empty);
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(input));
                break;

            case "PUT":
                request = new UnityWebRequest(url, "PUT");
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(input));
                break;

            case "DELETE":
                request = new UnityWebRequest(url, "DELETE");
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(input));
                break;

            default:
                ui_manager_r.ShowPrompt("Método no soportado.");
                yield break;
        }

        // Agregar headers necesarios
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.downloadHandler = new DownloadHandlerBuffer();

        // Enviar solicitud
        yield return request.SendWebRequest();

        // Manejar la respuesta
        if (request.result == UnityWebRequest.Result.Success)
        {
            ui_manager_r.test_parent_go.transform.GetChild(ui_manager_r.testNow_i).GetComponent<TestRol>().workspaceSelect_txt.text = "Operación Realizada"; ;
            Debug.Log("SQL Realizado:" + request.downloadHandler.text);

            //Solo sirve para el prompt y el chat.
            if (type == "GET")
            {
                listaIngredientes = request.downloadHandler.text;

                //Hacer el texto más presentable
                /*listaIngredientes = listaIngredientes.Replace("\"", "");
                listaIngredientes = listaIngredientes.Replace("Conectado[{", "");
                listaIngredientes = listaIngredientes.Replace("}", "");
                listaIngredientes = listaIngredientes.Replace("idIngrediente", "\n idIngrediente");*/

                string formatted = listaIngredientes
                   .Replace("Conectado[", "")
                   .Replace("]", "")
                   .Replace("{", "\n")
                   .Replace("},", "\n")
                   .Replace("}", "\n")
                   .Replace("[", "")
                   .Replace("\"", "");

                // Etiquetas más legibles
                formatted = formatted
                    .Replace("idIngrediente:", "ID Ingrediente: ")
                    .Replace("Nombre_Ingrediente:", "Nombre: ")
                    .Replace("Tipo_Ingrediente:", "Tipo: ")
                    .Replace("Cantidad_gr:", "Cantidad (g): ")
                    .Replace("Calidad_Comida:", "Calidad: ")
                    .Replace("LugarDeOrigen:", "Origen: ")
                    .Replace("EstacionComida:", "Estación: ");

                listaIngredientes = formatted;
                ui_manager_r.chatAa.text = formatted;
                ui_manager_r.textareaSql_go.SetActive(true);

                Debug.Log(listaIngredientes);
            }
            else
            {
                listaIngredientes = "";
                ui_manager_r.chatAa.text = "";
            }
        }
        else
        {
            ui_manager_r.ShowPrompt($"Error al realizar la consulta: {request.error}");
            Debug.Log("SQL Error:" + request.error);
        }

        ui_manager_r.waiting_b = false;

        //
        TestRol test = ui_manager_r.test_parent_go.transform.GetChild(ui_manager_r.testNow_i).GetComponent<TestRol>();

        if (test.query.Contains("Create"))
        {
            TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData(test.infoExtra[0]);
            
            test.modifyDropdownIng.options.Add(opt);
            test.deleteDropdownIng.options.Add(opt);
        }

        if (test.query.Contains("Borrar"))
        {
            int x = test.deleteDropdownIng.value;

            test.modifyDropdownIng.options.RemoveAt(x);
            test.deleteDropdownIng.options.RemoveAt(x);

            test.modifyDropdownIng.RefreshShownValue();
            test.deleteDropdownIng.RefreshShownValue();
        }
    }
    
    private IEnumerator SendQuery_SQLSOLOGET(string type) 
    {     
        string input = "{}";
        string url = $"http://localhost/tfg/Api_MySQL_TFG.php";
        UnityWebRequest request;

        url += $"?{input}"; // Añadir parámetros a la URL
        request = UnityWebRequest.Get(url);
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(input));

        // Agregar headers necesarios
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            listaIngredientes = request.downloadHandler.text;

            //Hacer el texto más presentable
            listaIngredientes = listaIngredientes.Replace("\"", "");
            listaIngredientes = listaIngredientes.Replace("idIngrediente:", "");
            listaIngredientes = listaIngredientes.Replace("Nombre_Ingrediente:", "");
            listaIngredientes = listaIngredientes.Replace("LugarDeOrigen:", "Origen");
            listaIngredientes = listaIngredientes.Replace("Conectado[{", "");
            listaIngredientes = listaIngredientes.Replace("}", "");
            listaIngredientes = listaIngredientes.Replace("idIngrediente", "\n idIngrediente");

            Debug.Log("SQL Realizado:" + request.downloadHandler.text);
            Debug.Log(listaIngredientes);
        }
    }

    private IEnumerator SendQuery_SQLCHAT(string workspaceId, string prompt, string query, string thread)
    {      
        string baseUrlNEW = "http://localhost:3001/api/v1/workspace/";
        string urlIA = baseUrlNEW + $"{workspaceId}/{query.Replace("*", thread)}";

        string url = $"http://localhost/tfg/Api_MySQL_TFG.php";

        if (urlIA.Contains("receta")) //Enviar la lista de la compra
        {
            yield return StartCoroutine(SendQuery_SQLSOLOGET("GET"));
            prompt += "Tengo estos ingredientes: " + listaIngredientes;
        }

        string json = $"{{'message': '{prompt}', 'urlIA': '{urlIA}'}}"; //tocar chat y query segun el tipo
        json = json.Replace("'","\"");
        //json = JsonUtility.ToJson(json);

        UnityWebRequest request;

        Debug.Log("URL" + urlIA);
        Debug.Log("JSON" + json);

        request = UnityWebRequest.Post(url, string.Empty);
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        // Agregar headers necesarios
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.downloadHandler = new DownloadHandlerBuffer();

        // Enviar solicitud
        yield return request.SendWebRequest();

        // Manejar la respuesta
        if (request.result == UnityWebRequest.Result.Success)
        {
            ui_manager_r.errorMessage_txt.text = "Petición enviada";

            Debug.Log("SQL Realizado:" + request.downloadHandler.text);

            listaIngredientes = request.downloadHandler.text;

            //Hacer el texto más presentable
            /*listaIngredientes = listaIngredientes.Replace("ConectadoResponse:", "Conectado Response: \n");
            listaIngredientes = listaIngredientes.Replace("\",\"", "");
            listaIngredientes = listaIngredientes.Replace("\n","\n");
            listaIngredientes = listaIngredientes.Replace("\"textResponse\":\"", "textResponse: \n\n");*/
            string formatted = listaIngredientes
            .Replace("ConectadoResponse:", "\nConectado Response:\n")
            .Replace("{", "\n")
            .Replace("}", "\n")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("\"", "")
            .Replace(",", "\n");

            // Etiquetas más legibles
            formatted = formatted
                .Replace("id:", "ID: ")
                .Replace("type:", "Tipo: ")
                .Replace("close:", "Cerrar: ")
                .Replace("error:", "Error: ")
                .Replace("chatId:", "Chat ID: ")
                .Replace("textResponse:", "Respuesta de texto: ")
                .Replace("sources:", "\nFuentes:\n")
                .Replace("url:", "URL: ")
                .Replace("title:", "Título: ")
                .Replace("docAuthor:", "Autor del documento: ")
                .Replace("description:", "Descripción: ")
                .Replace("docSource:", "Fuente del documento: ")
                .Replace("chunkSource:", "Origen del fragmento: ")
                .Replace("published:", "Publicado: ")
                .Replace("wordCount:", "Número de palabras: ")
                .Replace("token_count_estimate:", "Estimación de tokens: ")
                .Replace("text:", "Texto: ")
                .Replace("_distance:", "Distancia: ")
                .Replace("score:", "Puntaje: ");

            listaIngredientes = formatted;
            ui_manager_r.chatIA.text = formatted;
            ui_manager_r.textareaChat_go.SetActive(true);
        }
        else
        {
            ui_manager_r.ShowPrompt($"Error al realizar la consulta: {request.error}");
            Debug.Log("SQL Error:" + request.error);
        }

        ui_manager_r.waiting_b = false;
    }
}
