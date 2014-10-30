using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameThrivePush.MiniJSON;

public class MainController : MonoBehaviour {

    private class GameThrivePlayer {
        public string name, playerId;
    }

    List<GameThrivePlayer> players;

    private static string GTAppId = "abfa40c2-606a-11e4-a646-23007197f2dd";

    private static string incomingMessage = null;

    private string thisPlayerId = null;

    public static MainController instance;

	// Use this for initialization
	void Start () {
        instance = this;

        GameThrive.Init(GTAppId, "703322744261", HandleNotification);
        GameThrive.GetIdsAvailable((playerId, pushToken) => {
            thisPlayerId = playerId;
        });

        // Testing Datas
        players = new List<GameThrivePlayer>();
        players.Add(new GameThrivePlayer() { name = "Player 1", playerId = "bcc6392e-606a-11e4-b132-37bd580286bd" });
        players.Add(new GameThrivePlayer() { name = "Player 2", playerId = "0d1444d4-606b-11e4-b1ff-b750284a9f04" });
        players.Add(new GameThrivePlayer() { name = "Player 3", playerId = "bcc6392e-606a-11e4-b132-37bd580286bd" });
        players.Add(new GameThrivePlayer() { name = "Player 4", playerId = "0d1444d4-606b-11e4-b1ff-b750284a9f04" });
        players.Add(new GameThrivePlayer() { name = "Player 5", playerId = "bcc6392e-606a-11e4-b132-37bd580286bd" });
        players.Add(new GameThrivePlayer() { name = "Player 6", playerId = "0d1444d4-606b-11e4-b1ff-b750284a9f04" });
        players.Add(new GameThrivePlayer() { name = "Player 7", playerId = "bcc6392e-606a-11e4-b132-37bd580286bd" });
        players.Add(new GameThrivePlayer() { name = "Player 8", playerId = "0d1444d4-606b-11e4-b1ff-b750284a9f04" });

        // Test new message window
        //incomingMessage = "Test message 11111 2222222 33 4444444444 55555";

        // TODO: Get player list from server
        StartCoroutine(GetDevices());
	}

    // Gets called when the player opens the notification.
    private static void HandleNotification(string message, Dictionary<string, object> additionalData, bool isActive) {
        if (isActive)
            incomingMessage = message;
        else if (additionalData != null && additionalData.ContainsKey("actionSelected")) {
            if (additionalData["actionSelected"].Equals("sound"))
                instance.SendSound((string)additionalData["sender"], "I got your messsage! Here is a custom sound for you!");
            else if (additionalData["actionSelected"].Equals("message"))
                instance.SendButtons((string)additionalData["sender"], "I got your messsage! Here is a message back at you!");
        }
    }

    private Vector2 scrollPosition = Vector2.zero;
    
    void OnGUI() {
        float screenScale = (Screen.width / 1920f) * 4.5f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(screenScale, screenScale, 1f));

        if (incomingMessage != null) {
            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            //windowStyle.fontSize = 30;
            GUI.Window(0, new Rect(20, 20, 400, 400), DoMyWindow, "New Message!", windowStyle);
        }

