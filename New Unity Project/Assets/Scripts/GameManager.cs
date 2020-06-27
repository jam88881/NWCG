using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject blankCard;
    public static string sGetURL = "https://friendpaste.com/6j78vddJ26maCJgIFCcNpp";
    public static string sPostURL = "https://www.friendpaste.com/";
    public static string sGameListURL = "https://friendpaste.com/6j78vddJ26maCJgIFCwMuD";
    public List<string> Deck;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(CrossSceneData.sCreateGameID);

        GenerateCards();

        string responseCreate = postData(sPostURL, CrossSceneData.sCreateGameID, DateTime.Now.Ticks.ToString().Substring(10, 4));
        //put the response into the game list
        updateData(sGameListURL, "NWCGList", responseCreate.Split(',')[0].Replace(responseCreate.Split(':')[0], "").Replace("\"","").Replace(": ",""));
        Debug.Log(getData());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateCards()
    {
        GameObject goCard = (GameObject)Instantiate(blankCard);
        goCard.GetComponent<Card>().Deck = "Austria";
        goCard.GetComponent<Card>().Name = "Jagers";
        goCard.GetComponent<Card>().Cost = 3;
        goCard.GetComponent<Card>().Attack = "4";
        goCard.GetComponent<Card>().Health = 5;
        goCard.transform.position = new Vector3(0, 0, 0);

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
                    Deck.Add(sEntry.Replace("\r", ""));
                }
            }
        }
        Debug.Log("Deck Object:");
        foreach (string cardEntry in Deck)
        {
            Debug.Log(cardEntry);
        }

        //JUST WORRY ABOUT THE LOCAL PLAYER RIGHT NOW
        //Remote Player Cards
        //TextAsset RemotePlayerTextAsset = (TextAsset)Resources.Load("AustriaDeckData", typeof(TextAsset));
        //string sRemotePlayerDeckData = RemotePlayerTextAsset.text;
        //Debug.Log(sRemotePlayerDeckData);
    }

    static string getData()
    {
        var request = (HttpWebRequest)WebRequest.Create(sGetURL);

        request.Method = "GET";

        var response = (HttpWebResponse)request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //Debug.Log(responseString);
        return responseString;
    }

    static string postData(string pPostURL, string pTitle, string pBody)
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


    static string updateData(string pPutURL, string pTitle, string pBody)
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
