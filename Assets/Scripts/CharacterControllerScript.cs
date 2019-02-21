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

	void Start()
	{

		// コンポーネントの取得
		this.userObjects = new Hashtable();
		controller = GetComponent<CharacterController>();
		channel = new Channel("127.0.0.1:57601", ChannelCredentials.Insecure);
        sendPositon();
	}

	void Update()
	{

		// 回転
		transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);

		// キャラクターのローカル空間での方向
		Vector3 forward = transform.transform.forward;

		float curSpeed = speed * Input.GetAxis("Vertical");

		// SimpleMove関数で移動させる
		controller.SimpleMove(forward * curSpeed);
        if (Input.GetKey("up"))
		{
            controller.SimpleMove(forward * 1);
		}
        setUsers();
	}
    private async Task sendPositon()
    {
        var client = new Multiplay.MultiplayClient(channel);
        try
        {
            var call = client.ConnectPosition();
            var responseReaderTask = Task.Run(async () =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                while (true)
                {
					await call.ResponseStream.MoveNext(token);
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
                var req = new ConnectPositionRequest { Id = id, X = x, Y = y };
                await call.RequestStream.WriteAsync(req);
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
    }
    void setUsers()
    {
		foreach (UserPosition user in this.users) {
            if (!this.userObjects.Contains(user.Id))
            {
                GameObject otherPlayer = (GameObject)Instantiate(playerPrefab, new Vector3((float)user.X, (float)user.Y, 0.0f), Quaternion.identity) as GameObject;
                otherPlayer.name = user.Id;
                this.userObjects.Add(user.Id, otherPlayer);
            }
            else {
                GameObject activePlayer = (GameObject)this.userObjects[user.Id];
                activePlayer.transform.position = new Vector3((float)user.X, (float)user.Y, 0.0f);
            }
		}
	}
}