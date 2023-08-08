using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Numerics;
using Newtonsoft.Json;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using System.Text;
using Nethereum.Util;
using Nethereum.ABI;
using System.IO;

[RequireComponent(typeof(HallidayClient))]
public class Walkthrough : MonoBehaviour
{
    HallidayClient hallidayClient;
    void Awake()
    {
        hallidayClient = GetComponent<HallidayClient>();
    }

    // Start is called before the first frame update
    void Start()
    {
        hallidayClient.Initialize("INSERT_HALLIDAY_API_KEY", BlockchainType.MUMBAI, true);
        walkthrough();
    }

    async void walkthrough()
    {
        hallidayClient.logInWithGoogle(); //login to first user (can change this to different provider)
        while (true)
        {
            if (hallidayClient.getUserInfo() != null)
            {
                break;
            }
            await Task.Delay(3000);
        }
        string PLAYER_ID_1 = "INSERT_PLAYER_ID";
        var player1Wallet = await getPlayerWallet(PLAYER_ID_1);
        Debug.Log(JsonConvert.SerializeObject(player1Wallet)); //Prints wallet content - use this info to transfer tokens to account address to enable next functions
        hallidayClient.logOut(); //logout first player

        hallidayClient.logInWithGoogle(); //login with second user (can change this to different provider)
        while (true)
        {
            if (hallidayClient.getUserInfo() != null)
            {
                break;
            }
            await Task.Delay(3000);
        }
        string PLAYER_ID_2 = "INSERT_PLAYER_ID";
        var player2Wallet = await getPlayerWallet(PLAYER_ID_2);

        //Prints wallet content - use this info to transfer tokens to account address to enable next functions 
        // (3 MATIC, 10000 AAVE, and 2 Test NFTs should enable the following function calls)
        Debug.Log(JsonConvert.SerializeObject(player2Wallet));

        // Initial Balances
        Debug.Log("Player 1 Balances: ");
        await displayBalances(PLAYER_ID_1);

        Debug.Log("Player 2 Balances: ");
        await displayBalances(PLAYER_ID_2);

        // Execute some operations(note that these are sent from playerId2 because we are currently signed into player 2 and player 2 will be signing the transaction)
        const string test_erc20_contract_address = "0xf14f9596430931e177469715c591513308244e8f"; // Aave - can change to different erc20 contract
        const string test_erc721_contract_address = "INSERT_HEX_ADDRESS"; // enter sample NFT contract address
        BigInteger test_erc721_token_id = 7032; //change as needed


        // Send MUMBAI matic from player 2 to player 1
        Debug.Log("Native Token Transfer: ");
        var nativeBalanceTransferTxInfo = await hallidayClient.transferBalance(
            from_in_game_player_id: PLAYER_ID_2,
            to_in_game_player_id: PLAYER_ID_1,
            token_address: null,
            value: "0.025", blockchain_type:
            BlockchainType.MUMBAI,
            sponsor_gas: true
        );

        // Transaction may take around ~20 seconds
        Debug.Log(JsonConvert.SerializeObject(nativeBalanceTransferTxInfo));


        Debug.Log("ERC20 Token Transfer: ");
        // Send Aave from player 2 to player 1
        var erc20BalanceTransferTxInfo = await hallidayClient.transferBalance(
             from_in_game_player_id: PLAYER_ID_2,
             to_in_game_player_id: PLAYER_ID_1,
             token_address: test_erc20_contract_address,
             value: "1000", blockchain_type:
             BlockchainType.MUMBAI,
             sponsor_gas: false
         );
        // Transaction may take around ~20 seconds 
        Debug.Log(JsonConvert.SerializeObject(erc20BalanceTransferTxInfo));

        Debug.Log("Asset Transfer: ");
        // Send NFT from plater 2 to player 1
        var transferAssetTxInfo = await hallidayClient.transferAsset(
            from_in_game_player_id: PLAYER_ID_2,
            to_in_game_player_id: PLAYER_ID_1,
            collection_address: test_erc721_contract_address,
            token_id: test_erc721_token_id.ToString(),
            blockchain_type: BlockchainType.MUMBAI,
            sponsor_gas: true
        );
        // Transaction may take around ~20 seconds
        Debug.Log(JsonConvert.SerializeObject(transferAssetTxInfo));

        // Custom Contract Call(upload ABI and encode function data)
        // Numerical(i.e uint256) values must be defined as integer types when creating callContract calldata
        BigInteger test_transfer_amount = 1000;
        string player1Address = player1Wallet.account_address;
        string player2Address = player2Wallet.account_address;

        Web3 web3 = new Web3();

        // Custom ERC 20 Transfer (similar to before except constructed manually)
        string abi = getAbiString("Assets/Plugins/HallidaySDK/SampleScripts/ERC20ABI.json");
        var contract = web3.Eth.GetContract(abi, test_erc20_contract_address);
        var function = contract.GetFunction("transfer");
        var calldata = function.GetData(new object[] { player1Address, test_transfer_amount });

        Debug.Log("Custom Contract Call (ERC 20 Transfer): ");
        var callContractTxInfo = await hallidayClient.callContract(
            from_in_game_player_id: PLAYER_ID_2,
            target_address: test_erc20_contract_address,
            calldata: calldata,
            value: "0",
            blockchain_type: BlockchainType.MUMBAI,
            sponsor_gas: true
        );

        // Transaction may take around ~20 seconds 
        Debug.Log(JsonConvert.SerializeObject(callContractTxInfo));

        // // Custom ERC 721 Transfer(similar to before except constructed manually) - rename this variables or comment the other custom transfer creation to test
        // string abi = getAbiString("Assets/Plugins/HallidaySDK/SampleScripts/ERC271ABI.json");
        // var contract = web3.Eth.GetContract(abi, test_erc721_contract_address);
        // var function = contract.GetFunction("transferFrom");
        // var calldata = function.GetData(new object[] { player2Address, player1Address, test_erc721_token_id });

        // Final Balances
        Debug.Log("Player 1 Balances: ");
        await displayBalances(PLAYER_ID_1);

        Debug.Log("Player 2 Balances: ");
        await displayBalances(PLAYER_ID_2);
    }


