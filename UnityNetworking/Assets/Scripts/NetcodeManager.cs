using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetcodeManager : MonoBehaviour
{
    /// <summary>
    /// This extends MonoBehavior, meaning the physical changes you're seeing and doing in game are done here.
    /// That also means when things go out of sync, your changes here might rubberband after Player.cs finally updates Network variables that affect your game
    /// </summary>
    GameObject instructions, panelGameplay;
    float timeout = .5f;
    Vector3 moveVec = Vector3.zero;
    float h = 0f;
    float v = 0f;
    float speed = 5f;
    int hostNumberInClients = 0;
    bool isPlaying = false;
    bool waitOnce = true;
    bool gameOver = true;
    bool lobby = true;

    public Animator anim;
    //public CharacterController controller;
    float rotationSpeed = 1000f;
    float rotateAngle = -26f;
    public bool isMoving = false;
    public GameObject startButton, lobbyMenu, gameTimer, pointUI;

    AudioManager aud;
    bool audioOnce = false;

    float spawnTime = 3f;

    private void Start()
    {
        aud = FindObjectOfType<AudioManager>();
        instructions = GameObject.Find("/Canvas/Instructions");
        panelGameplay = GameObject.Find("/Canvas/Panel - Gameplay");
        gameTimer = GameObject.Find("/Canvas/Panel - Gameplay/GameTimer");
        pointUI = GameObject.Find("/Canvas/Panel - Gameplay/Points");
        panelGameplay.SetActive(false);
    }

    public void OnGUI()
    {
        //runs at the start of the game and whenever UI has to do something
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            //SubmitNewPosition();
            isPlaying = true;
            gameOver = false;

            if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() != null && NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Playing.Value == false)
            {
                lobbyMenu.SetActive(true);
                if (NetworkManager.Singleton.IsServer) startButton.SetActive(true);
            }
        }

        GUILayout.EndArea();
    }

    void Update()
    {
        if (isPlaying)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
            var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            FindObjectOfType<CameraFollow>().MoveToPlayer(p.GetComponent<Player>().Position.Value);
        }
    }

    void FixedUpdate()
    {
        //testing: use this in conjunction with NetworkManager.Singleton stuff to see if
        GameObject.Find("/Canvas/Test").GetComponent<TMPro.TMP_Text>().text = NetworkManager.Singleton.gameObject.tag;

        /*int clientNum = 0;
        foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            //can use this to set each smol character as well as set variable for all of them to play

            //client.PlayerObject.transform.GetComponent<Player>().ReadyToPlay.Value = true;//or something like that

            //below will be needed on connections
            client.PlayerObject.transform.GetComponent<Player>().PlayerNum.Value = clientNum;
            ++clientNum;
        }
        //next line will be useful along with the foreach stuff
        if(NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().PlayerNum.Value < 5)
        {
            //add something to the if statement so that we can say start playing stuff below when everything is good to go
        }
        */

        if (lobby && NetworkManager.Singleton.IsServer)
        {
            var host = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if(host != null)
            {
                if(host.GetComponent<Player>().Playing.Value)
                {
                    foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                    {
                        client.PlayerObject.transform.GetComponent<Player>().Playing.Value = true;
                        if(!audioOnce) client.PlayerObject.transform.GetComponent<Player>().GameTime.Value = 120f;
                    }
                }
            }
        }



        
        if (isPlaying)
        {



            var t = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (t.GetComponent<Player>().Playing.Value)
            {
                lobbyMenu.SetActive(false);


                //wait for objects to spawn so there aren't any fetch errors
                if (waitOnce) timeout -= Time.fixedDeltaTime;
                if (!audioOnce)
                {
                    aud.Stop("Cassette Tape Dream");
                    aud.Play("Blue Clapper Instrumental");
                    audioOnce = true;
                    instructions.SetActive(false);
                    panelGameplay.SetActive(true);
                }

                //////////////////////////////////////////////////////////////////////////////////////GAME LOOP STARTS HERE
                if (timeout <= 0f)
                {
                    //set animator
                    var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    anim = p.GetComponent<Animator>();

                    //set timer
                    gameTimer.GetComponent<TMPro.TMP_Text>().text = Mathf.Floor(t.GetComponent<Player>().GameTime.Value).ToString();//currently only updates for host since there isn't code to change the value for other clients

                    waitOnce = false;

                    //move player
                    if (h != 0f || v != 0f)
                    {
                        moveVec = Vector3.Normalize(new Vector3(h, 0f, v));
                        isMoving = true;
                        Quaternion toRotation = Quaternion.LookRotation(Quaternion.AngleAxis(rotateAngle, Vector3.up) * moveVec, Vector3.up);
                        //transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
                        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed);// * Time.fixedDeltaTime);

                        var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                        var player = playerObj.GetComponent<Player>();
                        player.MoveInput(moveVec * speed * Time.fixedDeltaTime, finalRotation);
                    }
                    else
                    {
                        isMoving = false;
                        Quaternion toRotation = Quaternion.LookRotation(moveVec, Vector3.up);
                        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed);

                        var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                        var player = playerObj.GetComponent<Player>();
                        player.MoveInput(Vector3.zero, finalRotation);
                    }
                    anim.SetBool("isMoving", isMoving);

                    //spawn drops
                    spawnTime -= Time.fixedDeltaTime;
                    if (spawnTime <= 0f)
                    {
                        spawnTime = 3f;
                        transform.gameObject.GetComponent<DropServer>().CallSpawnDrop();
                    }

                    //check speed timer
                    var pO = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var pl = pO.GetComponent<Player>();
                    if (pl.Speed.Value > 150f)
                    {
                        pl.DecreaseTime();
                        if (pl.SpeedTime.Value <= 0f)
                        {
                            pl.DecreaseSpeed();
                        }
                    }

                    //point text
                    //panelGameplay.GetComponent<TMPro.TMP_Text>().text = "Points: " + pl.Points.Value;
                    pointUI.GetComponent<TMPro.TMP_Text>().text = "Points: " + pl.Points.Value;

                }//if(timeout <= 0f)
                 //////////////////////////////////////////////////////////////////////////////////////GAME LOOP ENDS HERE

                //look to server player's time to see if game is over
                //isPlaying = false;
                //gameOver = true;




            }






        }//if(isPlaying)
        else if(gameOver)
        {
            if (audioOnce)
            {
                aud.Stop("Blue Clapper Instrumental");
                aud.Play("Cassette Tape Dream");
                audioOnce = false;
                instructions.SetActive(true);
                panelGameplay.SetActive(false);
            }

            waitOnce = true;
            timeout = .5f;

            //play again
            //gameOver = false;
            //isPlaying = true;
        }
    }

    public static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        //if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" :
            NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Mode: " + mode);
    }

    /*static void SubmitNewPosition()
    {
        string buttonLabel = NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change";
        if(GUILayout.Button(buttonLabel))
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObj.GetComponent<Player>();
            player.Move();
        }
    }*/

    public void Restart()
    {
        StartButtons();
    }
}
