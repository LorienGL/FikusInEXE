using FikusIn.Model.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Serializers
{
    public static class DocumentSerializer
    {
        public static string Serialize(Document document)
        {
            return JsonConvert.SerializeObject(document);
        }

        public static Document? Deserialize(string serializedData)
        {
            return JsonConvert.DeserializeObject<Document>(serializedData);
        }
    }
}
