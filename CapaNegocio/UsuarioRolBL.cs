using CapaDatos.DAOs;
namespace CapaNegocio
{
    public static class UsuarioRolBL
    {
        public static bool Asignar(int codigoUsuario, int codigoRol)
        {
            return UsuarioRolDAO.Asignar(codigoUsuario, codigoRol);
        }
    }
}
