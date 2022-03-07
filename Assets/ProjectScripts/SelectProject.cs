using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Backend;

public class SelectProject : MonoBehaviour
{

    public GameObject savedPathViewsPanel;
    public GameObject savedProjectsPanel;
    public GameObject selectedProjectDisplayText;
    public GameObject createdByMeToggle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectedProject() 
    {
        GameObject projectContainer = gameObject.transform.parent.gameObject;
        GameObject titleField = projectContainer.transform.Find("Text (TMP)").gameObject;
        string title = titleField.GetComponent<TextMeshProUGUI>().text;
        GraphProject project = null;


        if (createdByMeToggle.GetComponent<Toggle>().isOn)
        {
            List<GraphProject> projects = savedProjectsPanel.GetComponent<SavedProjects>().user.Projects;
            foreach (GraphProject p in projects)
            {
                if (p.Title == title) {
                    project = p;
                }
            }

            savedProjectsPanel.GetComponent<SavedProjects>().readOnly = false;
        }
        else
        {
            List<GraphProject> projects = savedProjectsPanel.GetComponent<SavedProjects>().user.ReadOnlyProjects;
            foreach (GraphProject p in projects)
            {
                if (p.Title == title) {
                    project = p;
                }
            }

            savedProjectsPanel.GetComponent<SavedProjects>().readOnly = true;
            
        }
        
        savedProjectsPanel.GetComponent<SavedProjects>().selectedProject = project;
        selectedProjectDisplayText.GetComponent<TextMeshProUGUI>().text = title;
      

    }
}
