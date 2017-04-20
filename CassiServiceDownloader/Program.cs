using CassiServiceDownloader.Resources;
using System;
using System.Linq;
using CassiServiceDownloader.Forms;

namespace CassiServiceDownloader
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            if (args == null || !args.Any())
                IniciarModoGrafico();
            else
                IniciarModoLinhaDeComando(args.First());
        }

        private static void IniciarModoGrafico()
        {
            var f = new MainForm();
            f.ShowDialog();
        }

        private static void IniciarModoLinhaDeComando(string argumento)
        {
            var app = new CassiServiceDownloader(argumento);
            app.Processar();
        }
    }
}
