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
    GameObject instructions, panelGameplay, panelGameover;
    float timeout = 3.5f;
    Vector3 moveVec = Vector3.zero;
    float h = 0f;
    float v = 0f;
    float speed = 5f;
    public bool isPlaying = false;
    bool waitOnce = true;
    bool gameOver = true;
    public bool lobby = true;

    public Animator anim;
    //public CharacterController controller;
    float rotationSpeed = 1000f;
    float rotateAngle = -26f;
    public bool isMoving = false;
    public GameObject startButton, lobbyMenu, gameTimer, pointUI, credits;

    AudioManager aud;
    bool audioOnce = false;
    public static bool gui = true;

    float spawnTime = 0f;

    private void Start()
    {
        aud = FindObjectOfType<AudioManager>();
        instructions = GameObject.Find("/Canvas/Instructions");
        panelGameplay = GameObject.Find("/Canvas/Panel - Gameplay");
        panelGameover = GameObject.Find("/Canvas/Panel - GameOver");
        gameTimer = GameObject.Find("/Canvas/Panel - Gameplay/GameTimer");
        pointUI = GameObject.Find("/Canvas/Panel - Gameplay/Points");
        credits = GameObject.Find("/Canvas/Credits");
        panelGameplay.SetActive(false);
        panelGameover.SetActive(false);
    }

    public void OnGUI()
    {
        //runs at the start of the game and whenever UI has to do something
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        if((!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) || gui)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            //SubmitNewPosition();
            
            

            if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() != null && NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Playing.Value == false && lobby)
            {
                lobbyMenu.SetActive(true);
                isPlaying = true;
                gameOver = false;
                credits.SetActive(false);
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

        //prep every client to play the game (set timer)
        if (lobby)
        {
            if (!gui && lobbyMenu.activeSelf)
            {
                int clientSize = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().ReturnClientCount();
                for (int i = 0; i < 5; ++i)
                {
                    if (i < clientSize)
                    {
                        GameObject.Find("/Canvas/Panel - Lobby/Players/PlayerCard (" + i + ")/Text - Waiting").SetActive(false);
                        GameObject.Find("/Canvas/Panel - Lobby/Players/PlayerCard (" + i + ")/Text - Ready").SetActive(true);
                    }
                    else
                    {
                        GameObject.Find("/Canvas/Panel - Lobby/Players/PlayerCard (" + i + ")/Text - Ready").SetActive(false);
                        GameObject.Find("/Canvas/Panel - Lobby/Players/PlayerCard (" + i + ")/Text - Waiting").SetActive(true);
                    }
                }
            }
            

            if (NetworkManager.Singleton.IsServer)
            {
                var host = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (host != null)
                {
                    if (host.GetComponent<Player>().Playing.Value)
                    {
                        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                        {
                            client.PlayerObject.transform.GetComponent<Player>().Playing.Value = true;
                            //client.PlayerObject.transform.GetComponent<Player>().SetNames();
                            if (!audioOnce) client.PlayerObject.transform.GetComponent<Player>().GameTime.Value = 10f;
                        }
                    }
                }
            }
        }//if (lobby)

        if (isPlaying)
        {
            var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (p.GetComponent<Player>().Playing.Value)
            {
                lobbyMenu.SetActive(false);
                lobby = false;

                if (!audioOnce)
                {
                    aud.Stop("Cassette Tape Dream");
                    aud.Play("Blue Clapper Instrumental");
                    audioOnce = true;
                    instructions.SetActive(false);
                    panelGameplay.SetActive(true);
                }
                //wait for objects to spawn so there aren't any fetch errors, as well as music
                if (waitOnce) timeout -= Time.fixedDeltaTime;

                //////////////////////////////////////////////////////////////////////////////////////GAME LOOP STARTS HERE
                if (timeout <= 0f)
                {
                    waitOnce = false;
                    var player = p.GetComponent<Player>();

                    //set animator
                    anim = p.GetComponent<Animator>();

                    //set timer
                    gameTimer.GetComponent<TMPro.TMP_Text>().text = Mathf.Floor(p.GetComponent<Player>().GameTime.Value).ToString();//currently only updates for host since there isn't code to change the value for other clients

                    //move player
                    if (h != 0f || v != 0f)
                    {
                        moveVec = Vector3.Normalize(new Vector3(h, 0f, v));
                        isMoving = true;
                        Quaternion toRotation = Quaternion.LookRotation(Quaternion.AngleAxis(rotateAngle, Vector3.up) * moveVec, Vector3.up);
                        //transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
                        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed);// * Time.fixedDeltaTime);

                        player.MoveInput(moveVec * speed * Time.fixedDeltaTime, finalRotation);
                    }
                    else
                    {
                        isMoving = false;
                        Quaternion toRotation = Quaternion.LookRotation(moveVec, Vector3.up);
                        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed);

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
                    if (player.Speed.Value > 150f)
                    {
                        player.DecreaseTime();
                        if (player.SpeedTime.Value <= 0f)
                        {
                            player.DecreaseSpeed();
                        }
                    }

                    //point text
                    //panelGameplay.GetComponent<TMPro.TMP_Text>().text = "Points: " + pl.Points.Value;
                    pointUI.GetComponent<TMPro.TMP_Text>().text = "Points: " + player.Points.Value;

                }//if(timeout <= 0f)
                 //////////////////////////////////////////////////////////////////////////////////////GAME LOOP ENDS HERE

                //look to server player's time to see if game is over
                if (p.GetComponent<Player>().GameTime.Value <= 0f)
                {
                    p.GetComponent<Player>().GameTime.Value = 0f;
                    p.GetComponent<Player>().Stop();
                    gameTimer.GetComponent<TMPro.TMP_Text>().text = "0";
                    isPlaying = false;
                    gameOver = true;
                    lobby = false;
                }
            }
        }//if(isPlaying)
        else if(gameOver && !lobby)
        {
            if (audioOnce)
            {
                aud.Stop("Blue Clapper Instrumental");
                aud.Play("Cassette Tape Dream");
                audioOnce = false;
                panelGameplay.SetActive(false);
                waitOnce = true;
                timeout = 5f;
                
                //show winners/scores
                panelGameover.SetActive(true);
                NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().ShowResults();
            }

            

            //play again
            timeout -= Time.fixedDeltaTime;
            if(timeout <= 0f)
            {
                timeout = 3.5f;
                panelGameover.SetActive(false);
                credits.SetActive(true);
                instructions.SetActive(true);
                gameOver = true;
                isPlaying = false;
                audioOnce = true;
                waitOnce = true;
                lobby = true;
                //StartButtons();
                gui = true;
            }
        }
    }//FixedUpdate()

    public static void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
            gui = false;
        }
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
            gui = false;
        }
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

    public void SubmitName(string name)
    {
        Debug.Log("Called. Name is: " + name);
        switch(name)
        {
            case "hell":
            case "fuck":
            case "fvck":
                break;
            default:
                NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().username = name;
                NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>().SetNames();
                break;
        }
    }
}
