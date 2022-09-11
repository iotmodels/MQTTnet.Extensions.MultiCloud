# X509Certificates

`ConnectionSettings` allow to load client certificates to establish the TLS connection, these certificates must have the private key, sometimes protected with a password.

The `X509Key` setting allows 3 different options to load the certificate with the key.

## From a PFX file

`X509Key=pathTo.pfx|pfx-password` 

## From the Certificate Store

Load the certificate from the Certificate Store `CurrentUser\my` and locate by thumbprint

`X509Key=8E983707D3F802E6717BBCD193129946573F31D4`

> Note: the thumbprint must be 40 chars long

## From PEM/KEY files

Load the certificate from the PEM and KEY files

`X509Key=pathTo.pem|pathTo.key|key-password`

> Note: if the key does not have password, it can be omitted

### Converting PFX to PEM/Key

Using OpenSSL from Linux (in Windows can use WSL)

#### PFX to PEM/KEY

```bash
openssl pkcs12 -in onething.pfx -out onething.pem -nokeys
openssl pkcs12 -in onething.pfx -out onething.priv.key -nocerts # will prompt for new password
openssl rsa -in onething.priv.key -out onething.key # key without password !! use with caution
```

#### PEM/KEY to PFX

```bash
openssl pkcs12 -export -in onething.pem -inkey onething.key -out onething.pfx # will prompt for new password
```