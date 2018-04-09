## Configurate GlobalConfiguration. 
```csharp
// In Application_Start
// Those two setting are for DataEnode Attribute
// Set the key size for dataEncoding 128 or 256.
// Default is 128
EntityWorker.Core.GlobalConfiguration.DataEncode_Key_Size = DataCipherKeySize.Key_128;

// Set the secret key for encoding. 
// Default is "EntityWorker.Default.Key.Pass"
EntityWorker.Core.GlobalConfiguration.DataEncode_Key = "the key used to Encode the data";

/// <summary>
/// The Default Value for package encryption
/// </summary>
EntityWorker.Core.GlobalConfiguration.PackageDataEncode_Key = "packageSecurityKey";


// Last set the culture for converting the data eg DateTime, decimal.
EntityWorker.Core.GlobalConfiguration.CultureInfo = new CultureInfo("en");
```
