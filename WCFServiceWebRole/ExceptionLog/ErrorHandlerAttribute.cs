using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFServiceWebRole.ExceptionLog
{
    public class ErrorHandlerAttribute : Attribute, IServiceBehavior
    {
        private readonly Type errorHandlerType;

        /// <summary>
        /// Dependency injection to dynamically inject error handler 
        /// if we have multiple global error handlers
        /// </summary>
        public ErrorHandlerAttribute(Type errorHandlerType)
        {
            this.errorHandlerType = errorHandlerType;
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description
            , ServiceHostBase serviceHostBase
            , Collection<ServiceEndpoint> endpoints
            , BindingParameterCollection parameters) { }

        void IServiceBehavior.Validate(ServiceDescription description
            , ServiceHostBase serviceHostBase) { }

        /// <summary>
        /// Registering the instance of global error handler in 
        /// dispatch behavior of the service
        /// </summary>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description
            , ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler;
            try
            {
                errorHandler = (IErrorHandler)Activator.CreateInstance(errorHandlerType);
            }
            catch (MissingMethodException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have a public empty constructor.", e);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.", e);
            }
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }
    }
}