using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public string sGamesListURL = "https://friendpaste.com/6j78vddJ26maCJgIFCwMuD";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        GUIStyle myGUIStyle = new GUIStyle(GUI.skin.button);
        myGUIStyle.fontSize = 50;
        Font myFont = (Font)Resources.Load("Fonts/comic", typeof(Font));
        myGUIStyle.font = myFont;
        myGUIStyle.normal.textColor = Color.white;
        myGUIStyle.hover.textColor = Color.red;
        //create
        if (GUI.Button(new Rect((Screen.width / 2) - 100, 3 * (Screen.height / 4), 162, 100), "Create", myGUIStyle))
        {
            CrossSceneData.sCreateGameName = "chadwarmachine" + DateTime.Now.ToString("yyyyMMddHHmmss");
            SceneManager.LoadScene("InGame");
        }

        //join
        if (GUI.Button(new Rect((Screen.width/2) + 100, 3*(Screen.height/4) , 162, 100), "Join ", myGUIStyle))
        {
            CrossSceneData.sJoinGameURL = getHostedGames(sGamesListURL).Split('`')[1].Split(',')[1];
            SceneManager.LoadScene("InGame");
        }
    }

    public string getHostedGames(string pURL)
    {
        var request = (HttpWebRequest)WebRequest.Create(pURL);
        request.Method = "GET";
        var response = (HttpWebResponse)request.GetResponse();
        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        return responseString;
    }

}
