using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaycastManager : MonoBehaviour
{
    #region Fields
    //private GameObject selectedCharacter;
    private TurnHandler turnHandler;
    [Header("Raycast Components:")]
    private Ray mouseRay;
    private RaycastHit hitInfo;
    [SerializeField]
    private LayerMask characterLayerMask;
    [SerializeField]
    private LayerMask tileLayerMask;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        turnHandler = gameObject.GetComponent<TurnHandler>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TestRaycast()
    {
        //Get raycast from camera
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(mouseRay, out hitInfo,characterLayerMask))
        {
            //If the game object is a character (has the tag) send it to the turn handler
            GameObject hitObject = hitInfo.collider.gameObject;
            if (hitObject.tag == "Character")
            {
                turnHandler.SelectedCharacter = hitObject;
            }
        }
    }

    public Tile TileRaycast()
    {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //we are only checking for tiles so we need a different layerMask
        if (Physics.Raycast(mouseRay, out hitInfo, tileLayerMask))
        {
            GameObject hitObject = hitInfo.collider.gameObject;
            return hitObject.GetComponent<Tile>();
        }
        return null;
    }
}
