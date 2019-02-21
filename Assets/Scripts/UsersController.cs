using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Anharu;

public class UsersController : MonoBehaviour
{
    // Start is called before the first frame update
    private Channel channel;
    async void Start()
    {
        channel = new Channel("127.0.0.1:57601", ChannelCredentials.Insecure);
        try {
            await GetUsersPosition();
	    }
        catch (Exception e)
        {
            Debug.Log(e);
        }
	}

    // Update is called once per frame
    void Update()
    {
        
    }
    private async Task GetUsersPosition() {
        var client = new Multiplay.MultiplayClient(channel);
        var request = new GetUsersRequest { RoomId = "room_id" };
        var call = client.GetUsers(request);
        while (await call.ResponseStream.MoveNext()) {
            GetUsersResponse response = call.ResponseStream.Current;
            Debug.Log(response.ToString());
            if (Input.GetKey(KeyCode.A)) break;
        }
        channel.ShutdownAsync().Wait();
    }
}
