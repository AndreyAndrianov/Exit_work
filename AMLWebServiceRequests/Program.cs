using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AMLWebServiceRequests
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            InvokeRequestResponseService().Wait();
        }

        static async Task InvokeRequestResponseService()
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>()
                    {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"UserId", "DeviceId", "Amount"},
                                Values = new string[,] {
                                    { "eaeefda6-7b38-400c-861d-a7e140de5adc", "07acf9f0-ea80-4c93-ad7f-2f665f295238", "250"},
                                {"eaeefda6-7b38-400c-861d-a7e140de5adc", "07acf9f0-ea80-4c93-ad7f-2f665f295238", "500"},}
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "nUnszLU9YqhJPzt+EWRwrPCnGOKyqOzH0zZ3c5ztutcp9F/+eSyenWYFDq3pa60DkgM76k8CYFknJpSR8U6oBQ=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress =
                    new Uri(
                        "https://ussouthcentral.services.azureml.net/workspaces/26fe83c523d34bb1bd11a07f03586048/services/a65e89b16d9b4fcb8e93d03e40715a71/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);
                Console.WriteLine(response.RequestMessage);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    File.WriteAllText(@"F:\response.json", result);
                    Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
}
