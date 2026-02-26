using NexusERP.Extensions;
using NexusERP.Models;

namespace NexusERP.Helpers
{
    public class HelperSessionContextAccessor
    {
        private IHttpContextAccessor contextAccessor;

        public HelperSessionContextAccessor(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public int GetEmpresaIdSession()
        {
            if (this.contextAccessor.HttpContext == null)
            {
                return 0;
            }
            UsuarioSessionModel usuarioActual =
                this.contextAccessor.HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");

            if (usuarioActual != null)
            {
                return usuarioActual.EmpresaId;
            }

            return 0;

        }

    }
}
