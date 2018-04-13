using System;
using System.Threading;
using WorkingTools.Repository;

namespace WorkingTools.Parallel
{
    /// <summary>
    /// Асинхронная очередь задач
    /// </summary>
    /// <typeparam name="TActionParams">тип параметра передаваемого в action при выполнении очереди; 
    /// данный тип обязательно должен поддерживать стандартную сериализацию</typeparam>
    /// <typeparam name="TKey">уникальный идентификатор задачи</typeparam>
    /// <remarks>очередь задач сохраняется в репозиторий и загружается от туда, 
    /// что позволяет продолжить выполение после перезапуска приложеиня;
    /// задачи завершившиеся ошибкой будут повторно обработаны после вызова метода ReRunFailTasks</remarks>
    public class QueueTasks<TKey, TActionParams> : QueueTasksLite<TKey, TActionParams>, IDisposable
    {
        protected new readonly IRepository<TKey, TActionParams> Repository;

        public QueueTasks(Action<TActionParams> action, IRepository<TKey, TActionParams> repository,
            Action<TActionParams> continuation = null, Action<TActionParams, Exception> wasError = null)
            : base(action, repository, continuation, wasError)
        {
            Repository = repository;

            _autoSave = true;
        }

        private bool _autoSave;
        /// <summary>
        /// Установка параметра автоматического сохранения изменений
        /// </summary>
        /// <remarks>true если необходимо сохранять значение в репозитории после каждого изменения; 
        /// false изменения сохраняются только при вызове метода Save(), или при изменении параметра с false на true</remarks>
        public bool AutoSave
        {
            get { return _autoSave; }
            set
            {
                if (!_autoSave && value)
                    Save();

                _autoSave = value;
            }
        }

        protected override void LoadAndInvokeTasks(CancellationToken? token, bool autoLoad)
        {
            if (autoLoad) lock (Lock) Repository.Save();
            base.LoadAndInvokeTasks(token, autoLoad);
        }

        protected override void PerformCompleted(TKey key)
        {
            base.PerformCompleted(key);

            lock (Lock)
            {
                Repository.Remove(key);
                if (AutoSave) Repository.Save();
            }
        }

        /// <summary>
        /// Добавить задачу в очередь
        /// </summary>
        /// <param name="key">ключь</param>
        /// <param name="actionParams">значение</param>
        /// <param name="token">токен отмены операции</param>
        /// <remarks>при добавлении новой задачи очередь автоматически начинает выполняться, 
        /// при этом первыми будут выполнены ранее сохраненные задачи</remarks>
        public void Enqueue(TKey key, TActionParams actionParams, CancellationToken? token = null)
        {
            Start(token);//продолжить выполнять задачи, если они есть в очереди

            //добавить в очередь задачу
            lock (Lock)
            {
                Repository.Set(key, actionParams);
                if (_autoSave) Save();


                if (ProcessingTasks.Contains(key))
                    throw new Exception(string.Format("Задача с ключом {0} уже выполнялась", key));

                if (PoolTasks.Contains(key))
                    throw new Exception(string.Format("Задача с ключом {0} уже содержется в пуле", key));

                if (FailTasks.Contains(key))
                    throw new Exception(string.Format("Задача с ключом {0} уже завершилась ошибкой", key));

                PoolTasks.Add(key);
                Pool.Invoke(Perform, key, actionParams, token);
            }
        }

        /// <summary>
        /// Очистить очередь задач
        /// </summary>
        public void Clear(bool wait = true)
        {
            lock (Lock)
            {
                Repository.Clear();

                Pool.Clear();

                if (wait) Pool.WaitAll();
            }
        }

        /// <summary>
        /// Сохранить состояние очереди
        /// </summary>
        public void Save()
        {
            lock (Lock)
            {
                Repository.Save();
            }
        }
    }
    

    public class QueueTasksAutoKey<TKey, TActionParams> : QueueTasks<TKey, TActionParams>, IDisposable
    {
        protected new IRepositoryAutoKey<TKey, TActionParams> Repository;

        public QueueTasksAutoKey(Action<TActionParams> action, IRepositoryAutoKey<TKey, TActionParams> repository, Action<TActionParams> continuation = null, Action<TActionParams, Exception> wasError = null)
            : base(action, repository, continuation, wasError)
        {
            Repository = repository;
        }

        public void Enqueue(TActionParams actionParams, CancellationToken? token = null)
        {
            Start(token);//продолжить выполнять задачи, если они есть в очереди

            var key = Repository.Add(actionParams);
            Enqueue(key, actionParams);
        }
    }
}