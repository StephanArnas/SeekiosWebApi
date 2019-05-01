using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web;

namespace WCFEmbeddedServiceWebRole
{
    // Not use
    public class CustomInspector : IClientMessageInspector
    {
        private StringBuilder _output = null;

        public CustomInspector(ref StringBuilder output) : base()
        {
            _output = output;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (_output != null) _output.Append("reply = "+reply.ToString() + "<br/>");
            //System.IO.File.WriteAllText(@"./reply.txt", reply.ToString());
        }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            //System.IO.File.WriteAllText(@"./request.txt", request.ToString());
            if (_output != null) _output.Append("request = "+request.ToString() + "<br/>");
            return null;
        }
    }
}