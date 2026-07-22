using SuttorLib.Core.Services.Notifications;
using SuttorLibrary.Core.Interfaces;

namespace SuttorLibrary.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthRepository AuthRepo { get; }
        IUserRepository UserRepo { get; }
        IBookRepository BookRepo { get; }
        IFCMService FCMRepo { get; }
        Task CompleteAsync();
    }
}
