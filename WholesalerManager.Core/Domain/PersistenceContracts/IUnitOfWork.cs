using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.Domain.PersistenceContracts
{
    /// <summary>
    /// Defines a unit of work for managing database transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Asynchronously saves all the changes made in the database. Returns the number of state entries written to the database.
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Starts a new transaction. All the operations performed after this method call will be part of the transaction until CommitTransactionAsync or RollbackTransactionAsync is called.
        /// </summary>
        /// <returns></returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction. All the operations performed after the last BeginTransactionAsync call will be permanently saved in the database. If any of the operations fail, the transaction will be rolled back and no changes will be saved.
        /// </summary>
        /// <returns></returns>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction. All the operations performed after the last BeginTransactionAsync call will be discarded and no changes will be saved in the database.
        /// </summary>
        /// <returns></returns>
        Task RollbackTransactionAsync();
    }
}
