using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LitUpTile : MonoBehaviour
{
    #region Fields
    [SerializeField] private Material unlitMaterial;
    [SerializeField] private Material unlitPlayerMaterial;
    [SerializeField] private Material unselectedStandardMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Tile associatedTile;
    [Header("Raycast Components:")]
    private Ray mouseRay;
    private RaycastHit hitInfo;
    [SerializeField]
    private LayerMask selectedArea;
    #endregion

    #region Properties
    public Tile AssociatedTile
    {
        get { return associatedTile; }
    }

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        unselectedStandardMaterial = unlitMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        GetUnselectedMaterial();

        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out hitInfo, selectedArea))
        {
            if(hitInfo.collider.gameObject == gameObject)
            {
                gameObject.GetComponent<MeshRenderer>().material = selectedMaterial;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = unselectedStandardMaterial;
            }
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = unselectedStandardMaterial;
        }
    }

    private void GetUnselectedMaterial()
    {
        if(associatedTile.occupant == null)
        {
            unselectedStandardMaterial = unlitMaterial;
        }
        else
        {
            if (associatedTile.occupant.GetComponent<Character>())
            {
                unselectedStandardMaterial = unlitPlayerMaterial;
            }
            else
            {
                unselectedStandardMaterial = unlitMaterial;
            }
        }

    }
}
