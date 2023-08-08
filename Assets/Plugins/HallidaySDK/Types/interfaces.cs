using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

public enum BlockchainType
{
    ETHEREUM,
    POLYGON,
    MUMBAI,
    BINANCE,
    AVALANCHE,
    SOLANA_DEVNET,
    SUI,
    SUI_TESTNET,
    SOLANA,
    GOERLI,
    IMMUTABLE_PROD, // this pairs w/ ETH mainnet
    IMMUTABLE_DEV, // this pairs w/ Goerli
    DFK_TESTNET,
    DFK
}

public enum HallidayErrorCode
{
    INVALID_PARAMETER,
    INTERNAL_ERROR,
    INCOMPLETE_CONFIG,
    USER_TOKEN_MISSING,
    SER_TOKEN_INVALID,
    USER_ALREADY_EXISTS,
    USER_DOES_NOT_EXIST,
    USER_NO_PHONE_NUMBER,
    USER_NO_EMAIL,
    USER_NO_NAME,
    USER_EMAIL_NOT_VERIFIED,
    AUTH_FAILED,
    AUTH_MISSING,
    USER_BANNED,
    USER_DEFAULTED,
    USER_UNDER_REVIEW,
    USER_NOT_SETUP_FOR_SUI_GAS_STATION,
    TXHASH_MISSING,
    TOKEN_TYPE_MISSING,
    AMOUNT_TOKEN_MISSING,
    NOT_SUPPORTED,
    LOCATION_NOT_SUPPORTED,
    OBJECT_DOES_NOT_EXIST,
    MULTIPLE_OBJECTS_FOR_SAME_ID,
    COLLECTION_NOT_ON_MARKETPLACE,
    COLLECTION_NOT_ALLOWED,
    ITEM_NOT_FOR_SALE,
    PRICE_OR_CURRENCY_NOT_SUPPORTED,
    NOT_A_BUSINESS_ACCOUNT,
    BLOCKCHAIN_TYPE_MISSING,
}

public class GetUserInfoResponse
{
    public string Name { get; }
    public string Email { get; }

    public GetUserInfoResponse(string name, string email)
    {
        Name = name;
        Email = email;
    }
}

public class WalletList
{
    public List<Wallet> wallets;
}
public class Wallet
{
    /** The name of the blockchain (e.g. ETHEREUM, POLYGON, etc.) */
    [JsonProperty(Required = Required.Always)]
    public BlockchainType blockchain_type;

    /** Your in-game user ID for this user */
    [JsonProperty(Required = Required.Always)]
    public string in_game_player_id;

    /** The Halliday wallet address corresponding to this user */
    [JsonProperty(Required = Required.Always)]
    public string account_address;
}

public class ErrorResponse
{
    [JsonProperty(Required = Required.Always)]
    public HallidayErrorCode code;
}

public class GetAssetsResponse
{
    [JsonProperty(Required = Required.Always)]
    int num_assets;
    [JsonProperty(Required = Required.Always)]
    public List<Asset> assets;
}

public class Asset
{
    [JsonProperty(Required = Required.Always)]
    BlockchainType blockchain_type;
    [JsonProperty(Required = Required.Always)]
    string collection_address;
    [JsonProperty(Required = Required.Always)]
    string token_id;
}

public class GetBalancesResponse
{
    [JsonProperty(Required = Required.Always)]
    int num_erc20_tokens;
    [JsonProperty(Required = Required.Always)]
    List<ERC20Token> erc20_tokens;
    int num_native_tokens;
    [JsonProperty(Required = Required.Always)]
    List<NativeToken> native_tokens;
}

public class ERC20Token
{
    [JsonProperty(Required = Required.Always)]
    string blockchain_type;
    [JsonProperty(Required = Required.Always)]
    string token_address;
    [JsonProperty(Required = Required.Always)]
    string balance;
    [JsonProperty(Required = Required.Always)]
    int decimals;
}

public class NativeToken
{
    [JsonProperty(Required = Required.Always)]
    string blockchain_type;
    [JsonProperty(Required = Required.Always)]
    string balance;
    [JsonProperty(Required = Required.Always)]
    int decimals;
}

public class GetTransactionResponse
{
    /** The blockchain on which this transaction occurred (e.g. ETHEREUM, IMMUTABLE_PROD) */
    [JsonProperty(Required = Required.Always)]
    public string blockchain_type;
    /** The ID of the transaction */
    [JsonProperty(Required = Required.Always)]
    public string tx_id;
    /** The status of the transaction (e.g. PENDING, COMPLETE, FAILED) */
    [JsonProperty(Required = Required.Always)]
    public string status;
    /** The number of times we've retried sending this transaction */
    [JsonProperty(Required = Required.Always)]
    public int retry_count;
    /** The blockchain transaction id if the transaction succeeded */
    public string on_chain_id;
    /** The error message if the transaction failed */
    public string error_message;
}

public enum TransactionType
{
    TRANSFER_ASSET,
    TRANSFER_BALANCE,
    CALL_CONTRACT,
}

public class SubmitTransactionResponse
{
    [JsonProperty(Required = Required.Always)]
    public string tx_id;
}