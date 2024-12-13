using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ui_manager_start : MonoBehaviour
{
    public GameObject infoJuegoVoz_go;
    public GameObject api_go;

    int button_i = -1;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void NextScene(int xscene)
    {
        if (xscene == 1)
        {
            if (button_i == 2) { button_i = -1; }

            if (button_i == -1)
            {
                button_i = xscene;
                infoJuegoVoz_go.SetActive(true);
                api_go.SetActive(false);
            }
            else
            {
                SceneManager.LoadScene("RolAI", LoadSceneMode.Single);
            }
        }
        
        if (xscene == 2)
        {
            if (button_i == 1) { button_i = -1; }
            
            if (button_i == -1)
            {
                button_i = xscene;
                infoJuegoVoz_go.SetActive(false);
                api_go.SetActive(true);
            }
            else
            {
                SceneManager.LoadScene("SampleScene",LoadSceneMode.Single);
            }
        }
    }
}
