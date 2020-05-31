using System.Data;
using System.Data.SqlClient;

namespace Transport
{
    public class DBO
    {
        public class CardReader
        {
            public string SEQ { get; set; }
            public string DistrictNo { get; set; }
            public string Route { get; set; }
            public string Status { get; set; }
            public string TerminalID { get; set; }
            public string IPAddress { get; set; }
            public string MacAddress { get; set; }
            public string SerialNo { get; set; }
            public string ModelName { get; set; }
            public string FirmwareVersion { get; set; }
            public string LastModify { get; set; }
            public string IsUse { get; set; }
        }
    }
}
