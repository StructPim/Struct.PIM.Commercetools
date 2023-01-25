# Setup

## appsettings.conf

- IncludeProductStructureAliases  
Specify a list of PIM product structures aliases that should be available in Commerce Tools. To include all attributes set the value to  
<code>"IncludeProductStructureAliases": ["all"]</code>

- ImportOptions
  - RollBackOnFailure: in case of a failure while importing a rollback will be performed
  - AllowCleanCommerce: enable the user to clean Commerce Tools from the API

- XApiKey:   
API key
- Struct
  - BaseUrl:  
  The URL to Struct API
  - ApiKey:  
  The apikey to be used for the Struct API
  Can be setup / found in Struct Backend -> Settings -> Api configuration
- Client (https://githubhelp.com/commercetools/commercetools-dotnet-core-sdk)
  - ClientId:  
  Commerce client id.  
  Can be setup / found in Commerce Tools -> Settings -> Developer Settings
  - ClientSecret:  
  Commerce client secret.  
  Can be setup / found in Commerce Tools -> Settings -> Developer Settings
  - ProjectKey:   
  Commerce project key  
  https://mc.europe-west1.gcp.commercetools.com/account/projects
  - AuthorizationBaseAddress:  
  https://docs.commercetools.com/api/authorization
  - ApiBaseAddress:  
  https://api.europe-west1.gcp.commercetools.com/

# Limitations
- A product can not have more than 100 variants
- Attribute can't have the same value in a different variant
- Product structure change is not possible
