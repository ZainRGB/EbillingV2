namespace EbillingV2.Models
{
    public class ConnectionFactory
    {
        private readonly IConfiguration _config;

        public ConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public string GetConnectionString(int hospitalId)
        {
            return hospitalId switch
            {
                0 => _config.GetConnectionString("DefaultConnection"),
                1 => _config.GetConnectionString("MySql_Connection_1"),
                2 => _config.GetConnectionString("MySql_Connection_2"),
                3 => _config.GetConnectionString("MySql_Connection_3"),
                5 => _config.GetConnectionString("MySql_Connection_5"),
                6 => _config.GetConnectionString("MySql_Connection_6"),
                7 => _config.GetConnectionString("MySql_Connection_7"),
                _ => throw new Exception("Invalid hospital ID")
            };
        }
    }


}
