using FluentValidation;

namespace Piedrazul.Application.Modules.Users.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede superar 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("El nombre de usuario solo puede contener letras, números y guiones bajos.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre completo no puede superar 150 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("El número de documento es obligatorio.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.");
    }
}
