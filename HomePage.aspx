<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HomePage.aspx.cs" Inherits="HomePage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="carousel-example-generic" class="carousel slide" data-ride="carousel">
  <!-- Indicators -->
  <ol class="carousel-indicators">
    <li data-target="#carousel-example-generic" data-slide-to="0" class="active"></li>
    <li data-target="#carousel-example-generic" data-slide-to="1"></li>
    <li data-target="#carousel-example-generic" data-slide-to="2"></li>
      <li data-target="#carousel-example-generic" data-slide-to="3"></li>
      <li data-target="#carousel-example-generic" data-slide-to="4"></li>
      <li data-target="#carousel-example-generic" data-slide-to="5"></li>
     </ol>

  <!-- Wrapper for slides -->
  <div class="carousel-inner" role="listbox">
    <div class="item active">
     <center> <img src="images/LVHM_ED.jpg" alt="Portmore Medical Center" width="1000" height="350"/></center>
      <div class="carousel-caption">
        <h1>Welcome To Portmore Medical Center</h1>
          ...
      </div>
    </div>
    <div class="item">
    <center><img src="images/1 (1).jpg" alt="WaitingArea" width="1000" height="350"/></center>
      <div class="carousel-caption">
        ...
      </div>
    </div>
    <div class="item">
      <center><img src="images/images-of-a-group-of- hhdoctors-6930.jpg" alt="DoctorOffice" width="1000" height="350"/></center>
      <div class="carousel-caption">
        ...
      </div>
  </div>
       <div class="item">
     <center><img src="images/doctors-office-waiting-room-17dwxkz.jpg" alt="CheckUpArea" width="1000" height="350"/></center>
      <div class="carousel-caption">
        ...
      </div>
        </div>
       <div class="item">
      <center><img src="images/10.jpg" alt="Picture8"  width="1000" height="350"/></center>
      <div class="carousel-caption">
        ...
      </div>
      </div>
       <div class="item">
      <center><img src="images/uuu.jpg" alt="CheckUpArea2" width="1000" height="350"/></center>
      <div class="carousel-caption">
        ...
      </div>
  </div>

  <!-- Controls -->
  <a class="left carousel-control" href="#carousel-example-generic" role="button" data-slide="prev">
    <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
    <span class="sr-only">Previous</span>
  </a>
  <a class="right carousel-control" href="#carousel-example-generic" role="button" data-slide="next">
    <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
    <span class="sr-only">Next</span>
  </a>
</div>
  </div>           
             <!----Carousel------->



             <!------- Start Container------->
<!----<div class="well">------->
            <center><h1> HEALTH CARE MATTERS at Portmore Medical Center</h1></center>
             <div class="container">
             <div class="col-58percent-sm">

<p>Welcome to Portmore Medical Center. We’re a nationally recognized academic medical center offering primary care for all ages, specialty care in 10 fields, and the latest treatment options and expertise for the most complex health conditions. Some highlights about us:</p>
<ul>
<li>Nationally ranked in multiple specialties</li>
<li>Home of nationally ranked children’s hospital</li>
<li>One of “Most Wired” Jamaican hospitals</li>
<li>Top honors for nursing excellence</li>
<li>Patient-centered primary care model</li>
</ul>
<p><a href="Vision Mission.aspx"><strong>Learn more</strong> »</a></p>
<!---</div>---->
</div>
</div>
             <div class="container">
             <div class="panel panel-variation-2">
			 <div class="col-58percent-sm">			         
								 
								 
									<div class="panel-heading">
										 
										
											<center><h1 class="panel-title"><strong>Check up on Health</strong></h1></center>
											
						          </div>  
								  
								  
						         
						                <div class="panel-body">						             
						                  
											  
                <center>
                    <h4>
                        <span class="glyphicon glyphicon-heart" style="color:#c0392b;"></span>
                        Today’s AI Health Tip
                        <small style="font-size:11px;color:#aaa;margin-left:8px;">Updated daily &middot; Powered by GPT-4</small>
                    </h4>
                </center>
<p>
    <img alt="Doctor" class="img-responsive-left" src="images/inderrrrrrrrrrrwwwwwx.jpg" width="50"/>
    <asp:Literal ID="LitHealthTip" runat="server" />
</p>
<ul>
<li><a href="SymptomChecker.aspx" title="AI Symptom Checker">Not sure which doctor to see? Try our AI Symptom Checker</a></li>
<li><a href="AIChatAssistant.aspx" title="AI Assistant">Chat with our AI Assistant for clinic information</a></li>
</ul>
<p>&nbsp;</p>
<div class="mcePaste" id="_mcePaste" style="position: absolute; width: 1px; height: 1px; overflow: hidden; top: -25px; left: -40px;"></div>
												
											  
											 
						           		</div>
						             </div>
                 </div>
             </div>
     <footer>
        <div class="container">
            <p class="pull-right"><a href="#">Go Back Up</a></p>
            <p class="pull-down">&copy; 2011 MedicalCenter.com &middot;<a href="HomePage.aspx">Home</a>&middot;<a href="#">About Us</a>&middot;<a href="#">Service</a>&middot;<a href="#">Bio</a></p>
        </div>
    </footer>
     <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" ></script>
    </asp:Content>

