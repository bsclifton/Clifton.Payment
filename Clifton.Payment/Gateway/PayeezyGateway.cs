using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Clifton.Payment.Gateway {
    /// <summary>
    /// Wraps the newer REST based API for First Data.
    /// </summary>
    /// <see cref="https://developer.payeezy.com/payeezy-api-reference/apis"/>
    /// <seealso cref="https://developer.payeezy.com/"/>
    public class PayeezyGateway : BaseGateway {
        #region Types

        public enum TransactionType {
            Authorize,
            Purchase,
            Void,
            Capture,
            Split,
            Refund
        }

        public enum MethodType {
            CreditCard
        }

        public enum TransactionStatus {
            Approved,
            Declined,
            NotProcessed
        }

        public enum GatewayResponseCode {
            Unknown,
            TransactionNormal,

            CVV2_CID_CVC2_DataNotVerified,
            InvalidCreditCardNumber,
            InvalidExpiryDate,
            InvalidAmount,
            InvalidCardHolder,
            InvalidAuthorizationNo,
            InvalidVerificationString,
            InvalidTransactionCode,
            InvalidReferenceNo,
            InvalidAvsString,
            InvalidCustomerReferenceNumber,
            InvalidDuplicate,
            InvalidRefund,
            RestrictedCardNumber,
            InvalidTransactionTag,
            DataWithinTransactionIncorrect,
            InvalidAuthNumberOnPreAuthCompletion,

            InvalidSequenceNo,
            MessageTimedOutAtHost,
            BceFunctionError,
            InvalidResponseFromPayeezy,
            InvalidDateFromHost,

            InvalidTransactionDescription,
            InvalidGatewayID,
            InvalidTransactionNumber,
            ConnectionInactive,
            UnmatchedTransaction,
            InvalidReversalResponse,
            UnableToSendSocketTransaction,
            UnableToWriteTransactionToFile,
            UnableToVoidTransaction,
            PaymentTypeNotSupportedByMerchant,
            UnableToConnect,
            UnableToSendLogon,
            UnableToSendTrans,
            InvalidLogon,
            TerminalNotActivated,
            Terminal_Gateway_Mismatch,
            InvalidProcessingCenter,
            NoProcessorsAvailable,
            DatabaseUnavailable,
            SocketError,
            HostNotReady,

            AddressNotVerified,
            TransactionPlacedInQueue,
            TransactionReceivedFromBank,
            ReversalPending,
            ReversalComplete,
            ReversalSentToBank,

            FraudSuspected_AddressCheckFailed,
            FraudSuspected_Card_Check_Number_CheckFailed,
            FraudSuspected_CountryCheckFailed,
            FraudSuspected_CustomerReferenceCheckFailed,
            FraudSuspected_EmailAddressCheckFailed,
            FraudSuspected_IpAddressCheckFailed
        }

        public class Response {
            //public MethodType Method { get; set; }
            public string Amount { get; set; }
            public string Currency { get; set; }
            /*
            "avs": "{string}",
            "cvv2": "{string}",
            "card": {
                "type": "{string}",
                "cardholder_name": "{string}",
                "card_number": "{string}",
                "exp_date": "{string}"
            },
            "token": {
                "token_type": "{string}",
                "token_data": {
                    "value": "{string}"
                }
            },
            */
            //public TransactionStatus TransactionStatus { get; set; }
            public string ValidationStatus { get; set; }
            //public TransactionType TransactionType { get; set; }
            public string TransactionId { get; set; }
            public string TransactionTag { get; set; }
            public string BankResponseCode { get; set; }
            public string BankMessage { get; set; }
            public string GatewayResponseCode { get; set; }
            public GatewayResponseCode ParsedGatewayResponseCode { get; set; }
            public string GatewayMessage { get; set; }
            public string CorrelationId { get; set; }
        }

        #endregion

        #region Members

        protected Dictionary<CreditCardType, string> CardTypeLookup = new Dictionary<CreditCardType, string>() {
            { CreditCardType.AmericanExpress, "American Express" },
            { CreditCardType.Visa, "Visa" },
            { CreditCardType.MasterCard, "Mastercard" },
            { CreditCardType.Discover, "Discover" }
        };

        protected Dictionary<TransactionType, string> TransactionTypeLookup = new Dictionary<TransactionType, string>() {
            { TransactionType.Authorize, "authorize" },
            { TransactionType.Purchase, "purchase" },
            { TransactionType.Void, "void" },
            { TransactionType.Capture, "capture" },
            { TransactionType.Split, "split" },
            { TransactionType.Refund, "refund" }
        };

        protected Dictionary<MethodType, string> MethodTypeLookup = new Dictionary<MethodType, string>() {
            { MethodType.CreditCard, "credit_card" }
        };

        protected Dictionary<string, GatewayResponseCode> GatewayResponseCodeLookup = new Dictionary<string, GatewayResponseCode>() {
            //This response code indicates that the transaction was processed normally.
            //Please refer to the bank and approval response information for bank approval Status.
            { "00", GatewayResponseCode.TransactionNormal },
            //The following response codes indicate invalid data in the transaction.
            //In these cases, the data should be changed before attempting to resend the transaction.
            //These response codes are generated by the remote Plug-In.
            { "08", GatewayResponseCode.CVV2_CID_CVC2_DataNotVerified },
            { "22", GatewayResponseCode.InvalidCreditCardNumber },
            { "25", GatewayResponseCode.InvalidExpiryDate },
            { "26", GatewayResponseCode.InvalidAmount },
            { "27", GatewayResponseCode.InvalidCardHolder },
            { "28", GatewayResponseCode.InvalidAuthorizationNo },
            { "31", GatewayResponseCode.InvalidVerificationString },
            { "32", GatewayResponseCode.InvalidTransactionCode },
            { "57", GatewayResponseCode.InvalidReferenceNo },
            { "58", GatewayResponseCode.InvalidAvsString },
            { "60", GatewayResponseCode.InvalidCustomerReferenceNumber },
            { "63", GatewayResponseCode.InvalidDuplicate },
            { "64", GatewayResponseCode.InvalidRefund },
            { "68", GatewayResponseCode.RestrictedCardNumber },
            { "69", GatewayResponseCode.InvalidTransactionTag },
            { "72", GatewayResponseCode.DataWithinTransactionIncorrect },
            { "93", GatewayResponseCode.InvalidAuthNumberOnPreAuthCompletion },
            // The following response codes indicate a problem with the merchant configuration at the financial institution.
            // Please contact Payeezy for further investigation.
            { "11", GatewayResponseCode.InvalidSequenceNo },
            { "12", GatewayResponseCode.MessageTimedOutAtHost },
            { "21", GatewayResponseCode.BceFunctionError },
            { "23", GatewayResponseCode.InvalidResponseFromPayeezy },
            { "30", GatewayResponseCode.InvalidDateFromHost },
            // The following response codes indicate a problem with the Global Gateway e4℠ host or an error in the merchant configuration.
            // Please contact Payeezy for further investigation.
            { "10", GatewayResponseCode.InvalidTransactionDescription },
            { "14", GatewayResponseCode.InvalidGatewayID },
            { "15", GatewayResponseCode.InvalidTransactionNumber },
            { "16", GatewayResponseCode.ConnectionInactive },
            { "17", GatewayResponseCode.UnmatchedTransaction },
            { "18", GatewayResponseCode.InvalidReversalResponse },
            { "19", GatewayResponseCode.UnableToSendSocketTransaction },
            { "20", GatewayResponseCode.UnableToWriteTransactionToFile },
            { "24", GatewayResponseCode.UnableToVoidTransaction },
            { "37", GatewayResponseCode.PaymentTypeNotSupportedByMerchant },
            { "40", GatewayResponseCode.UnableToConnect },
            { "41", GatewayResponseCode.UnableToSendLogon },
            { "42", GatewayResponseCode.UnableToSendTrans },
            { "43", GatewayResponseCode.InvalidLogon },
            { "52", GatewayResponseCode.TerminalNotActivated },
            { "53", GatewayResponseCode.Terminal_Gateway_Mismatch },
            { "54", GatewayResponseCode.InvalidProcessingCenter },
            { "55", GatewayResponseCode.NoProcessorsAvailable },
            { "56", GatewayResponseCode.DatabaseUnavailable },
            { "61", GatewayResponseCode.SocketError },
            { "62", GatewayResponseCode.HostNotReady },
            // The following response codes indicate the final state of a transaction.
            // In the event of one of these codes being returned, please contact Payeezy for further investigation.
            { "44", GatewayResponseCode.AddressNotVerified },
            { "70", GatewayResponseCode.TransactionPlacedInQueue },
            { "73", GatewayResponseCode.TransactionReceivedFromBank },
            { "76", GatewayResponseCode.ReversalPending },
            { "77", GatewayResponseCode.ReversalComplete },
            { "79", GatewayResponseCode.ReversalSentToBank },
            // The following response codes indicate the final state of a transaction due to custom Fraud Filters created by the Merchant.
            { "F1", GatewayResponseCode.FraudSuspected_AddressCheckFailed },
            { "F2", GatewayResponseCode.FraudSuspected_Card_Check_Number_CheckFailed },
            { "F3", GatewayResponseCode.FraudSuspected_CountryCheckFailed },
            { "F4", GatewayResponseCode.FraudSuspected_CustomerReferenceCheckFailed },
            { "F5", GatewayResponseCode.FraudSuspected_EmailAddressCheckFailed },
            { "F6", GatewayResponseCode.FraudSuspected_IpAddressCheckFailed }
        };

        #endregion

        #region Properties

        protected string ApiKey { get; set; }

        protected string ApiSecret { get; set; }

        protected string CurrencyCode { get; private set; }

        protected string Token { get; set; }

        protected string Url { get; set; }

        #endregion

        public PayeezyGateway(string apiKey, string apiSecret, string token, string url) {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            Token = token;
            Url = url;
            //NOTE: hardcoded to USD for now
            CurrencyCode = new RegionInfo("US").ISOCurrencySymbol;
        }

        #region Common methods

        protected string GenerateHmac(string apiKey, string apiSecret, string token, int nonce, string currentTimestamp, string payload) {
            string message = apiKey + nonce.ToString() + currentTimestamp + token + payload;
            HMAC hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            byte[] hmacData = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hmacData);
        }

        protected HttpWebRequest CreateRequest(string apiKey, string apiSecret, string token, string url, string payloadString) {
            string currentTimestamp = GetEpochTimestampInMilliseconds();
            int nonce = GetNonce();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = MimeTypes.ApplicationJson;
            webRequest.Accept = MimeTypes.ApplicationJson;
            webRequest.Headers.Add("apikey", apiKey);
            webRequest.Headers.Add("token", token);
            webRequest.Headers.Add("nonce", nonce.ToString());
            webRequest.Headers.Add("timestamp", currentTimestamp);
            webRequest.Headers.Add("Authorization", GenerateHmac(apiKey, apiSecret, token, nonce, currentTimestamp, payloadString));
            webRequest.ContentLength = payloadString.Length;

            // write and send request data
            using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream())) {
                streamWriter.Write(payloadString);
            }

            return webRequest;
        }

        public Response ParseResponse(string responseString) {
            dynamic responseObject = JObject.Parse(responseString);

            Response response = new Response {
                //TODO: method
                Amount = responseObject.amount,
                Currency = responseObject.currency,

                //...
                //TODO: avs/cvv2/card/token
                //...

                //TODO: transaction_status
                ValidationStatus = responseObject.validation_status,
                //TODO: transaction_type
                TransactionId = responseObject.transaction_id,
                TransactionTag = responseObject.transaction_tag,
                BankResponseCode = responseObject.bank_resp_code,
                BankMessage = responseObject.bank_message,
                GatewayResponseCode = responseObject.gateway_resp_code,
                GatewayMessage = responseObject.gateway_message,
                CorrelationId = responseObject.correlation_id
            };

            if (GatewayResponseCodeLookup.ContainsKey(response.GatewayResponseCode)) {
                response.ParsedGatewayResponseCode = GatewayResponseCodeLookup[response.GatewayResponseCode];
            } else {
                response.ParsedGatewayResponseCode = GatewayResponseCode.Unknown;
            }

            return response;
        }

        protected Response ProcessRequest(dynamic payload, string transactionId = null) {
            string resourceUrl;

            if (!string.IsNullOrWhiteSpace(transactionId)) {
                resourceUrl = string.Format("{0}/{1}", this.Url, transactionId);
            } else {
                resourceUrl = this.Url;
            }

            string payloadString = JsonConvert.SerializeObject(payload);
            HttpWebRequest webRequest = CreateRequest(this.ApiKey, this.ApiSecret, this.Token, resourceUrl, payloadString);
            string responseString;

            try {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) {
                    using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream())) {
                        responseString = responseStream.ReadToEnd();
                    }
                }
            } catch (WebException ex) {
                if (ex.Response != null) {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response) {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream())) {
                            string exception = reader.ReadToEnd();
                            //TODO: handle/process failure. Until then, rethrow.
                            throw;
                        }
                    }
                }
                throw;
            }

            return ParseResponse(responseString);
        }

        protected CreditCardType ValidateAndParseCardDetails(string cardNumber, string expirationMonth, string expirationYear, out DateTime parsedExpirationDate) {
            CreditCardType cardType = base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            if (!CardTypeLookup.ContainsKey(cardType)) {
                throw new CardTypeNotSupportedException(string.Format("Card type {0} is not supported", cardType.ToString()));
            }
            return cardType;
        }

        #endregion

        /// <see cref="https://developer.payeezy.com/creditcardpayment/apis/post/transactions"/>
        public Response CreditCardAuthorize(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Authorize],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/creditcardpayment/apis/post/transactions"/>
        public Response CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Purchase],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                partial_redemption = false,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public Response CreditCardRefund(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Refund],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public Response CreditCardVoid(string transactionId, string referenceNumber, string transactionTag, string dollarAmount) {
            //TODO: validate

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_tag = transactionTag,
                transaction_type = TransactionTypeLookup[TransactionType.Void],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode
            };

            return ProcessRequest(payload, transactionId);
        }
    }
}