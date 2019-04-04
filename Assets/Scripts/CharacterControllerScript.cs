using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using Google.Protobuf;
using System.Threading;
using System.Threading.Tasks;
using Anharu;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerScript : MonoBehaviour
{
    public GameObject player;
    public GameObject playerPrefab;
    public float speed = 3.0F;
    public float rotateSpeed = 3.0F;
    private Channel channel;
    private Multiplay.MultiplayClient client;
    private AsyncDuplexStreamingCall<ConnectPositionRequest, ConnectPositionResponse> call;
    private Google.Protobuf.Collections.RepeatedField<UserPosition> users = new Google.Protobuf.Collections.RepeatedField<UserPosition>();
    private Hashtable userObjects;

    private CharacterController controller;
    private Animator animCon;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private string id;

    void Start()
    {

        // コンポーネントの取得
        this.userObjects = new Hashtable();
        controller = GetComponent<CharacterController>();
        animCon = GetComponent<Animator>();
        channel = new Channel("127.0.0.1:57601", ChannelCredentials.Insecure);
        client = new Multiplay.MultiplayClient(channel);
        call = client.ConnectPosition();
        id = PlayerPrefs.GetString("userId");
    }

    void Update()
    {
        animCon.SetBool("Run", true);
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (moveDirection.x == 0.0f & moveDirection.y == 0.0f & moveDirection.z == 0.0f)
            {
                animCon.SetBool("Run", false);
            }
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        SetUsersPosition();
        SetUsersOnGround();
        SendPosition();
        if (Input.GetKey(KeyCode.Q))
        {
            QuitConnection();
        }
    }
    private async Task SendPosition()
    {
        Vector3 tmp = player.transform.position;
        var x = tmp.x;
        var y = tmp.y;
        var z = tmp.z;
        var req = new ConnectPositionRequest { Id = id, X = x, Y = y, Z = z };
        await call.RequestStream.WriteAsync(req);
    }
    private async Task SetUsersPosition()
    {
        if (await call.ResponseStream.MoveNext())
        {
            var position = call.ResponseStream.Current;
            Debug.Log("Received " + position);
            this.users = position.Users;
        }
    }
    private void SetUsersOnGround()
    {
        var myId = PlayerPrefs.GetString("userId");
        foreach (UserPosition user in this.users)
        {
            if (!this.userObjects.Contains(user.Id) & user.Id == myId)
            {

            }
            else if (!this.userObjects.Contains(user.Id))
            {
                GameObject otherPlayer = (GameObject)Instantiate(playerPrefab, new Vector3((float)user.X, (float)user.Y, (float)user.Z), Quaternion.identity) as GameObject;
                otherPlayer.name = user.Id;
                this.userObjects.Add(user.Id, otherPlayer);
            }
            else
            {
                GameObject activePlayer = (GameObject)this.userObjects[user.Id];
                Vector3 tmp = activePlayer.transform.position;
                activePlayer.transform.position = new Vector3((float)user.X, (float)user.Y, (float)user.Z);
                var aniPreCon = activePlayer.GetComponent<Animator>();
                if ((float)user.X - tmp.x == 0.0f & (float)user.Y - tmp.y == 0.0f & (float)user.Z - tmp.z == 0.0f)
                {
                    aniPreCon.SetBool("Run", false);
                }
                else
                {
                    aniPreCon.SetBool("Run", true);
                }
            }
        }
    }
    private async Task QuitConnection()
    {
        await call.RequestStream.CompleteAsync();
        await SetUsersPosition();
        channel.ShutdownAsync().Wait();
    }
}