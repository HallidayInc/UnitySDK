<p align="center">
<br />
<a href="https://halliday.xyz"><img src="https://github.com/HallidayInc/UnitySDK/blob/master/hallidayLogo.svg" width="100" alt=""/></a>
</p>
<h1 align="center">Halliday Unity SDK</h1>

# Installation

Download the latest `.unitypackage` from the [releases](https://github.com/HallidayInc/UnitySDK/releases) page.

Right click the **Assets** in your Unity Project, select **Import Package**, and select the `.unitypackage` you downloaded. Make sure you select all the files and import them. You can now find the **HallidayClient** in your **Plugins** folder.

The package comes with a walkthrough script that you can modify and run to use to the SDK.

Note: The Nethereum DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.

# Build

Add the HallidayClient script to a GameObject. Call the Initialize() function with your Halliday API key and use the Client's public API function to manage your user's blockchain actions.

# Usage

```csharp
// Get the Halliday Component
hallidayClient = GetComponent<HallidayClient>();

// Initialize the component with your Halliday Api Key
hallidayClient.Initialize("INSERT_HALLIDAY_API_KEY", BlockchainType.MUMBAI, true);

// Get (or Create if new user) wallet for playerId
// If this is a new player, HallidayClient will create a new wallet and return it
// If this is a returning player, HallidayClient will return the wallet stored for the player
// If returning player, ensure that the playerId matches what was previously used create a wallet (otherwise an error will be thrown)
Wallet wallet = hallidayClient.getOrCreateHallidayAAWallet(playerId);

//Get Player NFTs
GetAssetsResponse getAssetsResponse = await hallidayClient.getAssets(playerId);

//Get Player ERC-20 and Native Token balances
GetBalancesResponse getBalancesResponse = await hallidayClient.getBalances(playerId);

//Call balanceTransfer to transfer native tokens
var nativeBalanceTransferTxInfo = await hallidayClient.transferBalance(
    from_in_game_player_id: PLAYER_ID_2,
    to_in_game_player_id: PLAYER_ID_1,
    token_address: null,
    value: "0.025", blockchain_type:
    BlockchainType.MUMBAI,
    sponsor_gas: true
);

//Call balanceTransfer to transfer erc-20 tokens
var erc20BalanceTransferTxInfo = await hallidayClient.transferBalance(
    from_in_game_player_id: PLAYER_ID_2,
    to_in_game_player_id: PLAYER_ID_1,
    token_address: test_erc20_contract_address,
    value: "1000", blockchain_type:
    BlockchainType.MUMBAI,
    sponsor_gas: false
);

// Send NFT from plater 2 to player 1
var transferAssetTxInfo = await hallidayClient.transferAsset(
    from_in_game_player_id: PLAYER_ID_2,
    to_in_game_player_id: PLAYER_ID_1,
    collection_address: test_erc721_contract_address,
    token_id: test_erc721_token_id.ToString(),
    blockchain_type: BlockchainType.MUMBAI,
    sponsor_gas: true
);


// Make Contract Call (upload ABI as JSON and encode function data)
// Numerical (i.e uint256) values must be defined as integer types when creating callContract calldata
BigInteger test_transfer_amount = 1000;
string player1Address = player1Wallet.account_address;
string player2Address = player2Wallet.account_address;

var web3 = new Web3();
// Custom ERC 20 Transfer (similar to before except constructed manually)
// Import ABI as a Json and specify your target contract address
var contract = web3.Eth.GetContract(abi, test_erc20_contract_address);
var function = contract.GetFunction("transfer");

// Specify parameters according to function signature
var calldata = function.GetData(new object[] { player1Address, test_transfer_amount });

var callContractTxInfo = await hallidayClient.callContract(
    from_in_game_player_id: PLAYER_ID_2,
    target_address: test_erc20_contract_address,
    calldata: calldata,
    value: "1000",
    blockchain_type: BlockchainType.MUMBAI,
    sponsor_gas: true
);
```

### Notes on Walkthrough

- To build the walkthrough, add the Walkthrough script to your game object and run the game (HallidayClient script will automatically be added as it's a dependency)
- Before running the walkthrough, specify the player ids wherever `INSERT_PLAYER_ID` is specified
- The walkthrough will open 2 login screens (one login for Player 1, then logout, then another login for Player 2)
- After you get the player wallets, send MATIC, Aave (or another ERC20 token), and NFTs to the wallet so you can test the transfer functions or your custom calls
- Replace the required addresses wherever `INSERT_HEX_ADDRESS` is specified

### Notes on General Usage

- To enable current user management, you can build a singleton class on top of this component and expose the hallidayClient as a static instance
