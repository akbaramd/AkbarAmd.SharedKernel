using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

/// <summary>
/// Test specification: Finds active reservations (Draft or OnHold non-expired) for a tour and national number
/// Used for querying existing reservations before starting a new one
/// Tests the exact pattern: Where(b => b.And(...).And(...).AndGroup(g => g.Where(...).Or(...)))
/// Note: Uses AndGroup (not OrGroup) because the group should be combined with AND, not OR
/// </summary>
public sealed class ActiveReservationsForTourTestSpecification : Specification<TourReservation>
{
    public ActiveReservationsForTourTestSpecification(int tourId, string nationalNumber)
    {
        Where(b => b
            .And(r => r.TourId == tourId)
            .And(r => r.Participants.Any(p => p.NationalNumber == nationalNumber))
            .AndGroup(g => g
                .Where(r => r.Status == TourReservationStatus.Draft)
                .Or(r => r.Status == TourReservationStatus.OnHold && 
                          r.ExpiryDate.HasValue && 
                          r.ExpiryDate.Value > DateTime.UtcNow)));
    }
}

