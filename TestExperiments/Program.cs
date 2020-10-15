using System;
using QuantConnect.Util;
using QuantConnect.Orders.Serialization;
using QuantConnect.Orders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace TestExperiments { 


        class Program
        {


            private static string GenerateHashTag()
            {
                Guid guid = Guid.NewGuid();
                string hashtag = guid.ToString();
                return hashtag;
            }

            static void Main(string[] args)
            {

            //Console.WriteLine(GenerateHashTag());

            //Console.Read();

            var order = new MarketOrder();


            var list = new List<Order>
            {
                order
            };

            var js = new SerializedIEnumerableOrderJsonConverter();
            //var js = new SerializedOrderJsonConverter();
            var serializer = new JsonSerializer();

            //StringBuilder sb = new StringBuilder();

            //StringWriter sw = new StringWriter(sb);
            //JsonWriter jsonWriter = new JsonTextWriter(sw);

            JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(File.Open(@"s.json", FileMode.Create)));

            js.WriteJson(jsonWriter, list, serializer);
            jsonWriter.Flush();
            jsonWriter.Close();

            JsonReader jsonReader = new JsonTextReader(new StreamReader(File.Open(@"s.json", FileMode.Open, FileAccess.Read)));
            var res = js.ReadJson(jsonReader, typeof(IEnumerable<Order>), list, serializer);


            Console.WriteLine(res.ToString());



            

            //Console.WriteLine(sb);

            //js.ReadJson()





            Console.ReadKey();

        }

        }
}