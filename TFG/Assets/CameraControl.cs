using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera Camera;
    public Vector3 originalPosition_v3 = Vector3.zero;
    public Quaternion originalRotation_v3;
    public float fieldviewOriginal_f = 60;

    public GameObject npcSelected_go;

    public Vector3 ve3npc;

    [Header("Visual")]
    public AudioSource clipSource;

    bool zooming_b = false;
    float targetFieldviewOriginal_f = -1;
    float waitingTime_f = 0;

    void Start()
    {
        originalPosition_v3 = Camera.transform.position;
        originalRotation_v3 = Camera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (zooming_b)
        {
            waitingTime_f += Time.deltaTime;
            
            if (targetFieldviewOriginal_f != fieldviewOriginal_f)
            {
                if (targetFieldviewOriginal_f > fieldviewOriginal_f) 
                {
                    fieldviewOriginal_f += waitingTime_f;
                }
                else
                {
                    fieldviewOriginal_f -= waitingTime_f;
                }
            }
            else
            {
                zooming_b = false;
            }

            Camera.fieldOfView = fieldviewOriginal_f;

            if ((targetFieldviewOriginal_f - fieldviewOriginal_f) < 2 && (fieldviewOriginal_f - targetFieldviewOriginal_f) < 2)
            {
                zooming_b = false;
                clipSource.Stop();
            }
        }
    }

    public void FollowNpc(GameObject npc)
    {
        if (npcSelected_go != null)
        {
            if (npcSelected_go.GetComponent<Npc_Player_Gameplay>().nameNpc_str == npc.GetComponent<Npc_Player_Gameplay>().nameNpc_str)
            {
                npcSelected_go = null;

                Camera.fieldOfView = fieldviewOriginal_f;
                this.transform.position = originalPosition_v3;
                this.transform.rotation = originalRotation_v3;
            }
            else
            {
                npcSelected_go = npc;
            }
        }
        else
        {
            npcSelected_go = npc;
        }
    }
    
    public void ActivateCamara(int leftRight, int height)
    {
        switch (height) //Todo,hacerlo progresivo -Darle un flash
        {
            case -1://Bajo
                targetFieldviewOriginal_f = fieldviewOriginal_f;
                break;

            case 0://Medio
                targetFieldviewOriginal_f = 40;
                break;

            case 1://Alto
                targetFieldviewOriginal_f = 20;
                break;
        }
        zooming_b = true;
        waitingTime_f = 0;

        if (leftRight == -1 && height == 0)
        {
            Camera.fieldOfView = fieldviewOriginal_f;
            this.transform.position = originalPosition_v3;
            this.transform.rotation = originalRotation_v3;
        }
        if (leftRight == 1 && height == 0)
        {
            transform.rotation = Quaternion.Euler(
            33 % 360f,
            36 % 360f,
            originalRotation_v3.z
            );
        }
        
        if (leftRight == -1 && height == 0)
        {
            transform.rotation = Quaternion.Euler(
            38 % 360f,
            -12 % 360f,
            originalRotation_v3.z
            );
        }
        if (leftRight == 1 && height == 0)
        {
            transform.rotation = Quaternion.Euler(
            38 % 360f,
            36 % 360f,
            originalRotation_v3.z
            );
        }
        
        if (leftRight == -1 && height == 1)
        {
            transform.rotation = Quaternion.Euler(
            21 % 360f,
            -12 % 360f,
            originalRotation_v3.z
            );
        }
        if (leftRight == 1 && height == 1)
        {
            transform.rotation = Quaternion.Euler(
            21 % 360f,
            16 % 360f,
            originalRotation_v3.z
            );
        }

        clipSource.Play();
    }


}
