using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Backend;

public class SavedProjects : MonoBehaviour
{

    private List<string> projectList;
    public Transform savedProjectsContent;
    public Transform savedProjectContainer;
    public GameObject newProjectName;
    public GameObject copyProjectName;
    public GameObject createNewProjectPanel;
    public GameObject createCopyProjectPanel;
    public GameObject createdByMeToggle;
    public GameObject sharedWithUserEmail;
    public GameObject errorMessage;
    public GameObject errorMessage_copy;
    [HideInInspector]
    public GraphUser user;
    [HideInInspector]
    public GraphProject selectedProject;
    [HideInInspector]
    public bool readOnly = false;
    public GameObject signInButton;
    public GameObject actionsPanel;
    public string userEmail;


    IEnumerator Start()
    {
        // instantiate new list for project names
        projectList = new List<string>();

        // Coroutine code to display project selections when user logs in
        DatabaseView database = new DatabaseView(); // constructor that doesnt load in Neo4J drivers

        // can get username from login like this
        userEmail = signInButton.GetComponent<AuthenticateUser>().usernameField.GetComponent<TMP_InputField>().text;
        // Debug.Log(userEmail);

        // string userEmail = "foo.bar@doc.ic.ac.uk";
        // string userEmail = "balazs.frei@ic.ac.uk";
        yield return GraphUser.CreateIfNotExists(userEmail);
        user = new GraphUser(userEmail);
        CreatedByMeToggleValueChanged();
    }

    void Update()
    {
        if (selectedProject == null)
        {
            actionsPanel.SetActive(false);
        }
        else
        {
            actionsPanel.SetActive(true);
        }
    }
    
    

    // function to add a project to the list and for display
    public void AddProject(string projectName, Transform chosenContainer, Transform ChosenParentPanel)
    {
        projectList.Add(projectName);
        RectTransform savedGraphRectTransform = Instantiate(chosenContainer, ChosenParentPanel).GetComponent<RectTransform>();
        TextMeshProUGUI textComponent = savedGraphRectTransform.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = projectName;
        savedGraphRectTransform.gameObject.SetActive(true);

    }

    // function to iterate through all projects of a user and add them
    public void DisplayProjectTitles(GraphUser user)
    {
        if (createdByMeToggle.GetComponent<Toggle>().isOn)
        {
            foreach (GraphProject project in user.Projects)
            {
                AddProject(project.Title, savedProjectContainer, savedProjectsContent);
            }
        }
        else {
            foreach (GraphProject project in user.ReadOnlyProjects)
            {
                AddProject(project.Title, savedProjectContainer, savedProjectsContent);
            }
        }
    }

    //This function is called when we select create new project in pop-up panel
    public void CreateProjectButtonClicked()
    {
        bool error = false;
        string projectTitle = newProjectName.GetComponent<TMP_InputField>().text;
        foreach (GraphProject project in user.Projects)
        {
            if (projectTitle == project.Title) 
            {
                error = true;
                errorMessage.SetActive(true);
            }
        }

        if (error == false)
        {
            errorMessage.SetActive(false);
            createNewProjectPanel.SetActive(false);
        
            GraphProject newProject = new GraphProject(user, projectTitle);
            Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(newProject.CreateInDatabase(CreatedByMeToggleValueChanged));
            // StartCoroutine(newProject.CreateInDatabase(CreatedByMeToggleValueChanged));
            newProjectName.GetComponent<TMP_InputField>().text = "";
        }
        
    }

    // this function is called then toggle value is changed, or when we want to refresh the list of projects displayed
    public void CreatedByMeToggleValueChanged() 
    {
        ClearProjectDisplay();
        if (createdByMeToggle.GetComponent<Toggle>().isOn) {
            // StartCoroutine(
            //     user.ReadEmptyProjectsOwned(DisplayProjectTitles)
            // );
            Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(user.ReadEmptyProjectsOwned(DisplayProjectTitles));
        }
        else 
        {
            // StartCoroutine(
            //     user.ReadEmptyProjectsShared(DisplayProjectTitles)
            // );
            Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(user.ReadEmptyProjectsShared(DisplayProjectTitles));
            // Debug.Log("Read Only Projects");
        }
    }

    // function to destroy all objects shown in screen
    private void ClearProjectDisplay()
    {
        foreach(Transform child in savedProjectsContent)
        {
            Destroy(child.gameObject);
        }
    }

    // this function is called when the user clicks share in the ShareWithUserPopUpPanel
    public void ShareProject()
    {
        string userEmail = sharedWithUserEmail.GetComponent<TMP_InputField>().text;
        // Debug.Log(userEmail);
        GraphUser user = new GraphUser(userEmail);

        // StartCoroutine(
        //     selectedProject.ShareWith(user)
        // );
        Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(selectedProject.ShareWith(user));
        sharedWithUserEmail.GetComponent<TMP_InputField>().text = "";
    }

    // function to delete a project
    public void DeleteProject()
    {
        // StartCoroutine(
        //     selectedProject.DeleteFromDatabase(CreatedByMeToggleValueChanged)
        // );  
        Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(selectedProject.DeleteFromDatabase(CreatedByMeToggleValueChanged));
        selectedProject = null;
    }

    // function to copy a project
    public void CopyProject()
    {
        bool error = false;
        string projectTitle = copyProjectName.GetComponent<TMP_InputField>().text;
        foreach (GraphProject project in user.Projects)
        {
            if (projectTitle == project.Title) 
            {
                error = true;
                errorMessage.SetActive(true);
            }
        }

        if (error == false)
        {
            errorMessage_copy.SetActive(false);
            createCopyProjectPanel.SetActive(false);
        
            // StartCoroutine(
            //     selectedProject.CreateCopyInDatabase(user,projectTitle,CreatedByMeToggleValueChanged)
            // );  

            Camera.main.GetComponent<CoroutineRunner>().RunCoroutine(selectedProject.CreateCopyInDatabase(user,projectTitle,CreatedByMeToggleValueChanged));
            copyProjectName.GetComponent<TMP_InputField>().text = "";
        }
       
    }

    // function to set whether project is read only, called when loading a project
    public void setWhetherProjectIsReadOnly()
    {
       Camera.main.GetComponent<CameraReadOnly>().readOnly = !createdByMeToggle.GetComponent<Toggle>().isOn;
    }
}
