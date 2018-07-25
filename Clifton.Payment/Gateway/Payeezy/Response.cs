using System.Collections.Generic;

namespace Clifton.Payment.Gateway {
    public partial class PayeezyGateway : BaseGateway {
        public class Response {
            /// <summary>Inputted transaction method.</summary>
            public string MethodType { get; set; }

            /// <summary>MethodType as an enum.</summary>
            public MethodType ParsedMethodType { get; set; }

            /// <summary>Processed Amount in cents.</summary>
            public string Amount { get; set; }

            /// <summary>ISO 4217 currency code.</summary>
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

            /// <summary>Approved = Card Approved , Declined = Gateway declined, Not Processed = For any internal errors.</summary>
            public string TransactionStatus { get; set; }

			public string Status { get; set; }

            /// <summary>TransactionStatus as an enum.</summary>
            public TransactionStatus ParsedTransactionStatus { get; set; }

            /// <summary>values - success / failure. Input validation errors encountered if status returned is failure.</summary>
            public string ValidationStatus { get; set; }

            /// <summary>The transaction_type provided in request.</summary>
            public string TransactionType { get; set; }

            /// <summary>TransactionType as an enum.</summary>
            public TransactionType ParsedTransactionType { get; set; }

            /// <summary>Needed as part of the url to process secondary transactions like capture/void/refund/recurring/split-shipment.</summary>
            public string TransactionId { get; set; }

            /// <summary>Needed as part of the payload to process secondary transactions like capture/void/refund/recurring/split-shipment.</summary>
            public string TransactionTag { get; set; }

            /// <summary>Standardized response code from the card issuing bank.</summary>
            public string BankResponseCode { get; set; }

            /// <summary>BankResponseCode as an enum.</summary>
            public BankResponseCode ParsedBankResponseCode { get; set; }

            /// <summary>Description / translation for the 3 digit bank response code.</summary>
            public string BankMessage { get; set; }

            /// <summary>Indicates the status of a transaction as it is sent to the financial institution and returned to the client.</summary>
            public string GatewayResponseCode { get; set; }

            /// <summary>GatewayResponseCode as an enum.</summary>
            public GatewayResponseCode ParsedGatewayResponseCode { get; set; }

            /// <summary>Description / translation for the gateway response code.</summary>
            public string GatewayMessage { get; set; }

            /// <summary>Payeezy Internal log id.</summary>
            public string CorrelationId { get; set; }

            public sealed class ErrorMessage {
                public string Code { get; set; }
                public string Description { get; set; }
            }

			//Token fields
			public string type { get; set; }
			public string cardholderName { get; set; }
			public string expDate { get; set; }
			public string tokenStr { get; set; }

			/// <summary>Objects with error codes and description.</summary>
			public List<ErrorMessage> ErrorMessages { get; set; }

        }
    }
}