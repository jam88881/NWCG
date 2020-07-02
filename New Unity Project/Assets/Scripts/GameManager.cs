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
    public string sPostURL = "https://www.friendpaste.com/";
    public string sThisGameURL = "https://friendpaste.com/6j78vddJ26maCJgIFCcNpp";
    public string sGamesListURL = "https://friendpaste.com/6j78vddJ26maCJgIFCwMuD";
    public string sOrder = "HostMoveFirst";
    public List<string> Deck;
    public List<string> playerHand;
    public Vector3 vHand = new Vector3(0,-1,-14);
    public int frameCount = 0;
    public int localHP = 20;
    public int remoteHP = 20;
    public bool localIsHost = true;

    // Start is called before the first frame update
    void Start()
    {
        if (CrossSceneData.sJoinGameURL.Length == 0)
        {
            CreateMultiplayerGame();
        }
        else
        {
            sThisGameURL = CrossSceneData.sJoinGameURL;
        }

        Deck = AssembleCardList();

        //draw 5 cards
        for(int i=0; i < 5; i++)
        { 
            playerHand.Add(DrawCard());
        }

        //generate hand
        GenerateCards(playerHand, vHand);
    }

    // Update is called once per frame
    void Update()
    {

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
        string responseCreate = postData(sPostURL, CrossSceneData.sCreateGameName, "`" + sOrder + "`");
        //DateTime.Now.Ticks.ToString().Substring(10, 4)

        //put the game data URL in the listing
        string gameURLToList = responseCreate.Split(',')[0].Replace(responseCreate.Split(':')[0], "").Replace("\"", "").Replace(": ", "");
        sThisGameURL = gameURLToList;
        updateData(sGamesListURL, "NWCGList", "`" + CrossSceneData.sCreateGameName + "," + gameURLToList + "`");
        Debug.Log(getData(sGamesListURL).Split('`')[1]);
    }

    public void GenerateCards(List<string> lCardList, Vector3 pPosition)
    {
        int i = lCardList.Count;
        foreach (string sCard in lCardList)
        {
            string[] sArrayCard = sCard.Split(',');
            GameObject goCard = (GameObject)Instantiate(blankCard);
            goCard.GetComponent<Card>().Deck = sArrayCard[0];
            goCard.GetComponent<Card>().Name = sArrayCard[1];
            goCard.GetComponent<Card>().Cost = Convert.ToInt32(sArrayCard[2]);
            goCard.GetComponent<Card>().Attack = sArrayCard[3];
            try { goCard.GetComponent<Card>().Health = Convert.ToInt32(sArrayCard[4]); } catch (Exception ex) { goCard.GetComponent<Card>().Health = 0; }
            goCard.transform.position = new Vector3(0-(i*3),pPosition.y, pPosition.z);
            goCard.name = sArrayCard[0] + "_" + sArrayCard[1] + "_" + sArrayCard[2] + "_" + sArrayCard[3] + "_" + sArrayCard[4] + "_" + sArrayCard[6].Replace("\r", "");
            i--;
        }
    }

    public List<string> AssembleCardList(bool bShuffle = true)
    {
        List<string> cardList = new List<string>();

        //Local Player Cards
        TextAsset LocalPlayerTextAsset = (TextAsset)Resources.Load("FranceDeckData", typeof(TextAsset));
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

        cardList = cardList.OrderBy(x => UnityEngine.Random.value).ToList();

        Debug.Log("Shuffled Deck List:");
        foreach (string cardEntry in Deck)
        {
            Debug.Log(cardEntry);
        }

        return cardList;

    }

    public string getData(string pURL)
    {
        var request = (HttpWebRequest)WebRequest.Create(pURL);
        request.Method = "GET";
        var response = (HttpWebResponse)request.GetResponse();
        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        return responseString;
    }

    public string postData(string pPostURL, string pTitle, string pBody)
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

        //Debug.Log(responseString);
        return responseString;
    }

    public string updateData(string pPutURL, string pTitle, string pBody)
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

        Debug.Log(responseString);
        return responseString;
    }

}
