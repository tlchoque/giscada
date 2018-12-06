<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="giscada.Default" %>

<!DOCTYPE html>

<html>
<head>
    <title>ola3.mbtiles</title>
    <meta charset="utf-8"/>
    <meta name='viewport' content='initial-scale=1,maximum-scale=1,user-scalable=no' />
    <%--<script src='https://api.tiles.mapbox.com/mapbox-gl-js/v0.48.0/mapbox-gl.js'></script>
    <link href='https://api.tiles.mapbox.com/mapbox-gl-js/v0.48.0/mapbox-gl.css' rel='stylesheet' />--%>
    <script src='scripts/mapbox-gl.js'></script>
    <link href='styles/mapbox-gl.css' rel='stylesheet' />

    <link href='styles/main.css' rel='stylesheet' />
    <link href='styles/search.css' rel='stylesheet' />
    <link href='styles/popup.css' rel='stylesheet' />
    <link href='styles/menu.css' rel='stylesheet' />

    <script src="scripts/jquery-3.3.1.min.js"></script>
    <script src="scripts/jquery.signalR-2.3.0.js"></script>
    
    <%--<script src="/signalr/hubs"></script>--%>
    <script src='<%: ResolveClientUrl("~/signalr/hubs") %>'></script>
    <style>
        body { margin:0; padding:0; }
        #map { position:absolute; top:0; bottom:0; width:100%; }
    </style>
    
</head>
<body>
    <form autocomplete="off" >
        <div class="autocomplete" style="width:300px;">
        <input id="myInput" type="text" name="myCountry" placeholder="Etiqueta">
        </div>
        <input type="submit">
    </form>
    
    <div id="map"></div>
    
    <div id='menu'>
        <input id='dark-matter' type='radio' name='rtoggle' value='dark-matter' checked='checked'>
        <label for='dark-matter'>Dark</label>
        <input id='osm-bright' type='radio' name='rtoggle' value='osm-bright'>
        <label for='osm-bright'>Bright</label>
        <input id='satellite-osm-bright' type='radio' name='rtoggle' value='satellite-osm-bright'>
        <label for='satellite-osm-bright'>Satellite</label>
    </div>

    <script type="text/javascript" src="scripts/scada.js"></script>
    <script type="text/javascript" src="scripts/search.js"></script>
    <script type="text/javascript" src="scripts/menu.js"></script>
    <script type="text/javascript" src="scripts/control.js"></script>
</body>

</html>