using CassiServiceDownloader.Resources;
using System;
using System.Linq;

namespace CassiServiceDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || !args.Any())
                throw new ArgumentNullException(Mensagens.MN01);

            var argumento = args.First();

            var app = new CassiServiceDownloader(argumento);
            app.Processar();
        }
    }
}
