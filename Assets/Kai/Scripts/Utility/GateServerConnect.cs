using IFancing.Net;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public interface IGateServerConnect
{
    void Get(string url, Action<Hashtable> cb);
    void Post(string url, WWWForm form, Action<Hashtable> cb);

    void SetResult(string roomName, string questionID, object[] answer, object[] submit, object actual_score, object total_socre, Action<Hashtable> cb);
    void GetResult(string roomName, Action<Hashtable> cb);

}
public class GateServerConnect : MonoBehaviour, IGateServerConnect
{
    [SerializeField]
    private string m_ipAddress = "";

    void Awake()
    {
        GetAddress();
    }
    void GetAddress()
    {
        var path = Application.streamingAssetsPath + "/config.json";
        Get(path, GetAddress_CB);
    }

    void GetAddress_CB(Hashtable hash)
    {
        m_ipAddress = (string)hash["gate_server"];
        print("Gate server ip = " + m_ipAddress);
    }

    public void Get(string url, Action<Hashtable> Get_CB)
    {
        StartCoroutine(Get_Coroutine(m_ipAddress + url, Get_CB));
    }
    IEnumerator Get_Coroutine(string url, Action<Hashtable> Get_CB)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (!request.isNetworkError && !request.isHttpError)
            {
                var str = request.downloadHandler.text;
                var hash = MiniJSON.jsonDecode(str) as Hashtable;
                Get_CB?.Invoke(hash);
            }
        }
    }
    public void Post(string url, WWWForm form, Action<Hashtable> Post_CB)
    {
        StartCoroutine(Post_Enumerator(m_ipAddress + url, form, Post_CB));
    }
    IEnumerator Post_Enumerator(string url, WWWForm form, Action<Hashtable> Post_CB)
    {
        using (var request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();
            if (!request.isNetworkError && !request.isHttpError)
            {
                var str = request.downloadHandler.text;
                var hash = MiniJSON.jsonDecode(str) as Hashtable;
                Post_CB?.Invoke(hash);
            }
        }
    }

    public void SetResult(string roomName, string questionID, object[] answer, object[] submit, object actual_score, object total_socre, Action<Hashtable> cb)
    {
        var results = new ArrayList();
        var hash = new Hashtable();
        hash.Add(questionID, new Hashtable()
        {
            { "answer",answer},
            { "submit",submit },
            { "actual_score",actual_score},
            { "total_score",total_socre}
        });
        results.Add(hash);
        var form = new WWWForm();
        form.AddField("room_name", roomName);
        form.AddField("results", MiniJSON.jsonEncode(results));
        Post("/set_crisispr_result", form, cb);
    }

    public void GetResult(string roomName, Action<Hashtable> cb)
    {
        var form = new WWWForm();
        form.AddField("room_name", roomName);
        Post("/get_crisispr_result", form, cb);
    }
}
