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

        internal enum TransactionType {
            authorize,
            purchase
        }

        internal enum MethodType {
            credit_card
        }

        #endregion

        #region Properties

        protected override string ContentType {
            get { return "application/json"; }
        }

        #endregion

        private string GenerateHmac(string apiKey, string apiSecret, string token, int nonce, string currentTimestamp, string payload) {
            string message = apiKey + nonce.ToString() + currentTimestamp + token + payload;
            HMAC hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            byte[] hmacData = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hmacData);
        }

        /// <summary>
        /// Generates a cryptographically strong random number.
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Cryptographic_nonce"/>
        private int GetNonce() {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(bytes);
                }
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        private string GetEpochTimestampInMilliseconds() {
            long millisecondsSinceEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return millisecondsSinceEpoch.ToString();
        }

        private HttpWebRequest CreateRequest(string apiKey, string apiSecret, string token, string url, string payloadString) {
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

        /// <see cref="https://developer.payeezy.com/payeezy_new_docs/apis/post/transactions%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20%20-1"/>
        public override void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string referenceNumber) {
            int parsedExpirationMonth, parsedExpirationYear;

            CreditCardType cardType = base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationMonth, out parsedExpirationYear);

            dynamic payload = new {
                merchant_ref = referenceNumber,
                transaction_type = TransactionType.purchase.ToString(),
                method = MethodType.credit_card.ToString(),
                amount = dollarAmount,//TODO: Processed Amount in cents
                currency_code = "USD",
                credit_card = new {
                    type = CardTypeLookup[cardType],
                    cardholder_name = cardHoldersName,
                    card_number = cardNumber,
                    exp_date = string.Format("{0:00}{1:00}", parsedExpirationMonth, parsedExpirationYear),
                    cvv = "123"
                }
            };

            //TODO: put in config. These values are from the demo page.
            string sandboxUrl = "https://api-cert.payeezy.com/v1/transactions";
            string apiKey = "y6pWAJNyJyjGv66IsVuWnklkKUPFbb0a";
            string apiSecret = "86fbae7030253af3cd15faef2a1f4b67353e41fb6799f576b5093ae52901e6f7";
            string token = "fdoa-a480ce8951daa73262734cf102641994c1e55e7cdf4c02b6";
            /////////////////////

            string payloadString = JsonConvert.SerializeObject(payload);
            HttpWebRequest webRequest = CreateRequest(apiKey, apiSecret, token, sandboxUrl, payloadString);

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