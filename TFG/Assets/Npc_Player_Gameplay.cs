using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Npc_Player_Gameplay : MonoBehaviour
{
    [Header("Scripts")]
    public UI_Manager ui_manager_r;
    
    [Header("Datos")]
    public string nameNpc_str;
    public List<string> nameNpc_Variants_str = new List<string>(); //Por si la IA falla

    public Sprite iconUser_sprite;

    [Header("Status")]
    public float lifePoints_f;
    
    [Header("Gameplay")]
    public string lastTask_str;//Todo: Cambiar el cod dependiente por el ukltimo del array de tareas.


    void Start()
    {
        ui_manager_r = FindAnyObjectByType<UI_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
