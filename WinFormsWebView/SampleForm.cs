﻿using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.WebView.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace WinForms
{
    public partial class SampleForm : Form
    {
        private OidcClient _oidcClient;
        private HttpClient _apiClient;

        public SampleForm()
        {
            InitializeComponent();

            var options = new OidcClientOptions
            {
                Authority = "https://demo.identityserver.io",
                ClientId = "native.hybrid",
                Scope = "openid email api offline_access",
                RedirectUri = "http://localhost/winforms.client",

                Browser = new WinFormsWebView()
            };

            _oidcClient = new OidcClient(options);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            AccessTokenDisplay.Clear();
            OtherDataDisplay.Clear();

            var result = await _oidcClient.LoginAsync(DisplayMode.Visible);

            if (result.IsError)
            {
                MessageBox.Show(this, result.Error, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                AccessTokenDisplay.Text = result.AccessToken;

                var sb = new StringBuilder(128);
                foreach (var claim in result.User.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                {
                    sb.AppendLine($"refresh token: {result.RefreshToken}");
                }

                OtherDataDisplay.Text = sb.ToString();

                _apiClient = new HttpClient(result.RefreshTokenHandler);
                _apiClient.BaseAddress = new Uri("https://api.identityserver.io/");
            }
        }

        private async void LogoutButton_Click(object sender, EventArgs e)
        {
            //await _oidcClient.LogoutAsync(trySilent: Silent.Checked);
            //AccessTokenDisplay.Clear();
            //OtherDataDisplay.Clear();
        }

        private async void CallApiButton_Click(object sender, EventArgs e)
        {
            if (_apiClient == null)
            {
                return;
            }

            var result = await _apiClient.GetAsync("identity");
            if (result.IsSuccessStatusCode)
            {
                OtherDataDisplay.Text = JArray.Parse(await result.Content.ReadAsStringAsync()).ToString();
            }
            else
            {
                OtherDataDisplay.Text = result.ReasonPhrase;
            }
        }
    }
}