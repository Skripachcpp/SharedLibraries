using System;
using System.Collections.Generic;
using System.Threading;
using WorkingTools.Extensions;
using WorkingTools.Repository;

namespace WorkingTools.Parallel
{
    public class RepositoryTasks<TParams> : QueueTasks<TParams>, IDisposable
    {
        private readonly IRepository<TParams> _repository;

        public RepositoryTasks(Action<TParams, CancellationToken> taskHandler, IRepository<TParams> repository,
            int? parallel = null, IEqualityComparer<TParams> comparer = null)
            : base(taskHandler, parallel, comparer)
        {
            _repository = repository;

            TasksGetter = () =>
            {
                lock (repository)
                {
                    if (ReloadAuto) { repository.Save(); repository.Load(); }
                    return repository.Get();
                }
            };

            TaskHandlerContinuation = (prms, token) => { lock (repository) { repository.Remove(prms); if (SaveAuto) repository.Save(); } };

            ReloadAuto = false;
            SaveAuto = true;
        }

        public RepositoryTasks(Action<TParams, CancellationToken> taskHandler, string repositoryFile,
            int? parallel = null, IEqualityComparer<TParams> comparer = null)
            : this(taskHandler, new FileRepository<TParams>(repositoryFile, comparer), parallel, comparer)
        {
        }

        public bool ReloadAuto { get; set; }
        public bool SaveAuto { get; set; }


        public bool AddTask(TParams item)
        {
            var addFl = _repository.Add(item);
            if (addFl && SaveAuto) Save();
            return addFl;
        }

        public void AddTasks(IEnumerable<TParams> items)
        {
            var addFl = false;
            items.ForEach(item => { if (_repository.Add(item)) addFl = true; });
            if (addFl && SaveAuto) Save();
        }

        public void Load()
        {
            _repository.Load();
        }

        public void Save()
        {
            _repository.Save();
        }
    }
}