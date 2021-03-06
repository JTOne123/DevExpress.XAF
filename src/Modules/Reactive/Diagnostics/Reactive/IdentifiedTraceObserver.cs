﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace Xpand.XAF.Modules.Reactive.Diagnostics.Reactive{
    /// <summary>
    ///     Provides a mechanism for tracing push-based notifications with a unique identifier for the observer.
    /// </summary>
    /// <typeparam name="T">Type of value notifications.</typeparam>
    public class IdentifiedTraceObserver<T> : TraceObserver<T>{
        #region Public Properties

        /// <summary>
        ///     Gets or sets the observer's identity in the trace output.
        /// </summary>
        public string Identity{
            get{
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return _id;
            }
            set{
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _id = value;
            }
        }

        #endregion

        #region Private / Protected

        private readonly Func<string, T, string> _onNext;
        private readonly Func<string, Exception, string> _onError;
        private readonly Func<string, string> _onCompleted;
        private string _id;

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class with default trace actions for all
        ///     notification kinds.
        /// </summary>
        public IdentifiedTraceObserver()
            : this(TraceDefaults.DefaultOnNext, TraceDefaults.DefaultOnError, TraceDefaults.DefaultOnCompleted){
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext calls.
        /// </summary>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        public IdentifiedTraceObserver(Func<string, T, string> onNext){
            Contract.Requires(onNext != null);

            _id = AutoIdentify();

            this._onNext = onNext;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnError
        ///     calls.
        /// </summary>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onError">A function that returns the message to be traced for the error.</param>
        public IdentifiedTraceObserver(Func<string, T, string> onNext, Func<string, Exception, string> onError)
            : this(onNext){
            Contract.Requires(onNext != null);
            Contract.Requires(onError != null);

            this._onError = onError;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnCompleted
        ///     calls.
        /// </summary>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onCompleted">A function that returns the message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(Func<string, T, string> onNext, Func<string, string> onCompleted)
            : this(onNext){
            Contract.Requires(onNext != null);
            Contract.Requires(onCompleted != null);

            this._onCompleted = onCompleted;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext, OnError and
        ///     OnCompleted calls.
        /// </summary>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onError">A function that returns the message to be traced for the error.</param>
        /// <param name="onCompleted">A function that returns the message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(Func<string, T, string> onNext, Func<string, Exception, string> onError,
            Func<string, string> onCompleted)
            : this(onNext, onError){
            Contract.Requires(onNext != null);
            Contract.Requires(onError != null);
            Contract.Requires(onCompleted != null);

            this._onCompleted = onCompleted;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext calls.
        /// </summary>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        public IdentifiedTraceObserver(string nextFormat)
            : this(TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat)){
            Contract.Requires(nextFormat != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnError
        ///     calls.
        /// </summary>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="errorFormat">The format in which the error will be traced.  A single replacement token {0} is supported.</param>
        public IdentifiedTraceObserver(string nextFormat, string errorFormat)
            : this(TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat),
                TraceDefaults.GetIdentityFormatOnError(errorFormat)){
            Contract.Requires(nextFormat != null);
            Contract.Requires(errorFormat != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext, OnError and
        ///     OnCompleted calls.
        /// </summary>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="errorFormat">The format in which the error will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="completedMessage">The message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(string nextFormat, string errorFormat, string completedMessage)
            : this(TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat),
                TraceDefaults.GetIdentityFormatOnError(errorFormat),
                TraceDefaults.GetIdentityMessageOnCompleted(completedMessage)){
            Contract.Requires(nextFormat != null);
            Contract.Requires(errorFormat != null);
            Contract.Requires(completedMessage != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class with default trace actions for all
        ///     notification kinds.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        public IdentifiedTraceObserver(TraceSource trace)
            : this(trace, TraceDefaults.DefaultOnNext, TraceDefaults.DefaultOnError, TraceDefaults.DefaultOnCompleted){
            Contract.Requires(trace != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        public IdentifiedTraceObserver(TraceSource trace, Func<string, T, string> onNext)
            : base(trace){
            Contract.Requires(trace != null);
            Contract.Requires(onNext != null);

            _id = AutoIdentify();

            this._onNext = onNext;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnError
        ///     calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onError">A function that returns the message to be traced for the error.</param>
        public IdentifiedTraceObserver(TraceSource trace, Func<string, T, string> onNext,
            Func<string, Exception, string> onError)
            : base(trace){
            Contract.Requires(trace != null);
            Contract.Requires(onNext != null);
            Contract.Requires(onError != null);

            _id = AutoIdentify();

            this._onNext = onNext;
            this._onError = onError;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnCompleted
        ///     calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onCompleted">A function that returns the message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(TraceSource trace, Func<string, T, string> onNext,
            Func<string, string> onCompleted)
            : base(trace){
            Contract.Requires(trace != null);
            Contract.Requires(onNext != null);
            Contract.Requires(onCompleted != null);

            _id = AutoIdentify();

            this._onNext = onNext;
            this._onCompleted = onCompleted;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext, OnError and
        ///     OnCompleted calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="onNext">A function that returns the message to be traced for each notification.</param>
        /// <param name="onError">A function that returns the message to be traced for the error.</param>
        /// <param name="onCompleted">A function that returns the message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(TraceSource trace, Func<string, T, string> onNext,
            Func<string, Exception, string> onError, Func<string, string> onCompleted)
            : base(trace){
            Contract.Requires(trace != null);
            Contract.Requires(onNext != null);
            Contract.Requires(onError != null);
            Contract.Requires(onCompleted != null);

            _id = AutoIdentify();

            this._onNext = onNext;
            this._onError = onError;
            this._onCompleted = onCompleted;
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        public IdentifiedTraceObserver(TraceSource trace, string nextFormat)
            : this(trace, TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat)){
            Contract.Requires(trace != null);
            Contract.Requires(nextFormat != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext and OnError
        ///     calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="errorFormat">The format in which the error will be traced.  A single replacement token {0} is supported.</param>
        public IdentifiedTraceObserver(TraceSource trace, string nextFormat, string errorFormat)
            : this(trace, TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat),
                TraceDefaults.GetIdentityFormatOnError(errorFormat)){
            Contract.Requires(trace != null);
            Contract.Requires(nextFormat != null);
            Contract.Requires(errorFormat != null);
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="IdentifiedTraceObserver{T}" /> class for tracing OnNext, OnError and
        ///     OnCompleted calls.
        /// </summary>
        /// <param name="trace">The <see cref="TraceSource" /> to be associated with the trace messages.</param>
        /// <param name="nextFormat">The format in which values will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="errorFormat">The format in which the error will be traced.  A single replacement token {0} is supported.</param>
        /// <param name="completedMessage">The message to be traced for the completed notification.</param>
        public IdentifiedTraceObserver(TraceSource trace, string nextFormat, string errorFormat,
            string completedMessage)
            : this(trace, TraceDefaults.GetIdentityFormatOnNext<T>(nextFormat),
                TraceDefaults.GetIdentityFormatOnError(errorFormat),
                TraceDefaults.GetIdentityMessageOnCompleted(completedMessage)){
            Contract.Requires(trace != null);
            Contract.Requires(nextFormat != null);
            Contract.Requires(errorFormat != null);
            Contract.Requires(completedMessage != null);
        }

        #endregion

        #region Methods

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification =
            "Required for code contracts.")]
        private void ObjectInvariant(){
            Contract.Invariant(!string.IsNullOrWhiteSpace(_id));
        }

        private static string AutoIdentify(){
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            var identity = Interlocked.Increment(ref TraceDefaults.IdentityCounter)
                .ToString(CultureInfo.InvariantCulture);

            Contract.Assume(!string.IsNullOrWhiteSpace(identity));

            return identity;
        }

        /// <summary>
        ///     Formats the specified <paramref name="value" />.
        /// </summary>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>
        ///     The formatted value if calls to <see cref="IObserver{T}.OnNext" /> are supported by this instance; otherwise,
        ///     <see langword="null" />.
        /// </returns>
        protected sealed override string FormatOnNext(T value){
            return FormatOnNext(_id, value);
        }

        /// <summary>
        ///     Formats the specified <paramref name="exception" />.
        /// </summary>
        /// <param name="exception">The exception to be formatted.</param>
        /// <returns>
        ///     The formatted exception if calls to <see cref="IObserver{T}.OnError" /> are supported by this instance and the
        ///     specified <paramref name="exception" /> is
        ///     not <see langword="null" />; otherwise, <see langword="null" />.
        /// </returns>
        protected sealed override string FormatOnError(Exception exception){
            return FormatOnError(_id, exception);
        }

        /// <summary>
        ///     Returns a string for <see cref="IObserver{T}.OnCompleted" />.
        /// </summary>
        /// <returns>
        ///     The string to be traced if calls to <see cref="IObserver{T}.OnCompleted" /> are supported by this instance;
        ///     otherwise, <see langword="null" />.
        /// </returns>
        protected sealed override string FormatOnCompleted(){
            return FormatOnCompleted(_id);
        }

        /// <summary>
        ///     Formats the specified <paramref name="value" /> with the specified <paramref name="observerId" />.
        /// </summary>
        /// <param name="observerId">The identity of the observer.</param>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>
        ///     The formatted value if calls to <see cref="IObserver{T}.OnNext" /> are supported by this instance; otherwise,
        ///     <see langword="null" />.
        /// </returns>
        protected virtual string FormatOnNext(string observerId, T value){
            return _onNext?.Invoke(observerId, value);
        }

        /// <summary>
        ///     Formats the specified <paramref name="exception" /> with the specified <paramref name="observerId" />.
        /// </summary>
        /// <param name="observerId">The identity of the observer.</param>
        /// <param name="exception">The exception to be formatted.</param>
        /// <returns>
        ///     The formatted exception if calls to <see cref="IObserver{T}.OnError" /> are supported by this instance and the
        ///     specified <paramref name="exception" /> is
        ///     not <see langword="null" />; otherwise, <see langword="null" />.
        /// </returns>
        protected virtual string FormatOnError(string observerId, Exception exception){
            return _onError?.Invoke(observerId, exception);
        }

        /// <summary>
        ///     Returns a string for <see cref="IObserver{T}.OnCompleted" /> with the specified <paramref name="observerId" />.
        /// </summary>
        /// <param name="observerId">The identity of the observer.</param>
        /// <returns>
        ///     The string to be traced if calls to <see cref="IObserver{T}.OnCompleted" /> are supported by this instance;
        ///     otherwise, <see langword="null" />.
        /// </returns>
        protected virtual string FormatOnCompleted(string observerId){
            return _onCompleted?.Invoke(observerId);
        }

        #endregion
    }
}