using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using log4net;
using System.Reflection;
using log4net.Config;

namespace ReadingAnalyser.WebApi
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += ErroNaoTratado;

            var host = HostFactory.New(x =>
            {
                x.Service<InicializadorServicoNancy>(sc =>
                {
                    sc.ConstructUsing(name => new InicializadorServicoNancy());
                    sc.WhenStarted((servico, hostControl) =>
                    {
                        hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(60));
                        return servico.Iniciar();
                    });
                    sc.WhenStopped((servico, hostControl) =>
                    {
                        return servico.Parar();
                    });
                });
                x.RunAsLocalSystem();

                x.SetDescription("BookStatistics");
                x.SetDisplayName("BookStatistics");
                x.SetServiceName("BookStatistics");

                x.EnableServiceRecovery(rc => rc.RestartService(delayInMinutes: 1));
                x.SetStartTimeout(TimeSpan.FromMinutes(1));
                x.StartAutomatically();
            });

            host.Run();
        }

        private static void ErroNaoTratado(object sender, UnhandledExceptionEventArgs args)
        {
            logger.ErrorFormat("Exception e = (Exception)args.ExceptionObject;");
            Exception e = (Exception)args.ExceptionObject;
            logger.ErrorFormat("ErroNaoTratado: {0}", e);
        }
    }
}
