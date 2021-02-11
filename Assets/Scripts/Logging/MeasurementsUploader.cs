using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MeasurementsUploader : MonoBehaviour
{
    public string host;
    public int port;

    /// <summary>
    /// The experiment that this build is for. This is not the participant/trial ID - experimentid determines where the log files are written.
    /// </summary>
    public string experimentid;

    /// <summary>
    /// The folder in which the experimental data will be written.
    /// </summary>
    public string user;

    public void Send<T>(T serialisable)
    {
        var json = JsonUtility.ToJson(serialisable);
        StartCoroutine(PostRequest(json));
    }

    private IEnumerator PostRequest(string json)
    {
        UriBuilder uriBuilder = new UriBuilder();
        uriBuilder.Scheme = "https";
        uriBuilder.Host = host;
        uriBuilder.Port = port;

        // it is important to create new handlers for each request, as old handlers (even the certificate handler) are disposed of, and will fail if re-used.

        var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        var downloadHandler = new DownloadHandlerBuffer();
        var certificateHandler = new PrivateCertificateHandler();

        var req = new UnityWebRequest(uriBuilder.Uri, "POST");
        req.uploadHandler = uploadHandler;
        req.downloadHandler = downloadHandler;
        req.certificateHandler = certificateHandler;
        req.SetRequestHeader("content-type", "application/json");
        req.SetRequestHeader("x-key", "placeholder");
        req.SetRequestHeader("x-id", experimentid);
        req.SetRequestHeader("x-user", user);

        yield return req.SendWebRequest();

        if(req.isNetworkError)
        {
            Debug.LogError("Measurements upload failed. Network error " + req.error);
        }
        else
        {
            switch (req.responseCode)
            {
                case 200:
                    Debug.Log("Measurements Uploaded");
                    break;
                default:
                    Debug.LogError(string.Format("{0} {1} {2} {3}",
                        "Measurements upload failed with server error",
                        req.responseCode.ToString(), 
                        req.error,
                        downloadHandler.text));
                    break;
            }          
        }
    }

    private class PrivateCertificateHandler : CertificateHandler
    {
        /// <summary>
        /// This public key is produced for vrgroupdatacollection.cs.ucl.ac.uk. To work with a different server, 
        /// use OpenSSL to generate a new certificate and replace this string in your application.
        /// </summary>
        private static string PUB_KEY = "3082010A0282010100F6C8211C70270E72AD033647" +
            "96718909C8B6690B95BBC61FEBF3705390BA6A73EB6D521627BB5D085C9DC11FF36850" +
            "1B588ED40E70549B03E091D6AB42D71ED8ECBD554E9CADBAC35727D9A04F690C87C9DC" +
            "AD30FE2DA0FD0A17A7C63B10050DF43A7C907A76E72A4761C82F550AF9509F50614923" +
            "40385E3F6628D97340E1A7A6D284931CBDF7B76FB884CFE99BA812804221022208B52F" +
            "F22BC8492886DD9F84C98D8D61E256AA46A7DA2FE12F7487DC314F7D7861C9D81C442F" +
            "E6939A31281E46C2D84244833A9DEAD51E92AA0A773FBC2B463FC071DBA78E121E4496" +
            "5BCDA4A8F9FBA6F23C52924F625B2537BA2978BC1CE58CCE6CD539987B4B177D6ED102" +
            "03010001";

        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 certificate = new X509Certificate2(certificateData);
            string pk = certificate.GetPublicKeyString();
            return pk.Equals(PUB_KEY);
        }
    }

    private class DummyCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // encryption with no authentication
        }
    }

}
