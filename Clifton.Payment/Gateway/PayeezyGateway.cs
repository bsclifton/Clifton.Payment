using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        #region Members

        protected Dictionary<CreditCardType, string> CardTypeLookup = new Dictionary<CreditCardType, string>() {
            { CreditCardType.AmericanExpress, "American Express" },
            { CreditCardType.Visa, "Visa" },
            { CreditCardType.MasterCard, "Mastercard" },
            { CreditCardType.Discover, "Discover" }
        };

        protected enum TransactionType {
            Authorize,
            Purchase,
            Void,
            Capture,
            Split,
            Refund
        }

        protected Dictionary<TransactionType, string> TransactionTypeLookup = new Dictionary<TransactionType, string>() {
            { TransactionType.Authorize, "authorize" },
            { TransactionType.Purchase, "purchase" },
            { TransactionType.Void, "void" },
            { TransactionType.Capture, "capture" },
            { TransactionType.Split, "split" },
            { TransactionType.Refund, "refund" }
        };

        protected enum MethodType {
            CreditCard
        }

        protected Dictionary<MethodType, string> MethodTypeLookup = new Dictionary<MethodType, string>() {
            { MethodType.CreditCard, "credit_card" }
        };

        protected const string UsDollars = "USD";

        #endregion

        #region Properties

        protected string ApiKey { get; set; }

        protected string ApiSecret { get; set; }

        protected override string ContentType {
            get { return "application/json"; }
        }

        protected string Token { get; set; }

        protected string Url { get; set; }

        #endregion

        public PayeezyGateway(string apiKey, string apiSecret, string token, string url) {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            Token = token;
            Url = url;
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
            webRequest.ContentType = ContentType;
            webRequest.Accept = ContentType;
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

        protected void ProcessRequest(dynamic payload, string transactionId=null) {
            string resourceUrl;

            if (!string.IsNullOrWhiteSpace(transactionId)) {
                resourceUrl = string.Format("{0}/{1}", this.Url, transactionId);
            } else {
                resourceUrl = this.Url;
            }

            string payloadString = JsonConvert.SerializeObject(payload);
            HttpWebRequest webRequest = CreateRequest(this.ApiKey, this.ApiSecret, this.Token, resourceUrl, payloadString);

            try {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) {
                    using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream())) {
                        string responseString = responseStream.ReadToEnd();
                    }

                    //TODO: handle/process success
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
            }
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
        public void CreditCardAuthorize(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Authorize],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = UsDollars,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/creditcardpayment/apis/post/transactions"/>
        public override void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Purchase],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                partial_redemption = false,
                currency_code = UsDollars,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public override void CreditCardRefund(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Refund],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = UsDollars,
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = FormatCardExpirationDate(parsedExpirationDate),
                    cvv = cardVerificationValue //TODO: validate
                }
            };

            ProcessRequest(payload);
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public void CreditCardVoid(string transactionId, string referenceNumber, string transactionTag, string dollarAmount) {
            //TODO: validate

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_tag = transactionTag,
                transaction_type = TransactionTypeLookup[TransactionType.Void],
                method = MethodTypeLookup[MethodType.CreditCard],
                amount = dollarAmount,
                currency_code = UsDollars
            };

            ProcessRequest(payload, transactionId);
        }
    }
}