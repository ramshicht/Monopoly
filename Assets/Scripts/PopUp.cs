using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour {

    public GameObject popUpBox;
    public Animator animator;
    public TextMesh PopUpText;
	public void Pop(string text)
    {
        popUpBox.active = true;
        this.PopUpText.text = text;
        animator.SetTrigger("pop");
    }
}
