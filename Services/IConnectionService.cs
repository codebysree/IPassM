namespace IPassM.Services
{
    public abstract class ConnectionService
    {
        private readonly IConfiguration _configuration;
        protected ConnectionService()
        {
            //string connectionString = _configuration.GetConnectionString("default");
        }

    }
    public class IConnectionService
    {
    }
}
