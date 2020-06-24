using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GenerateCards();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateCards()
    {
        //Local Player Cards
        TextAsset LocalPlayerTextAsset = (TextAsset)Resources.Load("FranceDeckData", typeof(TextAsset));
        string sLocalPlayerDeckData = LocalPlayerTextAsset.text;
        Debug.Log(sLocalPlayerDeckData);

        //Remote Player Cards
        TextAsset RemotePlayerTextAsset = (TextAsset)Resources.Load("AustriaDeckData", typeof(TextAsset));
        string sRemotePlayerDeckData = RemotePlayerTextAsset.text;
        Debug.Log(sRemotePlayerDeckData);



    }

}
