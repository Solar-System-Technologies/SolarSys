using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Backend;


public class ClickOn : MonoBehaviour
{   
    [SerializeField]
    private Material unselected;
    [SerializeField]
    private Material selected;

    [HideInInspector]
    public bool currentlySelected = false;
    [HideInInspector]
    public bool currentlyHidden = false;

    private Backend.Graph graph = new Backend.Graph();

    private MeshRenderer myRend;
    // Start is called before the first frame update
    void Start()
    {
        myRend = GetComponent<MeshRenderer>();
        ClickMe();

    }

    public void ClickMe() 
    {
        if (currentlySelected == false) 
        {
            myRend.material = unselected;
        }
        else
        {
            getNodeOfSelectedPlanet();
            myRend.material = selected;
        }

    }

    public void HideUnhideMe() 
    {
        if (currentlyHidden == false) 
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void getNodeOfSelectedPlanet()
    {
        FrontEndNode node = GetComponent<FrontEndNode>();
        Debug.Log(node.databaseNode.Text);
    }
   
}
