using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Patients.Queries.GetPatientByDocument;

public class GetPatientByDocumentHandler
    : IRequestHandler<GetPatientByDocumentQuery, PatientDto?>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByDocumentHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDto?> Handle(
        GetPatientByDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByDocumentIdAsync(request.DocumentId);

        if (patient is null)
            return null;

        return new PatientDto(
            Id: patient.Id,
            DocumentId: patient.DocumentId,
            FullName: patient.FullName,
            Phone: patient.Phone,
            Gender: patient.Gender,
            BirthDate: patient.BirthDate,
            Email: patient.Email);
    }
}
