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
    public int AttacksPerTurn = 1;
    public int AttacksMadeThisTurn = 0;
    public int lane = 0;
    public bool inPlay = false;
    public bool remote = false;
    public bool selected = false;
    public Light inPlayIndicatorLight;
    public GameManager oGameManager;

    // Start is called before the first frame update
    void Start()
    {
        oGameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

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

    }

    // Update is called once per frame
    void Update()
    {
        inPlayIndicatorLight.enabled = selected;

        //death condition
        if (Health <= 0)
        {
            Debug.Log(this.name + " is dead");
            Destroy(this.gameObject);
        }
    }

    void OnGUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(screenPos.x - (Screen.width/20), (Screen.height - screenPos.y) - (Screen.height/15), Screen.width / 20, Screen.height / 20), Attack);
        GUI.Label(new Rect(screenPos.x - (Screen.width/20), (Screen.height - screenPos.y) - (Screen.height/40), Screen.width / 15, Screen.height / 15), Health.ToString());
    }

    private void OnMouseDown()
    {
        if (!remote)
        {
            StartCoroutine(SelectMe());
        }
        else 
        {
            oGameManager.keepSelection = true;
            //if this card can still attack this turn and it is your turn and both cards are in Play 
            //and the difference between the lane number less than or equal to 1
            if (AttacksMadeThisTurn < AttacksPerTurn && oGameManager.sWhoseTurnIsItAnyway == oGameManager.sLocalRole
                && oGameManager.selectedCard.inPlay && inPlay && Math.Abs(oGameManager.selectedCard.lane - lane) <= 1)
            {
                AttacksMadeThisTurn++;
                PerformAttack();
            }
            
        }
    }

    void PerformAttack()
    {
        //local
        int iOffensiveCardAttack = Convert.ToInt32(oGameManager.selectedCard.Attack);
        Health -= iOffensiveCardAttack;

        //send the information to remote
        string sGetData = oGameManager.GetComponent<GameManager>().StringGetData(oGameManager.GetComponent<GameManager>().sThisGameURL).Split('`')[1];
        string sPutData = "`" + sGetData + ",Attack-" + this.name + "-" + iOffensiveCardAttack.ToString() + "-" + (DateTime.Now.Ticks - 1).ToString();

        //if cards in the same lane then attacking card takes retaliation damage
        if (oGameManager.selectedCard.lane - lane == 0)
        {
            //local
            oGameManager.selectedCard.Health -= Convert.ToInt32(Attack);

            //remote
            sPutData = sPutData + ",Attack-" + oGameManager.selectedCard.gameObject.name + "-" + Attack + "-" + DateTime.Now.Ticks.ToString();
        }

        sPutData = sPutData + "`";
        oGameManager.GetComponent<GameManager>().UpdateData(oGameManager.GetComponent<GameManager>().sThisGameURL, CrossSceneData.sCreateGameName, sPutData);

    }

    private IEnumerator SelectMe()
    {
        //wait one frame to deselect the other card
        yield return 0;
        selected = true;
        oGameManager.selectedCard = this;
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
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.name.Contains("local") && !(hit.collider.gameObject.GetComponent<CardSpace>().occupied))
            {
                //is it actually my turn? Can I afford to Pay for this card?
                if (oGameManager.sWhoseTurnIsItAnyway == oGameManager.sLocalRole && Cost <= oGameManager.supply)
                {
                    oGameManager.supply -= Cost;
                    try
                    {
                        this.transform.position = hit.collider.gameObject.transform.position + new Vector3(0, 0.1f, 0);
                        hit.collider.gameObject.GetComponent<CardSpace>().occupied = true;
                        Destroy(GetComponent<Draggable>());
                        //oGameManager
                        string sGetData = oGameManager.GetComponent<GameManager>().StringGetData(oGameManager.GetComponent<GameManager>().sThisGameURL).Split('`')[1];
                        string sPutData = "`" + sGetData + "," + "Move-" + this.name + "-" + hit.collider.gameObject.GetComponent<CardSpace>().name + "-" + DateTime.Now.Ticks.ToString() + "`";
                        oGameManager.GetComponent<GameManager>().UpdateData(oGameManager.GetComponent<GameManager>().sThisGameURL, CrossSceneData.sCreateGameName, sPutData);
                        inPlay = true;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else 
                {
                    Debug.Log("It's not your turn");
                }
            }
        }
    }
}
