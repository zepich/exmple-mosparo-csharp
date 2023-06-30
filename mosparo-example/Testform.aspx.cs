using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using System.Threading.Tasks;

namespace mosparo_example
{
    public partial class Testform : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                StringBuilder output = new StringBuilder();

                // Get the form data
                NameValueCollection form = Request.Form;
                NameValueCollection formData = new NameValueCollection();
                formData.Set("first-name", form.GetValues("first-name")[0]);
                formData.Set("last-name", form.GetValues("last-name")[0]);
                formData.Set("email-address", form.GetValues("email-address")[0]);
                formData.Set("website", form.GetValues("website")[0]);
                formData.Set("message", form.GetValues("message")[0]);

                // Get the submit and validation token
                String submitToken = form.GetValues("_mosparo_submitToken")[0];
                String validationToken = form.GetValues("_mosparo_validationToken")[0];

                // Build all the signatures, see for example here:
                // https://github.com/mosparo/php-api-client/blob/master/src/Client.php#L89
                NameValueCollection preparedFormData = PrepareFormData(formData);
                String formSignature = CreateFormDataHmacHash(preparedFormData);

                String validationSignature = GenerateHmacHash(validationToken);
                String verificationSignature = GenerateHmacHash(validationSignature + formSignature);

                String apiEndpoint = "/api/v1/verification/verify";
                Dictionary<string, object> requestDataForSignature = new Dictionary<string, object>();
                requestDataForSignature.Add("submitToken", submitToken);
                requestDataForSignature.Add("validationSignature", validationSignature);
                requestDataForSignature.Add("formSignature", formSignature);
                requestDataForSignature.Add("formData", preparedFormData.AllKeys.ToDictionary(k => k, k => preparedFormData[k]));

                string test = new JavaScriptSerializer().Serialize(requestDataForSignature);

                // Build the request signature
                // https://github.com/mosparo/php-api-client/blob/master/src/Client.php#L103
                string requestSignature = GenerateHmacHash(apiEndpoint + new JavaScriptSerializer().Serialize(requestDataForSignature));
                
                // We have to prepare the data twice for C# since the FormUrlEncodedContent expects an Dictionary<string, string> while above we have to
                // keep the formData variable as object, not as JSON string. Otherwise it would escape the formData JSON when we create the request
                // signature twice.
                Dictionary<string, string> requestData = new Dictionary<string, string>();
                requestData.Add("submitToken", submitToken);
                requestData.Add("validationSignature", validationSignature);
                requestData.Add("formSignature", formSignature);

                foreach (string key in preparedFormData.Keys)
                {
                    requestData.Add("formData[" + key + "]", preparedFormData[key]);
                }

                string mosparoHost = ConfigurationManager.AppSettings["mosparoHost"];
                string mosparoPublicKey = ConfigurationManager.AppSettings["mosparoPublicKey"];

                // Accept self-signed certificates for the development system
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                // Build the request
                HttpClient client = new HttpClient(handler);
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(mosparoHost + apiEndpoint),
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(requestData),
                };
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(mosparoPublicKey + ":" + requestSignature)));
                request.Headers.Add("Accept", "application/json");

                // Send the request and store the response in the label for debugging purpose only
                var task = Task.Run(() => client.SendAsync(request).ContinueWith((taskResponse) =>
                    {
                        return taskResponse.Result.Content.ReadAsStringAsync();
                    })
                );
                task.Wait();

                PageMessage.Text = "submit received; " + task.Result.Result;
            }
        }

        // https://github.com/mosparo/php-api-client/blob/master/src/RequestHelper.php#L60
        protected NameValueCollection PrepareFormData(NameValueCollection formData)
        {
            NameValueCollection preparedFormData = new NameValueCollection();

            foreach (string key in formData)
            {
                String value = formData[key];

                preparedFormData[key] = GenerateSha256Hash(value);
            }

            return preparedFormData;
        }

        // https://github.com/mosparo/php-api-client/blob/master/src/RequestHelper.php#L130
        protected String CreateFormDataHmacHash(NameValueCollection preparedFormData)
        {
            String jsonData = new JavaScriptSerializer().Serialize(preparedFormData.AllKeys.ToDictionary(k => k, k => preparedFormData[k]));

            return GenerateHmacHash(jsonData);
        }

        protected String GenerateHmacHash(String data)
        {
            string privateKey = ConfigurationManager.AppSettings["mosparoPrivateKey"];

            using (HMACSHA256 hmacHash = new HMACSHA256(Encoding.UTF8.GetBytes(privateKey)))
            {
                byte[] bytes = hmacHash.ComputeHash(Encoding.UTF8.GetBytes(data));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        static string GenerateSha256Hash(string data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}