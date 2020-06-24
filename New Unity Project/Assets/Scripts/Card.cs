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

    // Start is called before the first frame update
    void Start()
    {
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

    }
}
