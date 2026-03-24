using MediatR;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Users.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Roles elevados solo pueden ser asignados por un Admin autenticado
        if (request.Role is UserRole.Admin or UserRole.Scheduler or UserRole.Doctor)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.Role != nameof(UserRole.Admin))
                throw new UnauthorizedAccessException(
                    "Solo un administrador puede registrar usuarios con rol Admin, Scheduler o Doctor.");
        }

        if (await _userRepository.UsernameExistsAsync(request.Username.ToLower()))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        if (request.Role == UserRole.Patient &&
            await _patientRepository.DocumentIdExistsAsync(request.DocumentId))
            throw new InvalidOperationException("El número de documento ya está registrado.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Create(
            request.Username,
            passwordHash,
            request.FullName,
            request.Role,
            request.Email);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _userRepository.AddAsync(user);

            // Solo se crea perfil de paciente cuando el rol es Patient
            if (request.Role == UserRole.Patient)
            {
                var patient = Patient.Create(
                    request.DocumentId,
                    request.FullName,
                    request.Phone,
                    request.Gender,
                    request.BirthDate,
                    request.Email);
                patient.AssignUser(user.Id);
                await _patientRepository.AddAsync(patient);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        var token = _jwtService.GenerateToken(user);

        return new RegisterResult(
            Token: token,
            Role: user.Role.ToString(),
            FullName: user.FullName,
            ExpiresAt: DateTime.UtcNow.AddHours(24));
    }
}
