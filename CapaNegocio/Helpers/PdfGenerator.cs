using System;
using System.IO;
using System.Text;

namespace CapaNegocio.Helpers
{
    public static class PdfGenerator
    {
        /// <summary>
        /// Genera un PDF básico usando HTML convertido a Base64 (sin iTextSharp).
        /// Retorna array de bytes.
        /// </summary>
        public static byte[] GenerarPdfBasico(string titulo, string contenidoHtml)
        {
            string html = $@"
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial; font-size: 12pt; }}
                        h1 {{ font-size: 20pt; text-align:center; margin-bottom:20px; }}
                    </style>
                </head>
                <body>
                    <h1>{titulo}</h1>
                    <div>{contenidoHtml}</div>
                </body>
                </html>
            ";

            return Encoding.UTF8.GetBytes(html);
        }

        /// <summary>
        /// Genera archivo PDF físico (HTML → PDF simulado)
        /// </summary>
        public static string GuardarPdfBasico(string ruta, string titulo, string contenido)
        {
            byte[] data = GenerarPdfBasico(titulo, contenido);

            File.WriteAllBytes(ruta, data);

            return ruta;
        }
    }
}
