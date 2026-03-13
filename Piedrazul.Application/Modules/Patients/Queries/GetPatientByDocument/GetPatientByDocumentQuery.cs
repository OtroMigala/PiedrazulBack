using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Patients.Queries.GetPatientByDocument;

public record GetPatientByDocumentQuery(string DocumentId)
    : IRequest<PatientDto?>;

public record PatientDto(
    Guid Id,
    string DocumentId,
    string FullName,
    string Phone,
    Gender Gender,
    DateTime? BirthDate,
    string? Email);
