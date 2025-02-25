param([SecureString] $password)
$fileName = "CoursePlanner"
$keyName = "$($fileName.ToLowerInvariant())_key"


dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore="$fileName.keystore" -p:AndroidSigningKeyAlias="$keyName" -p:AndroidSigningKeyPass="$password" -p:AndroidSigningStorePass="$password"