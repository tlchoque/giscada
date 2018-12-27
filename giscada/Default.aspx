<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="giscada.Default" %>

<!DOCTYPE html>

<html>
<head>
    <title>GISCADA</title>
    <meta charset="utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name='viewport' content='initial-scale=1,maximum-scale=1,user-scalable=no' />

    <link rel="stylesheet" href="styles/bootstrap.min.css" />
    <link rel="stylesheet" href="styles/font-awesome.min.css" />
    <link rel="stylesheet" href='styles/mapbox-gl.css'  />

    <link rel='stylesheet' href='styles/search.css'  />
    <link rel='stylesheet' href='styles/popup.css'  />
    <link rel='stylesheet' href='styles/menu.css'  />

    <script type="text/javascript" src='scripts/mapbox-gl.js'></script>
    <script type="text/javascript" src="scripts/jquery-3.3.1.min.js"></script>
    <script type="text/javascript" src="scripts/jquery.signalR-2.3.0.js"></script>
    <script type="text/javascript" src="scripts/bootstrap.min.js"></script>
    
    <%--<script src="/signalr/hubs"></script>--%>
    <script src='<%: ResolveClientUrl("~/signalr/hubs") %>'></script> 
</head>
<body>
    <%--<form autocomplete="off"  >
        <div class="autocomplete" style="width:300px;">
        <input id="myInput" type="text" name="myCountry" placeholder="Etiqueta" >
        </div>
        <input type="submit">
    </form>--%>
    
    <%--<div id='map-menu'>
        <input id='dark-matter' type='radio' name='rtoggle' value='dark-matter' checked='checked'>
        <label for='dark-matter'>Dark</label>
        <input id='osm-bright' type='radio' name='rtoggle' value='osm-bright'>
        <label for='osm-bright'>Bright</label>
        <input id='satellite-osm-bright' type='radio' name='rtoggle' value='satellite-osm-bright'>
        <label for='satellite-osm-bright'>Satellite</label>
    </div>--%>
    
    <%--menu for map viewer--%>
    <div class="container">
      <nav class="navbar navbar-fixed-top navbar-default" role="navigation">
        <div class="container-fluid">
          <!-- Brand and toggle get grouped for better mobile display -->
          <div class="navbar-header">
            <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1">
            <span class="sr-only">Toggle navigation</span>
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
            </button>
            <a class="navbar-brand" href="#">Control de Operaciones</a>
          </div>
          <!-- Collect the nav links, forms, and other content for toggling -->
          <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
            <ul class="nav navbar-nav">
              <li class="active"><a href="#">Link</a></li>
              <li class="dropdown">
                <a href="#" class="dropdown-toggle" data-toggle="dropdown">Dropdown <b class="caret"></b></a>
                <ul class="dropdown-menu">
                  <li><a href="#">Action</a></li>
                  <li><a href="#">Another action</a></li>
                  <li><a href="#">Something else here</a></li>
                  <li class="divider"></li>
                  <li><a href="#">Separated link</a></li>
                  <li class="divider"></li>
                  <li><a href="#">One more separated link</a></li>
                </ul>
              </li>
              <%--<li class="dropdown">
                   <input id="myInput" type="text" placeholder="Buscar">
              </li>--%>
            </ul>
            <form class="navbar-form navbar-left" role="search" autocomplete="off">
              <div class="autocomplete form-group" style="width:300px;">
                <input id="myInput" type="text" placeholder="Buscar">
              </div>      
            </form>
            <ul class="nav navbar-nav navbar-right">
              <%--<li><a href="#">Link</a></li>
              <li class="dropdown">
                <a href="#" class="dropdown-toggle" data-toggle="dropdown">Dropdown <b class="caret"></b></a>
                <ul class="dropdown-menu">
                  <li><a href="#">Action</a></li>
                  <li><a href="#">Another action</a></li>
                  <li><a href="#">Something else here</a></li>
                  <li class="divider"></li>
                  <li><a href="#">Separated link</a></li>
                </ul>
              </li>--%>
            </ul>
            </div><!-- /.navbar-collapse -->
            </div><!-- /.container-fluid -->
          </nav>
        </div>
      </nav>
      <div class="navbar-offset"></div>
      <div id="map">
      </div>
      <div class="row main-row">
        <div class="col-sm-4 col-md-3 sidebar sidebar-left pull-left">
          <div class="panel-group sidebar-body" id="accordion-left">
            <div class="panel panel-default">
              <div class="panel-heading">
                <h4 class="panel-title">
                  <a data-toggle="collapse" href="#layers">
                    <i class="fa fa-list-alt"></i>
                    Mapas
                  </a>
                  <span class="pull-right slide-submenu">
                    <i class="fa fa-chevron-left"></i>
                  </span>
                </h4>
              </div>
              <div id="layers" class="panel-collapse collapse in">
                <div class="panel-body list-group" id="map-style">
                  <a href="#" class="list-group-item" id="dark-matter">
                    <i class="fa fa-globe"></i> Dark
                  </a>
                  <a href="#" class="list-group-item" id="osm-bright">
                    <i class="fa fa-globe"></i> Bright
                  </a>
                  <a href="#" class="list-group-item" id="satellite-osm-bright">
                    <i class="fa fa-globe"></i> Satellite
                  </a>
                </div>
              </div>
            </div>
            <div class="panel panel-default">
              <div class="panel-heading">
                <h4 class="panel-title">
                  <a data-toggle="collapse" href="#properties">
                    <i class="fa fa-list-alt"></i>
                    Equipos Abiertos
                  </a>
                </h4>
              </div>
              <div id="properties" class="panel-collapse collapse in">
                <div class="panel-body">
                  <p>
                  Lorem ipsum dolor sit amet, vel an wisi propriae. Sea ut graece gloriatur. Per ei quando dicant vivendum. An insolens appellantur eos, doctus convenire vis et, at solet aeterno intellegebat qui.
                  </p>
                  <p>
                  Elitr minimum inciderint qui no. Ne mea quaerendum scriptorem consequuntur. Mel ea nobis discere dignissim, aperiam patrioque ei ius. Stet laboramus eos te, his recteque mnesarchum an, quo id adipisci salutatus. Quas solet inimicus eu per. Sonet conclusionemque id vis.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-sm-4 col-md-6 mid"></div>
        <div class="col-sm-4 col-md-3 sidebar sidebar-right pull-right" style:"height:100%">
          <div class="panel-group sidebar-body" id="accordion-right">
            <div class="panel panel-default">
              <div class="panel-heading">
                <h4 class="panel-title">
                  <a data-toggle="collapse" href="#taskpane">
                    <i class="fa fa-tasks"></i>
                    Reclamos
                  </a>
                  <span class="pull-right slide-submenu">
                    <i class="fa fa-chevron-right"></i>
                  </span>
                </h4>
              </div>
              <div id="taskpane" class="panel-collapse collapse in" style:"height:100%">
                <div class="panel-body" style:"height:100%">
                  <div class="listings" id="listings" >
                      <div  class="item">
                          <a class="title">Primer reclamo</a>
                          <div>Descripcion 1</div>
                      </div>
                      <div  class="item">
                          <a class="title">Segundo reclamo</a>
                          <div>Descripcion 2</div>
                      </div>
                      <div  class="item">
                          <a class="title">Tercer reclamo</a>
                          <div>Descripcion 3</div>
                      </div>
                      <div  class="item">
                          <a class="title">Cuarto reclamo</a>
                          <div>Descripcion 4</div>
                      </div>
                  </div>

                  <%--<p>
                  Lorem ipsum dolor sit amet, vel an wisi propriae. Sea ut graece gloriatur. Per ei quando dicant vivendum. An insolens appellantur eos, doctus convenire vis et, at solet aeterno intellegebat qui.
                  </p>
                  <p>
                  Elitr minimum inciderint qui no. Ne mea quaerendum scriptorem consequuntur. Mel ea nobis discere dignissim, aperiam patrioque ei ius. Stet laboramus eos te, his recteque mnesarchum an, quo id adipisci salutatus. Quas solet inimicus eu per. Sonet conclusionemque id vis.
                  </p>
                  <p>
                  Eam vivendo repudiandae in, ei pri sint probatus. Pri et lorem praesent periculis, dicam singulis ut sed. Omnis patrioque sit ei, vis illud impetus molestiae id. Ex viderer assentior mel, inani liber officiis pro et. Qui ut perfecto repudiandae, per no hinc tation labores.
                  </p>
                  <p>
                  Pro cu scaevola antiopam, cum id inermis salutatus. No duo liber gloriatur. Duo id vitae decore, justo consequat vix et. Sea id tale quot vitae.
                  </p>--%>

                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="mini-submenu mini-submenu-left pull-left">
        <i class="fa fa-list-alt"></i>
      </div>
      <div class="mini-submenu mini-submenu-right pull-right">
        <i class="fa fa-tasks"></i>
      </div>
    </div>

    <script type="text/javascript" src="scripts/menu.js"></script>
    <script type="text/javascript" src="scripts/scada.js"></script>
    <script type="text/javascript" src="scripts/search.js"></script>
    <script type="text/javascript" src="scripts/breaker-menu.js"></script>
    <script type="text/javascript" src="scripts/control.js"></script>
</body>

</html>