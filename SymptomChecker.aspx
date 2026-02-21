<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
         CodeFile="SymptomChecker.aspx.cs" Inherits="SymptomChecker"
         Title="Symptom Checker | Portmore Medical Center" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<style>
  #sc-wrapper { max-width: 680px; margin: 44px auto 60px; }
  #sc-wrapper h2 { color: #337ab7; margin-bottom: 6px; }
  #sc-wrapper .subhead { color: #666; margin-bottom: 24px; font-size: 14px; }
  #sc-textarea {
    width: 100%; height: 130px; border: 1px solid #ccc; border-radius: 6px;
    padding: 12px; font-size: 14px; resize: vertical;
  }
  #sc-textarea:focus { border-color: #337ab7; outline: none; }
  #sc-btn {
    margin-top: 12px; background: #337ab7; color: #fff; border: none;
    border-radius: 6px; padding: 10px 28px; font-size: 15px; cursor: pointer;
  }
  #sc-btn:disabled { background: #aaa; cursor: not-allowed; }
  #sc-result {
    display: none; margin-top: 28px;
    border: 1px solid #b8daf6; border-radius: 8px; background: #eaf4fd; padding: 20px 24px;
  }
  #sc-result h4 { color: #1a6496; margin: 0 0 8px; font-size: 18px; }
  #sc-result p  { color: #333; margin: 0 0 16px; font-size: 14px; }
  #sc-book-btn  {
    display: inline-block; background: #5cb85c; color: #fff;
    padding: 8px 22px; border-radius: 6px; text-decoration: none; font-size: 14px;
  }
  #sc-book-btn:hover { background: #449d44; color: #fff; text-decoration: none; }
  #sc-error  { display: none; color: #a94442; margin-top: 12px; font-size: 14px; }
  #sc-spinner { display: none; margin-top: 12px; color: #888; font-size: 14px; font-style: italic; }
  .disclaimer { margin-top: 20px; font-size: 12px; color: #888; }
  #char-info   { font-size: 11px; color: #999; text-align: right; margin-top: 4px; }
</style>

<div id="sc-wrapper">
  <h2><span class="glyphicon glyphicon-search"></span> Symptom Checker</h2>
  <p class="subhead">Describe your symptoms and our AI will suggest the right specialist service to book.</p>

  <textarea id="sc-textarea" placeholder="e.g. I have been experiencing chest pain and shortness of breath for the past two days…" maxlength="1000"></textarea>
  <div id="char-info">0 / 1000</div>

  <button id="sc-btn">Check My Symptoms</button>
  <div id="sc-spinner"><span class="glyphicon glyphicon-refresh"></span> Analysing your symptoms…</div>
  <div id="sc-error"></div>

  <div id="sc-result">
    <h4>Recommended: <span id="sc-specialist"></span></h4>
    <p id="sc-reason"></p>
    <a id="sc-book-btn" href="AppointmentForm.aspx">Book an Appointment &rarr;</a>
  </div>

  <p class="disclaimer">
    <span class="glyphicon glyphicon-info-sign"></span>
    This tool provides general guidance only and is not a medical diagnosis.
    Always consult a qualified healthcare professional. For emergencies, call 911 or visit your nearest emergency room.
  </p>
</div>

<script>
(function () {
  var textarea  = document.getElementById('sc-textarea');
  var btn       = document.getElementById('sc-btn');
  var spinner   = document.getElementById('sc-spinner');
  var errorDiv  = document.getElementById('sc-error');
  var resultDiv = document.getElementById('sc-result');
  var specEl    = document.getElementById('sc-specialist');
  var reasonEl  = document.getElementById('sc-reason');
  var charInfo  = document.getElementById('char-info');

  textarea.addEventListener('input', function () {
    charInfo.innerText = textarea.value.length + ' / 1000';
  });

  btn.addEventListener('click', function () {
    var text = textarea.value.trim();
    if (!text) { errorDiv.style.display = 'block'; errorDiv.innerText = 'Please describe your symptoms first.'; return; }

    btn.disabled     = true;
    spinner.style.display  = 'block';
    errorDiv.style.display = 'none';
    resultDiv.style.display = 'none';

    $.ajax({
      url:  'SymptomHandler.ashx',
      type: 'POST',
      data: { symptoms: text },
      success: function (data) {
        if (data.error) {
          errorDiv.style.display = 'block';
          errorDiv.innerText = 'Sorry, something went wrong. Please try again.';
        } else {
          specEl.innerText  = data.specialist;
          reasonEl.innerText = data.reason;
          resultDiv.style.display = 'block';
        }
      },
      error: function () {
        errorDiv.style.display = 'block';
        errorDiv.innerText = 'Could not connect. Please try again shortly.';
      },
      complete: function () {
        btn.disabled = false;
        spinner.style.display = 'none';
      }
    });
  });
})();
</script>

</asp:Content>
