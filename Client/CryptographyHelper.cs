using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class CryptographyHelper
    {
        public X509Certificate2 GetX509CertificateBySerialNumber(string serialNumber, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store certificateStore = new X509Store(storeName, storeLocation);
            certificateStore.Open(OpenFlags.ReadOnly);
            X509Certificate2 certificate;

            try
            {
                certificate = certificateStore.Certificates.OfType<X509Certificate2>().
                    FirstOrDefault(cert => cert.SerialNumber.ToLower().Contains(serialNumber.ToLower()));
            }
            finally
            {
                certificateStore.Close();
            }

            if (certificate == null)
                throw new Exception(String.Format("Certificate '{0}' not found.", serialNumber));

            return certificate;
        }

        public X509Certificate2 GetX509CertificateBySerialNumber(string serialNumber)
        {
            return GetX509CertificateBySerialNumber(serialNumber, StoreName.My, StoreLocation.LocalMachine);
        }
    }
}
