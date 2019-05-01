using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFServiceWebRole.CORS
{
    public class CorsSupportBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // register a message inspector, and an operation invoker for undhandled operations
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorsMessageInspector());
            var invoker = endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker;
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker = new CustomOperationInvoker(invoker);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // make sure that the behavior is applied to an endpoing with WebHttp binding
            if (!(endpoint.Binding is WebHttpBinding)) throw new InvalidOperationException("The CorsSupport behavior can only be used in WebHttpBinding endpoints");
        }
    }
}