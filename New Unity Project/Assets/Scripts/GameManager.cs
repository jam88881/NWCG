using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    //Deck,Name,Cost,Attack,Health,Effect,Count

    public GameObject blankCard;
    public GameObject localZone1;
    public GameObject localZone2;
    public GameObject localZone3;
    public GameObject localZone4;
    public GameObject localZone5;
    public GameObject remoteZone1;
    public GameObject remoteZone2;
    public GameObject remoteZone3;
    public GameObject remoteZone4;
    public GameObject remoteZone5;
    public Card selectedCard;
    public string sPostURL = "https://www.friendpaste.com/";
    public string sThisGameURL = "https://friendpaste.com/6j78vddJ26maCJgIFCcNpp";
    public string sGamesListURL = "https://friendpaste.com/6j78vddJ26maCJgIFCwMuD";
    public string sWhoseTurnIsItAnyway = "Host";
    public string sLocalRole = "Host";
    public List<string> Deck;
    public List<string> playerHand;
    public Vector3 vHand = new Vector3(0,-1,-14);
    public int frameCount = 0;
    public int localHP = 20;
    public int remoteHP = 20;
    public int turn = 1;
    public int supply = 3;
    public long lastTick = long.MaxValue;
    public bool myTurn = true;
    public bool keepSelection;

    // Start is called before the first frame update
    void Start()
    {
        lastTick = DateTime.Now.Ticks;
        string sDeckDataFilename;
        if (CrossSceneData.sJoinGameURL.Length == 0)
        {
            CreateMultiplayerGame();
            sDeckDataFilename = "AustriaDeckData";
        }
        else
        {
            sThisGameURL = CrossSceneData.sJoinGameURL;
            sDeckDataFilename = "FranceDeckData";
            sLocalRole = "Guest";
            myTurn = false;
        }

        Deck = AssembleCardList(sDeckDataFilename, true);

        List<Vector3> listHand = new List<Vector3>();
        //draw 5 cards
        for(int i=0; i < 5; i++)
        { 
            playerHand.Add(DrawCard());
            listHand.Add(new Vector3(vHand.x - (i * 3), vHand.y, vHand.z));
        }

        //generate hand
        GenerateCards(playerHand, listHand);

        //if you are going first thendraw a card
        if (sLocalRole == "Host")
        {
            DrawAndGenerateCard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        //get an update from the server every 50 frames
        if (frameCount % 400 == 0)
        {
            StartCoroutine(GetAndPerformUpdates());

        }

        //deselect card
        if (Input.GetMouseButtonDown(0))
        {
            if (!keepSelection && selectedCard != null)
            {
                selectedCard.selected = false;
                selectedCard = null;
            }
            else 
            {
                keepSelection = false;
            }
        }
    }

    void WhoseTurn(string sGottenData)
    {
        //if the incoming value is different from the current value and it matches local role, then draw a card
        //this is detecting the turn change event


        if (sGottenData.StartsWith("Host"))
        {
            sWhoseTurnIsItAnyway = "Host";
        }
        else
        {
            sWhoseTurnIsItAnyway = "Guest";
        }
    }

    void EndTurn()
    {
        //incrament turn
        turn++;

        if (sLocalRole == "Guest")
        {
            sWhoseTurnIsItAnyway = "Host";
        }
        if (sLocalRole == "Host")
        {
            sWhoseTurnIsItAnyway = "Guest";
        }
        string sGetData = GetComponent<GameManager>().StringGetData(GetComponent<GameManager>().sThisGameURL).Split('`')[1];
        sGetData = sGetData.Replace(sGetData.Split(',')[0], sWhoseTurnIsItAnyway);
        UpdateData(sThisGameURL, CrossSceneData.sCreateGameName, "`" + sGetData + ",Draw---" + DateTime.Now.Ticks.ToString() + "`");

        //set supply
        supply = turn + 2;

        //max 10 supply universal rule
        if (supply > 10)
        {
            supply = 10;
        }

        //reset all "Card.AttacksMadeThisTurn" back to 0
        Card[] cards = GameObject.FindObjectsOfType<Card>();
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].AttacksMadeThisTurn = 0;
        }

    }

    void DrawAndGenerateCard()
    {
        //draw card
        List<string> lDrawnCard = new List<string>();
        lDrawnCard.Add(DrawCard());
        Vector3 drawnCardPosition = new Vector3(-14.88f, -1f, -2.5f);
        List<Vector3> lDrawnCardPosition = new List<Vector3>();
        lDrawnCardPosition.Add(drawnCardPosition);
        GenerateCards(lDrawnCard, lDrawnCardPosition);
    }

    IEnumerator<Coroutine> GetAndPerformUpdates()
    {
        CoroutineWithData cd = new CoroutineWithData(this, GetData(sThisGameURL));
        yield return cd.coroutine;
        string[] gameDataArray = cd.result.ToString().Split('`')[1].Split(',');
        WhoseTurn(gameDataArray[0]);
        for (int i = 1; i < gameDataArray.Length; i++)
        {
            try 
            {
                Debug.Log(gameDataArray[i]);
                string[] entryArray = gameDataArray[i].Split('-');
                if (lastTick < long.Parse(entryArray[3]))
                {
                    //these are the entries we are intrested in, the ones we don't know about yet
                    switch(entryArray[0])
                    {
                        case "Move":
                            string inCard = entryArray[1];
                            List<string> lInCard = new List<string>();
                            lInCard.Add(inCard.Replace("_", ","));
                            string space = entryArray[2].Replace("local", "remote");
                            GameObject oSpace = GameObject.Find(space);
                            List<Vector3> lPos = new List<Vector3>();
                            lPos.Add(oSpace.transform.position + new Vector3(0, 0.1f, 0));
                            GenerateCards(lInCard, lPos, true, true);
                            break;
                        case "Draw":
                            DrawAndGenerateCard();
                            break;
                        case "Attack":
                            Debug.Log(entryArray[1]);
                            GameObject oCardUnderAttack = GameObject.Find(entryArray[1]);
                            oCardUnderAttack.GetComponent<Card>().Health -= Convert.ToInt32(entryArray[2]);
                            break;
                    }
                    lastTick = long.Parse(entryArray[3]);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void OnGUI()
    {
        if (sWhoseTurnIsItAnyway == sLocalRole)
        {
            if (GUI.Button(new Rect(0, 0, 100, 100), "End Turn"))
            {
                EndTurn();
            }
        }
        else
        {
            GUI.Label(new Rect(0, 0, 100, 100), "Other player's turn");
        }

        GUI.Label(new Rect(0, 100, 100, 100), "Supply: " + supply.ToString());

    }

    public string DrawCard()
    {
        string sCard = Deck[0];
        Deck.RemoveAt(0);
        return sCard;
    }

    public void CreateMultiplayerGame()
    {

        Debug.Log(CrossSceneData.sCreateGameName);
        //POST the inital game data
        string responseCreate = PostData(sPostURL, CrossSceneData.sCreateGameName, "`" + sWhoseTurnIsItAnyway + "`");
        //DateTime.Now.Ticks.ToString().Substring(10, 4)

        //put the game data URL in the listing
        string gameURLToList = responseCreate.Split(',')[0].Replace(responseCreate.Split(':')[0], "").Replace("\"", "").Replace(": ", "");
        sThisGameURL = gameURLToList;
        UpdateData(sGamesListURL, "NWCGList", "`" + CrossSceneData.sCreateGameName + "," + gameURLToList + "`");
        Debug.Log(StringGetData(sGamesListURL).Split('`')[1]);
    }

    public void GenerateCards(List<string> lCardList, List<Vector3> pPosition, bool bInPlay = false, bool bRemote = false)
    {
        int i = 0;
        foreach (string sCard in lCardList)
        {
            // x 0-(i*3)
            string[] sArrayCard = sCard.Split(',');
            GameObject goCard = (GameObject)Instantiate(blankCard);
            goCard.GetComponent<Card>().Deck = sArrayCard[0];
            goCard.GetComponent<Card>().Name = sArrayCard[1];
            goCard.GetComponent<Card>().Cost = Convert.ToInt32(sArrayCard[2]);
            goCard.GetComponent<Card>().Attack = sArrayCard[3];
            try { goCard.GetComponent<Card>().Health = Convert.ToInt32(sArrayCard[4]); } catch (Exception ex) { goCard.GetComponent<Card>().Health = 0; }
            goCard.transform.position = new Vector3(pPosition[i].x, pPosition[i].y, pPosition[i].z);
            goCard.GetComponent<Card>().inPlay = bInPlay;
            goCard.GetComponent<Card>().remote = bRemote;
            if (bRemote)
            {
                goCard.name = sArrayCard[0] + "_" + sArrayCard[1] + "_" + sArrayCard[2] + "_" + sArrayCard[3] + "_" + sArrayCard[4] + "_" + sArrayCard[5].Replace("\r", "");
            }
            else
            {
                goCard.name = sArrayCard[0] + "_" + sArrayCard[1] + "_" + sArrayCard[2] + "_" + sArrayCard[3] + "_" + sArrayCard[4] + "_" + sArrayCard[6].Replace("\r", "");
            }
            i++;
        }
    }

    public List<string> AssembleCardList(string sDeckDataFileName, bool bShuffle)
    {
        List<string> cardList = new List<string>();

        //Local Player Cards
        TextAsset LocalPlayerTextAsset = (TextAsset)Resources.Load(sDeckDataFileName, typeof(TextAsset));
        string sLocalPlayerDeckData = LocalPlayerTextAsset.text;
        Debug.Log(sLocalPlayerDeckData);


            foreach (string sEntry in sLocalPlayerDeckData.Split('\n'))
            {
                if (sEntry.Split(',')[0] != "Deck")
                {
                    int cardCount = Convert.ToInt32(sEntry.Split(',')[6].Replace("\r", ""));
                    for (int i = 0; i < cardCount; i++)
                    {
                        cardList.Add(sEntry.Replace("\r", ""));
                    }
                }
            }
        
        if (bShuffle)
        { 
            cardList = cardList.OrderBy(x => UnityEngine.Random.value).ToList();
        }

        Debug.Log("Shuffled Deck List:");
        foreach (string cardEntry in Deck)
        {
            Debug.Log(cardEntry);
        }

        return cardList;

    }

    public string StringGetData(string pURL)
    {
        var request = (HttpWebRequest)WebRequest.Create(pURL);
        request.Method = "GET";
        var response = (HttpWebResponse)request.GetResponse();
        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        return responseString;
    }

    public IEnumerator<string> GetData(string pURL)
    {
        var request = (HttpWebRequest)WebRequest.Create(pURL);
        request.Method = "GET";
        var response = (HttpWebResponse)request.GetResponse();
        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        yield return responseString;
    }

    public string PostData(string pPostURL, string pTitle, string pBody)
    {
        var request = (HttpWebRequest)WebRequest.Create(pPostURL);

        var postData = "{\"title\":\"" + pTitle + "\"," +
                        "\"snippet\":\"" + pBody + "\"," +
                        "\"language\":\"text\"}";
        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }

        var response = (HttpWebResponse)request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        lastTick = DateTime.Now.Ticks;
        return responseString;
    }

    public string UpdateData(string pPutURL, string pTitle, string pBody)
    {
        var request = (HttpWebRequest)WebRequest.Create(pPutURL);

        var postData = "{\"title\":\"" + pTitle + "\"," +
                        "\"snippet\":\"" + pBody + "\"," +
                        "\"language\":\"text\"}";
        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "PUT";
        request.ContentType = "application/json";
        request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }

        var response = (HttpWebResponse)request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        lastTick = DateTime.Now.Ticks;
        return responseString;
    }

}
