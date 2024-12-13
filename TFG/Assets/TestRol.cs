using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRol : MonoBehaviour
{
    public string testName = "";
    public string idWorkspace = "";
    public string query = "";
    public string idArea = "";

    [Header("SQL")]
    public List<string> infoExtra = new List<string>();
    public TMP_Text workspaceSelect_txt;

    [Header("")]
    public TMP_Dropdown typeDropdownIng;
    public TMP_Dropdown modifyDropdownIng;
    public TMP_Dropdown deleteDropdownIng;

    [Header("")]
    public TMP_InputField newIngName_INPUT;
    public TMP_InputField newCantidadName_INPUT;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Set_Workspace(int type)
    {
        if (type == 0) { idWorkspace = "tfg-pdf-informatico"; }
        
        if (type == 1) { idWorkspace = "tfg-pdf-fusion"; }
        
        if (type == 2) { idWorkspace = "tfg-receta-sql"; }

        if (type == 11) { idWorkspace = query; }

        if (workspaceSelect_txt != null)
        {
            workspaceSelect_txt.text = idWorkspace;
        }
    }
    
    public void Set_Query_SQL(int type)
    {
        infoExtra.Clear();

        //

        if (type == 0)
        {
            query = "Create";
            try
            {
                infoExtra.Add(newIngName_INPUT.text);
                infoExtra.Add(typeDropdownIng.options[typeDropdownIng.value].text);

                workspaceSelect_txt.text = query;
            }
            catch (System.Exception)
            {
                workspaceSelect_txt.text = "Faltan valores";
                throw;
            }
        }
        if (type == 1)
        {
            query = "Modificar";
            try
            {
                infoExtra.Add(modifyDropdownIng.options[modifyDropdownIng.value].text);
                infoExtra.Add(newCantidadName_INPUT.text);
                workspaceSelect_txt.text = query;
            }
            catch (System.Exception)
            {
                workspaceSelect_txt.text = "Faltan campos";
                throw;
            }

            try
            {
                int.Parse(newCantidadName_INPUT.text);
            }
            catch (System.Exception)
            {
                workspaceSelect_txt.text = "Cantidad inválida";
                infoExtra.Clear();
                throw;
            }
        }
        if (type == 2)
        {
            query = "Borrar";
            infoExtra.Add(deleteDropdownIng.options[typeDropdownIng.value].text);
            workspaceSelect_txt.text = query;
        }

        if (type == 4)
        {
            query = "GET";
            infoExtra.Add("");
            workspaceSelect_txt.text = query;
        }
    }
}
