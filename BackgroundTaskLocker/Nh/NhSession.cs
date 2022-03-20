using NHibernate;
using NHibernate.Context;

namespace BackgroundTaskLocker.Nh
{
    public class NhSession : IDisposable, IAsyncDisposable
    {
        public static ISession Current
        {
            get
            {
                var currentSession = NHibernateManager.SessionFactory.GetCurrentSession();

                if (currentSession is null)
                    throw new InvalidOperationException("No session");

                return currentSession;
            }
        }
        
        public ISession Session { get; }

        private readonly ISessionFactory _sessionFactory;
        private readonly ITransaction _transaction;

        private NhSession(ISessionFactory sessionFactory,
                          ISession session,
                          ITransaction transaction)
        {
            _sessionFactory = sessionFactory;
            Session         = session;
            _transaction    = transaction;
        }

        public static NhSession Bind(ISessionFactory sessionFactory)
        {
            var session = sessionFactory.OpenSession();
            CurrentSessionContext.Bind(session);

            var transaction = session.BeginTransaction();

            return new NhSession(sessionFactory,
                                 session,
                                 transaction);
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public void Dispose()
        {
            CurrentSessionContext.Unbind(_sessionFactory);

            if (!_transaction.WasCommitted && !_transaction.WasRolledBack)
                _transaction.Rollback();

            Session.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            CurrentSessionContext.Unbind(_sessionFactory);

            if (!_transaction.WasCommitted && !_transaction.WasRolledBack)
                await _transaction.RollbackAsync();

            Session.Dispose();
        }
    }
}