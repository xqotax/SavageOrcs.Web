// See https://aka.ms/new-console-template for more information
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Security.Cryptography.X509Certificates;

namespace SavageOrcs.Services.Parser
{
    public class GoogleServiceFactory
    {
        public static BaseClientService CreateService(string keyPath, string botEmail, string applicationName, IEnumerable<string> scopes, GoogleServiceType googleService)
        {
            var certificate = new X509Certificate2(keyPath, "notasecret", X509KeyStorageFlags.Exportable);

            var credentials = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(botEmail)
               {
                   Scopes = scopes
               }.FromCertificate(certificate));

            return googleService switch
            {
                GoogleServiceType.None => throw new NotSupportedException(),
                GoogleServiceType.Sheet => new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = applicationName
                }),
                GoogleServiceType.Drive => new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = applicationName
                }),
                _ => throw new NotSupportedException(),
            };
        }
    }
} 