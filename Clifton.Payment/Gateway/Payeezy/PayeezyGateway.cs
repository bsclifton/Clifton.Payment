using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
    public partial class PayeezyGateway : BaseGateway {
        #region Properties

        protected string ApiKey { get; set; }

        protected string ApiSecret { get; set; }

        /// <summary>ISO 4217 currency code. Defaulted to USD.</summary>
        protected string CurrencyCode { get; set; }

        protected string Token { get; set; }

        protected string Url { get; set; }

        #endregion

        public PayeezyGateway(string apiKey, string apiSecret, string token, string url) {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            Token = token;
            Url = url;
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
                MethodType = responseObject.method,
                ParsedMethodType = MethodType.Unknown,
                Amount = responseObject.amount,
                Currency = responseObject.currency,
                //...
                //TODO: avs/cvv2/card/token
                //...
                TransactionStatus = responseObject.transaction_status,
                ParsedTransactionStatus = TransactionStatus.Unknown,
                ValidationStatus = responseObject.validation_status,
                TransactionType = responseObject.transaction_type,
                ParsedTransactionType = TransactionType.Unknown,
                TransactionId = responseObject.transaction_id,
                TransactionTag = responseObject.transaction_tag,
                BankResponseCode = responseObject.bank_resp_code,
                ParsedBankResponseCode = BankResponseCode.Unknown,
                BankMessage = responseObject.bank_message,
                GatewayResponseCode = responseObject.gateway_resp_code,
                ParsedGatewayResponseCode = GatewayResponseCode.Unknown,
                GatewayMessage = responseObject.gateway_message,
                CorrelationId = responseObject.correlation_id,
                ErrorMessages = new System.Collections.Generic.List<Response.ErrorMessage>()
            };

            #region Parse error messages (if response has any)

            if (responseObject.Error != null && responseObject.Error.messages != null) {
                foreach (dynamic error in responseObject.Error.messages) {
                    Response.ErrorMessage msg = new Response.ErrorMessage {
                        Code = error.code,
                        Description = error.description
                    };

                    response.ErrorMessages.Add(msg);
                }
            }

            #endregion

            #region Convert response fields into an enum (if possible)

            if (!string.IsNullOrWhiteSpace(response.MethodType) && MethodTypeByString.ContainsKey(response.MethodType)) {
                response.ParsedMethodType = MethodTypeByString[response.MethodType];
            }

            if (!string.IsNullOrWhiteSpace(response.TransactionStatus) && TransactionStatusByString.ContainsKey(response.TransactionStatus.ToLower())) {
                response.ParsedTransactionStatus = TransactionStatusByString[response.TransactionStatus.ToLower()];
            }

            if (!string.IsNullOrWhiteSpace(response.TransactionType) && TransactionTypeByString.ContainsKey(response.TransactionType)) {
                response.ParsedTransactionType = TransactionTypeByString[response.TransactionType];
            }

            if (!string.IsNullOrWhiteSpace(response.BankResponseCode) && BankResponseCodeByString.ContainsKey(response.BankResponseCode)) {
                response.ParsedBankResponseCode = BankResponseCodeByString[response.BankResponseCode];
            }

            if (!string.IsNullOrWhiteSpace(response.GatewayResponseCode) && GatewayResponseCodeByString.ContainsKey(response.GatewayResponseCode)) {
                response.ParsedGatewayResponseCode = GatewayResponseCodeByString[response.GatewayResponseCode];
            }

            #endregion

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
                            string exceptionResponse = reader.ReadToEnd();

                            return ParseResponse(exceptionResponse);
                        }
                    }
                }
                throw;
            }

            return ParseResponse(responseString);
        }

        protected CreditCardType ValidateAndParseCardDetails(string cardNumber, string expirationMonth, string expirationYear, out DateTime parsedExpirationDate) {
            CreditCardType cardType = base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            if (!CardTypeToString.ContainsKey(cardType)) {
                throw new CardTypeNotSupportedException(string.Format("Card type {0} is not supported", cardType.ToString()));
            }
            return cardType;
        }

        #endregion

        /// <see cref="https://developer.payeezy.com/creditcardpayment/apis/post/transactions"/>
        public Response CreditCardAuthorize(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;

            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            cardVerificationValue = ValidateCardSecurityCode(cardType, cardVerificationValue);
            dollarAmount = ValidateDollarAmount(dollarAmount);

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeToString[TransactionType.Authorize],
                method = MethodTypeToString[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeToString[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/creditcardpayment/apis/post/transactions"/>
        public Response CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;

            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            cardVerificationValue = ValidateCardSecurityCode(cardType, cardVerificationValue);
            dollarAmount = ValidateDollarAmount(dollarAmount);

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeToString[TransactionType.Purchase],
                method = MethodTypeToString[MethodType.CreditCard],
                amount = dollarAmount,
                partial_redemption = false,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeToString[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public Response CreditCardRefund(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;

            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            cardVerificationValue = ValidateCardSecurityCode(cardType, cardVerificationValue);
            dollarAmount = ValidateDollarAmount(dollarAmount);

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeToString[TransactionType.Refund],
                method = MethodTypeToString[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode,
                credit_card = new {
                    type = CardTypeToString[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue
                }
            };

            return ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public Response CreditCardVoid(string transactionId, string referenceNumber, string transactionTag, string dollarAmount) {
            dollarAmount = ValidateDollarAmount(dollarAmount);

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_tag = transactionTag,
                transaction_type = TransactionTypeToString[TransactionType.Void],
                method = MethodTypeToString[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = CurrencyCode
            };

            return ProcessRequest(payload, transactionId);
        }
    }
}