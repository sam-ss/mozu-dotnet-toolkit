using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Logging;
using Mozu.Api.ToolKit.Config;
using Mozu.Api.ToolKit.Models;

namespace Mozu.Api.ToolKit.Handlers
{
    public interface IEmailHandler
    {
        void SendErrorEmail(ErrorInfo errorInfo, string toEmail = null);
    }

    public class EmailHandler : IEmailHandler
    {
        private readonly string _supportEmail;
        private readonly string _smptServerUrl;
        private readonly string _templatePath;
        private readonly string _fromEmail;
        private readonly string _appName;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(EmailHandler));

        public EmailHandler(IAppSetting appSetting)
        {
            _smptServerUrl = appSetting.SMTPServerUrl;
            _appName = appSetting.AppName;
            if (appSetting.Settings.ContainsKey("SupportEmail"))
                _supportEmail = appSetting.Settings["SupportEmail"].ToString();

            if (appSetting.Settings.ContainsKey("FromEmail"))
                _fromEmail = appSetting.Settings["FromEmail"].ToString();

            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            _templatePath = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)), "EmailTemplates");
        }


        public void SendErrorEmail(ErrorInfo errorInfo, string toEmail = null)
        {
            if (String.IsNullOrEmpty(toEmail) && String.IsNullOrEmpty(_supportEmail)) return;

            var toEmails = new List<string>();
            if (!string.IsNullOrEmpty(toEmail))
                  toEmails = toEmail.Split(new[] { ";", ",", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var content = LoadTemplate("ErrorEmailTemplate.txt");
            content = content.Replace("#{appName}", _appName).Replace("#{message}", errorInfo.Message).Replace("#{context}", errorInfo.ApiContextStr).Replace("#{exception}", errorInfo.Exception.ToString());

            SendEmail(toEmails, content, "Integration App "+_appName+" Error Notification");
        }



        private void SendEmail(List<string> toEmails, string body, string subject)
        {
            
            if (string.IsNullOrEmpty(_fromEmail) || string.IsNullOrEmpty(_smptServerUrl))
                return;

            var userName = "";
            var password = "";
            var port = 25;
            var host = "";


            Uri uri;
            Uri.TryCreate(_smptServerUrl, UriKind.Absolute, out uri);
            if (!String.IsNullOrEmpty(uri.UserInfo))
            {
                var ui = uri.UserInfo.Split(':');
                userName = ui[0];
                password = ui[1];
            }
            host = uri.Host;
            if (uri.Port != -1) port = uri.Port;

            var client = new SmtpClient();
            client.Host = host;
            client.Port = port;
            if (!string.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(password))
            {
                client.UseDefaultCredentials = false;
                var networkCredentials = new NetworkCredential(userName, password);
                client.Credentials = networkCredentials;
            }


            var message = new MailMessage();
            var fromAdderss = new MailAddress(_fromEmail);
            message.From = fromAdderss;
            message.Body = body;
            message.Subject = subject;
            if (toEmails.Count > 0) message.Bcc.Add(_supportEmail);
            else if (!string.IsNullOrEmpty(_supportEmail)) toEmails.Add(_supportEmail);

            if (toEmails.Count == 0) return;

            foreach (var email in toEmails)
            {
                message.To.Add(email);
            }


            message.IsBodyHtml = true;


            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

      

        private string LoadTemplate(string templateFile)
        {
            var filename = Path.Combine(_templatePath, templateFile);
            var fileContents = File.ReadAllText(filename);
            return fileContents;
        }


    }
}
