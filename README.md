# WCF Mutual SSL Authentication
A skeleton version to build a secured communication between client and WCF service using Mutual SSL Authentication.

[Read the article here](http://tech.manabtawi.com/2016/02/25/securing-wcf-service-using-mutual-ssl-authentication/)

## The Service
In *Web.Config* Within the `<system.serviceModel>` tag, the following the configuration is for the WCF service that will be triggered by the client. The configuration should include a trusted SSL certificate, and also to be provided to the client for handshaking process:

### Behavior Configuration
Make sure that the `httpsGetEnabled` is set to **true**, and to configure a **trusted SSL certificate**:
```
<!-- Behavior Configurations -->
<behaviors>
  <serviceBehaviors>
    <behavior name="ServiceBehavior">
      <serviceMetadata httpGetEnabled="true" httpsGetEnabled="true" />
      <serviceDebug includeExceptionDetailInFaults="false" />
      <serviceCredentials> 
        
        <!-- Set Service Certificate Serial Number, a trusted SSL certificate -->
        <serviceCertificate findValue="XXXXXXXXXX"
                            storeLocation="LocalMachine"
                            storeName="My"            
                            x509FindType="FindBySerialNumber" />                        
      </serviceCredentials>
    </behavior>
  </serviceBehaviors>
</behaviors>
```
### Binding Configuration
Binding configuration for `wsHttpBinding` is to set the **security mode** to **Transport** and **clientCredentialType** to **Certificate**:
```
<!-- Binding Configurations -->
<bindings>
  <wsHttpBinding>
    <binding name="InteropCertificateBinding"
               closeTimeout="00:01:00" openTimeout="00:01:00"
               receiveTimeout="00:10:00" sendTimeout="00:01:00" allowCookies="false"
               bypassProxyOnLocal="false" maxBufferPoolSize="2147483647"
               maxReceivedMessageSize="2147483647" messageEncoding="Text" textEncoding="utf-8"
               useDefaultWebProxy="true">
      <readerQuotas maxDepth="2147483647" maxStringContentLength="2147483647"
                    maxArrayLength="2147483647" maxBytesPerRead="2147483647"
                    maxNameTableCharCount="2147483647"/>
      <security mode="Transport">
        <transport clientCredentialType="Certificate"></transport>
      </security>
    </binding>
  </wsHttpBinding>
</bindings>
```
### Service Configuration
Define the **behavior configuration** and **endpoint address** and **binding** from the previous setup:
```
<!-- Service Configurations -->
<services>
  <service behaviorConfiguration="ServiceBehavior"
           name="MutualSSLAuthWCF.Service"> 
    
    <!-- Endpoint always Https with a trusted SSL certificate -->
    <endpoint address="https://www.example.com/service.svc"
                  binding="wsHttpBinding"
                  bindingConfiguration="InteropCertificateBinding"
                  name="WSHttpBinding_IService"
                  contract="MutualSSLAuthWCF.IService" />
  </service>
</services>
```

## The Client
The WCF service will be triggered from a C# code by defining the same **binding configuration** and both **client and service certificates**. The service certificate should the same certificate configured in the WCF service.
```
WSHttpBinding communicationBinding = new WSHttpBinding();
communicationBinding.Security.Mode = SecurityMode.Transport;
communicationBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
```
### Required libraries
```
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
```
### The Program using Console Application
```
class Program
    {
        static private string _EndpointURL = ConfigurationSettings.AppSettings["endpointURL"];
        static private string _ServiceCertSerial = ConfigurationSettings.AppSettings["serviceCertificate"];
        static private string _ClientCertSerial = ConfigurationSettings.AppSettings["clientCertificate"];

        static void Main(string[] args)
        {
            try
            {
                // Binding Configurations
                WSHttpBinding communicationBinding = new WSHttpBinding();
                communicationBinding.Security.Mode = SecurityMode.Transport;
                communicationBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                CryptographyHelper cryptographyHelper = new CryptographyHelper();
                X509Certificate2 serviceCertificate = cryptographyHelper.GetX509CertificateBySerialNumber(_ServiceCertSerial, StoreName.My, StoreLocation.LocalMachine);
                X509Certificate2 clientCertificate = cryptographyHelper.GetX509CertificateBySerialNumber(_ClientCertSerial);//from configiration

                // Create the endpoint address. 
                EndpointAddress endpointAddress = new EndpointAddress(new Uri(_EndpointURL), EndpointIdentity.CreateX509CertificateIdentity(serviceCertificate));

                // Call the client service
                // Client service class can generated from the WSDL file
                ServiceReference1.ServiceClient ws = new ServiceReference1.ServiceClient(communicationBinding, endpointAddress);

                // Specify a certificate to use for authenticating the client.
                ws.ClientCredentials.ClientCertificate.SetCertificate(
                    StoreLocation.LocalMachine,
                    StoreName.My,
                    X509FindType.FindBySerialNumber,
                    clientCertificate.SerialNumber);

                // Specify a default certificate for the service.
                ws.ClientCredentials.ServiceCertificate.SetDefaultCertificate(
                    StoreLocation.LocalMachine,
                    StoreName.My,
                    X509FindType.FindBySerialNumber,
                    serviceCertificate.SerialNumber);

                // Call the service
                ws.DoWork();
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
```


