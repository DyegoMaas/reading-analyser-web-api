using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using log4net;
using System.Reflection;

namespace ReadingAnalyser.WebApi
{
    public class InicializadorServicoNancy : IDisposable
    {
        private NancyHost host;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Iniciar()
        {
            IniciarNancy();
            return true;
        }
        
        private void IniciarNancy()
        {
            var hostConfiguration = new HostConfiguration
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true }
            };

            var uri = new Uri("http://localhost:3579");
            host = new NancyHost(hostConfiguration, uri);
            host.Start();
            logger.Info("Nancy inicializado.");
        }

        public bool Parar()
        {
            return true;
        }

        public void Dispose()
        {
            logger.Info(" Dispose() Inicializador servico nancy");
            if (host != null)
                host.Dispose();
        }
    }
}
