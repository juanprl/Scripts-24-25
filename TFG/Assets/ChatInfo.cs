using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatInfo : MonoBehaviour
{
    public string workspace = "";
    public string thread = "";

    public TMP_Text buttonText;

    public void Set_Info(string workspace, string thread)
    {
        this.workspace = workspace;
        this.thread = thread;

        buttonText.text = "Chat: " + this.transform.parent.childCount;
    }
}
