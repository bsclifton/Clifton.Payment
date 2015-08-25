# Clifton.Payment
Code relating to payment processing or point of sale

## Check / Credit Card validation
Very basic check and credit card validation is provided. Keep in mind, web sites are better off using JavaScript to validate client side (rather than hitting the server to do validation).

Please note: this code is currently geared towards e-Commerce in the United States (but other payment logic is very welcome- submit a pull request!)

## Supported payment gateways

Each gateway is intended to process payments over the internet. These are providers where you can setup a merchant account and send over credits, debits, refunds, etc.

### First Data (Payeezy Gateway)
This integration is for the [newer RESTful API](https://developer.payeezy.com/docs-sandbox).

#### Getting started

You can run test transactions with the [values used in the unit tests](https://github.com/clifton-io/Clifton.Payment/blob/02c1b4c18ea90cffb03d37e4df4dacdb6c55e62e/Clifton.Payment.Tests/Gateway/PayeezyGatewayTests.cs#L17) (I've captured the values from the demo page). But you'll likely want to create your own account; here's how you get started:

1. You'll want to sign up for an account (if you don't already have one). Visit the developer home page [here](https://developer.payeezy.com/) and click "Create Account" in the top right of the site.
2. Follow the instructions to create, verify, and login to your new account.
3. Create a new app. It'll ask if you want this app to be a sandbox and I'd recommend saying yes (until your integration is perfect).
4. Once your app is created, you can get the API key and API secret by opening the details for your new app and going to the "Keys" tab.
5. The merchant token can be obtained by clicking "My Merchants" in the top right of the site. Grab the value in the token field (likely it's the demo account, Acme Sock).

At this point, you're ready add a reference to the Clifton.Payment assembly and start using it.

```csharp
using Clifton.Payment.Gateway;

public class Demo {
  public function Charge() {
    PayeezyGateway gw = new PayeezyGateway(
        apiKey: "y6pWAJNyJyjGv66IsVuWnklkKUPFbb0a",
        apiSecret: "86fbae7030253af3cd15faef2a1f4b67353e41fb6799f576b5093ae52901e6f7",
        token: "fdoa-a480ce8951daa73262734cf102641994c1e55e7cdf4c02b6",
        environment: PayeezyEnvironment.Certification
    );
    var response = gw.CreditCardPurchase("4111111111111111", "01", "20", ...);
  }
}

```

#### Notes about the older SOAP API
*Please note that I've removed the SOAP integration from this repo since it's an older (and less flexible) solution.*

You can find the old integration [here](https://github.com/clifton-io/Clifton.Payment/blob/6ef2733171e9ce54281de5f5e9c4e32a003a6ef2/Clifton.Payment/Gateway/PayeezyGateway.cs), but there are a few reasons why I don't think you should use it (and you should use the RESTful version instead).

1. The [developers have stated](https://developer.payeezy.com/content/preferred-integration-first-data) that the RESTful version is intended to be the new version. This means new features will be added to it.
2. The older SOAP library only supports US merchants while the RESTful version supports US/UK and more countries/regions coming soon.
3. There are [way more examples available](https://github.com/payeezy/payeezy_direct_API) which show how to integrate with the RESTful APIs (in several programming languages).

# License
Clifton.Payment is released under the MIT License. See the [bundled LICENSE](https://github.com/clifton-io/Clifton.Payment/blob/master/LICENSE) file for details.