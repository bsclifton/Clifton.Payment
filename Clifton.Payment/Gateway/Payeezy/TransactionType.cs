using System.Collections.Generic;

namespace Clifton.Payment.Gateway {
    public partial class PayeezyGateway : BaseGateway {
        public enum TransactionType {
            Unknown,
            Authorize,
            Purchase,
            Void,
            Capture,
            Split,
            Refund
        }

        protected Dictionary<TransactionType, string> TransactionTypeToString = new Dictionary<TransactionType, string>() {
            { TransactionType.Authorize, "authorize" },
            { TransactionType.Purchase, "purchase" },
            { TransactionType.Void, "void" },
            { TransactionType.Capture, "capture" },
            { TransactionType.Split, "split" },
            { TransactionType.Refund, "refund" }
        };

        protected Dictionary<string, TransactionType> TransactionTypeByString = new Dictionary<string, TransactionType>() {
            { "authorize", TransactionType.Authorize },
            { "purchase", TransactionType.Purchase },
            { "void", TransactionType.Void },
            { "capture", TransactionType.Capture },
            { "split", TransactionType.Split },
            { "refund", TransactionType.Refund }
        };
    }
}
