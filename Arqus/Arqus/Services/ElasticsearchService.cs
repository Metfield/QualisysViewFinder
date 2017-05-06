using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Arqus.Services
{

    public class ElasticEvent
    {
        public string Name;
        public string Message { get; set; }
        public object Payload { get; set; }
        public DateTime Date { get; set; }

        public ElasticEvent()
        {
            Date = DateTime.UtcNow;
        }
    }

    static class ElasticsearchService
    {
        private static ElasticClient client;
        private static string index = "arqus";
        private static Uri node = new Uri("http://elastic.kreativet.org");
        private static ConnectionSettings settings = new ConnectionSettings(node)
            .DefaultIndex(index)
            .ConnectionLimit(-1);

        
        static ElasticsearchService()
        {
            client = new ElasticClient(settings);
        }

        public static void TrackEvent(ElasticEvent elasticEvent)
        {
            try
            {
                var result = client.Index(elasticEvent);

                if (result.IsValid)
                    Debug.WriteLine("Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private static void CreateIndex()
        {
            client.CreateIndex(index, index =>
                index.Settings(settings =>
                   settings.NumberOfShards(2)
                   .NumberOfReplicas(0))
                    );
        }

    }
}
