# Clifton.Payment
Code relating to payment processing or point of sale

## Check / Credit Card validation
Very basic check and credit card validation is provided. Keep in mind, web sites are better off using JavaScript to validate client side (rather than hitting the server to do validation).

## Supported payment gateways

### First Data (Payeezy Gateway)
Documentation about the API can be [found here](https://support.payeezy.com/hc/en-us/articles/204029989-First-Data-Payeezy-Gateway-Web-Service-API-Reference-Guide-)

This has the following configurable parameters (via the app.config):
* keyId
* hmacKey
* id
* password

You'll have to [sign up for a demo account](https://support.payeezy.com/hc/en-us/articles/203730579-Global-Gateway-e4-Demo-Accounts) to get those values.
Once logged in, you can go to the Administration tab (top right) and then pick "Terminals". Find the eCommerce terminal and click it for details. You'll find the id (Gateway ID) and password on the
default screen ("details"). You can find the Key id / Hmac key on the "API Access" screen.