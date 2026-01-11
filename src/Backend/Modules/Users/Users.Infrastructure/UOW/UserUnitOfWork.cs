// using Shared.Infrastructure.Common;
// using Users.Domain.Repositories;
// using Users.Domain.UnitOfWork;
// using Users.Infrastructure.Persistence.Contexts;

// namespace Users.Infrastructure.UnitOfWork
// {
//     public class UserUnitOfWork 
//         : GenericUnitOfWork<UserDbContext>, IUserUnitOfWork
//     {
//         private readonly IUserRepository _userRepository;

//         public UserUnitOfWork(UserModuleDbContext dbContext, IUserRepository userRepository)
//             : base(dbContext)
//         {
//             _userRepository = userRepository;
//         }

//         public IUserRepository Users => _userRepository;
//     }

// }
