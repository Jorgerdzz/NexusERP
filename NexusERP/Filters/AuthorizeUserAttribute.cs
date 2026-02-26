using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NexusERP.Enums;
using NexusERP.Extensions;
using NexusERP.Models;

namespace NexusERP.Filters
{
    public class AuthorizeUserAttribute: ActionFilterAttribute
    {
        public RolesUsuario Rol { get; set; } = RolesUsuario.Ninguno;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            UsuarioSessionModel usuarioActual = context.HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");

            if (usuarioActual == null)
            {
                context.Result = new RedirectToActionResult("LogIn", "Account", null);
                return; 
            }

            if (Rol != RolesUsuario.Ninguno && usuarioActual.Rol != Rol)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return; 
            }

            base.OnActionExecuting(context);
        }

    }
}
