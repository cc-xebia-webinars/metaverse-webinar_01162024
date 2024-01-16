using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Models;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json;
using Serilog;
using UnityEngine;
using Web3Unity.Scripts.Library.ETHEREUEM.Connect;
using Web3Unity.Scripts.Library.Ethers.Network;
using Web3Unity.Scripts.Library.IPFS;
using Web3Unity.Scripts.Library.Web3Wallet;
using static MyControls;
using static UnityEngine.InputSystem.InputAction;
using static Web3Unity.Scripts.Library.Ethers.Network.Chains;

public class BuildingDoorTrigger : MonoBehaviour, IPlayerActions
{
    // Chainsafe Storage API Key
    private const string apiKey = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE3MDUxNzA1MjEsImNuZiI6eyJqa3UiOiIvY2VydHMiLCJraWQiOiI5aHE4bnlVUWdMb29ER2l6VnI5SEJtOFIxVEwxS0JKSFlNRUtTRXh4eGtLcCJ9LCJ0eXBlIjoiYXBpX3NlY3JldCIsImlkIjoxNDQ5MiwidXVpZCI6IjkwOWJlNTYzLWU4NzktNGI3Ni1hYzkxLTJjMTUzYjhjODBkNyIsInBlcm0iOnsiYmlsbGluZyI6IioiLCJzZWFyY2giOiIqIiwic3RvcmFnZSI6IioiLCJ1c2VyIjoiKiJ9LCJhcGlfa2V5IjoiRFdRWUpBSUJFVFZPTVBEVVRSWEQiLCJzZXJ2aWNlIjoic3RvcmFnZSIsInByb3ZpZGVyIjoiIn0.973LWW-1w6yIhwu4f__RDuWfWUY9U_BDlfsGakHP-4VQnuATEH7fkJAu_6jjR3hY6LCw-vAPnq1EvcIjPCqYEQ";

    //Firestore SDK Instance
    FirebaseFirestore db;
    //Firebase Auth SDK Instance
    FirebaseAuth auth;

    //In these constants we define the different messages we want to display to the user.
    const string BUSY_BUILDING_TEXT = "Sorry\nThis building is occupied and cannot be acquired";
    const string ACQUIRE_BUILDING_TEXT = "Do you want to acquire this building?\nPress 'C' key to acquire it";
    const string CONFIRMATION_TEXT = "Congratulations, you are now the owner of this building!";
    const string YOU_ARE_THE_OWNER_TEXT = "You are the owner of this building ;)";

    //In this variable we will store the current message we want to display.
    string displayMessage = string.Empty;

    //To prevent the window from appearing if we press the E key without being in the area of influence, we will use this flag
    private bool canInteract = false;

    //Here we will store information about the building when we consult it in the database.
    private Building building;

    MyControls controls;

