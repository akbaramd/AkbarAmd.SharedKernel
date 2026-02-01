using AkbarAmd.SharedKernel.Domain;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

/// <summary>
/// Test entity for tour reservation with complex criteria testing.
/// </summary>
public sealed class TourReservation : Entity<int>
{
    public int TourId { get; private set; }
    public TourReservationStatus Status { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation property for participants (one-to-many relationship)
    public ICollection<TourParticipant> Participants { get; private set; } = new List<TourParticipant>();

    private TourReservation() { }

    public TourReservation(int tourId, TourReservationStatus status, DateTime? expiryDate = null)
    {
        TourId = tourId;
        Status = status;
        ExpiryDate = expiryDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddParticipant(string nationalNumber, string name)
    {
        Participants.Add(new TourParticipant(this, nationalNumber, name));
    }

    public void UpdateStatus(TourReservationStatus newStatus)
    {
        Status = newStatus;
    }

    public void SetExpiryDate(DateTime? expiryDate)
    {
        ExpiryDate = expiryDate;
    }
}

