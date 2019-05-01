using System;
using System.Net;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Collections.Generic;
using WCFServiceWebRole.Data.ERROR;
using System.ServiceModel.Web;

namespace WCFServiceWebRole.CORS
{
    public class CorsMessageInspector : IDispatchMessageInspector
    {
        private List<string> _lsNotRestrictedMethods = new List<string>()
        {
            "",
            "Login",
            "UserExists",
            "GetSeekiosHardwareReport",
            "GetSeekiosIMEIAndPIN",
            "Imei2Pin",
            "IsSeekiosVersionApplicationNeedForceUpdate",
            "LastEmbeddedVersion",
            "SeekiosVersion",
            "UpdateVersionEmbedded",
            "InsertUser",
            "ValidateUser",
            "AskForNewPassword",
            "SendNewPassword",
            "SeekiosName"
        };

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            HttpRequestMessageProperty httpRequest = request.Properties["httpRequest"] as HttpRequestMessageProperty;

            if (httpRequest != null)
            {
                // check if the client sent an "OPTIONS" request (necessary for the webapp)
                if (httpRequest.Method == "OPTIONS")
                {
                    // store the requested headers
                    OperationContext.Current.Extensions.Add(new PreflightDetected(httpRequest.Headers["Access-Control-Request-Headers"]));
                }
                else
                {
                    var methodName = OperationContext.Current.IncomingMessageProperties["HttpOperationName"].ToString();
                    var requestUrl = OperationContext.Current.IncomingMessageProperties.Via.PathAndQuery;

                    // methods allow to pass (without authentification)
                    if (_lsNotRestrictedMethods.Contains(methodName)) return 0x00;

                    // the user must have a valid token to access to the resource
                    var token = httpRequest.Headers["token"];
                    if (!string.IsNullOrEmpty(token) && token.Length == 36)
                    {
                        using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
                        {
                            var tokenFromBdd = seekiosEntities.token.FirstOrDefault(t => t.authToken == token);
                            if (tokenFromBdd == null)
                            {
                                SeekiosService.Telemetry.TrackEvent("Unauthorized access : No Token");
                                throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0000"
                                    , "unauthorized access", "no token"), HttpStatusCode.Unauthorized);
                            }
                            if (tokenFromBdd.dateExpiresToken < DateTime.UtcNow)
                            {
                                SeekiosService.Telemetry.TrackEvent("Unauthorized access : Token Expired");
                                throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0001"
                                    , "unauthorized access", "token has expired"), HttpStatusCode.Unauthorized);
                            }
                        }
                    }
                    else
                    {
                        SeekiosService.Telemetry.TrackEvent("Unauthorized access : Invalid Format Token");
                        throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0002"
                            , "unauthorized access", "no token in the HTTP header"), HttpStatusCode.Unauthorized);
                    }
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            HttpResponseMessageProperty property = null;

            if (reply == null)
            {
                // this will usually be for a preflight response
                reply = Message.CreateMessage(MessageVersion.None, null);
                property = new HttpResponseMessageProperty();
                reply.Properties[HttpResponseMessageProperty.Name] = property;
                property.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                if (reply.Properties.Any(a => a.Key == HttpResponseMessageProperty.Name))
                {
                    property = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                }
            }

            var preflightRequest = OperationContext.Current.Extensions.Find<PreflightDetected>();
            if (preflightRequest != null && property != null)
            {
                // Add allow HTTP headers to respond to the preflight request
                if (preflightRequest.RequestedHeaders == string.Empty)
                {
                    property.Headers.Add("Access-Control-Allow-Headers", "Accept");
                }
                else
                {
                    property.Headers.Add("Access-Control-Allow-Headers", preflightRequest.RequestedHeaders + ", Accept");
                }
                property.Headers.Add("Access-Control-Allow-Methods", "OPTIONS,TRACE,GET,HEAD,POST,PUT,DELETE");
            }

            // Add allow-origin header to each response message, because client expects it
            if (property != null) property.Headers.Add("Access-Control-Allow-Origin", "*");
        }
    }
}