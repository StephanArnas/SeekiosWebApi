using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using WCFServiceWebRole.Data.ERROR;

namespace WCFServiceWebRole.ExceptionLog
{
    public class ErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error) { return true; }

        /// <summary>
        /// Custom behavior when an exception is raised
        /// </summary>
        /// <param name="error">exception error</param>
        /// <param name="version">MessageVersion version (not used)</param>
        /// <param name="fault">Message fault (not used)</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            // get the json if it's a DefaultCustomError
            var defaultCustomError = string.Empty;
            if (error is WebFaultException<DefaultCustomError>)
            {
                var detail = ((WebFaultException<DefaultCustomError>)error).Detail;
                defaultCustomError = string.Format("{0} - {1} - {2}", detail.ErrorCode, detail.ErrorInfo, detail.ErrorDetails);
                // bypass those errors, it's indicate a new update for android or ios is available
                if (detail.ErrorCode == "0x0102" || detail.ErrorCode == "0x0103") return;
            }
            // log the exception in the database
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var log = new logExceptionSeekios()
                {
                    data = OperationContext.Current?.RequestContext.RequestMessage == null ? string.Empty : OperationContext.Current.RequestContext.RequestMessage.ToString(),
                    exceptionDate = DateTime.UtcNow,
                    exceptionMessage = error.Message,
                    defaultCustomError = defaultCustomError,
                    headers = WebOperationContext.Current?.IncomingRequest.Headers.ToString(),
                    innerException = error.InnerException == null ? string.Empty : GetInnersExceptionsRecursive(error.InnerException),
                    method = WebOperationContext.Current?.IncomingRequest.Method,
                    url = OperationContext.Current?.IncomingMessageProperties.Via.PathAndQuery
                };
                seekiosEntities.logExceptionSeekios.Add(log);
                seekiosEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Recursive method to get all the InnerException
        /// </summary>
        /// <param name="ex">exception</param>
        /// <returns>return the InnerException</returns>
        private static string GetInnersExceptionsRecursive(Exception ex)
        {
            return string.Format(" - {0}{1}"
                , ex.Message
                , ex.InnerException == null ? string.Empty : Environment.NewLine + GetInnersExceptionsRecursive(ex.InnerException)
                );
        }
    }
}