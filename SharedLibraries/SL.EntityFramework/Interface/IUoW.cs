using System;

namespace SL.EntityFramework.Interface
{
    /// <summary>
    /// Unit of Work
    /// </summary>
    public interface IUoW : IDisposable
    {
        int SaveChanges();
    }
}