﻿/**
 * Modified MIT License
 * 
 * Copyright 2014 GameThrive
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by GameThrive.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameThrivePush.MiniJSON;

public class GameThrive : MonoBehaviour {

	// NotificationReceived - Delegate is called when a push notification is opend or one is received when the user is in your game.
	// message        = The message text the use seen in the push notification.
	// additionalData = Dictionary of key value pairs sent with the push notification.
	// isActive       = True when the user is currentlying in your game when a notification was received.
	public delegate void NotificationReceived(string message, Dictionary<string, object> additionalData, bool isActive);
	
	public delegate void IdsAvailable(string playerID, string pushToken);
	public delegate void TagsReceived(Dictionary<string, object> tags);

#if !UNITY_EDITOR
	private static GameThrivePlatform gameThrivePlatform = null;
	private static bool initialized = false;

	internal static NotificationReceived notificationDelegate = null;
	internal static IdsAvailable idsAvailableDelegate = null;
	internal static TagsReceived tagsReceivedDelegate = null;

	// Name of the GameObject to put into your game scene.
	private const string gameObjectName = "GameThrive";
#endif

    // Init - Only required method you call to setup GameThrive to recieve push notifications.
	//        Call this on the first scene that is loaded.
	// appId                  = Your GameThrive app id from gamethrive.com
	// googleProjectNumber    = Your google project number that is only required for Android GCM pushes.
	// inNotificationDelegate = Calls this delegate when a notification is opened or one is received when the user is in your game.
	// autoRegister           = Set false to delay the iOS accept notification system prompt. Defaults true.
	//                          You can then call RegisterForPushNotifications at a better point in your game to prompt them.
	public static void Init(string appId, string googleProjectNumber, NotificationReceived inNotificationDelegate, bool autoRegister) {
		#if !UNITY_EDITOR
			if (initialized) return;
			#if UNITY_ANDROID
				gameThrivePlatform = new GameThriveAndroid(gameObjectName, googleProjectNumber, appId);
			#elif UNITY_IPHONE
				gameThrivePlatform = new GameThriveIOS(gameObjectName, appId, autoRegister);
            #elif UNITY_WP8
                gameThrivePlatform = new GameThriveWP(appId);
			#endif
			notificationDelegate = inNotificationDelegate;
			GameObject go = new GameObject(gameObjectName);
			go.AddComponent<GameThrive>();
			DontDestroyOnLoad(go);
			initialized = true;
		#else
			print("Please run GameThrive on a device to see push notifications.");
		#endif
	}

	// Parameter defaulting split out into different methods so they are compatible with UnityScript (AKA Unity Javascript).
	public static void Init(string appId, string googleProjectNumber, NotificationReceived inNotificationDelegate) {
		Init(appId, googleProjectNumber, inNotificationDelegate, true);
	}
	public static void Init(string appId, string googleProjectNumber) {
		Init(appId, googleProjectNumber, null, true);
	}
	public static void Init(string appId) {
		Init(appId, null, null, true);
	}



	// Tag player with a key value pair to later create segments on them at the gamethrive site.
	public static void SendTag(string tagName, string tagValue) {
		#if !UNITY_EDITOR
			gameThrivePlatform.SendTag(tagName, tagValue);
		#endif
	}

	// Makes a request to gamethrive.com to get current tags set on the player and then run the callback passed in.
	public static void GetTags(TagsReceived inTagsReceivedDelegate) {
		#if !UNITY_EDITOR
			tagsReceivedDelegate = inTagsReceivedDelegate;
			gameThrivePlatform.GetTags();
		#endif
	}

	public static void DeleteTag(string key) {
		#if !UNITY_EDITOR
			gameThrivePlatform.DeleteTag(key);
		#endif
	}

	// Call when the player has made an IAP purchase in your game so you can later send push notifications based on free or paid users.
	public static void SendPurchase(double amount) {
		#if !UNITY_EDITOR
			gameThrivePlatform.SendPurchase(amount);
		#endif
	}

	// Call this when you would like to prompt an iOS user accept push notifications with the default system prompt.
	// Only use if you passed false to autoRegister when calling Init.
	public static void RegisterForPushNotifications() {
		#if !UNITY_EDITOR
			gameThrivePlatform.RegisterForPushNotifications();
		#endif
	}

	// Call this if you need the playerId and/or pushToken
	// NOTE: pushToken maybe null if notifications are not accepted or there is connectly issues. 
	public static void GetIdsAvailable(IdsAvailable inIdsAvailableDelegate) {
		#if !UNITY_EDITOR
			idsAvailableDelegate = inIdsAvailableDelegate;
			gameThrivePlatform.IdsAvailable();
		#endif
	}


	/*** protected and private methods ****/
	#if !UNITY_EDITOR
		// Called from the navtive SDK - Called when a push notificaiton is open or app is running when one comes in.
		private void onPushNotificationReceived(string jsonString) {
			if (notificationDelegate != null)
				gameThrivePlatform.FireNotificationReceivedEvent(jsonString, notificationDelegate);
		}
		
		// Called from the navtive SDK - Called when device is registered with gamethrive.com service or right after GetIdsAvailable
		// 								 if already registered.
		private void onIdsAvailable(string jsonString) {
			if (idsAvailableDelegate != null) {
				var ids = Json.Deserialize(jsonString) as Dictionary<string, object>;
				idsAvailableDelegate((string)ids["playerId"], (string)ids["pushToken"]);
			}
		}

		// Called from the navtive SDK - Called After calling GetTags(...)
		private void onTagsReceived(string jsonString) {
			tagsReceivedDelegate(Json.Deserialize(jsonString) as Dictionary<string, object>);
		}

		// Called automatically by Unity
		void OnApplicationPause(bool paused) {
			gameThrivePlatform.OnApplicationPause(paused);
		}
	#endif
}