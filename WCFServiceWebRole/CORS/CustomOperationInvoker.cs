using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace WCFServiceWebRole.CORS
{
    public class CustomOperationInvoker : IOperationInvoker
    {
        IOperationInvoker _innerInvoker = null;

        public CustomOperationInvoker(IOperationInvoker innerInvoker)
        {
            _innerInvoker = innerInvoker;
        }

        public object[] AllocateInputs()
        {
            return _innerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            // check if the unhandled request is due to preflight checks (OPTIONS header)
            if (OperationContext.Current.Extensions.Find<PreflightDetected>() != null)
            {
                // override the standard error handling, so the request won't contain an error
                outputs = null;
                return null;
            }

            // no preflight - probably a missed call (wrong URI or method)
            return _innerInvoker.Invoke(instance, inputs, out outputs);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            // not supported - an exception will be thrown
            return _innerInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            // not supported - an exception will be thrown
            return _innerInvoker.InvokeEnd(instance, out outputs, result);
        }

        public bool IsSynchronous
        {
            get
            {
                return _innerInvoker.IsSynchronous;
            }
        }
    }
}