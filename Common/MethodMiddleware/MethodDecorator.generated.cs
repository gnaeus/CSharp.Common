using System;
using System.Threading.Tasks;

namespace Common.MethodMiddleware
{
    public partial class MethodDecorator
    {
        #region Function

        public dynamic Execute(
            Func<object> method)
        {
            return Invoke(null, method);
        }

        public dynamic Execute(
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            Func<object> method)
        {
            return Invoke(null, method, middleware1);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            Func<object> method)
        {
            return Invoke(null, method, middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            Func<object> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            Func<object> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            Func<object> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            IMethodMiddleware middleware6,
            Func<object> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            IMethodMiddleware middleware6,
            object arguments, Func<object> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        #endregion

        #region Action

        public dynamic Execute(
            Action method)
        {
            return Invoke(null, MakeFunc(method));
        }

        public dynamic Execute(
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method));
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            IMethodMiddleware middleware6,
            Action method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        public dynamic Execute(
            IMethodMiddleware middleware1,
            IMethodMiddleware middleware2,
            IMethodMiddleware middleware3,
            IMethodMiddleware middleware4,
            IMethodMiddleware middleware5,
            IMethodMiddleware middleware6,
            object arguments, Action method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        #endregion

        #region AsyncFunction

        public dynamic Execute(
            Func<Task<object>> method)
        {
            return Invoke(null, method);
        }

        public dynamic Execute(
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            IMethodAsyncMiddleware middleware6,
            Func<Task<object>> method)
        {
            return Invoke(null, method, middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            IMethodAsyncMiddleware middleware6,
            object arguments, Func<Task<object>> method)
        {
            return Invoke(arguments, method, middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        #endregion

        #region AsyncAction

        public dynamic Execute(
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method));
        }

        public dynamic Execute(
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method));
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            IMethodAsyncMiddleware middleware6,
            Func<Task> method)
        {
            return Invoke(null, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        public dynamic Execute(
            IMethodAsyncMiddleware middleware1,
            IMethodAsyncMiddleware middleware2,
            IMethodAsyncMiddleware middleware3,
            IMethodAsyncMiddleware middleware4,
            IMethodAsyncMiddleware middleware5,
            IMethodAsyncMiddleware middleware6,
            object arguments, Func<Task> method)
        {
            return Invoke(arguments, MakeFunc(method), middleware1, middleware2, middleware3, middleware4, middleware5, middleware6);
        }

        #endregion
    }
}
