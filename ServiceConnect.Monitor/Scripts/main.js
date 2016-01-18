//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

require.config({
    urlArgs: "version=0.0.1",
    paths: {
        "jquery": 'bower_components/jquery/dist/jquery',
        "underscore": 'bower_components/underscore/underscore',
        "backbone": 'bower_components/backbone/backbone',
        "bootstrap": 'bower_components/bootstrap/dist/js/bootstrap',
        "toastr": "bower_components/toastr/toastr",
        'backbone.stickit': 'bower_components/backbone.stickit/backbone.stickit',
        'backgrid': 'bower_components/backgrid/lib/backgrid',
        'select2': 'bower_components/select2/select2',
        'backbone-pageable': 'bower_components/backbone-pageable/lib/backbone-pageable',
        'backgrid-filter': 'bower_components/backgrid-filter/backgrid-filter',
        'backgrid-paginator': 'bower_components/backgrid-paginator/backgrid-paginator',
        'moment': "bower_components/moment/moment",
        "vis": "bower_components/vis/dist/vis",
        "datetimepicker": "app/lib/bootstrap-datetimepicker",
        "highchartsMain": "bower_components/highcharts/highcharts",
        "highcharts": "bower_components/highcharts/themes/grid-light",
        'signalr.hubs': "app/lib/signalr.hubs",
        "signalr": "bower_components/signalr/jquery.signalR",
        "slickgrid": "bower_components/slickgrid/slick.grid",
        "jquery.event.drag": "app/lib/jquery.event.drag-2.2",
        "slick.core": "bower_components/slickgrid/slick.core",
        "highcharts-histogram": "app/lib/highcharts-histogram",
        "slick.checkboxselectcolumn": "bower_components/slickgrid/plugins/slick.checkboxselectcolumn",
        "slick.rowselectionmodel": "bower_components/slickgrid/plugins/slick.rowselectionmodel"
    },
    shim: {
        "highcharts-histogram": {
            "deps": [ "highcharts" ]    
        },
        "slick.core": {
            "deps": [ "jquery" ]
        },
        "jquery.event.drag": {
            "deps": ["jquery"]    
        },
        "slickgrid": {
            "deps": [
                "jquery",
                "jquery.event.drag",
                "slick.core"
            ],
            "exports": "Slick"
        },
        "slick.checkboxselectcolumn": {
            "deps": ["jquery", "slickgrid"]
        },
        "slick.rowselectionmodel": {
            "deps": ["jquery", "slickgrid"]
        },
        "highchartsMain": {
            "exports": "Highcharts",
            "deps": ["jquery"]
        },
        "highcharts": {
            "exports": "Highcharts",
            "deps": ["jquery", "highchartsMain"]
        },
        'backbone-pageable': {
            deps: ["backbone", "underscore", "jquery"]
        },
        'backgrid-filter': {
            deps: ["backbone", "underscore", "jquery", "backgrid"]
        },
        'backgrid-paginator': {
            deps: ["backbone", "underscore", "jquery", "backgrid"]
        },
        'select2': {
            deps: ["jquery"]
        },
        'backgrid': {
            deps: ["jquery", "backbone", "underscore"],
            exports: "Backgrid"
        },
        "bootstrap": {
            deps: ["jquery"]
        },
        'jquery': {
            deps: [],
            exports: '$'
        },
        'backbone': {
            deps: ["underscore", "jquery"],
            exports: "Backbone"
        },
        'underscore': {
            exports: "_"
        },
        'backbone.stickit': {
            deps: ["backbone", "underscore", "jquery"]
        },
        'signalr': {
            deps: ["jquery"],
            exports: '$'
        },
        "signalr.hubs": {
            deps: ["signalr"]
        }
    },
    wrapShim: true,
    waitSeconds: 200
});

require(['backbone',
         'jquery',
         'toastr',
         'app/routers/main',
         "highcharts",
         'bootstrap',
         'backgrid-paginator',
         'app/helpers/backbone.extensions',
         'app/helpers/underscore.extensions',
         'app/helpers/backgrid.extensions',
         "vis",
         "datetimepicker",
         "backbone.stickit",
         'signalr',
         "signalr.hubs",
         "slickgrid",
         "highcharts-histogram",
         "slick.checkboxselectcolumn",
         "slick.rowselectionmodel"],
function (Backbone, $, toastr, Router, Highcharts) {
    Backbone.Application = {};
    Backbone.Application.Router = new Router();

    Backbone.Hubs = {
        AuditHub: $.connection.auditHub,
        ErrorHub: $.connection.errorHub,
        HeartbeatHub: $.connection.heartbeatHub
    };
    
    Backbone.Hubs.AuditHub.client.init = function () { };
    Backbone.Hubs.ErrorHub.client.init = function () { };
    Backbone.Hubs.HeartbeatHub.client.init = function () { };
    
    Highcharts.setOptions({
        plotOptions: {
            series: {
                animation: false
            }
        },
        credits: {
            enabled: false
        }
    });

    toastr.options = {
        "closeButton": false,
        "debug": false,
        "positionClass": "toast-bottom-right",
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    $.connection.hub.start().done(function () {
        Backbone.history.start();
    });
});