        if (incomingMessage == null) {
            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 425, 650), scrollPosition, new Rect(0, 0, 220, 240 + PLAYER_GUI_Y_SIZE * players.Count));
            ShowPlayerList();

            // Refresh button
            GUIStyle refeshButtonStyle = new GUIStyle(GUI.skin.button);
            refeshButtonStyle.fontSize = 25;
            if (GUI.Button(new Rect(150, 60, 120, 50), "Refresh", refeshButtonStyle))
                StartCoroutine(GetDevices());

            GUI.EndScrollView();
        }
    }

    void DoMyWindow(int windowID) {
        GUIStyle messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.fontSize = 25;
        GUI.Label(new Rect(20, 60, 350, 200), incomingMessage, messageStyle);

        GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
        closeButtonStyle.fontSize = 25;

        if (GUI.Button(new Rect(150, 330, 120, 50), "Close", closeButtonStyle))
            incomingMessage = null;
    }

    private const int PLAYER_GUI_Y_SIZE = 120;

    void ShowPlayerList() {
        GUIStyle playerLabelFont = new GUIStyle(GUI.skin.label);
        playerLabelFont.fontSize = 25;
        playerLabelFont.normal.textColor = Color.white;

        GUIStyle customTextSize = new GUIStyle("button");
        customTextSize.fontSize = 25;

        GUIStyle playerListStyle = new GUIStyle("box");
        playerListStyle.fontSize = 30;
        playerListStyle.normal.textColor = Color.white;
        playerListStyle.fontStyle = FontStyle.Bold;
        GUI.Box(new Rect(10, 10, 395, 190 + PLAYER_GUI_Y_SIZE * players.Count), "Player List", playerListStyle);

        float yOffset;
        for(int i = 0; i < players.Count; i++) {
            yOffset = i * PLAYER_GUI_Y_SIZE;
            //GUI.Box(new Rect(10, 80 + yOffset, 300, 100), "", customBoxSize);

            GUI.Label(new Rect(150, 140 + yOffset, 200, 40), players[i].name, playerLabelFont);

            if (GUI.Button(new Rect(20, 180 + yOffset, 178, 60), "Sound", customTextSize))
                SendSound(players[i].playerId, "You got a sound message from someone!");

            if (GUI.Button(new Rect(215, 180 + yOffset, 178, 60), "Message", customTextSize))
                SendButtons(players[i].playerId, "You got a button message from someone!");
        }
    }

    private void SendSound(string playerId, string contentString) {
        print("Send Sound!");
        StartCoroutine(DoPost(playerId, contentString, "\"android_sound\": \"notification\","
                                                     + "\"ios_sound\": \"notification.wav\","));
    }

    private void SendButtons(string playerId, string contentString) {
        print("SendButtons");
        StartCoroutine(DoPost(playerId, contentString + " Reply back to the sender with a sound or a message?",
                                        "\"buttons\": ["
                                           + "{\"id\": \"sound\", \"text\": \"Send Sound\"},"
                                           + "{\"id\": \"message\", \"text\": \"Send Message\"}"
                                        + "],"
                                        + "\"data\": {"
                                           + "\"sender\": " + "\"" + thisPlayerId + "\""
                                        + "},"));
    }

    private IEnumerator DoPost(string playerId, string content, string extraOptions) {
        print("DoPost");

        string jsonString = "{"
                              + "\"app_id\": \"" + GTAppId + "\"," 
                              + "\"contents\": {\"en\": \"" + content + "\"},"
							  + "\"isAndroid\": true,"
                              + "\"isIos\": true,"
                              + extraOptions
                              + "\"include_player_ids\": [\"" + playerId + "\"]"
                            + "}";

	    var headers = new Dictionary<string, string>();
	    headers.Add("Content-Type", "application/json");
	
	    print("Sending JSON:" + jsonString);
	
	    byte[] pData = System.Text.Encoding.ASCII.GetBytes(jsonString.ToCharArray());

	    WWW request = new WWW("https://gamethrive.com/api/v1/notifications", pData, headers);

	    yield return request;

	    if (!System.String.IsNullOrEmpty(request.error))
		    print(request.error);
	    
	    print(request.text);
    }

    private IEnumerator GetDevices() {
        print("GetDevices");
        WWW request = new WWW("http://gamethrive.com:3100/devices");
        yield return request;

        if (!System.String.IsNullOrEmpty(request.error))
            print(request.error);
        else if (request.text != null && request.text != "[]") {
            print(request.text);
            players = new List<GameThrivePlayer>();
            var playerList = Json.Deserialize(request.text) as List<object>;
            print(playerList);
            foreach(Dictionary<string, object> player in playerList)
                players.Add(new GameThrivePlayer() { name = (string)player["device_model"], playerId = (string)player["id"] });
        }
        print(request.text);
    }

    // Scroll Unity's ScrollView by draging your finger anywhere on the screen.
    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Moved)
                scrollPosition.y += touch.deltaPosition.y;
        }
    }
}
