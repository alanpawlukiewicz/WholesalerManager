namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersDeleterService
    {
        Task<bool> DeleteUser(Guid userID);
    }
}
