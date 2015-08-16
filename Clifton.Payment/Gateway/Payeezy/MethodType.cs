using System.Collections.Generic;

namespace Clifton.Payment.Gateway {
    public partial class PayeezyGateway : BaseGateway {
        public enum MethodType {
            Unknown,
            CreditCard
        }

        protected Dictionary<MethodType, string> MethodTypeToString = new Dictionary<MethodType, string>() {
            { MethodType.CreditCard, "credit_card" }
        };

        protected Dictionary<string, MethodType> MethodTypeByString = new Dictionary<string, MethodType>() {
            { "credit_card", MethodType.CreditCard }
        };
    }
}
