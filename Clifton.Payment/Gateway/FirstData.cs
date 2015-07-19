using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Clifton.Payment.Gateway {
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://support.payeezy.com/hc/en-us/articles/204029989-First-Data-Payeezy-Gateway-Web-Service-API-Reference-Guide-"/>
    public class FirstData : BaseGateway {
        #region Members

        protected enum TransactionType {
            Purchase,
            PreAuthorization,
            PreAuthorizationCompletion,
            ForcedPost,
            Refund,
            PreAuthorizationOnly,
            PayPalOrder,
            Void,
            TaggedPreAuthorizationCompletion,
            TaggedVoid,
            TaggedRefund,
            CashOut, /// (ValueLink, v9 or higher end point only)
            Activation, /// (ValueLink, v9 or higher end point only)
            BalanceInquiry, /// (ValueLink, v9 or higher end point only)
            Reload, /// (ValueLink, v9 or higher end point only)
            Deactivation /// (ValueLink, v9 or higher end point only)
        }

        protected Dictionary<TransactionType, string> TransactionTypeLookup = new Dictionary<TransactionType, string>() {
            { TransactionType.Purchase, "00" },
            { TransactionType.PreAuthorization, "01" },
            { TransactionType.PreAuthorizationCompletion, "02" },
            { TransactionType.ForcedPost, "03" },
            { TransactionType.Refund, "04" },
            { TransactionType.PreAuthorizationOnly, "05" },
            { TransactionType.PayPalOrder, "07" },
            { TransactionType.Void, "13" },
            { TransactionType.TaggedPreAuthorizationCompletion, "32" },
            { TransactionType.TaggedVoid, "33" },
            { TransactionType.TaggedRefund, "34" },
            { TransactionType.CashOut, "83" },
            { TransactionType.Activation, "85" },
            { TransactionType.BalanceInquiry, "86" },
            { TransactionType.Reload, "88" },
            { TransactionType.Deactivation, "89" }
        };

        #endregion

        #region Properties

        private string ContentType {
            get { return "application/xml"; }
        }

        private string Uri {
            get { return "/transaction/v19"; }
        }

        #endregion

        private string GetPayloadHash(string payload) {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] payloadBytes = encoder.GetBytes(payload);
            SHA1CryptoServiceProvider sha1Crypto = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(sha1Crypto.ComputeHash(payloadBytes)).Replace("-", "");
            return hash.ToLower();
        }

        private HttpWebRequest CreateRequest(string keyId, string key, string baseUrl, string xmlString, string hashedContent) {
            string method = "POST\n";
            string time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string hashData = method + ContentType + "\n" + hashedContent + "\n" + time + "\n" + Uri;
            HMAC hmacSha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] hmacData = hmacSha1.ComputeHash(Encoding.UTF8.GetBytes(hashData));
            string base64Hash = Convert.ToBase64String(hmacData);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(baseUrl + Uri);
            webRequest.Method = "POST";
            webRequest.ContentType = ContentType;
            webRequest.Accept = "*/*";
            webRequest.Headers.Add("x-gge4-date", time);
            webRequest.Headers.Add("x-gge4-content-sha1", hashedContent);
            webRequest.Headers.Add("Authorization", "GGE4_API " + keyId + ":" + base64Hash);
            webRequest.ContentLength = xmlString.Length;

            // write and send request data
            using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream())) {
                streamWriter.Write(xmlString);
            }

            return webRequest;
        }

        public override void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName) {
            int parsedExpirationMonth, parsedExpirationYear;

            base.ValidateCreditCard(cardNumber, expirationMonth, expirationYear, out parsedExpirationMonth, out parsedExpirationYear);

            var config = Clifton.Payment.Configuration.GetConfig().Gateways["FirstData"];

            StringBuilder stringBuilder = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(stringBuilder)) {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter)) {
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteStartElement("Transaction");
                    xmlWriter.WriteElementString("ExactID", config.Id);
                    xmlWriter.WriteElementString("Password", config.Password);
                    xmlWriter.WriteElementString("Transaction_Type", TransactionTypeLookup[TransactionType.Purchase]);
                    xmlWriter.WriteElementString("DollarAmount", dollarAmount);
                    xmlWriter.WriteElementString("Expiry_Date", string.Format("{0:00}{1:00}", parsedExpirationMonth, parsedExpirationYear));
                    xmlWriter.WriteElementString("CardHoldersName", cardHoldersName);
                    xmlWriter.WriteElementString("Card_Number", cardNumber);
                    xmlWriter.WriteEndElement();
                }
            }
            string xmlString = stringBuilder.ToString();
            string hashedContent = GetPayloadHash(xmlString);
            HttpWebRequest webRequest = CreateRequest(config.KeyId, config.HmacKey, config.Url, xmlString, hashedContent);

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