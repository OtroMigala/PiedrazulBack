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

    public RegisterHandler(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<RegisterResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username.ToLower()))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = Piedrazul.Domain.Entities.User.Create(
            request.Username,
            passwordHash,
            request.FullName,
            UserRole.Patient,
            request.Email);

        await _userRepository.AddAsync(user);

        var patient = Patient.Create(
            request.DocumentId,
            request.FullName,
            request.Phone,
            request.Gender,
            request.BirthDate,
            request.Email);
        patient.AssignUser(user.Id);
        await _patientRepository.AddAsync(patient);

        var token = _jwtService.GenerateToken(user);

        return new RegisterResult(
            Token: token,
            Role: user.Role.ToString(),
            FullName: user.FullName,
            ExpiresAt: DateTime.UtcNow.AddHours(24));
    }
}
