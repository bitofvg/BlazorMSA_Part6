using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Security.Cryptography;
using System.IO;

//doc: https://bitofvg.wordpress.com/2021/01/29/identity-server-4-self-signed-certificates/

namespace IdServer {
  public static class SelfSignedServerCertificate {

    public class NewCertificate {
      public string Password { get; set; } //MyComplexAndSecretPassword
      public string CertificateName { get; set; } //BlazorMSA.IdServer.Debug 
      public string SubjectName { get; set; } //IISServer.Mydomain.net
      public IEnumerable<string> DnsNames { get; set; } //[ "IISServer.Mydomain.net" ]
      public IEnumerable<string> IpAddresses { get; set; } //[],
      public int ValidityYears { get; set; } //10   
    }


    public static byte[] Create(NewCertificate newCert) {

      SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();

      if (newCert.IpAddresses != null)
        foreach (var IpAddr in newCert.IpAddresses) {
          sanBuilder.AddIpAddress(IPAddress.Parse(IpAddr));
        }
      foreach (var DnsName in newCert.DnsNames) {
        sanBuilder.AddDnsName(DnsName);
      }

      X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={newCert.SubjectName}");

      using (RSA rsa = RSA.Create(2048)) {
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
              X509KeyUsageFlags.DataEncipherment | 
              X509KeyUsageFlags.KeyEncipherment | 
              X509KeyUsageFlags.DigitalSignature, 
              false));


        request.CertificateExtensions.Add(
           new X509EnhancedKeyUsageExtension(
               new OidCollection {
                  // TLS Server Authentication
                  new Oid("1.3.6.1.5.5.7.3.1"),  
                  // TLS Client Authentication (just to test with IIS, NOT necessary for Identity Server)
                  new Oid("1.3.6.1.5.5.7.3.2"),  
               }
               , false));

        request.CertificateExtensions.Add(sanBuilder.Build());
        var notBefore = new DateTimeOffset(DateTime.UtcNow.AddDays(-1));
        var notAfter = new DateTimeOffset(DateTime.UtcNow.AddYears(newCert.ValidityYears));
        var certificate = request.CreateSelfSigned(notBefore, notAfter);
        certificate.FriendlyName = newCert.CertificateName;

        return certificate.Export(X509ContentType.Pfx, newCert.Password);

      }

    }




  }
}
