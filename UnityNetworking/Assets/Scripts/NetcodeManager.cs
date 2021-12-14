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
    GameObject instructions, pointText;
    float timeout = .5f;
    float gameTime = 120f;
    Vector3 moveVec = Vector3.zero;
    float h = 0f;
    float v = 0f;
    float speed = 5f;
    bool isPlaying = false;
    bool waitOnce = true;
    bool gameOver = true;

    public Animator anim;
    //public CharacterController controller;
    float rotationSpeed = 1000f;
    float rotateAngle = -26f;
    public bool isMoving = false;

    AudioManager aud;
    bool audioOnce = false;

    float spawnTime = 3f;

    private void Start()
    {
        aud = FindObjectOfType<AudioManager>();
        instructions = GameObject.Find("/Canvas/Instructions");
        pointText = GameObject.Find("/Canvas/Points");
        pointText.SetActive(false);
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
        }

        GUILayout.EndArea();
    }

    void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        if (isPlaying)
        {
            var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            FindObjectOfType<CameraFollow>().MoveToPlayer(p.GetComponent<Player>().Position.Value);
        }
    }

    void FixedUpdate()
    {
        if(isPlaying)
        {
            //wait for objects to spawn so there aren't any fetch errors
            if (waitOnce) timeout -= Time.fixedDeltaTime;
            if(!audioOnce)
            {
                aud.Stop("Cassette Tape Dream");
                aud.Play("Blue Clapper Instrumental");
                audioOnce = true;
                instructions.SetActive(false);
                pointText.SetActive(true);
            }

            //////////////////////////////////////////////////////////////////////////////////////GAME LOOP STARTS HERE
            if (timeout <= 0f)
            {
                //set animator
                var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                anim = p.GetComponent<Animator>();

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
                if(spawnTime <= 0f)
                {
                    spawnTime = 3f;
                    transform.gameObject.GetComponent<DropServer>().CallSpawnDrop();
                }

                //check speed timer
                var pO = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var pl = pO.GetComponent<Player>();
                if(pl.Speed.Value > 150f)
                {
                    pl.DecreaseTime();
                    if (pl.SpeedTime.Value <= 0f)
                    {
                        pl.DecreaseSpeed();
                    }
                }

                //point text
                pointText.GetComponent<TMPro.TMP_Text>().text = "Points: " + pl.Points.Value;

            }//if(timeout <= 0f)
             //////////////////////////////////////////////////////////////////////////////////////GAME LOOP ENDS HERE

            //look to server player's time to see if game is over
            //isPlaying = false;
            //gameOver = true;
        }//if(isPlaying)
        else if(gameOver)
        {
            if (audioOnce)
            {
                aud.Stop("Blue Clapper Instrumental");
                aud.Play("Cassette Tape Dream");
                audioOnce = false;
                instructions.SetActive(true);
                pointText.SetActive(false);
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
