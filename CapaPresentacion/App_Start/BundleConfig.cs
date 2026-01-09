using System.Web.Optimization;

namespace CapaPresentacion
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery core
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            // jQuery Validation
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Modernizr - usar solo para desarrollo
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            // Bootstrap + Plugins JS
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Content/plugins/jquery/jquery.min.js",
                "~/Content/plugins/bootstrap/js/bootstrap.bundle.min.js",

                // DataTables
                "~/Content/plugins/datatables/jquery.dataTables.min.js",
                "~/Content/plugins/datatables-bs4/js/dataTables.bootstrap4.min.js",
                "~/Content/plugins/datatables-responsive/js/dataTables.responsive.min.js",
                "~/Content/plugins/datatables-responsive/js/responsive.bootstrap4.min.js",
                "~/Content/plugins/datatables-buttons/js/dataTables.buttons.min.js",
                "~/Content/plugins/datatables-buttons/js/buttons.bootstrap4.min.js",
                "~/Content/plugins/datatables-buttons/js/buttons.html5.min.js",
                "~/Content/plugins/datatables-buttons/js/buttons.print.min.js",
                "~/Content/plugins/datatables-buttons/js/buttons.colVis.min.js",

                // Utilidades
                "~/Content/plugins/jszip/jszip.min.js",
                "~/Content/plugins/pdfmake/pdfmake.min.js",
                "~/Content/plugins/pdfmake/vfs_fonts.js",
                "~/Content/plugins/sweetalert2/sweetalert2.min.js",

                // AdminLTE y lógica personalizada
                "~/Content/dist/js/adminlte.min.js",
                "~/Content/dist/js/dgac.js"
            ));

            // Estilos principales
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/plugins/fontawesome-free/css/all.min.css",
                "~/Content/dist/css/adminlte.min.css",
                "~/Content/dist/css/dgac.css"
            ));

            // Estilos de plugins
            bundles.Add(new StyleBundle("~/Content/plugins-css").Include(
                "~/Content/plugins/datatables-bs4/css/dataTables.bootstrap4.min.css",
                "~/Content/plugins/datatables-responsive/css/responsive.bootstrap4.min.css",
                "~/Content/plugins/datatables-buttons/css/buttons.bootstrap4.min.css",
                "~/Content/plugins/sweetalert2/sweetalert2.min.css"
            ));

            // Validación extra
            bundles.Add(new ScriptBundle("~/Content/plugins-js").Include(
                "~/Scripts/jquery.validate.min.js"
            ));
        }
    }
}
