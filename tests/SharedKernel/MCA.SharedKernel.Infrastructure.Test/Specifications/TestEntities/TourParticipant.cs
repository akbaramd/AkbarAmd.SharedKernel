using AkbarAmd.SharedKernel.Domain;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

/// <summary>
/// Participant in a tour reservation.
/// </summary>
public sealed class TourParticipant : Entity<int>
{
    public int TourReservationId { get; private set; }
    public TourReservation TourReservation { get; private set; } = null!;
    public string NationalNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private TourParticipant() { }

    public TourParticipant(TourReservation tourReservation, string nationalNumber, string name)
    {
        TourReservation = tourReservation ?? throw new ArgumentNullException(nameof(tourReservation));
        TourReservationId = tourReservation.Id;
        NationalNumber = nationalNumber ?? throw new ArgumentNullException(nameof(nationalNumber));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}

