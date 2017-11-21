using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigNet;
using RestApiClient;
using SharedTemplate;
using Utils;

namespace ApiAdminClientTest
{
    public partial class RegisterUser : Form
    {
        public RegisterUser()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var userName = tbUserName.Text;
            var roles = cbRoles.Text;
            var userPassword = tbPassword.Text;
            var userPasswordRt = tbRePassword.Text;


            if (userPassword != userPasswordRt)
            {
                lblResult.Text = "Passwords don't match!";
                return;
            }

            AsyncMethod(client, userName, userPassword, roles);
        }

        private async Task AsyncMethod(ApiAdministrationClient client, string userName, string userPassword, string roles)
        {
            Monitor.Enter(this);
            try
            {
                var res = await client.RegisterNewUserAsync(userName, userPassword, roles);
                if (res.IsSuccess)
                    lblResult.Text = "Success";
                else lblResult.Text = (res.Error);
            }
            catch (Exception e)
            {
                lblResult.Text = e.Message;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private ApiAdministrationClient client;

        private void RegisterUser_Load(object sender, EventArgs e)
        {
            cbRoles.DataSource = typeof(RoleNames).GetFields().Select(f => f.GetValue(null)).ToList();
            var config = ConfigReader.ReadFromSettings<ApiContext>();
            LinqPadLikeExtensions.Init(s => lblResult.Text = s);

            string admniUserName;
            string adminPassword;
            if (!string.IsNullOrEmpty(config.ApiCredentials))
            {
                var credentials =
                    CredentialParser.ParseCredentials(config
                        .ApiCredentials);
                admniUserName = credentials.UserName;
                adminPassword = credentials.Password;
                tbAdminUserName.Text = admniUserName;
                tbAdminPassword.Text = adminPassword;
            }
            admniUserName = tbAdminUserName.Text;
            adminPassword = tbAdminPassword.Text;

            client = new ApiAdministrationClient(admniUserName, adminPassword,
                //apiUrl: "http://api.test.com:8082/",
                apiUrl: config.ApiUrl,
                tokenSubUrl: config.ApiTokenUrl,
                logger: s => lblResult.Text = s);
            tbUserName.Text = "test";
            tbPassword.Text = "asdfqwer1234!@#$";
            tbRePassword.Text = tbPassword.Text;
            cbRoles.Text = "Users";
        }
    }
}
