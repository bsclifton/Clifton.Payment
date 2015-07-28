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
    public class PayeezyRest : BaseGateway {
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
            credit_card
        }

        protected const string UsDollars = "USD";

        #endregion

        #region Properties

        protected override string ContentType {
            get { return "application/json"; }
        }

        #endregion

        protected string GetEpochTimestampInMilliseconds() {
            long millisecondsSinceEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return millisecondsSinceEpoch.ToString();
        }

        /// <summary>
        /// Generates a cryptographically strong random number.
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Cryptographic_nonce"/>
        protected int GetNonce() {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(bytes);
                }
                return BitConverter.ToInt32(bytes, 0);
            }
        }

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

        protected CreditCardType ValidateAndParseCardDetails(string cardNumber, string expirationMonth, string expirationYear, out DateTime parsedExpirationDate) {
            CreditCardType cardType = base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);
            if (!CardTypeLookup.ContainsKey(cardType)) {
                throw new CardTypeNotSupportedException(string.Format("Card type {0} is not supported", cardType.ToString()));
            }
            return cardType;
        }

        /// <see cref="https://developer.payeezy.com/payeezy_new_docs/apis/post/transactions%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20-1"/>
        public override void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Purchase],
                method = MethodType.credit_card.ToString(),
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

            var config = Clifton.Payment.Configuration.GetConfig().Gateways["FirstData"];
            string payloadString = JsonConvert.SerializeObject(payload);
            HttpWebRequest webRequest = CreateRequest(config.ApiKey, config.ApiSecret, config.Token, config.Url, payloadString);

            string responseString;
            try {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) {
                    using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream())) {
                        responseString = responseStream.ReadToEnd();
                    }

                    //TODO: handle/process success
                }
            } catch (WebException ex) {
                if (ex.Response != null) {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response) {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream())) {
                            string remoteEx = reader.ReadToEnd();
                            //TODO: handle/process failure. Until then, rethrow.
                            throw;
                        }
                    }
                }
            }
        }

        /// <see cref="https://developer.payeezy.com/capturereversepayment/apis/post/transactions/%7Bid%7D"/>
        public override void CreditCardRefund(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber) {
            DateTime parsedExpirationDate;
            CreditCardType cardType = ValidateAndParseCardDetails(cardNumber, expirationMonth, expirationYear, out parsedExpirationDate);

            //TODO: validate amount (no decimal places allowed; must be in cents)

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionTypeLookup[TransactionType.Refund],
                method = MethodType.credit_card.ToString(),
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

            var config = Clifton.Payment.Configuration.GetConfig().Gateways["FirstData"];
            string payloadString = JsonConvert.SerializeObject(payload);
            HttpWebRequest webRequest = CreateRequest(config.ApiKey, config.ApiSecret, config.Token, config.Url, payloadString);

            string responseString;
            try {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) {
                    using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream())) {
                        responseString = responseStream.ReadToEnd();
                    }

                    //TODO: handle/process success
                }
            } catch (WebException ex) {
                if (ex.Response != null) {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response) {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream())) {
                            string remoteEx = reader.ReadToEnd();
                            //TODO: handle/process failure. Until then, rethrow.
                            throw;
                        }
                    }
                }
            }
        }
    }
}