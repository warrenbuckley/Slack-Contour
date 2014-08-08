using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;

namespace Slack.Contour
{
    public class SlackContourWorkflow : WorkflowType
    {
        [Setting("API Token", description = "Slack API token", control = "Umbraco.Forms.Core.FieldSetting.TextField")]
        public string Token { get; set; }

        [Setting("Channel", description = "Slack channel to post to", control = "Umbraco.Forms.Core.FieldSetting.TextField")]
        public string Channel { get; set; }

        [Setting("Username", description = "The username/bot to post as", control = "Umbraco.Forms.Core.FieldSetting.TextField")]
        public string Username { get; set; }

        [Setting("Avatar URL", description = "The full url including http to the avatar image", control = "Umbraco.Forms.Core.FieldSetting.TextField")]
        public string AvatarUrl { get; set; }


        public SlackContourWorkflow()
        {
            this.Id             = new Guid("3edfcb87-4498-4dad-ab16-ba47abe3c8f4");
            this.Name           = "Post form to Slack Chat";
            this.Description    = "Posts the form data to a specific channel on Slack Chat";
        }


        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            //Tokenise message with values in form
            var message = string.Format("Someone has posted the form '{0}' at {1}{2}", e.Form.Name, record.Created.ToString("dd/MM/yy @ HH:mm:ss"), Environment.NewLine);
            
            //Loop over fields & add to our message
            foreach (var field in record.RecordFields)
            {
                //Record field item
                var fieldItem   = field.Value;
                var fieldName   = fieldItem.Field.Caption;
                var fieldValue  = fieldItem.ValuesAsString();

                //Add the field name & value to the message string
                message += string.Format("{0}: {1}{2}", fieldName, fieldValue, Environment.NewLine);
            }


            //Ensure Channel starts with #
            if (!Channel.StartsWith("#"))
            {
                Channel = string.Format("#{0}", Channel);
            }


            using (var client = new WebClient())
            {
                //Set the form data values
                var values = new NameValueCollection();
                values.Add("channel", Channel);
                values.Add("token", Token);
                values.Add("username", Username);
                values.Add("icon_url", AvatarUrl);
                values.Add("text", message);

                try
                {
                    //Post the data to Slack API
                    var data = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", values);

                    //Try & decode the JSOn string back from Slack API
                    var response = client.Encoding.GetString(data);

                    //Log the response from Slack
                    Umbraco.Core.Logging.LogHelper.Debug<SlackContourWorkflow>("Slack.Contour - Return API Value:" + response);


                    //All is OK return as completed
                    return WorkflowExecutionStatus.Completed;
                }
                catch (WebException ex)
                {
                    //Log the exception/error
                    Umbraco.Core.Logging.LogHelper.Error<SlackContourWorkflow>("Error in Slack.Contour", ex);

                    //Notify Contour that Workflow Failed
                    return WorkflowExecutionStatus.Failed;
                }
            }
        }

        public override List<Exception> ValidateSettings()
        {
            var errors = new List<Exception>();

            if (string.IsNullOrEmpty(Token))
            {
                errors.Add(new Exception("'API Token' setting has not been set"));
            }

            if (string.IsNullOrEmpty(Channel))
            {
                errors.Add(new Exception("'Channel' setting has not been set"));
            }

            if (!Channel.StartsWith("#"))
            {
                errors.Add(new Exception("'Channel' is missing # at beginning"));
            }

            if (string.IsNullOrEmpty(Username))
            {
                errors.Add(new Exception("'Username' setting has not been set"));
            }

            if (string.IsNullOrEmpty(AvatarUrl))
            {
                errors.Add(new Exception("'Avatar URL' setting has not been set"));
            }

            return errors;
        }
    }
}
