using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Nethereum.Web3;
using Nethereum.Util;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.Encoders;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;
[RequireComponent(typeof(Web3Auth))]
public class HallidayClient : MonoBehaviour
{
    Web3Auth web3Auth;
    private bool _isInitialized = false;
    private BlockchainType blockchainType;
    private string authHeaderValue;
    private string apiEndpoint;
    private string privateKey;
    private UserInfo userInfo;
    private Account account;

    // [SerializeField]
    // private string clientId;
    void Awake()
    {
        web3Auth = GetComponent<Web3Auth>();
    }

    /**
     * Initializes Halliday Client
     *
     * @param {string} apiKey API key provided to you by the Halliday team
     * @param {BlockchainType} blockchainType blockchain of the Halliday AA wallet
     * @param {boolean} sandbox optional argument to use sandbox API
     */
    public void Initialize(string apiKey, BlockchainType blockchainType = BlockchainType.MUMBAI, bool sandbox = true)
    {
        string[] parts = apiKey.Split(':');
        if (parts.Length != 2)
        {
            throw new Exception("Invalid Api Key");
        }
        this.authHeaderValue = "Bearer " + parts[1];
        string web3AuthClientId = parts[0];

        web3Auth.setOptions(new Web3AuthOptions()
        {
            clientId = web3AuthClientId,
            network = Web3Auth.Network.CYAN,
        });
        web3Auth.onLogin += onLogin;
        web3Auth.onLogout += onLogout;
        this.blockchainType = blockchainType;

        if (sandbox == true)
        {
            this.apiEndpoint = "https://sandbox.halliday.xyz/v1/";
        }
        else
        {
            this.apiEndpoint = "https://api.halliday.xyz/v1/";
        }
        _isInitialized = true;
    }

    /**
     * Gives the state of initialization - only true after all steps of initialization are complete
     *
     */
    public bool isInitialized()
    {
        return _isInitialized;
    }

    /**
     * Sends user to Google to login. Will then redirect to OpenLogin to
     * generate the wallet key, then redirect back to your login page.
     * If the process was successful, you'll be able to use getSigner()
     * to get their signer.
     */
    public void logInWithGoogle()
    {
        this._logInWithProvider(Provider.GOOGLE);
    }

    /**
     * Sends user to Facebook to login. Will then redirect to OpenLogin to
     * generate the wallet key, then redirect back to your login page.
     * If the process was successful, you'll be able to use getSigner()
     * to get their signer.
     */
    public void logInWithFacebook()
    {
        this._logInWithProvider(Provider.FACEBOOK);
    }

    /**
     * Sends user to Twitter to login. Will then redirect to OpenLogin to
     * generate the wallet key, then redirect back to your login page.
     * If the process was successful, you'll be able to use getSigner()
     * to get their signer.
     */
    public void logInWithTwitter()
    {
        this._logInWithProvider(Provider.TWITTER);
    }

    /**
     * Logs in with email OTP. After the user clicks on the link in
     * their email, the page will redirect back to your login page.
     * If the process was successful, you'll be able to use getSigner()
     * to get their signer.
     *
     * @param {string} email The email to log in with
     */
    public void logInWithEmailOTP(string email)
    {
        this._logInWithProvider(Provider.EMAIL_PASSWORDLESS, email);
    }

    /**
     * Logs the user out
     */
    public void logOut()
    {
        this.userInfo = null;
        this.privateKey = null;
        this.account = null;
        web3Auth.logout();
    }


    private void _logInWithProvider(Provider loginProvider, string emailAddressField = "")
    {
        var selectedProvider = loginProvider;

        var options = new LoginParams()
        {
            loginProvider = selectedProvider
        };

        if (selectedProvider == Provider.EMAIL_PASSWORDLESS)
        {
            options.extraLoginOptions = new ExtraLoginOptions()
            {
                login_hint = emailAddressField
            };
        }
        this.web3Auth.login(options);
    }

    public void onLogin(Web3AuthResponse response)
    {
        this.privateKey = response.privKey;
        this.account = new Account(privateKey);
        this.userInfo = response.userInfo;
    }

    public void onLogout()
    {
        this.privateKey = null;
        this.userInfo = null;
        this.account = null;
    }

    /**
     * Returns the user's info (email, name, signer) if they are logged in.
     *
     * @returns {Promise<GetUserInfoResponse | null>} The user's info: email, name,
     * and ethers.JsonRpcSigner. Returns null if the user is not logged in.
     */
    public GetUserInfoResponse getUserInfo()
    {
        if (this.userInfo == null)
        {
            return null;
        }

        return new GetUserInfoResponse(this.userInfo.name, this.userInfo.email);
    }

