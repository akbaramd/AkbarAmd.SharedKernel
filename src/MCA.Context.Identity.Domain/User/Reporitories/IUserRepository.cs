using MCA.Identity.Domain.User.Entities;
using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Identity.Domain.User.Reporitories;

public interface IUserRepository : IRepository<UserEntity,Guid>
{
} 