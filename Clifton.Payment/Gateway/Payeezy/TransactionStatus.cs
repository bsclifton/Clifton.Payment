using System.Collections.Generic;

namespace Clifton.Payment.Gateway {
    public partial class PayeezyGateway : BaseGateway {
        public enum TransactionStatus {
            Unknown,
            Approved,
            Declined,
            NotProcessed
        }

        protected Dictionary<string, TransactionStatus> TransactionStatusByString = new Dictionary<string, TransactionStatus>() {
            { "approved", TransactionStatus.Approved },
            { "declined", TransactionStatus.Declined },
            { "not processed", TransactionStatus.NotProcessed }
        };
    }
}