    /// <summary>
    /// If this is a new player, HallidayClient will create a new wallet and return it
    /// If this is a returning player, HallidayClient will return the wallet stored for the player 
    /// If returning player, ensure that the playerId matches what was previously used create a wallet (otherwise an error will be thrown)
    /// </summary>
    /// <param name="playerId"></param> In Game Player Id stored for this player
    /// 
    async Task<Wallet> getPlayerWallet(string playerId)
    {
        Wallet playerWallet = await hallidayClient.getOrCreateHallidayAAWallet(playerId);
        return playerWallet;
    }

    private async Task displayBalances(string playerId)
    {
        GetAssetsResponse getAssetsResponse = await hallidayClient.getAssets(playerId);
        Debug.Log(playerId + " Player Assets: ");
        Debug.Log(JsonConvert.SerializeObject(getAssetsResponse));

        GetBalancesResponse getBalancesResponse = await hallidayClient.getBalances(playerId);
        Debug.Log(playerId + " Player Balances: ");
        Debug.Log(JsonConvert.SerializeObject(getBalancesResponse));
    }


    private static string ByteArrayToHexString(byte[] byteArray)
    {
        StringBuilder hexBuilder = new StringBuilder(byteArray.Length * 2 + 2); // +2 for "0x"

        hexBuilder.Append("0x");

        foreach (byte b in byteArray)
        {
            hexBuilder.AppendFormat("{0:X2}", b);
        }

        return hexBuilder.ToString();
    }

    private string getAbiString(string filepath)
    {
        using (StreamReader r = new StreamReader(filepath))
        {
            string json = r.ReadToEnd();
            return json;
        }
    }

}
