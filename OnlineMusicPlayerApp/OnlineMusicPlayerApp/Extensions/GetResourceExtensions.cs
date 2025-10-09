using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp
{
    public static class GetResourceExtensions
    {
        public static HtmlWebViewSource GetHtmlText(this HtmlWebViewSource htmlWebViewSource, string fileName)
        {
            HtmlWebViewSource htmlSource;
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var res = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(fileName));

            using (var stream = assembly.GetManifestResourceStream(res))

            using (var reader = new StreamReader(stream))
            {
                string htmlContent = reader.ReadToEnd();
                htmlSource = new HtmlWebViewSource { Html = htmlContent };
            }

            return htmlSource;
        }
    }
}
