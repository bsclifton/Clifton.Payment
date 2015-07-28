# Clifton.Payment
.NET/C# code relating to payment processing or point of sale

## Check / Credit Card validation
Very basic check and credit card validation is provided. Keep in mind, web sites are better off using JavaScript to validate client side (rather than hitting the server to do validation).

Please note: this code is geared towards e-Commerce in the United States (other payment logic is very welcome- submit a pull request!)

## Supported payment gateways

Each gateway is intended to process payments over the internet. These are providers where you can setup a merchant account and send over credits, debits, refunds, etc.

### First Data (Payeezy Gateway)
This integration is for the [newer RESTful API](https://developer.payeezy.com/docs-sandbox).

#### Getting started
You'll want to start off by adding a reference to this assembly and creating an app.config/web.config for your project. You can use the included app.config as an example.

You can already test transactions with the values currently in the app.config (I've captured the values from the demo page). But you'll likely want to create your own account; here's how you get started:
1. You'll want to sign up for an account (if you don't already have one). Visit the developer home page [here](https://developer.payeezy.com/) and click "Create Account" in the top right of the site.
2. Follow the instructions to create, verify, and login to your new account.
3. Create a new app. It'll ask if you want this app to be a sandbox and I'd recommend saying yes (until your integration is perfect).
4. Once your app is created, you can get the API key and API secret by opening the details for your new app and going to the "Keys" tab. Store those in your app.config.
5. The merchant token can be obtained by clicking "My Merchants" in the top right of the site. Grab the value in the token field (likely it's the demo account, Acme Sock) and store this in the app.config.

At this point, you're ready to go. Enjoy!

#### Notes about the older SOAP API (and some of the differences)
*Please note that I've removed the SOAP integration from this repo since it's an older (and less flexible) solution.*

You can find the old integration [here](https://github.com/clifton-io/Clifton.Payment/blob/6ef2733171e9ce54281de5f5e9c4e32a003a6ef2/Clifton.Payment/Gateway/PayeezyGateway.cs), but there are a few reasons why I don't think you should use it (and you should use the RESTful version instead).
1. The developers have stated that the RESTful version is intended to be the new version. This means new features will be added to it.
2. The older SOAP library only supports US merchants while the RESTful version supports US/UK and more countries coming soon.
3. There are way more examples available which show how to integrate with the RESTful APIs.

If you're still interested, the documentation about the older SOAP API can be [found here](https://support.payeezy.com/hc/en-us/articles/204029989-First-Data-Payeezy-Gateway-Web-Service-API-Reference-Guide-).

The old SOAP integration I put together had the following configurable parameters:
* keyId
* hmacKey
* id
* password

You'll have to [sign up for a demo account](https://support.payeezy.com/hc/en-us/articles/203730579-Global-Gateway-e4-Demo-Accounts) to get those values.
Once logged in, you can go to the Administration tab (top right) and then pick "Terminals". Find the eCommerce terminal and click it for details. You'll find the id (Gateway ID) and password on the
default screen ("details"). You can find the Key id / Hmac key on the "API Access" screen.