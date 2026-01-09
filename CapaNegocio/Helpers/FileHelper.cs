using System;
using System.IO;
using System.Linq;

namespace CapaNegocio.Helpers
{
    public static class FileHelper
    {
        // ===============================================================
        // GUARDAR ARCHIVO
        // ===============================================================
        public static string GuardarArchivo(byte[] archivo, string nombreArchivo, string carpetaDestino)
        {
            try
            {
                if (!Directory.Exists(carpetaDestino))
                    Directory.CreateDirectory(carpetaDestino);

                string extension = Path.GetExtension(nombreArchivo);
                string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);

                string nombreUnico =
                    $"{nombreSinExtension}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

                string rutaCompleta = Path.Combine(carpetaDestino, nombreUnico);

                File.WriteAllBytes(rutaCompleta, archivo);

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar archivo: " + ex.Message, ex);
            }
        }

        // ===============================================================
        // ELIMINAR ARCHIVO
        // ===============================================================
        public static bool EliminarArchivo(string rutaArchivo)
        {
            try
            {
                if (File.Exists(rutaArchivo))
                {
                    File.Delete(rutaArchivo);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar archivo: " + ex.Message, ex);
            }
        }

        // ===============================================================
        // FORMATEAR TAMAÑO
        // ===============================================================
        public static string FormatearTamanio(long tamanioBytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = tamanioBytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024.0;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // ===============================================================
        // VALIDAR SI ES IMAGEN
        // ===============================================================
        public static bool EsImagen(string nombreArchivo)
        {
            string[] extensionesImagen =
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff"
            };

            string extension = Path.GetExtension(nombreArchivo)?.ToLower();
            return extensionesImagen.Contains(extension);
        }

        // ===============================================================
        // VALIDAR SI ES PDF
        // ===============================================================
        public static bool EsPDF(string nombreArchivo)
        {
            string extension = Path.GetExtension(nombreArchivo)?.ToLower();
            return extension == ".pdf";
        }

        // ===============================================================
        // OBTENER MIME TYPE (COMPATIBLE C# 7 Y 8)
        // ===============================================================
        public static string ObtenerTipoMIME(string nombreArchivo)
        {
            string ext = Path.GetExtension(nombreArchivo)?.ToLower();

            switch (ext)
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".zip": return "application/zip";
                case ".rar": return "application/x-rar-compressed";

                default: return "application/octet-stream";
            }
        }
    }
}
