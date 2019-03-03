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
    private Google.Protobuf.Collections.RepeatedField<UserPosition> users;
    private Hashtable userObjects;

    private CharacterController controller;
    private Animator animCon;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private CancellationTokenSource cts = new CancellationTokenSource();

    void Start()
    {

        // コンポーネントの取得
        this.userObjects = new Hashtable();
        controller = GetComponent<CharacterController>();
        animCon = GetComponent<Animator>();
        channel = new Channel("127.0.0.1:57601", ChannelCredentials.Insecure);
        sendPositon(this.cts.Token);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q)) this.cts.Cancel();
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
        setUsers();
    }
    private async Task sendPositon(CancellationToken token)
    {
        var client = new Multiplay.MultiplayClient(channel);
        try
        {
            var call = client.ConnectPosition();
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken move_next_token = cts.Token;
            var responseReaderTask = Task.Run(async () =>
            {
                while (true)
                {
                    await call.ResponseStream.MoveNext(move_next_token);
                    var position = call.ResponseStream.Current;
                    Debug.Log("Received " + position);
                    this.users = position.Users;
                }
            });
            var num = 5;
            var i = 0;
            var id = PlayerPrefs.GetString("userId");
            while (true)
            {
                Vector3 tmp = player.transform.position;
                var x = tmp.x;
                var y = tmp.y;
                var z = tmp.z;
                var req = new ConnectPositionRequest { Id = id, X = x, Y = y, Z = z };
                await call.RequestStream.WriteAsync(req);
                if (token.IsCancellationRequested) break;
                Debug.Log(i);
                i++;
            }
            await responseReaderTask;
            await call.RequestStream.CompleteAsync();
        }
        catch
        {
            Debug.Log("failed");
            throw;
        }
        channel.ShutdownAsync().Wait();
        Debug.Log("finish!!");
    }
    void setUsers()
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
}