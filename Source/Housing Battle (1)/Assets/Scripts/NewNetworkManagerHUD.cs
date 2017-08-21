using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_UNET

namespace UnityEngine.Networking
{	
	[AddComponentMenu("Network/NewNetworkManagerHUD")]
	[RequireComponent(typeof(NetworkManager))]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class NewNetworkManagerHUD : MonoBehaviour
	{
		public bool isHosting = false;

        public Text Debugtext;  // using player controller for debug
		public NetworkManager manager;
		public GameController gameController;
		public MatchController matchController;

		[SerializeField] public bool showGUI = true;
		[SerializeField] public int offsetX;
		[SerializeField] public int offsetY;

		// Runtime variable
		bool showServer = false;

		void Awake()
		{
			manager = GetComponent<NetworkManager>();
		}

		void OnGUI()
		{
			if (!showGUI)
				return;

			int xpos = 10 + offsetX;
			int ypos = 40 + offsetY;
			int spacing = 24;



			if (GUI.Button(new Rect(0, 218, 86, 30), "Quit")){
				// if game is ongoing
				if ((matchController.IsReady) && !gameController.isGameOver()){
					if (isHosting) {
						Debug.Log ("Host rage quit");
						matchController.GameOver (2);
					} else {
						Debug.Log ("Client rage quit");
						matchController.GameOver (1);
					}
				}
				manager.StopHost ();
				manager.StopMatchMaker ();
				SceneManager.LoadScene(0);
			}
	
			/*
			if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
			{
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host(H)"))
				{
					manager.StartHost();
				}
				ypos += spacing;

				if (GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client(C)"))
				{
					manager.StartClient();
				}
				manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), manager.networkAddress);
				ypos += spacing;

				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server Only(S)"))
				{
					manager.StartServer();
				}
				ypos += spacing;
			}
			else
			{
				if (NetworkServer.active)
				{
					GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + manager.networkPort);
					ypos += spacing;
				}
				if (NetworkClient.active)
				{
					GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
					ypos += spacing;
				}
			}


			if (NetworkClient.active && !ClientScene.ready)
			{
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready"))
				{
					ClientScene.Ready(manager.client.connection);
				
					if (ClientScene.localPlayers.Count == 0)
					{
						ClientScene.AddPlayer(0);
					}
				}
				ypos += spacing;
			}

			if (NetworkServer.active || NetworkClient.active)
			{
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)"))
				{
					manager.StopHost();
				}
				ypos += spacing;
			}

			*/


			if (!NetworkServer.active && !NetworkClient.active) {
				ypos += 10;

				if (manager.matchMaker == null) {
					if (GUI.Button (new Rect (xpos, ypos+50, 200, 20), "Enable Matchmaking")) {
						manager.StartMatchMaker ();
					}
					ypos += spacing;

				} else {

					if (!matchController.IsReady) {
						if (GUI.Button (new Rect (xpos, 220, 200, 20), "Cancel")) {
							manager.StopHost ();
							manager.StopMatchMaker ();
							isHosting = false;
						}
					}

					if (manager.matchInfo == null) {
						if (manager.matches == null) {
							if (GUI.Button (new Rect (xpos+50, ypos+50, 240, 50), "Host Match")) {
								manager.matchMaker.CreateMatch (manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
								isHosting = true;
							}
							ypos += spacing;
							/*
							GUI.Label (new Rect (xpos, ypos, 100, 20), "Room Name:");
							manager.matchName = GUI.TextField (new Rect (xpos + 100, ypos, 100, 20), manager.matchName);
							ypos += spacing;

							ypos += 10;
							*/

							if (GUI.Button (new Rect (xpos+50, ypos+100, 240, 50), "Find Match")) {
								manager.matchMaker.ListMatches (0, 10, "", false, 0, 0, manager.OnMatchList);
                                Debugtext.text="Match Found";
							}
							ypos += spacing;

						} else {
							if (manager.matches [0] != null) {
								if (GUI.Button (new Rect (xpos+50, ypos+100, 240, 50), "Join Match")) {
									manager.matchName = manager.matches [0].name;
									manager.matchSize = (uint)manager.matches [0].currentSize;
                                    Debugtext.text = "Joining Match";
									manager.matchMaker.JoinMatch (manager.matches [0].networkId, "", "", "", 0, 0, manager.OnMatchJoined);
                                } /* else
                                {
                                    Debugtext.text = "Failed to join Match";
                                }*/
                            }
                            else
                            {
                                if (GUI.Button(new Rect(xpos + 50, ypos + 100, 240, 50), "No match found"))
                                {
                                    manager.StopHost();
                                    manager.StopMatchMaker();
                                }
                            }

							/*
							 * foreach (var match in manager.matches) {
								if (GUI.Button (new Rect (xpos, ypos, 200, 20), "Join Match")) {
									manager.matchName = match.name;
									manager.matchSize = (uint)match.currentSize;
									manager.matchMaker.JoinMatch (match.networkId, "", "", "", 0, 0, manager.OnMatchJoined);
								}
								ypos += spacing;
							}*/
						}
					}

					/*
					if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Change MM server"))
					{
						showServer = !showServer;
					}

					if (showServer)
					{
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Local"))
						{
							manager.SetMatchHost("localhost", 1337, false);
							showServer = false;
						}
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Internet"))
						{
							manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
							showServer = false;
						}
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Staging"))
						{
							manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
							showServer = false;
						}
					}

					ypos += spacing;

					GUI.Label(new Rect(xpos, ypos, 300, 20), "MM Uri: " + manager.matchMaker.baseUri);
					ypos += spacing;

					*/

				}
			} else {
				if (!matchController.IsReady) {
					if (GUI.Button (new Rect (xpos, 220, 200, 20), "Cancel")) {
						manager.StopHost ();
						manager.StopMatchMaker ();
						isHosting = false;
					}
				}
			}
		}

		public void StopMatch(){
			manager.StopHost ();
			manager.StopMatchMaker ();
			isHosting = false;
		}
	}
};
#endif //ENABLE_UNET
