## Photon Methods

**public class Blank : Photon.PunBehaviour**

instead of mono behavior, use this to receive photon callbacks in your script.

**public override void OnLeftRoom()**

An example of overriding a punbehavior callback

**if (!photonView.isMine)**

Since all scripts attached to any game object, whether yours or theirs, runs locally. You need to check if the instance of the script is yours if you want to perform an actions. Example: when hit with laser beam, on trigger enter, check if you are the one who was hit, and if so take damage. All other players will as well do this processing and take their own damage. 

```csharp

     void OnTriggerEnter(Collider other)
            {
            
                // we dont' do anything if we are not the local player.
                if (!photonView.isMine)
                {
                    return;
                }
          }

```

**if (PhotonNetwork.isMasterClient)**

One client will be the master (automatically negotiated). The rest of the clients are slaves. If you wanted to detect some event and perform some action, it should be encapsulated in isMasterClient so that only the master performs the action, not every client connected.

Example: Load next level after match has been won. Every client will receive some event that the match has been won, but only the master will load the next level.

```csharp
       if (PhotonNetwork.isMasterClient)
       {
           LoadArena();
       }
```            

**if (photonView.isMine == false && PhotonNetwork.connected == true)**

Restricts input to your local instance of a player game object. Put this as an early return in Update()

```csharp
      if (photonView.isMine == false && PhotonNetwork.connected == true)
      {
          return;
      }
```

**PhotonNetwork.Instantiate**

This is how you load an object in photon.

```csharp
    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
```


## Photon GameObject Scripts

**PhotonView**

A [PhotonView](https://doc.photonengine.com/en-us/pun/current/getting-started/feature-overview#_viewpun) is what connects together the various instances on each computers, and define what components to observe and how to observe these components.

**Photon Transform View**

Used in combination with PhotonView to sync transform properties of an objects across clients (player avatar would want this attached)

*Needs to be dragged into PhotonView as an Observed Component*

https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-networking

**PhotonAnimatorView**

Sync animations on an object, for example player avatar. You need to opted in every animation listed in "Synchronize Parameters" and set them to Discrete by default.

*Needs to be dragged into PhotonView as an Observed Component*

> `Discrete synchronization`  means that a value gets sent 10 times a
> second (in  `OnPhotonSerializeView`). The receiving clients pass the
> value on to their local Animator.
> 
> `Continuous`  synchronization means that the  `PhotonAnimatorView` 
> runs every frames. When  `OnPhotonSerializeView`  is called (10 times
> per second), the values recorded since the last call are sent
> together. The receiving client then applies the values in sequence to
> retain smooth transitions. While this mode is smoother, it also sends
> more data to achieve this effect.

## Remote Procedure Calls

Allows a player to call a method on all or some other players.

```csharp

public class MyCustomRPC : Monobevaiour
{
     public void CallRemoteMethod (){
     
          if (PhotonNetwork.offlineMode == true){ //use this you need to support offline mode.
               MyRemoteMethod(PhotonTargets.Others, new object [] { 42, true });
               return;
          }
          GetComponent<PhotonView>().RPC("MyRemoteMethod", PhotonTargets.Others, new object [] { 42, true }) 
               
               //Target Types
               //PhotonTargets.Others
               //PhotonTargets.All //triggered instantly
               //PhotonTargets.AllViaServer //local client gets even through server just like everyone else
               //PhotonTargets.MasterClient
               //PhotonNetwork.playerList[0]
               //PhotonTargets.AllBuffered
               //PhotonTargets.AllBufferedViaServer //used in item pickups where could be contested which client got it first
                                                    //An important use is also when a new player connects later, they will recieve this 
                                                    //buffered event that the item has been picked up and should be removed from scene
     }
     
     [PunRPC]
     void MyRemoteMethod(int someNumber, bool someBool) {
          Debug.Log(someNumber);
          Debug.Log(someBool);
     }
}


```

## Scene Objects

Objects that need to be synchronized across clients but aren't owned by anyone. Technically they are owned by the master client.

The are created two ways:

1) Objects placed into the scene by editor and have a PhotonView
2) By script using PhotonNetwork.InstantiateSceneObject 


```csharp
     
     void PickupObject ( Health health ){
          PhotonView.RPC("OnPickup", PhotonTargets.AllBufferedViaServer, new object [] { health.PhotonView.viewId }
     }
     
     [RPC]
     protected void OnPickup( int viewId ){
          PhotonView view = PhotonView.Find( viewId);
          
     }

```


## PhotonNetwork.time

Used to syncronize the timing of things like projectiles across all clients. 

