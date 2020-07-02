using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string Deck;
    public string Name;
    public int Cost;
    public string Attack;
    public int Health;
    public bool inPlay = false;
    public Light inPlayIndicatorLight;
    public GameObject oGameManager;

    // Start is called before the first frame update
    void Start()
    {
        oGameManager = GameObject.Find("GameManager");

        //set card face
        GameObject face = this.transform.Find("face").gameObject;
        Texture2D faceTexture = Resources.Load("Cards/" + Deck + "/" + Name) as Texture2D;
        Material matFace = new Material(Shader.Find("Diffuse"));
        matFace.mainTexture = faceTexture;
        face.GetComponent<Renderer>().material = matFace;

        //set card back
        GameObject back = this.transform.Find("back").gameObject;
        Texture2D backTexture = Resources.Load("Cards/back") as Texture2D;
        Material matBack = new Material(Shader.Find("Diffuse"));
        matBack.mainTexture = backTexture;
        back.GetComponent<Renderer>().material = matBack;

        //inPlayIndicatorLight.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(screenPos.x - (Screen.width/20), (Screen.height - screenPos.y) - (Screen.height/15), Screen.width / 20, Screen.height / 20), Attack);
        GUI.Label(new Rect(screenPos.x - (Screen.width/20), (Screen.height - screenPos.y) - (Screen.height/40), Screen.width / 15, Screen.height / 15), Health.ToString());
    }

    private void OnMouseUp()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.name.Contains("local") && !(hit.collider.gameObject.GetComponent<CardSpace>().occupied))
            {
                try
                {
                    this.transform.position = hit.collider.gameObject.transform.position + new Vector3(0, 0.1f, 0);
                    hit.collider.gameObject.GetComponent<CardSpace>().occupied = true;
                    Destroy(GetComponent<Draggable>());
                    //oGameManager
                    string sGetData = oGameManager.GetComponent<GameManager>().getData(oGameManager.GetComponent<GameManager>().sThisGameURL).Split('`')[1];
                    string sPutData = "`" + sGetData + "," + "Move-" + this.name + "-" + hit.collider.gameObject.GetComponent<CardSpace>().name + "`";
                    oGameManager.GetComponent<GameManager>().updateData(oGameManager.GetComponent<GameManager>().sThisGameURL, CrossSceneData.sCreateGameName, sPutData);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("huuupuuuuu");
    }
}