    /**
    * After logging in using one of the above options, you can call getUserInfo
    * and use the returned info to create an account on your end. Then, call
    * this function, getOrCreateHallidayAAWallet, with the user's id in your
    * system. This function will get or create the Halliday AA wallet, that
    * the non-custodial wallet will be the owner of.
    *
    * Need to call this before any of the below methods to make sure the user
    * has a Halliday AA Wallet
    *
    * @param {string} inGamePlayerId
    * @returns {Task<GetWalletResponse>} Halliday AA wallet info
    */
    public async Task<Wallet> getOrCreateHallidayAAWallet(string inGamePlayerId)
    {
        WWWForm form;
        UnityWebRequest request;

        string getPlayerWalletsUrl = this.apiEndpoint + "client/accounts/" + inGamePlayerId + "/wallets";
        request = UnityWebRequest.Get(getPlayerWalletsUrl);
        request.SetRequestHeader("Authorization", this.authHeaderValue);

        await sendRequestAndWait(request);
        Wallet walletForThisBlockchain = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            var walletList = JsonConvert.DeserializeObject<WalletList>(request.downloadHandler.text);
            var wallets = walletList.wallets;

            walletForThisBlockchain = wallets.Find(wallet => wallet.blockchain_type == this.blockchainType);
        }
        else
        {
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(request.downloadHandler.text);
            if (errorResponse.code != HallidayErrorCode.USER_DOES_NOT_EXIST)
            {
                throw new Exception("User Fetch Had error: " + request.downloadHandler.text);
            }
        }

        if (walletForThisBlockchain == null)
        {
            string newAccountUrl = this.apiEndpoint + "client/accounts";
            form = new WWWForm();
            form.AddField("email", userInfo.email);
            form.AddField("in_game_player_id", inGamePlayerId);
            form.AddField("non_custodial_address", account.Address);
            form.AddField("blockchain_type", blockchainType.ToString());
            request = UnityWebRequest.Post(newAccountUrl, form);

            request.SetRequestHeader("Authorization", this.authHeaderValue);

            await sendRequestAndWait(request);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Create wallet request failed" + request.downloadHandler.text);
            }

            //Check wallet was created
            request = UnityWebRequest.Get(getPlayerWalletsUrl);

            request.SetRequestHeader("Authorization", this.authHeaderValue);
            await sendRequestAndWait(request);

            var walletList = JsonConvert.DeserializeObject<WalletList>(request.downloadHandler.text);
            var wallets = walletList.wallets;

