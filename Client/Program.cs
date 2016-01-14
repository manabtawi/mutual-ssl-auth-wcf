using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace Client
{
    class Program
    {
        static private string _EndpointURL = ConfigurationSettings.AppSettings["endpointURL"];
        static private string _ServiceCertSerial = ConfigurationSettings.AppSettings["serviceCertificate"];
        static private string _ClientCertSerial = ConfigurationSettings.AppSettings["serviceCertificate"];

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
    }
}