```csharp

     void Update (){
          float timePassed = (float)( PhotonNetwork.time - CreationTime ); //CreationTime also was set using PhotonNetwork.Time
          transform.position = StartPosition + transform.forward * Speed * timePassed;
          
          if (timePassed > LifeTime){
               Destroy (gameObject);
          }
          
          if (transform.position.y < 0f){
               Destroy(gameObject);
               CreateHitFx();
          }
     }
     
     void OnCollisionEnter ( Collision collision) {
          if (collsion.collider.tag == "ObjectWithStaticPositionRotationSize"){
               OnProjectileHit();
          } 
          else if ( collsion.collider.tag == "MovingObjectThatMayNotBeInPerfectSyncAcrossClients){
               //Lets the client who owns the thing that was hit make the call
               
               Ship ship = collsion.collider.GetComponent<Ship>();
               
               if (ship.Team != ProjectileOwner.Team && ship.PhotonView.isMine){
                    ship.ShipCollision.OnProjectileHit(this);
                    OnProjectileHit();  //RPC to let everyone know what happened
                    ProjectileOwner.ShipShooting.SendPorjectileHit( ProjectileId);
               }
          }
     }

```


## IPunObservable (streams and manual synchronization)

This lets you sync data in your hand rolled scripts. For example knowing when an enemy has fired and rendering their laser. ***You need to add your script in the Observed Components list in Photon View***

Add to class:

```csharp
      public class PlayerManager : Photon.PunBehaviour, IPunObservable{}
```

Use in script:

```csharp
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                //This script is local, you write to stream
                stream.SendNext(IsFiring);
                stream.SendNext(17.8f); //the scructure of send and receive will match so you can send multiple things 
                                        //and receive them in that order
            }
            else
            {
                // Network player, receive data
                //This script is receiving data from remote players script
                this.IsFiring = (bool)stream.ReceiveNext();
                this.SomethingElse = (float)stream.ReceiveNext();
                
                LastNetworkDataReceivedTime = info.timestamp; //the stream is outdated due to lag so we need to know the time past since
                                                              //it was sent. This is used to modify time based things on local instances
                                                              //This is used for prediction models like position
                                                              //This should also be used in combination with PhotonNetwork.GetPing();
            }
        }
```        

This is confusing but remember to think of this as an instance not a singleton. If there are 2 players in the game, one is you and one is the remote player.

Your instance of this script is always going to write. Example: you're firing or your health decreased, the instance of the script on YOUR avatar will write this data.

If a remote player loses health, the instance of this script on THEIR avatar will receive data that was written by them on their client.

The If/else conditions arent both hit in the same instance... one will always be hit in your instance and the other will always be hit in their instance.

## Prefabs

Always make sure Prefabs that are supposed to be instantiated over the network are within a [Resources](http://docs.unity3d.com/ScriptReference/Resources.html) folder, this is a Photon Requirement.

**Preserve instance of network game object**

PlayerManger.cs

 ```csharp

     // #Important
    // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
    if (photonView.isMine)
    {
        PlayerManager.LocalPlayerInstance = this.gameObject;
    }
    // #Critical
    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
    DontDestroyOnLoad(this.gameObject);
```

GameManager.cs

 ```csharp
if (PlayerManager.LocalPlayerInstance==null)
{
    Debug.Log("We are Instantiating LocalPlayer from "+Application.loadedLevelName);
    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f,5f,0f), Quaternion.identity, 0);
}
else
{
    Debug.Log("Ignoring scene load for "+Application.loadedLevelName);
}
```

## Lobby Management

```csharp

PhotonNetwork.JoinRoom("Room Name");
PhotonNetwork.JoinRandomRoom();
PhotonNetwork.room.PlayerCount
PhotonNetwork.LoadLevel("RoomName");
PhotonNetwork.CreateRoom(RoomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
PhotonNetwork.ConnectUsingSettings(_gameVersion);
PhotonNetwork.JoinRandomRoom();
PhotonNetwork.connected;

// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
PhotonNetwork.automaticallySyncScene = true;

// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
PhotonNetwork.autoJoinLobby = false;

PhotonNetwork.logLevel = Loglevel;



```

```csharp

using UnityEngine;


namespace Com.ResultsGrid.Simulator
{
    public class Launcher : Photon.PunBehaviour
    {
        #region Public Variables
        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte MaxPlayersPerRoom = 4;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        public GameObject progressLabel;
        #endregion

        public string RoomName = "ResultsGrid";


        #region Private Variables


        /// <summary>
        /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
        /// </summary>
        string _gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;


        #endregion


        #region MonoBehaviour CallBacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #NotImportant
            // Force LogLevel
            PhotonNetwork.logLevel = Loglevel;

            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            PhotonNetwork.autoJoinLobby = false;


            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            //Connect(); //Enable for auto connect

            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }


        #endregion

        #region Photon.PunBehaviour CallBacks


        public override void OnConnectedToMaster()
        {
            // we don't want to do anything if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
                //PhotonNetwork.JoinRandomRoom();
                PhotonNetwork.JoinRoom(RoomName);
            }
        }


        public override void OnDisconnectedFromPhoton()
        {
            Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(RoomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
            if (PhotonNetwork.room.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");


                // #Critical
                // Load the Room Level.
                //PhotonNetwork.LoadLevel("Room for 1");
                PhotonNetwork.LoadLevel("NioFactoryNetworked");
            }
        }

        #endregion

    }
}

```