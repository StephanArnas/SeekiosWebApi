using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using SendGrid;
using System.Linq;
using System.Collections.Generic;

namespace WCFServiceWebRole.Helper
{
    public class SendGridHelper
    {
        #region ----- VARIABLES -------------------------------------------------------------------------------

        private const string _API_KEY = "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";
        private const string _EMAIL_SENDER = "alert@seekios.com";
        private const string _EMAIL_HELLO = "bonjour@seekios.com"; //est-ce qu'on mettrait une adresse hello@seekios.com pour les anglophones ?
        private const string _ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE = "text/html";

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        public static async void SendZoneAlertEmail(seekios_dbEntities seekiosEntities
            , string emailTo
            , string firstName
            , string lastName
            , string seekiosName
            , string title
            , string alertContent
            , double? lat
            , double? longi
            , int idcountryResources)
        {
            var latitude = lat != null ? lat.ToString().Replace(",", ".") : string.Empty;
            var longitude = longi != null ? longi.ToString().Replace(",", ".") : string.Empty;

            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new EmailAddress(emailTo);

            // add the text body
            var finalAlertContent = contentEmailBdd.alertZoneEmailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", string.IsNullOrEmpty(title) ? string.Empty : title)
                .Replace("{4}", string.IsNullOrEmpty(alertContent) ? string.Empty : alertContent)
                .Replace("{5}", latitude).Replace("{6}", longitude);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.alertZoneEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendZoneAlertEmail(seekios_dbEntities seekiosEntities
            , IEnumerable<string> emailsTo
            , string firstName
            , string lastName
            , string seekiosName
            , string title
            , string alertContent
            , double? lat
            , double? longi
            , int idcountryResources)
        {
            var latitude = lat != null ? lat.ToString().Replace(",", ".") : string.Empty;
            var longitude = longi != null ? longi.ToString().Replace(",", ".") : string.Empty;

            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new List<EmailAddress>();
            foreach (var email in emailsTo)
            {
                emailSendTo.Add(new EmailAddress() { Email = email });
            }

            // add the text body
            var finalAlertContent = contentEmailBdd.alertZoneEmailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", string.IsNullOrEmpty(title) ? string.Empty : title)
                .Replace("{4}", string.IsNullOrEmpty(alertContent) ? string.Empty : alertContent)
                .Replace("{5}", latitude).Replace("{6}", longitude);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmailToMultipleRecipients(emailSendFrom, emailSendTo, contentEmailBdd.alertZoneEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendMoveAlertEmail(seekios_dbEntities seekiosEntities
            , string emailTo
            , string firstName
            , string lastName
            , string seekiosName
            , string title
            , string alertContent
            , int idcountryResources)
        {
            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new EmailAddress(emailTo);

            // add the text body
            var finalAlertContent = contentEmailBdd.alertMoveEmailContent
                .Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", string.IsNullOrEmpty(title) ? string.Empty : title)
                .Replace("{4}", string.IsNullOrEmpty(alertContent) ? string.Empty : alertContent);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.alertMoveEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendMoveAlertEmail(seekios_dbEntities seekiosEntities
            , IEnumerable<string> emailsTo
            , string firstName
            , string lastName
            , string seekiosName
            , string title
            , string alertContent
            , int idcountryResources)
        {
            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new List<EmailAddress>();
            foreach (var email in emailsTo)
            {
                emailSendTo.Add(new EmailAddress() { Email = email });
            }

            // add the text body
            var finalAlertContent = contentEmailBdd.alertMoveEmailContent
                .Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", string.IsNullOrEmpty(title) ? string.Empty : title)
                .Replace("{4}", string.IsNullOrEmpty(alertContent) ? string.Empty : alertContent);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmailToMultipleRecipients(emailSendFrom, emailSendTo, contentEmailBdd.alertMoveEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendAlertSOSEmail(seekios_dbEntities seekiosEntities
            , string emailTo
            , string firstName
            , string lastName
            , string seekiosName
            , double? lat
            , double? longi
            , string alertContent
            , int idcountryResources
            , bool isGPS = true)
        {
            var latitude = lat != null ? lat.ToString().Replace(",", ".") : string.Empty;
            var longitude = longi != null ? longi.ToString().Replace(",", ".") : string.Empty;

            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailContent = string.IsNullOrEmpty(alertContent) ? contentEmailBdd.alertSOSEmailContent : contentEmailBdd.alertSOSEmailWithMsgContent;
            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new EmailAddress(emailTo);

            // add the text body
            string finalAlertContent = string.IsNullOrEmpty(alertContent) ?
                emailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", isGPS ? ResourcesHelper.GetLocalizedString("GPS", idcountryResources) : ResourcesHelper.GetLocalizedString("ApproxPosition", idcountryResources))
                .Replace("{4}", latitude)
                .Replace("{5}", longitude)
                : emailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", alertContent)
                .Replace("{4}", isGPS ? ResourcesHelper.GetLocalizedString("GPS", idcountryResources) : ResourcesHelper.GetLocalizedString("ApproxPosition", idcountryResources))
                .Replace("{5}", latitude)
                .Replace("{6}", longitude);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.alertSOSEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendAlertSOSEmail(seekios_dbEntities seekiosEntities
            , IEnumerable<string> emailsTo
            , string firstName
            , string lastName
            , string seekiosName
            , double? lat
            , double? longi
            , string alertContent
            , int idcountryResources
            , bool isGPS = true)
        {
            var latitude = lat != null ? lat.ToString().Replace(",", ".") : string.Empty;
            var longitude = longi != null ? longi.ToString().Replace(",", ".") : string.Empty;

            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailContent = string.IsNullOrEmpty(alertContent) ? contentEmailBdd.alertSOSEmailContent : contentEmailBdd.alertSOSEmailWithMsgContent;
            var emailSendFrom = new EmailAddress(_EMAIL_SENDER);
            var emailSendTo = new List<EmailAddress>();
            foreach (var email in emailsTo)
            {
                emailSendTo.Add(new EmailAddress() { Email = email });
            }

            // add the text body
            string finalAlertContent = string.IsNullOrEmpty(alertContent) ?
                emailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", isGPS ? ResourcesHelper.GetLocalizedString("GPS", idcountryResources) : ResourcesHelper.GetLocalizedString("ApproxPosition", idcountryResources))
                .Replace("{4}", latitude)
                .Replace("{5}", longitude)
                : emailContent.Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", seekiosName)
                .Replace("{3}", alertContent)
                .Replace("{4}", isGPS ? ResourcesHelper.GetLocalizedString("GPS", idcountryResources) : ResourcesHelper.GetLocalizedString("ApproxPosition", idcountryResources))
                .Replace("{5}", latitude)
                .Replace("{6}", longitude);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmailToMultipleRecipients(emailSendFrom, emailSendTo, contentEmailBdd.alertSOSEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendEmailValidationEmail(seekios_dbEntities seekiosEntities
            , string emailToVerify
            , string firstName
            , string lastName
            , string token
            , int idcountryResources)
        {
            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_HELLO);
            var emailSendTo = new EmailAddress(emailToVerify);

            // add the text body
            string finalAlertContent = contentEmailBdd.validationEmailContent
                .Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", token);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.validationEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendResetPasswordRequestEmail(seekios_dbEntities seekiosEntities
            , string userEmail
            , string firstName
            , string lastName
            , string token
            , int idcountryResources)
        {
            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_HELLO);
            var emailSendTo = new EmailAddress(userEmail);

            // add the text body
            string finalAlertContent = contentEmailBdd.resetPassordRequestEmailContent
                .Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", token);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.resetPassordRequestEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        public static async void SendResetedPasswordEmail(seekios_dbEntities seekiosEntities
            , string userEmail
            , string firstName
            , string lastName
            , string newPassword
            , int idcountryResources)
        {
            dynamic sendGridClient = new SendGridClient(_API_KEY);
            var contentEmailBdd = (from cr in seekiosEntities.countryResources
                                   where cr.idcountryResources == idcountryResources
                                   select cr).Take(1).First();

            var emailSendFrom = new EmailAddress(_EMAIL_HELLO);
            var emailSendTo = new EmailAddress(userEmail);

            // add the text body
            string finalAlertContent = contentEmailBdd.resetPassordEmailContent
                .Replace("{0}", firstName)
                .Replace("{1}", lastName)
                .Replace("{2}", newPassword)
                .Replace("{3}", userEmail);

            var content = new Content(_ACCOUNT_VERIFICATION_EMAIL_CONTENT_TYPE, finalAlertContent);
            var mail = MailHelper.CreateSingleEmail(emailSendFrom, emailSendTo, contentEmailBdd.resetPassordEmailTitle, null, content.Value);
            var response = await sendGridClient.SendEmailAsync(mail);
        }

        #endregion
    }
}