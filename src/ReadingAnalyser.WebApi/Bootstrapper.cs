using log4net;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Json;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace PontoDeVenda.Executor
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new DiagnosticsConfiguration { Password = @"nancy123" };
            }
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(x =>
                {
                    x.ResponseProcessors = new[] { typeof(JsonProcessor) };
                });
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            ConfigurarIoCContainer(container);
        }

        private void ConfigurarIoCContainer(TinyIoCContainer container)
        {
            //TODO registrar as 
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                return null;
            });
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => ConfigurarCabecalhos(ctx.Response));

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                if (SuportaCompressao(ctx))
                {
                    var jsonData = new MemoryStream();
                    ctx.Response.Contents.Invoke(jsonData);
                    jsonData.Position = 0;
                    if (jsonData.Length < 4096)
                    {
                        ctx.Response.Contents = s =>
                        {
                            jsonData.CopyTo(s);
                            s.Flush();
                        };
                    }
                    else
                    {
                        ctx.Response.Headers["Content-Encoding"] = "gzip";
                        ctx.Response.Contents = s =>
                        {
                            var gzip = new GZipStream(s, CompressionMode.Compress, true);
                            jsonData.CopyTo(gzip);
                            gzip.Close();
                        };
                    }
                }
            });

            pipelines.OnError.AddItemToEndOfPipeline((ctx, excecao) =>
            {
                log.Error(excecao.Message, excecao);
                var response = new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = excecao.Message
                };

                ConfigurarCabecalhos(response);

                return response;
            });
        }

        private static bool SuportaCompressao(NancyContext ctx)
        {
            return (ctx.Response.ContentType.Contains("application/json")) && ctx.Request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
        }

        private static string StreamToString(Stream stream)
        {
            var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            stream.Position = 0;
            return result;
        }        

        private void ConfigurarCabecalhos(Response response)
        {
            response
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "POST,GET,PUT,DELETE")
                .WithHeader("Access-Control-Allow-Headers", ObterParametrosPermitidosNoCabecalho());
        }

        private string ObterParametrosPermitidosNoCabecalho()
        {
            var parametrosFixos = new[]
            {
                "Accept",
                "Origin",
                "Content-type"
            };
            return string.Join(", ", parametrosFixos);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", @"static"));
        }
    }
}