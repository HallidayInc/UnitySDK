using System.Collections;
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

[RequireComponent(typeof(Web3Auth))]
public class Web3CustomTest : MonoBehaviour
{
    Web3Custom web3Custom;
    void Awake()
    {
        web3Custom = GetComponent<Web3Custom>();
        Debug.Log(web3Custom);
    }

    // Start is called before the first frame update
    void Start()
    {
        web3Custom.Initialize("INSERT_HALLIDAY_KEY", BlockchainType.MUMBAI, true);
        web3Custom.logInWithGoogle();
        test();
        // task.Wait();
    }

    async void test()
    {
        while (true)
        {
            if (web3Custom.getUserInfo() != null)
            {
                break;
            }
            await Task.Delay(10000);
        }
        const string ID1 = "INSERT_ID";
        const string ID2 = "INSERT_ID";

        // Initial Properties
        await displayPlayerInfo(1, ID1);
        await displayPlayerInfo(2, ID2);


        const string test_erc20_contract_address = "INSERT_HEX_ADDRESS";
        const string test_erc721_contract_address = "INSERT_HEX_ADDRESS";
        BigInteger test_erc721_token_id = 7032 // replace with your token id;


        Debug.Log("Native Token Transfer: ");
        var nativeBalanceTransferTxInfo = await web3Custom.transferBalance(
            from_in_game_player_id: ID2,
            to_in_game_player_id: ID1,
            token_address: null,
            value: "0.025", blockchain_type:
            BlockchainType.MUMBAI,
            sponsor_gas: false
        );
        Debug.Log(JsonConvert.SerializeObject(nativeBalanceTransferTxInfo));


        Debug.Log("ERC20 Token Transfer: ");
        var erc20BalanceTransferTxInfo = await web3Custom.transferBalance(
            from_in_game_player_id: ID2,
            to_in_game_player_id: ID1,
            token_address: test_erc20_contract_address,
            value: "1000", blockchain_type:
            BlockchainType.MUMBAI,
            sponsor_gas: false
        );
        Debug.Log(JsonConvert.SerializeObject(erc20BalanceTransferTxInfo));

        Debug.Log("Asset Transfer: ");
        var transferAssetTxInfo = await web3Custom.transferAsset(
            from_in_game_player_id: ID1,
            to_in_game_player_id: ID2,
            collection_address: test_erc721_contract_address,
            token_id: test_erc721_token_id.ToString(),
            blockchain_type: BlockchainType.MUMBAI,
            sponsor_gas: true
        );
        Debug.Log(JsonConvert.SerializeObject(transferAssetTxInfo));


        //Numerical (i.e uint256) values must be defined as integer types when creating callContract calldata 
        BigInteger test_transfer_amount = 1000;
        const string player1Address = "INSERT_HEX_ADDRESS";
        const string player2Address = "INSERT_HEX_ADDRESS";

        //CcallContract Test
        var web3 = new Web3();
        // //Custom ERC 721 Transfer
        // string abi = getAbiString("Assets/TestScripts/ERC271ABI.json");
        // var contract = web3.Eth.GetContract(abi, test_erc721_contract_address);
        // var function = contract.GetFunction("transferFrom");
        // var calldata = function.GetData(new object[] { player2Address, player1Address, test_erc721_token_id });

        // // Custom ERC 20 Transfer
        string abi = getAbiString("Assets/TestScripts/ERC20ABI.json");
        var contract = web3.Eth.GetContract(abi, test_erc20_contract_address);
        var function = contract.GetFunction("transfer");
        var calldata = function.GetData(new object[] { player1Address, test_transfer_amount });

        Debug.Log("Custom Contract Call (ERC 20 Transfer): ");
        var callContractTxInfo = await web3Custom.callContract(
            from_in_game_player_id: ID2,
            target_address: test_erc20_contract_address,
            calldata: calldata,
            value: "0",
            blockchain_type: BlockchainType.MUMBAI,
            sponsor_gas: true
        );
        Debug.Log(JsonConvert.SerializeObject(callContractTxInfo));

        // Final Properties
        await displayPlayerInfo(1, ID1);
        await displayPlayerInfo(2, ID2);

    }

    private async Task displayPlayerInfo(int num, string ID)
    {
        Wallet wallet = await web3Custom.getOrCreateHallidayAAWallet(ID);
        Debug.Log("Player " + num + "Wallet: ");
        Debug.Log(JsonConvert.SerializeObject(wallet));

        GetAssetsResponse getAssetsResponse = await web3Custom.getAssets(ID);
        Debug.Log("Player " + num + "Assets: ");
        Debug.Log(JsonConvert.SerializeObject(getAssetsResponse));

        GetBalancesResponse getBalancesResponse = await web3Custom.getBalances(ID);
        Debug.Log("Player " + num + "Balances: ");
        Debug.Log(JsonConvert.SerializeObject(getBalancesResponse));
    }
    // Update is called once per frame
    void Update()
    {

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
