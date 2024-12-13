using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task_Data_UI_Prefab : MonoBehaviour
{
    public Sprite taskIcon_sprite;
    public string taskName_str;
    public List<string> nameVariants_str = new List<string>();
    public int idTask_i = -1;
}