    private void Awake()
    {
        //We check that we have instantiated the variable "db" that allows access to the Firestore functions, otherwise we instantiate it
        if (db == null) db = FirebaseFirestore.DefaultInstance;

        if (auth == null) auth = FirebaseAuth.DefaultInstance;

        //We link the Interact action and enable it for detection in the code.
        if (controls == null)
        {
            controls = new MyControls();
            // Tell the "gameplay" action map that we want to get told about
            // when actions get triggered.
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();

    }

    //If the Script is in a GameObject that has a Colllider with the Is Trigger property enabled, it will call this function when another GameObject comes into contact.
    private void OnTriggerEnter(Collider other)
    {
        //The player's Prefab comes with a default tag called Player, this is an excellent way to identify that it is a player and not another object.
        if (other.gameObject.tag == "Player")
        {
            //This function will perform the database query
            GetBuildingInfo();
        }
    }

    //When the player leaves the area of influence, we will put a blank text back in. 
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            displayMessage = string.Empty;
            canInteract = false;
        }
    }


    void Update()
    {
        //We check if the user has pressed the C key and is also inside the collider.
        if (Input.GetKeyDown(KeyCode.C) && canInteract)
        {
            //This function will update the building in the database with our ID
            UpdateBuildingOwner();
        }
    }

    void OnGUI()
    {
        //We display a text on the screen
        GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), displayMessage);
    }

    void GetBuildingInfo()
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        //We get building data from the BuildingInstanceManager component which is located in the parent of this object.
        var buildingInfo = GetComponentInParent<BuildingInstanceManager>();

        //Create an instance to the Firestore Buildings Collection
        DocumentReference docRef = db.Collection("Buildings").Document(buildingInfo.buildingId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Obtaining building was canceled.");
                return;

            }
            if (task.IsFaulted)
            {
                Debug.LogError("Obtaining building encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("Building obtanined succesfully");


            DocumentSnapshot buildingRef = task.Result;

            building = buildingRef.ConvertTo<Building>();

            //No current owner
            if (string.IsNullOrEmpty(building.OwnerUserId))
            {
                displayMessage = ACQUIRE_BUILDING_TEXT;
                canInteract = true;
            }
            //I am the owner
            else if (auth.CurrentUser.UserId == building.OwnerUserId)
            {
                displayMessage = YOU_ARE_THE_OWNER_TEXT;
                canInteract = false;
            }
            //Another user is the owner
            else
            {
                displayMessage = BUSY_BUILDING_TEXT;
                canInteract = false;
            }

        });
    }


    public void UpdateBuildingOwner()
    {

        //If we have forgotten to assign the building, we cannot continue.
        if (building == null)
        {
            Debug.LogError("Building object is null, cannot continue");
            return;
        }


        //We create a reference to the "Buildings" collection with a new document that will have as ID the one we have generated for the building.
        DocumentReference docRef = db.Collection("Buildings").Document(building.Id);

        //We call the UpdateAsync method to write the changes to the database, we use the SetOptions.MergeAll
        //option not to replace all the properties in the database, but the ones that have been modified.

        Dictionary<string, object> updatedProperties = new Dictionary<string, object>();
        updatedProperties["OwnerUserId"] = auth?.CurrentUser.UserId;


        docRef.UpdateAsync(updatedProperties).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Update building encountered an error");
                return;
            }

            Debug.Log("Building created or updated succesfully");

            displayMessage = CONFIRMATION_TEXT;
            canInteract = false;

            LoginNft();

        });

    }

    public void OnMove(CallbackContext context)
    {

    }

    public void OnLook(CallbackContext context)
    {

    }

    public void OnJump(CallbackContext context)
    {

    }

    public void OnSprint(CallbackContext context)
    {

    }

    public void OnInteract(CallbackContext context)
    {
        if (context.action.triggered && canInteract)
        {
            //This function will update the building in the database with our ID
            UpdateBuildingOwner();
        }
    }

    public async void LoginNft()
    {
        Web3Wallet.url = "https://chainsafe.github.io/game-web3wallet/";
        // get current timestamp
        var timestamp = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // set expiration time
        var expirationTime = timestamp + 60;
        // set message
        var message = expirationTime.ToString();
        // sign message
        var signature = await Web3Wallet.Sign(message);
        // verify account
        var account = SignVerifySignature(signature, message);
        var now = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now)
        {
            PlayerPrefs.SetString("Account", account);
            
            await MintNft();
        }
    }

    public string SignVerifySignature(string signatureString, string originalMessage)
    {
        var msg = "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;
        var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
        var signature = MessageSigner.ExtractEcdsaSignature(signatureString);
        var key = EthECKey.RecoverFromSignature(signature, msgHash);
        return key.GetPublicAddress();
    }



    private async Task<string> UploadIPFS()
    {
        var capture = ScreenCapture.CaptureScreenshotAsTexture();

        byte[] bArray = capture.GetRawTextureData();

        var ipfs = new IPFS(apiKey);

        var bucketId = "f5fae0fa-d4bd-4807-a21b-b0f3edaec8eb";

        var folderName = "/MyFolder";

        var fileName = "MyCapture.jpg";

        var cid = await ipfs.Upload(bucketId, folderName, fileName, bArray, "application/octet-stream");

        return $"{cid}";
    }

    public async Task MintNft()
    {
        var account = PlayerPrefs.GetString("Account");
        // set chain: ethereum, moonbeam, polygon etc
        string chain = "ethereum";
        // chain id
        string chainId = "5";
        // set network mainnet, testnet
        string network = "goerli";
        // type
        string type = "721";

        // var nft = await UploadIPFS();

        var response = await EVM.CreateApproveTransaction(chain, network, account, type);
        Debug.Log("Response: " + response.connection.chain);

        string responseNft = await Web3Wallet.SendTransaction(chainId, response.tx.to, "0",
            response.tx.data, response.tx.gasLimit, response.tx.gasPrice);
        if (responseNft == null)
        {
            Debug.Log("Empty Response Object:");
        }

        print("My NFT Address: " + responseNft);

    //     var voucherResponse721 = await EVM.Get721Voucher();
    //     var voucher721 = new CreateRedeemVoucherModel.CreateVoucher721() {
    //         tokenId = voucherResponse721.tokenId,
    //         signer = voucherResponse721.signer,
    //         receiver = voucherResponse721.receiver,
    //         signature = voucherResponse721.signature
    //     };

    //     var chain = "ethereum";
    //     var chainId = "5";
    //     var network = "goerli";
    //     var type = "721";
    //     var voucherArgs = JsonUtility.ToJson(voucher721);

    //     var nft = await UploadIPFS();

    //     var voucherResponse = await EVM.CreateRedeemTransaction(chain, network, voucherArgs, type, nft, voucher721.receiver);

    //     string response = await Web3Wallet.SendTransaction(
    //       chainId,
    //       voucherResponse.tx.to,
    //       voucherResponse.tx.value.ToString(),
    //       voucherResponse.tx.data,
    //       voucherResponse.tx.gasLimit,
    //       voucherResponse.tx.gasPrice);

    //     print("My NFT Address: " + response);
    }
}