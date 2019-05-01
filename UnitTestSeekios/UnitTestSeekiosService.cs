using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestSeekios
{
    [TestClass]
    public class UnitTestSeekiosService
    {
        private WCFServiceWebRole.ISeekiosService _seekiosService = null;

        public UnitTestSeekiosService()
        {
            // if you have an error 'System.BadImageFormatException' you need to set up your project with x64.
            // go to : Test -> Test Settings -> Default Processor architecture -> x86
            _seekiosService = new WCFServiceWebRole.SeekiosService();
        }

        [TestMethod]
        public void ConnectUser()
        {

        }
    }
}
