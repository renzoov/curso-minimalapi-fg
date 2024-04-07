using FluentValidation;
using MinimalAPIPeliculas.DTOs;

namespace MinimalAPIPeliculas.Validaciones
{
    public class EditarClaimDTOValidador : AbstractValidator<EditarClaimDTO>
    {
        public EditarClaimDTOValidador()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje)
                .MaximumLength(256).WithMessage(Utilidades.MaximunLengthMensaje)
                .EmailAddress().WithMessage(Utilidades.EmailMensaje);
        }
    }
}
