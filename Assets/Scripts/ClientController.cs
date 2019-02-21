using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using Anharu;

public class ClientController : MonoBehaviour {
    public Text userNameText;
    private Channel channel;
    void Start() {
        channel = new Channel("127.0.0.1:57601", ChannelCredentials.Insecure);
    }
    void Update() {
		if (Input.GetKey(KeyCode.A)) {
            Debug.Log("A");
        }
	}
    public void SendUser() {
        var client = new User.UserClient(channel);
        Debug.Log(userNameText.text);
        var name = userNameText.text;
        var reply = client.Create(new CreateUserRequest { Name = name });
        Debug.Log("Your ID is" + reply.Id);
        PlayerPrefs.SetString("userId", reply.Id);
        channel.ShutdownAsync().Wait();
		SceneManager.LoadScene("Main");
    }
}
