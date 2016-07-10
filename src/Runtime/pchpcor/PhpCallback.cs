﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pchp.Core
{
    /// <summary>
    /// An object that can be invoked dynamically.
    /// </summary>
    public interface IPhpCallable
    {
        /// <summary>
        /// Invokes the object with given arguments.
        /// </summary>
        PhpValue Invoke(Context ctx, params PhpValue[] arguments);
    }

    /// <summary>
    /// Delegate for dynamic routine invocation.
    /// </summary>
    /// <param name="ctx">Current runtime context. Cannot be <c>null</c>.</param>
    /// <param name="arguments">List of arguments to be passed to called routine.</param>
    /// <returns></returns>
    public delegate PhpValue PhpCallableRoutine(Context ctx, PhpValue[] arguments);

    /// <summary>
    /// Callable object representing callback to a routine.
    /// Performs dynamic binding to actual method and provides <see cref="IPhpCallable"/> interface.
    /// </summary>
    public abstract class PhpCallback : IPhpCallable
    {
        /// <summary>
        /// Resolved routine to be invoked.
        /// </summary>
        protected PhpCallableRoutine _lazyResolved;

        #region PhpCallbacks

        sealed class CallableCallback : PhpCallback
        {
            public CallableCallback(PhpCallableRoutine routine)
            {
                _lazyResolved = routine;
            }

            protected override PhpCallableRoutine BindCore(Context ctx)
            {
                // cannot be reached
                throw new InvalidOperationException();
            }
        }

        sealed class FunctionCallback : PhpCallback
        {
            /// <summary>
            /// Name of the function to be called.
            /// </summary>
            readonly string _function;

            public FunctionCallback(string function)
            {
                _function = function;
            }

            protected override PhpCallableRoutine BindCore(Context ctx)
            {
                throw new NotImplementedException();
            }
        }

        sealed class MethodCallback : PhpCallback
        {
            readonly string _class, _method;

            // TODO: caller (to resolve accessibility)

            public MethodCallback(string @class, string method)
            {
                _class = @class;
                _method = method;
            }

            protected override PhpCallableRoutine BindCore(Context ctx)
            {
                throw new NotImplementedException();
            }
        }

        sealed class ArrayCallback : PhpCallback
        {
            readonly PhpValue _item1, _item2;

            // TODO: caller (to resolve accessibility)

            public ArrayCallback(PhpValue item1, PhpValue item2)
            {
                _item1 = item1;
                _item2 = item2;
            }

            protected override PhpCallableRoutine BindCore(Context ctx)
            {
                throw new NotImplementedException();
            }
        }

        sealed class InvalidCallback : PhpCallback
        {
            public InvalidCallback()
            {

            }

            protected override PhpCallableRoutine BindCore(Context ctx)
            {
                return null;
            }
        }

        #endregion

        #region Create

        public static PhpCallback Create(IPhpCallable callable) => new CallableCallback(callable.Invoke);

        public static PhpCallback Create(string function) => new FunctionCallback(function);

        public static PhpCallback Create(PhpValue item1, PhpValue item2) => new ArrayCallback(item1, item2);

        public static PhpCallback CreateInvalid() => new InvalidCallback();

        // TODO: Create(Delegate)
        // TODO: Create(object) // look for IPhpCallable, __invoke, PhpCallableRoutine, Delegate

        #endregion

        #region Bind

        /// <summary>
        /// Ensures the routine delegate is bound.
        /// </summary>
        private PhpCallableRoutine Bind(Context ctx) => _lazyResolved ?? BindNew(ctx);

        /// <summary>
        /// Binds the routine delegate.
        /// </summary>
        /// <returns>Instance to the delegate. Cannot be <c>null</c>.</returns>
        private PhpCallableRoutine BindNew(Context ctx)
        {
            var resolved = BindCore(ctx)
                ?? new PhpCallableRoutine((_ctx, _args) => PhpValue.Null);  // TODO: cache // TODO: report call to missing function

            _lazyResolved = resolved;

            return resolved;
        }

        /// <summary>
        /// Performs binding to the routine delegate.
        /// </summary>
        /// <returns>Actual delegate or <c>null</c> if routine cannot be bound.</returns>
        protected abstract PhpCallableRoutine BindCore(Context ctx);

        #endregion

        #region IPhpCallable

        /// <summary>
        /// Invokes the callback with given arguments.
        /// </summary>
        public PhpValue Invoke(Context ctx, params PhpValue[] arguments) => Bind(ctx)(ctx, arguments);

        #endregion
    }
}