            walletForThisBlockchain = wallets.Find(wallet => wallet.blockchain_type == this.blockchainType);
        }
        if (walletForThisBlockchain == null)
        {
            throw new Exception("Failed to create wallet for" + this.blockchainType.ToString());
        }

        return walletForThisBlockchain;
    }

    /**
     * Gets the player's NFTs.
     *
     * @param {string} inGamePlayerId
     * @returns {GetAssetsResponse>}
     */
    public async Task<GetAssetsResponse> getAssets(string inGamePlayerId)
    {
        UnityWebRequest request;

        string getPlayerWalletsUrl = this.apiEndpoint + "client/accounts/" + inGamePlayerId + "/assets";
        request = UnityWebRequest.Get(getPlayerWalletsUrl);
        request.SetRequestHeader("Authorization", this.authHeaderValue);

        await sendRequestAndWait(request);
        var getAssetsResponse = JsonConvert.DeserializeObject<GetAssetsResponse>(request.downloadHandler.text);
        return getAssetsResponse;
    }

    /**
     * Gets the player's ERC20 and Native Token Balances
     *
     * @param {string} inGamePlayerId
     * @returns {GetBalancesResponse}
     */
    public async Task<GetBalancesResponse> getBalances(string inGamePlayerId)
    {
        UnityWebRequest request;

        string getPlayerWalletsUrl = this.apiEndpoint + "client/accounts/" + inGamePlayerId + "/balances";
        request = UnityWebRequest.Get(getPlayerWalletsUrl);
        request.SetRequestHeader("Authorization", this.authHeaderValue);
        await sendRequestAndWait(request);
        var getAssetsResponse = JsonConvert.DeserializeObject<GetBalancesResponse>(request.downloadHandler.text);

        return getAssetsResponse;
    }

    /**
     * Poll this method to get the status of a transaction created using
     * transferAsset, transferBalance, or callContract.
     *
     * @param {string} txId The tx id returned by one of the above three
     * methods
     * @returns {GetTransactionResponse} Includes info about the
     * transaction, including status and on_chain_id
     */
    public async Task<GetTransactionResponse> getTransaction(string txId)
    {
        WWWForm form;
        UnityWebRequest request;

        string getPlayerWalletsUrl = this.apiEndpoint + "client/transactions/" + txId;
        form = new WWWForm();
        request = UnityWebRequest.Get(getPlayerWalletsUrl);
        request.SetRequestHeader("Authorization", this.authHeaderValue);
        await sendRequestAndWait(request);

        var getTransactionResponse = JsonConvert.DeserializeObject<GetTransactionResponse>(request.downloadHandler.text);
        return getTransactionResponse;
    }


    /**
     * Transfer an NFT to another player within your game.
     *
     * @param transfer asset params (TODO)
     * @returns {string} tx id of the transaction. Poll getTransaction with this
     * id to see when the transaction is completed and to get the on-chain
     * transaction hash.
     */
    public async Task<GetTransactionResponse> transferAsset(string from_in_game_player_id, string to_in_game_player_id, string collection_address, string token_id, BlockchainType blockchain_type, bool sponsor_gas)
    {
        WWWForm form = new WWWForm();
        form.AddField("from_in_game_player_id", from_in_game_player_id);
        form.AddField("to_in_game_player_id", to_in_game_player_id);

        form.AddField("collection_address", collection_address);
        form.AddField("token_id", token_id);
        form.AddField("blockchain_type", blockchain_type.ToString());
        form.AddField("sponsor_gas", sponsor_gas.ToString().ToLower());

        var txHash = await _transact(TransactionType.TRANSFER_ASSET, form, from_in_game_player_id);
        return txHash;
    }

    /**
    * Transfer an ERC20 to another player within your game.
    *
    * @param transfer balance params (TODO)
    * @returns {string} tx id of the transaction. Poll getTransaction with this
    * id to see when the transaction is completed and to get the on-chain
    * transaction hash.
    */
    public async Task<GetTransactionResponse> transferBalance(string from_in_game_player_id, string to_in_game_player_id, string token_address, string value, BlockchainType blockchain_type, bool sponsor_gas)
    {
        WWWForm form = new WWWForm();
        form.AddField("from_in_game_player_id", from_in_game_player_id);
        form.AddField("to_in_game_player_id", to_in_game_player_id);
        if (token_address != null)
        {
            form.AddField("token_address", token_address);
        }
        form.AddField("value", value);
        form.AddField("blockchain_type", blockchain_type.ToString());
        form.AddField("sponsor_gas", sponsor_gas.ToString().ToLower());

        var txHash = await _transact(TransactionType.TRANSFER_BALANCE, form, from_in_game_player_id);
        return txHash;
    }

    /**
     * Call an arbitrary contract.
     *
     * @param call contract params (TODO)
     * @returns {string} tx id of the transaction. Poll getTransaction with this
     * id to see when the transaction is completed and to get the on-chain
     * transaction hash.
     */
    public async Task<GetTransactionResponse> callContract(string from_in_game_player_id, string target_address, string value, string calldata, BlockchainType blockchain_type, bool sponsor_gas)
    {
        WWWForm form = new WWWForm();
        form.AddField("from_in_game_player_id", from_in_game_player_id);
        form.AddField("target_address", target_address);
        form.AddField("value", value);
        form.AddField("calldata", calldata);
        form.AddField("blockchain_type", blockchain_type.ToString());
        form.AddField("sponsor_gas", sponsor_gas.ToString().ToLower());

        var txHash = await _transact(TransactionType.CALL_CONTRACT, form, from_in_game_player_id);
        return txHash;
    }

    private async Task<GetTransactionResponse> _transact(TransactionType txType, WWWForm form, string from_in_game_player_id)
    {
        var buildUrl = this.apiEndpoint + "client/transactions/" + _getTransactionTypeString(txType);
        UnityWebRequest request = UnityWebRequest.Post(buildUrl, form);
        request.SetRequestHeader("Authorization", this.authHeaderValue);

        await sendRequestAndWait(request);
        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new Exception("Build Transaction Failed: " + request.downloadHandler.text);
        }
        var buildResponse = JsonConvert.DeserializeObject<BuildTransactionResponse>(request.downloadHandler.text);

        string submit_tx_id = await signSubmitWaitTransaction(buildResponse, from_in_game_player_id);


        GetTransactionResponse tx_info;
        string tx_status;
        int retry_count = 0;
        do
        {
            await Task.Delay(10000);
            tx_info = await this.getTransaction(submit_tx_id);

            tx_status = tx_info.status;

            var tx_retry_count = tx_info.retry_count;

            if (tx_retry_count > retry_count)
            {
                retry_count = tx_retry_count;
                string retryUrl = this.apiEndpoint + "client/transactions/retryUserOp";
                WWWForm retryForm = new WWWForm();
                retryForm.AddField("from_in_game_player_id", from_in_game_player_id);
                retryForm.AddField("blockchain_type", this.blockchainType.ToString());
                retryForm.AddField("tx_id", buildResponse.tx_id);
                request = UnityWebRequest.Post(retryUrl, retryForm);
                request.SetRequestHeader("Authorization", this.authHeaderValue);
                await sendRequestAndWait(request);
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception("Retry Endpoint Failed: " + request.downloadHandler.text);
                }
                var retryResponse = JsonConvert.DeserializeObject<BuildTransactionResponse>(request.downloadHandler.text);
                string retry_submit_tx_id = await signSubmitWaitTransaction(retryResponse, from_in_game_player_id);
            }
        } while (tx_status != "COMPLETE" && tx_status != "FAILED");

        return tx_info;
    }
    private async Task<string> signSubmitWaitTransaction(BuildTransactionResponse buildResponse, string from_in_game_player_id)
    {
        var tx_hash = buildResponse.tx_hash;
        var transaction = buildResponse.transaction;

        // Signature
        var signer = new EthereumMessageSigner();
        byte[] hashBytes = StringToByteArray(tx_hash);
        string signature = signer.Sign(hashBytes, new EthECKey(this.privateKey));
        transaction.signature = signature;

        var submitUrl = this.apiEndpoint + "client/transactions/";
        SubmitTransactionRequest submitTransactionRequest = new SubmitTransactionRequest();
        submitTransactionRequest.from_in_game_player_id = from_in_game_player_id;
        submitTransactionRequest.signed_tx = transaction;
        submitTransactionRequest.blockchain_type = this.blockchainType.ToString();
        submitTransactionRequest.tx_id = buildResponse.tx_id;

        var request = UnityWebRequest.Post(submitUrl, JsonConvert.SerializeObject(submitTransactionRequest), "");
        request.SetRequestHeader("Authorization", this.authHeaderValue);
        request.SetRequestHeader("Content-Type", "application/json");

        await sendRequestAndWait(request);
        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new Exception("Submit Transaction Failed: " + request.downloadHandler.text);
        }

        SubmitTransactionResponse submitResponse = JsonConvert.DeserializeObject<SubmitTransactionResponse>(request.downloadHandler.text);
        return submitResponse.tx_id;
    }
    private static string _getTransactionTypeString(TransactionType txType)
    {
        switch (txType)
        {
            case TransactionType.TRANSFER_ASSET:
                return "transferAsset";
            case TransactionType.TRANSFER_BALANCE:
                return "transferBalance";
            case TransactionType.CALL_CONTRACT:
                return "contract";
            default:
                throw new ArgumentException("Invalid Transaction Type");
        }
    }

    private static byte[] StringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[(numberChars - 2) / 2];
        for (int i = 2; i < numberChars; i += 2)
        {
            bytes[i / 2 - 1] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    private static async Task sendRequestAndWait(UnityWebRequest request)
    {
        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            await Task.Yield();
        }
    }

    private class BuildTransactionResponse
    {
        [JsonProperty(Required = Required.Always)]
        public string tx_id;
        [JsonProperty(Required = Required.Always)]
        public AATransaction transaction;
        [JsonProperty(Required = Required.Always)]
        public string tx_hash;
    }

    private class SubmitTransactionRequest
    {
        [JsonProperty(Required = Required.Always)]
        public string tx_id;
        [JsonProperty(Required = Required.Always)]
        public AATransaction signed_tx;
        [JsonProperty(Required = Required.Always)]
        public string from_in_game_player_id;
        [JsonProperty(Required = Required.Always)]
        public string blockchain_type;
    }
    private class AATransaction
    {
        [JsonProperty(Required = Required.Always)]
        public string sender;
        [JsonProperty(Required = Required.Always)]
        public BigNumber nonce;
        [JsonProperty(Required = Required.Always)]
        public string initCode;
        [JsonProperty(Required = Required.Always)]
        public string callData;
        [JsonProperty(Required = Required.Always)]
        public BigNumber callGasLimit;
        [JsonProperty(Required = Required.Always)]
        public BigNumber verificationGasLimit;
        public BigNumber preVerificationGas;
        [JsonProperty(Required = Required.Always)]
        public BigNumber maxFeePerGas;
        [JsonProperty(Required = Required.Always)]
        public BigNumber maxPriorityFeePerGas;
        [JsonProperty(Required = Required.Always)]
        public string paymasterAndData;
        [JsonProperty(Required = Required.Always)]
        public string signature;
    }

    private class BigNumber
    {
        [JsonProperty(Required = Required.Always)]
        string hex;
        [JsonProperty(Required = Required.Always)]
        string type;
    }


}