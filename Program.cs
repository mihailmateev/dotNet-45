using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace haengematte
{
    class Program
    {
        static void Main(string[] args)
        {
            //read config data from a command line parameters
            var user = args[0];
            var password = args[1];
            var database = args[2];

            //base request builder for all requests containing user name and database name
            var handler = new HttpClientHandler { Credentials = new NetworkCredential(user, password) };

            //create an http client
            using (var client = CreateHttpClient(handler, user, database))
            {
                
                var creationResponse = Create(client, new {name = "john", age = 15});
                PrintResponse(creationResponse);

                var id = GetString("id", creationResponse);
                var readResponse = Read(client, id);
                PrintResponse(readResponse);

                var returnedObj = GetString(readResponse);
                //Console.WriteLine(returnedObj);
                PrintDocument(returnedObj);
                readResponse = Read(client, id);

                var rev1 = GetString("_rev", readResponse);
                var updateResponse = Update(client, id, new {name = "john", age = 36, _rev = rev1});
                PrintResponse(updateResponse);
                
                var rev2 = GetString("rev", updateResponse); // note that an update produces a "rev" in the response rather than "_rev"
                var deleteResponse = Delete(client, id, rev2);
                PrintResponse(deleteResponse); 

                //unfinite loop until key is pressed
                Console.WriteLine("Press ESC to stop");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        //var sensor = Sensor.Generate();
                        Sensor sensor = Sensor.Generate();
                        //var creationSensorResponse = Create(client, new { hmdt = sensor.hmdt, temp = sensor.temp, dspl = sensor.dspl, time = sensor.time});
                        //var creationSensorResponse = Create(client, JsonConvert.SerializeObject(sensor));
                        var creationSensorResponse = Create(client, sensor);
                        PrintResponse(creationSensorResponse);

                        var _id = GetString("id", creationSensorResponse);
                        var readSensorResponse = Read(client, _id);
                        PrintResponse(readSensorResponse);

                        var returnedSensorObj = GetString(readSensorResponse);
                        PrintDocument(returnedSensorObj);
                        //// Do something

                        readSensorResponse = Read(client, _id);
                        rev1 = GetString("_rev", readSensorResponse);
                        var updateSensorResponse = Update(client, _id, new { time = sensor.time, dspl = sensor.dspl, temp = sensor.temp, hmdt = sensor.hmdt , _rev = rev1 });
                        //var updateSensorResponse = Update(client, id, JsonConvert.SerializeObject(Sensor.Update(sensor, rev1)));
                        PrintResponse(updateSensorResponse);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
        }

        private static HttpResponseMessage Create(HttpClient client, object doc)
        {
            var json = JsonConvert.SerializeObject(doc, Formatting.None);
            //var json = JObject.FromObject(doc);
            return client.PostAsync("", new StringContent(json, Encoding.UTF8, "application/json")).Result;
            //return client.PostAsync("", new StringContent(json.ToString(), Encoding.UTF8, "application/json")).Result;
        }

        private static HttpResponseMessage Read(HttpClient client, string id)
        {
            return client.GetAsync(id).Result;
        }

        private static HttpResponseMessage Update(HttpClient client, string id, object doc)
        {
            var json = JsonConvert.SerializeObject(doc, Formatting.None);
            return client.PutAsync(id, new StringContent(json, Encoding.UTF8, "application/json")).Result;
        }

        private static HttpResponseMessage Delete(HttpClient client, string id, string rev)
        {
            return client.DeleteAsync(id + "?rev=" + rev).Result;
            //return new HttpResponseMessage();
        }

        private static HttpClient CreateHttpClient(HttpClientHandler handler, string user, string database)
        {
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(string.Format("https://{0}.cloudant.com/{1}/", user, database))
            };
        }

        private static void PrintResponse(HttpResponseMessage response)
        {
            Console.WriteLine("Status code: {0}", response.StatusCode);
            Console.WriteLine(Convert.ToString(response));
        }

        private static void PrintDocument(string doc)
        {
            Console.WriteLine("Print document:");
            Console.WriteLine(doc);
        }

        private static string GetString(string propertyName, HttpResponseMessage creationResponse)
        {
            using (var streamReader = new StreamReader(creationResponse.Content.ReadAsStreamAsync().Result))
            {
                var responseContent = (JObject) JToken.ReadFrom(new JsonTextReader(streamReader));
                return responseContent[propertyName].Value<string>();
            }
        }

        private static string GetString(HttpResponseMessage creationResponse)
        {
            using (var streamReader = new StreamReader(creationResponse.Content.ReadAsStreamAsync().Result))
            {
                var responseContent = (JObject)JToken.ReadFrom(new JsonTextReader(streamReader));
                return responseContent.ToString();
            }
        }
    }
}
