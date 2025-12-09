using UnityEngine;
//using UnityWebRequest for HTTP requests
using UnityEngine.Networking;
//using System for basic types and DateTime utilities
using System;
//using System.Collections for IEnumerator and coroutines
using System.Collections;
//using System.Collections.Generic for List and other generic collections
using System.Collections.Generic;
//using System.Linq for ordering and filtering collections
using System.Linq;
//using System.Text for Encoding and StringBuilder
using System.Text;

//REST client for a Firebase Realtime Database leaderboard.
public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase")]
    [Tooltip("Your Firebase Realtime Database URL")]
    [SerializeField]
    //base Firebase Realtime Database URL
    private string databaseUrl = "https://upward-leaderboard-default-rtdb.firebaseio.com";

    //singleton instance reference
    public static FirebaseManager Instance { get; private set; }

    private void Awake()
    {
        //if an instance already exists and it is not this one
        if (Instance != null && Instance != this)
        {
            //destroy this duplicate object
            Destroy(gameObject);
            //stop running Awake in this instance
            return;
        }

        //set this object as the singleton instance
        Instance = this;
        //keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    //PUBLIC API: save a single leaderboard entry to Firebase
    public void saveLeaderboardEntry(string playerName, float timeSeconds, Action<bool> callback = null)
    {
        //if player name is null, empty, or whitespace, use default "Player"
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        //create data object to send to Firebase
        var data = new LeaderboardData
        {
            //store the player's name
            name = playerName,
            //store the time
            time = timeSeconds,
            //store current UTC timestamp as seconds since epoch
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        //serialize the data object to JSON string
        string json = JsonUtility.ToJson(data);
        //build the Firebase REST endpoint URL for the leaderboard collection
        string url = $"{databaseUrl}/leaderboard.json";

        //start a coroutine to POST the data to Firebase
        StartCoroutine(PostRequest(url, json, callback));
    }

    /// <summary>Load top N entries ordered by time (fastest first).</summary>
    public void getLeaderboardEntries(Action<List<LeaderboardEntry>> callback, int limit = 10)
    {
        //if no callback was provided, log an error and stop
        if (callback == null)
        {
            Debug.LogError("GetLeaderboardEntries needs a callback.");
            return;
        }

        //build URL with query parameters to order by time and limit results
        string url =
            $"{databaseUrl}/leaderboard.json?orderBy=\"time\"&limitToFirst={limit}";

        //start a coroutine to get the leaderboard from Firebase
        StartCoroutine(GetRequest(url, json =>
        {
            //create a list to hold parsed leaderboard entries
            var result = new List<LeaderboardEntry>();

            //if JSON is empty, null, or an empty object, return empty list
            if (string.IsNullOrEmpty(json) || json == "null" || json == "{}")
            {
                callback(result);
                return;
            }

            try
            {
                //JSON: { "id1": {...},
                //        "id2": {...},
                //               ... }
                //extract each child object as its own JSON string
                foreach (string objJson in ExtractChildObjects(json))
                {
                    //take this JSON string objJson and turn it into a C# object of type LeaderboardData store it in variable data
                    var data = JsonUtility.FromJson<LeaderboardData>(objJson);
                    //skip any entries that failed to parse
                    if (data == null)
                       continue;

                    //convert LeaderboardData into LeaderboardEntry and add to result
                    result.Add(new LeaderboardEntry
                    {
                        //use "Player" if name is blank
                        name = string.IsNullOrWhiteSpace(data.name) ? "Player" : data.name,
                        //copy time directly from data
                        time = data.time
                    });
                }

                //order the result by time ascending and apply the limit
                result = result
                    .OrderBy(e => e.time)
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                //log any exceptions that happen while parsing JSON
                Debug.LogError("Error parsing Firebase JSON: " + ex);
            }

            //call the callback with the final list of entries
            callback(result);
        }));
    }

    //JSON helper: extract each child from {"id1":{...}
    //                                     ,"id2":{...}}
    private static List<string> ExtractChildObjects(string json)
    {
        //list to hold each child object JSON
        var list = new List<string>();
        //if JSON is null or empty, return empty list
        if (string.IsNullOrEmpty(json)) return list;

        //remove whitespace at the ends
        json = json.Trim();
        //if JSON is just an empty object, return empty list
        if (json == "{}") return list;

        //strip leading '{' if present
        if (json[0] == '{') json = json.Substring(1);
        //strip trailing '}' if present
        if (json.Length > 0 && json[json.Length - 1] == '}')
            json = json.Substring(0, json.Length - 1);

        //brace nesting depth
        int depth = 0;
        //start index of current child object
        int start = -1;
        //flag to track if we are inside a quoted string
        bool inString = false;

        //iterate over each character in the JSON
        for (int i = 0; i < json.Length; i++)
        {
            //current character
            char c = json[i];

            //toggle inString when hitting an unescaped quote
            if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                inString = !inString;

            //if we are inside a string, ignore brace logic
            if (inString)
                continue;

            //opening brace marks start of an object
            if (c == '{')
            {
                //if depth is zero, this is the start of a new child object
                if (depth == 0) start = i;
                //increase depth for nested braces
                depth++;
            }
            //closing brace may mark end of an object
            else if (c == '}')
            {
                //decrease depth when closing an object
                depth--;
                //if depth returned to zero and we have a start index
                if (depth == 0 && start != -1)
                {
                    //length of the child object substring
                    int len = i - start + 1;
                    //add substring representing one child object
                    list.Add(json.Substring(start, len));
                    //reset start marker for next object
                    start = -1;
                }
            }
        }

        //return list of all extracted child object JSON strings
        return list;
    }

    //send POST request with JSON body
    private IEnumerator PostRequest(string url, string jsonData, Action<bool> callback)
    {
        //convert JSON string to UTF8 bytes for request body
        byte[] body = Encoding.UTF8.GetBytes(jsonData);

        //create a UnityWebRequest configured for POST
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            //use raw upload handler with the JSON body
            req.uploadHandler = new UploadHandlerRaw(body);
            //use buffer download handler to collect response text
            req.downloadHandler = new DownloadHandlerBuffer();
            //set content type header to JSON
            req.SetRequestHeader("Content-Type", "application/json");

#if UNITY_2020_1_OR_NEWER
            //send the request and wait until it finishes
            yield return req.SendWebRequest();
            //check for network or protocol errors in newer Unity versions
            bool hasError = req.result == UnityWebRequest.Result.ConnectionError ||
                            req.result == UnityWebRequest.Result.ProtocolError;
#else
            //send the request and wait until it finishes
            yield return req.SendWebRequest();
            //check for network or HTTP errors in older Unity versions
            bool hasError = req.isNetworkError || req.isHttpError;
#endif

            //if the request failed, log an error
            if (hasError)
                Debug.LogError($"POST {url} failed: {req.error}");

            //invoke callback with success status if provided
            callback?.Invoke(!hasError);
        }
    }

    // send GET request and return raw JSON text
    private IEnumerator GetRequest(string url, Action<string> callback)
    {
        //create a UnityWebRequest configured for GET
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
#if UNITY_2020_1_OR_NEWER
            //send the request and wait until it finishes
            yield return req.SendWebRequest();
            //check for network or protocol errors in newer Unity versions
            bool hasError = req.result == UnityWebRequest.Result.ConnectionError ||
                            req.result == UnityWebRequest.Result.ProtocolError;
#else
            //send the request and wait until it finishes
            yield return req.SendWebRequest();
            //check for network or HTTP errors in older Unity versions
            bool hasError = req.isNetworkError || req.isHttpError;
#endif

            //if there was an error, log it and invoke callback with null
            if (hasError)
            {
                Debug.LogError($"GET {url} failed: {req.error}");
                callback(null);
            }
            else
            {
                //on success, invoke callback with downloaded JSON text
                callback(req.downloadHandler.text);
            }
        }
    }
}

//data types sent to and from Firebase
[Serializable]
public class LeaderboardData
{
    public string name;
    public float time;
    public long timestamp;
}

//data type used by the game to display leaderboard entries
[Serializable]
public class LeaderboardEntry
{
    public string name;
    public float time;
}
