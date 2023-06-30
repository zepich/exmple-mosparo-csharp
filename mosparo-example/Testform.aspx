<%@ Page Title="Testform" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Testform.aspx.cs" Inherits="mosparo_example.Testform" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="contact_form">
        <asp:Label id="PageMessage" runat="server"/>
        <div class="row mb-3">
            <label class="col-sm-3 col-form-label required" for="first-name">First name</label>
            <div class="col-sm-9">
                <input type="text" name="first-name" id="first-name" class="form-control" required />
            </div>
        </div>
        <div class="row mb-3">
            <label class="col-sm-3 col-form-label required" for="last-name">Last name</label>
            <div class="col-sm-9">
                <input type="text" name="last-name" id="last-name" class="form-control" required />
            </div>
        </div>
        <div class="row mb-3">
            <label class="col-sm-3 col-form-label required" for="email-address">Email address</label>
            <div class="col-sm-9">
                <input type="email" name="email-address" id="email-address" class="form-control" required />
            </div>
        </div>
        <div class="row mb-3">
            <label class="col-sm-3 col-form-label" for="website">Website</label>
            <div class="col-sm-9">
                <input type="url" name="website" id="website" class="form-control" placeholder="https://" />
            </div>
        </div>
        <div class="row mb-3">
            <label class="col-sm-3 col-form-label required" for="message">Message</label>
            <div class="col-sm-9">
                <textarea class="form-control" name="message" id="message" style="height: 300px;" required></textarea>
            </div>
        </div>

        <div class="row my-5">
            <div class="col-sm-3"></div>
            <div class="col-sm-9">
                <div id="mosparo-box"></div>
            </div>
        </div>

        <div class="row mb-3">
            <div class="col-sm-3"></div>
            <div class="col-sm-9">
                <button type="submit" name="submitted" class="btn btn-primary btn-lg">
                    Submit
                </button>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="FormJavascript" ContentPlaceHolderID="JavascriptContent" runat="server">
    <script src="<%: ConfigurationManager.AppSettings["mosparoHost"] %>/build/mosparo-frontend.js" defer></script>
    <script>
        var m;
        window.onload = function(){
            m = new mosparo(
                'mosparo-box',
                '<%: ConfigurationManager.AppSettings["mosparoHost"] %>',
                '<%: ConfigurationManager.AppSettings["mosparoUuid"] %>',
                '<%: ConfigurationManager.AppSettings["mosparoPublicKey"] %>',
                {
                    loadCssResource: true,
                    onCheckForm: function (valid) {
                        console.log('onCheckForm', this, valid);
                    },
                    onResetState: function () {
                        console.log('onResetState', this);
                    },
                    onAbortSubmit: function () {
                        console.log('onAbortSubmit', this);
                    }
                }
            );

            document.getElementById('contact_form').addEventListener('form-checked', function (ev) {
                console.log(ev);
            });

            document.getElementById('contact_form').addEventListener('submit-aborted', function (ev) {
                console.log(ev);
            });

            document.getElementById('contact_form').addEventListener('state-reseted', function (ev) {
                console.log(ev);
            });
        };
    </script>
</asp:Content>
