using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

/// <summary>
/// Test specification: Finds active reservations in restricted tours for a given national number
/// Used to test the exact pattern: Where(b => b.And(...).And(...).OrGroup(g => g.Where(...).Or(...).Or(...)))
/// </summary>
public sealed class RestrictedTourReservationsTestSpecification : Specification<TourReservation>
{
    public RestrictedTourReservationsTestSpecification(IEnumerable<int> restrictedTourIds, string nationalNumber)
    {
        Where(b => b
            .And(r => restrictedTourIds.Contains(r.TourId))
            .And(r => r.Participants.Any(p => p.NationalNumber == nationalNumber))
            .AndGroup(g => g
                .Where(r => r.Status == TourReservationStatus.Draft)
                .Or(r => r.Status == TourReservationStatus.OnHold && 
                          r.ExpiryDate.HasValue && 
                          r.ExpiryDate.Value > DateTime.UtcNow)
                .Or(r => r.Status == TourReservationStatus.Waitlisted && 
                          r.ExpiryDate.HasValue && 
                          r.ExpiryDate.Value > DateTime.UtcNow)
                .Or(r => r.Status == TourReservationStatus.Confirmed)));
    }
}

