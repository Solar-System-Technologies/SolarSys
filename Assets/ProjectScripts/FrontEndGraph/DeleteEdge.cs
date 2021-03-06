using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Handles deleting edges; does not reposition remaining edges because it was very involved and adds little, but functionality
// is implemented below; only it is a little too computationally taxing for something so minor.
public class DeleteEdge : MonoBehaviour
{
    // The objects the class at some point may have to instantiate; must be public so that it can be set from editor.
    public GameObject _selfReferencePrefab;         // Self-reference prefab (ring)
    public GameObject _edgePrefab;                  // Straight edge prefab
    public GameObject _curvedPrefab;                // Curved edge prefab
    private GameObject[] _panels;                    // Panel States to check

    void Start() 
    {
        _panels = GameObject.FindGameObjectsWithTag("Panel");
    }

    void OnMouseOver () 
    {
        // depending on the order things are created we may not yet have the panels before start is called,
        // so we do have to poll for this
        if (_panels.Length == 0) _panels = GameObject.FindGameObjectsWithTag("Panel");

        // don't run if any of the panels are active
        foreach (GameObject p in _panels) if (p.activeSelf) return; 

        //this is fine as a loop here as it is the only functionality
        if (Input.GetKeyDown("delete") || Input.GetKeyDown("backspace")) 
         {
            
            gameObject.GetComponent<FrontEndEdge>()._child.GetComponent<FrontEndNode>().from.Remove(gameObject.GetComponent<FrontEndEdge>()._parent);

            gameObject.GetComponent<FrontEndEdge>()._parent.GetComponent<FrontEndNode>().to.Remove(gameObject.GetComponent<FrontEndEdge>()._child);

            gameObject.GetComponent<FrontEndEdge>()._parent.GetComponent<FrontEndNode>().edgeOut.Remove(gameObject);

            // We need these first two lines to avoid exceptions because Update() is called on a non-null text object which has already been destroyed
            GameObject objectRef = gameObject.GetComponent<FrontEndEdge>()._textObject;
            gameObject.GetComponent<FrontEndEdge>()._textObject = null;
            Destroy(objectRef);

            StartCoroutine(gameObject.GetComponent<FrontEndEdge>()._databaseEdge.DeleteFromDatabaseCo(() => Destroy(gameObject)));
        }
    }

    //reposition and remake edges upon deletion; not used in the end because it adds little and is actually quite involved
    void repositionOrRemakeEdges(GameObject fromNode, GameObject toNode) 
    {
        List<GameObject> allAdjacentEdgesFrom = findAllWithChildAndKeep(fromNode, toNode);
        List<GameObject> allAdjacentEdgesTo = findAllWithChildAndKeep(fromNode, toNode);
        var edgesToChange = allAdjacentEdgesFrom.Concat(allAdjacentEdgesTo);
        int edgeCount = edgesToChange.Count();

        if (edgeCount == 0) // will never be zero, but can't be too careful
        {
            return;
        }

        if (toNode == fromNode) // for self refereces, we rerotate
        {
            int i = 0;
            float rotation = 180/(edgeCount-1);
            foreach (GameObject edge in edgesToChange) 
            {
                gameObject.transform.rotation = new Quaternion(0.0f, 0.0f, rotation*i, 0.0f);
            }
        } 
        else if (edgeCount == 1)
        {
            GameObject edge = null;
            if (allAdjacentEdgesFrom.Count() > allAdjacentEdgesTo.Count()) {
                edge = allAdjacentEdgesFrom[0];
            } else {
                edge = allAdjacentEdgesTo[0];
            }

            GameObject edgeObject = Instantiate(_edgePrefab, new Vector3(UnityEngine.Random.Range(-10,10), UnityEngine.Random.Range(-10,10), UnityEngine.Random.Range(-10,10)), Quaternion.identity);
        
            edgeObject.GetComponent<FrontEndEdge>().InstantiateEdge(true,
                                                                    edge.GetComponent<FrontEndEdge>()._databaseEdge, 
                                                                    edge.GetComponent<FrontEndEdge>()._textObject, 
                                                                    edge.GetComponent<FrontEndEdge>()._parent, 
                                                                    edge.GetComponent<FrontEndEdge>()._child, 
                                                                    90f);
            
            Destroy(edge);

            if (allAdjacentEdgesFrom.Count() > allAdjacentEdgesTo.Count()) 
            {
                fromNode.GetComponent<FrontEndNode>().edgeOut.Add(edgeObject);
            }
            else
            {
                toNode.GetComponent<FrontEndNode>().edgeOut.Add(edgeObject);
            }
        } 
        else 
        {
            int i = 0;
            float rotation = 180/(edgeCount-1);
            foreach (GameObject edge in edgesToChange) 
            {
                edge.GetComponent<FrontEndEdge>()._rotation = (rotation*i);
                i=i+1;
            }
        } 
        return;
    
    }

    private List<GameObject> findAllWithChildAndKeep(GameObject fromNode, GameObject toNode) 
    {
        List<GameObject> edgesToChange = new List<GameObject>();
        foreach (GameObject edge in fromNode.GetComponent<FrontEndNode>().edgeOut) 
        {
            if (edge.GetComponent<FrontEndEdge>()._child == toNode) //&& edge.GetComponent<FrontEndEdge>()._child != edge.GetComponent<FrontEndEdge>()._parent) 
            {
                edgesToChange.Add(edge);
            }
        }
        return edgesToChange;
    }

}

