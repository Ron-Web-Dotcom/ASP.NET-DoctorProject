<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
         CodeFile="AIChatAssistant.aspx.cs" Inherits="AIChatAssistant" Title="AI Assistant | Portmore Medical Center" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<style>
  #chat-wrapper {
    max-width: 720px;
    margin: 40px auto 60px;
    font-family: inherit;
  }
  #chat-header {
    background: #337ab7;
    color: #fff;
    padding: 16px 20px;
    border-radius: 8px 8px 0 0;
    display: flex;
    align-items: center;
    gap: 10px;
  }
  #chat-header h4 { margin: 0; font-size: 18px; }
  #chat-header small { opacity: .8; font-size: 12px; }
  #chat-window {
    background: #f9f9f9;
    border: 1px solid #ddd;
    border-top: none;
    height: 420px;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }
  .bubble {
    max-width: 78%;
    padding: 10px 14px;
    border-radius: 16px;
    line-height: 1.5;
    font-size: 14px;
    word-wrap: break-word;
  }
  .bubble.user {
    background: #337ab7;
    color: #fff;
    align-self: flex-end;
    border-bottom-right-radius: 4px;
  }
  .bubble.assistant {
    background: #fff;
    border: 1px solid #ddd;
    color: #333;
    align-self: flex-start;
    border-bottom-left-radius: 4px;
  }
  .bubble.typing {
    background: #fff;
    border: 1px solid #ddd;
    align-self: flex-start;
    border-bottom-left-radius: 4px;
    color: #888;
    font-style: italic;
  }
  #chat-footer {
    border: 1px solid #ddd;
    border-top: none;
    border-radius: 0 0 8px 8px;
    background: #fff;
    padding: 12px;
    display: flex;
    gap: 8px;
  }
  #chat-input {
    flex: 1;
    border: 1px solid #ccc;
    border-radius: 20px;
    padding: 8px 16px;
    font-size: 14px;
    outline: none;
  }
  #chat-input:focus { border-color: #337ab7; }
  #send-btn {
    background: #337ab7;
    color: #fff;
    border: none;
    border-radius: 20px;
    padding: 8px 20px;
    cursor: pointer;
    font-size: 14px;
  }
  #send-btn:disabled { background: #aaa; cursor: not-allowed; }
  #char-count { font-size: 11px; color: #999; text-align: right; margin-top: 4px; }
  .disclaimer {
    text-align: center;
    font-size: 12px;
    color: #888;
    margin-top: 10px;
  }
</style>

<div id="chat-wrapper">
  <div id="chat-header">
    <span class="glyphicon glyphicon-comment" style="font-size:22px;"></span>
    <div>
      <h4>Portmore Medical Center — AI Assistant</h4>
      <small>Powered by GPT-4 &middot; Ask me anything about our services, doctors, or appointments</small>
    </div>
  </div>

  <div id="chat-window">
    <div class="bubble assistant">
      Hi there! I'm the Portmore Medical Center AI Assistant. I can help you with information about our services,
      how to book an appointment, or answer general questions about the clinic. How can I help you today?
    </div>
  </div>

  <div id="chat-footer">
    <input id="chat-input" type="text" placeholder="Type your message…" maxlength="1000" autocomplete="off" />
    <button id="send-btn">Send</button>
  </div>
  <div id="char-count">0 / 1000</div>
  <p class="disclaimer">
    <span class="glyphicon glyphicon-info-sign"></span>
    This assistant provides general information only and is not a substitute for professional medical advice.
    For emergencies, call 911 or go to your nearest emergency room.
  </p>
</div>

<script>
(function () {
  var window$ = document.getElementById('chat-window');
  var input   = document.getElementById('chat-input');
  var sendBtn = document.getElementById('send-btn');
  var charCount = document.getElementById('char-count');

  // Conversation history sent to server for context (last 10 turns)
  var history = [];
  var MAX_HISTORY = 10;

  function scrollBottom() {
    window$.scrollTop = window$.scrollHeight;
  }

  function addBubble(role, text) {
    var div = document.createElement('div');
    div.className = 'bubble ' + role;
    div.innerText = text;
    window$.appendChild(div);
    scrollBottom();
    return div;
  }

  function setLoading(on) {
    sendBtn.disabled = on;
    input.disabled   = on;
  }

  function sendMessage() {
    var msg = input.value.trim();
    if (!msg) return;

    addBubble('user', msg);
    input.value = '';
    charCount.innerText = '0 / 1000';
    setLoading(true);

    var typingBubble = addBubble('typing', 'Assistant is typing…');

    var historyJson = history.length ? JSON.stringify(history) : '';

    $.ajax({
      url:  'ChatHandler.ashx',
      type: 'POST',
      data: { message: msg, history: historyJson },
      success: function (data) {
        window$.removeChild(typingBubble);
        var reply = (data && data.reply) ? data.reply : 'Sorry, I could not get a response.';
        addBubble('assistant', reply);

        // Update rolling history (keep last MAX_HISTORY turns)
        history.push({ role: 'user',      content: msg   });
        history.push({ role: 'assistant', content: reply });
        if (history.length > MAX_HISTORY * 2) history = history.slice(-MAX_HISTORY * 2);
      },
      error: function () {
        window$.removeChild(typingBubble);
        addBubble('assistant', 'I\'m sorry, I could not connect. Please try again or contact us via the Contact Form.');
      },
      complete: function () {
        setLoading(false);
        input.focus();
      }
    });
  }

  sendBtn.addEventListener('click', sendMessage);

  input.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
  });

  input.addEventListener('input', function () {
    charCount.innerText = input.value.length + ' / 1000';
  });

  scrollBottom();
})();
</script>

</asp:Content>
