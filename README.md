<h1>Seekios Web Api</h1>

Seekios Web Api are two WCF services coded in C#. 
On service named <i>seekios embedded</i> used by the devices and the other named <i>seekios</i> used by the client (web app and mobile app).

<h3>Seekios Web Api for embedded devices</h3>
All the endpoints are accessibles by GET only because the hardware seekios handle only the GET verb HTTP. 
The list of the endpoints available:
<ul>
  <li><b>Get Seekios Instructions</b><br>GSI/{uidSeekios}/{battery}/{signal}/{isDateNeeded}/{timestamp}</li>
  <li><b>Respond On Demand Request</b><br>RODR/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}</li>
  <li><b>Respond On Demand Request By Cells Data</b><br>RODRBCD/{uidSeekios}/{battery}/{signal}/{cellsData}/{timestamp}</li>
  <li><b>Notify Seekios Out Of Zone</b><br>NSOOZ/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}</li>
  <li><b>Notify Seekios Moved</b><br>NSM/{uidSeekios}/{battery}/{signal}/{timestamp}/{modeId}</li>
  <li><b>Add New Tracking Location</b><br>ANTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}</li>
  <li><b>Add New Zone Tracking Location</b><br>ANZTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}</li>
  <li><b>Add New Dont Move Tracking Location</b><br>ANDMTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}</li>
  <li><b>Send SOS</b><br>SSOS/{uidSeekios}/{battery}/{signal}/{timestamp}</li>
  <li><b>Send SOS Location</b><br>SSOSL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}</li>
  <li><b>Send SOS Location By Cells Data</b><br>SSOSLBCD/{uidSeekios}/{battery}/{signal}/{cellsData}/{timestamp}</li>
  <li><b>Add New Seekios Hardware Report</b><br>SHR/{IMEI}/{UID}/{IMSI}/{MacAddress}/{BatteryLevel}/{Timestamp}/{BoolReport}/{OSVersion}</li>
  <li><b>Update Seekios Version</b><br>USV/{UID}/{battery}/{signal}/{version}/{timestamp}</li>
  <li><b>Critical Battery Alert</b><br>CBA/{uidSeekios}/{battery}/{signal}/{timestamp}</li>
  <li><b>Power Saving Disabled</b><br>PSD/{uidSeekios}/{battery}/{signal}/{timestamp}/{modeId}</li>
</ul>
<br>
<br>
<h3>Seekios Web Api for clients</h3>
The list of the endpoints available:
<ul>
  <li><b>GET Login</b><br>Login/{email}/{password}</li>
  <li><b>GET Register Device</b><br>RegisterDevice/{deviceModel}/{platform}/{version}/{uidDevice}/{countryCode}</li>
  <li><b>GET Register Device</b><br>UnregisterDevice/{uidDevice}</li>
  <li>(Comming soom) ...</li>
</ul>
