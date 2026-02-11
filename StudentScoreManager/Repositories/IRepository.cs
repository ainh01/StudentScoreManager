using System.Collections.Generic;

namespace StudentScoreManager.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();

        T GetById(int id);

        bool Insert(T entity);

        bool Update(T entity);

        bool Delete(int id);
    }
}