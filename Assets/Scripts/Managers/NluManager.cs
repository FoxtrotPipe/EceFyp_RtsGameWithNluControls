using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// A class to help in creating the Json object to be sent to the rasa server
public class PostMessageJson {
    public string text;
}

[Serializable]
public class IntentInfo {
    public string name;
    public float confidence;
}

[Serializable]
public class EntityInfo {
    public string entity;
    public int start;
    public int end;
    public float confidence_entity;
    public string value;
    public string extractor;
    public string[] processors;
}

[Serializable]
// A class to extract a single message returned from the bot
public class ReceiveMessageJson {
    public string text;
    public IntentInfo intent;
    public EntityInfo[] entities;
    public int[][] text_tokens;
    public IntentInfo[] intent_ranking;
}

/// <summary>
/// Manage RASA NLU component. Require a live RASA server to work properly
/// </summary>
public class NluManager : MonoBehaviour {

    public static NluManager instance { get; private set; }

    private const string rasa_url = "http://localhost:5005/model/parse";

    private IEnumerator PostRequest(string url, string jsonBody, Action<ReceiveMessageJson> onRespond = null) {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(rawBody);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        float startTime = Time.realtimeSinceStartup;

        yield return request.SendWebRequest();

        float endTime = Time.realtimeSinceStartup;
        float responseTime = endTime - startTime;
        Debug.Log("Response Time: " + responseTime + " seconds");

        string response = request.downloadHandler.text;
        ReceiveMessageJson receiveMessage = JsonUtility.FromJson<ReceiveMessageJson>(response);
        onRespond?.Invoke(receiveMessage);
    }

    /// <summary>
    /// Parse message using the NLU component
    /// </summary>
    /// <param name="message">The message to be parsed</param>
    /// <param name="onRespond">Optional callback function to handle the response</param>
    public void Parse(string message, Action<ReceiveMessageJson> onRespond = null) {
        if (string.IsNullOrEmpty(message)) return;

        // Create a json object from user message
        PostMessageJson postMessage = new PostMessageJson { text = message };

        string jsonBody = JsonUtility.ToJson(postMessage);
        print("[NluManager]: user send: \"" + message + "\"");

        // Create a post request with the data to send to Rasa server
        StartCoroutine(PostRequest(rasa_url, jsonBody, onRespond));
    }

    public void Awake() => instance = this;
}