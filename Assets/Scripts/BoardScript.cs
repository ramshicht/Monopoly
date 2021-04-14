using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    Button[] Buttons;
	// Use this for initialization
	void Start () {
        Buttons = new Button[40];
        //PlaceButtons();
	}
    private void OnMouseDown()
    {
        //RaycastHit hit;
        Ray ray = GameObject.Find("Camera0").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        //Debug.Log(ray.GetPoint(d));

        //Debug.Log(ray.direction);
        Debug.Log(Input.mousePosition);
       //Debug.Log(ray.GetPoint(21f));
        
        
       
    }
    private void PlaceButtons()
    {
        for (int i = 0; i < 40; i++)
        {
            Buttons[i].onClick.AddListener(() =>
            {
                Debug.Log(Buttons[i].transform.position);
            }
            );
            float y = 1.5F;
            Vector3 vector = new Vector3(4.2F, y, -4.2F);
            if (i == 0)
            {
               
            }
            else
            {
                if (i < 10)
                {
                    Buttons[i].transform.position= new Vector3(1.6F - (1.9F * (i - 1)), y, -4.2f);
                  
                }
                else
                {
                    if (i == 10)
                    {
                       
                    }
                    else
                    {
                        if (i < 20)
                        {
                            Buttons[i].transform.position = new Vector3(-16.5f, y, (float)(-1.7f + (float)((i % 10 - 1) * 1.9)));
                        }
                        else
                        {
                            if (i == 20)
                            {
                                ;
                            }
                            else
                            {
                                if (i < 30)
                                {
                                    Buttons[i].transform.position = new Vector3(-13.7f + (float)((i % 10 - 1) * 2f), y, 16f);
                                }
                                else
                                {
                                    if (i == 30)
                                    {
                                        

                                    }
                                    else
                                    {
                                        Buttons[i].transform.position = new Vector3(4.5f, y, (float)(14f - (float)((i % 10 - 1) * 1.9)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
