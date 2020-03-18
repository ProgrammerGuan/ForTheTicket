using WebSocketSharp;
using System;
using UnityEngine;

public partial struct Message
{
    public string Type;
    public string Data;
}
public class WsClient
{
    WebSocket ws;
    public Action<Message> OnMessage;

    public WsClient(string url)
    {
        ws = new WebSocket(url);
        ws.OnMessage += onMessage;
        ws.Connect();
    }

    void onMessage(object sender, MessageEventArgs e)
    {
        var msg = JsonUtility.FromJson<Message>(e.Data);
        Debug.Log("Type :" + msg.Type);
        OnMessage(msg);
    }

    public void SendMessage(string type, object data)
    {
        ws.Send(JsonUtility.ToJson(new Message
        {
            Type = type,
            Data = JsonUtility.ToJson(data)
        }));
    }

    public void Dispose()
    {
        ws.Close();
    }
}