using MCA.Context.Identity.Domain.User.Entities;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Reporitories;

public interface IUserRepository : IRepository<UserEntity,Guid>
{
} 