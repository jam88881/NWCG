using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
        GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = 50;
        Font myFont = (Font)Resources.Load("Fonts/comic", typeof(Font));
        myButtonStyle.font = myFont;
        myButtonStyle.normal.textColor = Color.white;
        myButtonStyle.hover.textColor = Color.red;
        if (GUI.Button(new Rect((Screen.width/2) - 81, 3*(Screen.height/4) , 162, 100), "Start", myButtonStyle))
        {
            CrossSceneData.sCreateGameID = "chadwarmachine";
            SceneManager.LoadScene("InGame");
        }
    }

}
