//You can calculate directions (using a variety of methods of transportation) by using the DirectionsService object.
var directionsService = new google.maps.DirectionsService();

//Define a variable with all map points.
var _mapPoints = new Array();

//Define a DirectionsRenderer variable.
var _directionsRenderer = '';

//This will give you the map zoom value.
var zoom_option = 6;

//LegPoints is your route points between two locations.
var LegPoints = new Array();

//Google map object
var map;

//InitializeMap() function is used to initialize google map on page load.
function InitializeMap() {

    //DirectionsRenderer() is a used to render the direction
    _directionsRenderer = new google.maps.DirectionsRenderer();

    //Set the your own options for map.
    var myOptions = {
        zoom: zoom_option,
        zoomControl: true,
        center: new google.maps.LatLng(21.7679, 78.8718),
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    //Define the map.
    map = new google.maps.Map(document.getElementById("dvMap"), myOptions);

    //Set the map for directionsRenderer
    _directionsRenderer.setMap(map);

    //Set different options for DirectionsRenderer mehtods.
    //draggable option will used to drag the route.
    _directionsRenderer.setOptions({
        draggable: true
    });

    //Add the doubel click event to map.
    google.maps.event.addListener(map, "dblclick", function (event) {
        var _currentPoints = event.latLng;
        _mapPoints.push(_currentPoints);
        LegPoints.push('');
        getRoutePointsAndWaypoints(_mapPoints);
    });

    //Add the directions changed event to map.
    google.maps.event.addListener(_directionsRenderer, 'directions_changed', function () {
        var myroute = _directionsRenderer.directions.routes[0];
        CreateRoute(myroute);
        zoom_option = map.getZoom();
    });
}

function CreateRoute(myroute) {

    var index = 0;
    if (_mapPoints.length > 10) {
        index = _mapPoints.length - 10;
    }

    for (var i = 0; i < myroute.legs.length; i++) {
        saveLegPoints(myroute.legs[i], index);
        index = index + 1;
    }
}

//Saving the all the legs points between two routes
function saveLegPoints(leg, index) {
    var points = new Array();
    for (var i = 0; i < leg.steps.length; i++) {
        for (var j = 0; j < leg.steps[i].lat_lngs.length; j++) {
            points.push(leg.steps[i].lat_lngs[j]);
        }
    }
    LegPoints[index] = points;
}

//This will draw the more then 10 points route on map.
function drawPreviousRoute(Legs) {
    var segPointValue = new Array();
    for (var i = 0; i < Legs; i++) {
        var innerArry = LegPoints[i];
        for (var j = 0; j < innerArry.length; j++) {
            segPointValue.push(innerArry[j]);
        }
        addPreviousMarker(innerArry[0]);
    }
    var polyOptions = {
        path: segPointValue,
        strokeColor: '#F75C54',
        strokeWeight: 3
    };
    var poly = new google.maps.Polyline(polyOptions);
    poly.setMap(map);
}

//This wil add the marker icon to the route.
function addPreviousMarker(myLatlng) {
    var marker = new google.maps.Marker({
        position: myLatlng,
        icon: "Images/red-circle.png",
        title: ""
    });
    marker.setMap(map);
}

//getRoutePointsAndWaypoints() will help you to pass points and waypoints to drawRoute() function
function getRoutePointsAndWaypoints(Points) {
    if (Points.length <= 10) {
        drawRoutePointsAndWaypoints(Points);
    }
    else {
        var newPoints = new Array();
        var startPoint = Points.length - 10;
        var Legs = Points.length - 10;
        for (var i = startPoint; i < Points.length; i++) {
            newPoints.push(Points[i]);
        }
        drawRoutePointsAndWaypoints(newPoints);
        drawPreviousRoute(Legs);
    }
}

function drawRoutePointsAndWaypoints(Points) {
    //Define a variable for waypoints.
    var _waypoints = new Array();

    if (Points.length > 2) //Waypoints will be come.
    {
        for (var j = 1; j < Points.length - 1; j++) {
            var address = Points[j];
            if (address !== "") {
                _waypoints.push({
                    location: address,
                    stopover: true  //stopover is used to show marker on map for waypoints
                });
            }
        }
        //Call a drawRoute() function
        drawRoute(Points[0], Points[Points.length - 1], _waypoints);
    } else if (Points.length > 1) {
        //Call a drawRoute() function only for start and end locations
        drawRoute(Points[_mapPoints.length - 2], Points[Points.length - 1], _waypoints);
    } else {
        //Call a drawRoute() function only for one point as start and end locations.
        drawRoute(Points[_mapPoints.length - 1], Points[Points.length - 1], _waypoints);
    }
}

//drawRoute() will help actual draw the route on map.
function drawRoute(originAddress, destinationAddress, _waypoints) {
    //Define a request variable for route .
    var _request = '';

    //This is for more then two locatins
    if (_waypoints.length > 0) {
        _request = {
            origin: originAddress,
            destination: destinationAddress,
            waypoints: _waypoints, //an array of waypoints
            optimizeWaypoints: true, //set to true if you want google to determine the shortest route or false to use the order specified.
            travelMode: google.maps.DirectionsTravelMode.DRIVING
        };
    } else {
        //This is for one or two locations. Here noway point is used.
        _request = {
            origin: originAddress,
            destination: destinationAddress,
            travelMode: google.maps.DirectionsTravelMode.DRIVING
        };
    }

    //This will take the request and draw the route and return response and status as output
    directionsService.route(_request, function (_response, _status) {
        if (_status == google.maps.DirectionsStatus.OK) {
            _directionsRenderer.setDirections(_response);
        }
    });
}