using CassiServiceDownloader.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml.Schema;

namespace CassiServiceDownloader
{
    public class CassiServiceDownloader
    {
        private const string LineBreak = "\n";
        private readonly string _servicoUrl;
        private readonly string _pastaDestino;
        private readonly ICollection<string> _xsdsProcessados;

        public CassiServiceDownloader(string servicoUrl)
        {
            _servicoUrl = servicoUrl;
            _pastaDestino = string.Format("{0}\\{1}\\", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), BuscarNomeServico(servicoUrl));
            _xsdsProcessados = new List<string>();
        }

        public void Processar()
        {
            if (string.IsNullOrWhiteSpace(_servicoUrl))
                Console.WriteLine(LineBreak + Mensagens.MN02 + LineBreak);
            else
                ProcessarWsdl(_servicoUrl);
        }

        private void ProcessarWsdl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url");

            Directory.CreateDirectory(_pastaDestino);
            SalvarWSDL(url);
        }

        private void SalvarWSDL(string url)
        {
            var bytes = BuscarArquivoNoServidor(url, "wsdl");
            bytes = ProcessarArquivoWSDL(bytes);
            SalvarEmDisco(bytes, "wsdl.wsdl");
        }

        private void SalvarEmDisco(byte[] bytes, string nomeDoArquivo)
        {
            using (var filestream = new FileStream(_pastaDestino + nomeDoArquivo, FileMode.OpenOrCreate, FileAccess.Write))
            {
                filestream.Write(bytes, 0, (int)bytes.Length);
            }
        }

        private byte[] ProcessarArquivoWSDL(byte[] bytes)
        {
            var serviceDescription = ServiceDescription.Read(new MemoryStream(bytes));

            var schemas = serviceDescription.Types.Schemas;
            foreach (XmlSchema xmlSchema in schemas)
            {
                foreach (var include in xmlSchema.Includes)
                {
                    var url = (string)include.GetType()
                        .GetProperty("SchemaLocation")
                        .GetValue(include, BindingFlags.Default, null, null, null);


                    var nomeXsd = url.Substring(url.LastIndexOf("xsd", StringComparison.CurrentCultureIgnoreCase)) + ".xsd";
                    include.GetType().GetProperty("SchemaLocation").SetValue(include, nomeXsd, BindingFlags.Default, null, null, null);

                    ProcessarXSD(nomeXsd);
                }
            }

            var ms = new MemoryStream();
            serviceDescription.Write(ms);

            return ms.ToArray();
        }

        private void ProcessarXSD(string nomeXsd)
        {
            var bytes = BuscarArquivoNoServidor(_servicoUrl, "xsd=" + nomeXsd.Remove(nomeXsd.LastIndexOf(".xsd", StringComparison.InvariantCultureIgnoreCase)));
            var xsdsEncontradosNoArquivo = new List<string>();

            using (var ms = new MemoryStream(bytes))
            {
                var xsd = XmlSchema.Read(ms, null);
                foreach (var item in xsd.Includes)
                {
                    var url = (string)item.GetType()
                        .GetProperty("SchemaLocation")
                        .GetValue(item, BindingFlags.Default, null, null, null);

                    var novoNomeXsd = url.Substring(url.LastIndexOf("xsd", StringComparison.CurrentCultureIgnoreCase)) + ".xsd";
                    item.GetType().GetProperty("SchemaLocation").SetValue(item, novoNomeXsd, BindingFlags.Default, null, null, null);

                    xsdsEncontradosNoArquivo.Add(novoNomeXsd);

                    if (_xsdsProcessados.All(x => x != novoNomeXsd))
                    {
                        _xsdsProcessados.Add(novoNomeXsd);
                    }
                    SalvarEmDisco(bytes, nomeXsd);
                }

                using (var msBuffer = new MemoryStream())
                {
                    xsd.Write(msBuffer);
                    SalvarEmDisco(msBuffer.ToArray(), nomeXsd);
                }
            }
            xsdsEncontradosNoArquivo.Except(_xsdsProcessados).ToList().ForEach(ProcessarXSD);
        }

        private byte[] BuscarArquivoNoServidor(string url, string queryParams)
        {
            var uriBuilder = new UriBuilder(url)
            {
                Query = queryParams
            };

            var webRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Method = "GET";
            webRequest.Accept = "text/xml";

            using (var response = webRequest.GetResponse())
            {
                var ms = new MemoryStream();
                var responseStream = response.GetResponseStream();
                responseStream?.CopyTo(ms);

                return ms.ToArray();
            }
        }

        private string BuscarNomeServico(string servicoUrl)
        {
            var indice = servicoUrl.LastIndexOf("/", StringComparison.Ordinal);

            var nome = servicoUrl.Substring(indice + 1);
            return nome;
        }
    }
}
