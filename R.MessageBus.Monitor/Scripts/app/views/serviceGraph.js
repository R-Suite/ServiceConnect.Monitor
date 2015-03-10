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

define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/serviceGraph.html',
    "highcharts"
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({
        initialize: function() {
            _.bindAll(this);
            this.collection.on("change", this._renderGraph);
            this.collection.on("reset", this._renderGraph);
        },

        render: function() {
            this.$el.html(template);
            this._renderGraph();
            return this;
        },

        refresh: function() {
            this._renderGraph();
        },

        _buildModel: function() {
            this.memoryPoints = [];
            this.cpuPoints = [];

            var that = this;
            _.each(this.collection.last(this.collection.length).reverse(), function(model) {
                var timestamp = moment.utc(model.get("Timestamp")).unix() * 1000;
                that.memoryPoints.push([timestamp, model.get("LatestMemory") / Math.pow(1024, 2)]);
                that.cpuPoints.push([timestamp, model.get("LatestCpu")]);
            });
        },

        _renderGraph: function() {
            this._buildModel();

            this.cpuSeries = null;
            this.memorySeries = null;
            var that = this;

            this.chart = this.$el.find('.serviceGraph').highcharts({
                plotOptions: {
                    series: {
                        marker: {
                            enabled: false
                        }
                    }
                },
                chart: {
                    events: {
                        load: function() {
                            that.cpuSeries = this.series[0];
                            that.memorySeries = this.series[1];
                        }
                    }
                },
                tooltip: {
                    valueDecimals: 2
                },
                credits: {
                    enabled: false
                },
                title: {
                    text: ''
                },
                xAxis: {
                    type: 'datetime'
                },
                yAxis: [{
                    title: {
                        text: 'CPU (%)'
                    }
                }, {
                    title: {
                        text: 'Memory (MB)'
                    },
                    opposite: true
                }],
                series: [{
                    name: 'CPU',
                    data: this.cpuPoints,
                    yAxis: 0,
                    color: "#7CB5EC"
                }, {
                    name: 'Memory',
                    data: this.memoryPoints,
                    yAxis: 1,
                    color: "#FF6600"
                }]
            });

        },

        addHeartbeats: function(heartbeats) {
            for (var i = 0; i < heartbeats.length; i++) {
                var timestamp = moment.utc(heartbeats[i].Timestamp).unix() * 1000;
                this.cpuSeries.addPoint([timestamp, heartbeats[i].LatestCpu], true, true);
                this.memorySeries.addPoint([timestamp, heartbeats[i].LatestMemory / Math.pow(1024, 2)], true, true);
            }
        },

        onClose: function() {
            this.collection.off("change", this._renderGraph);
            this.collection.off("reset", this._renderGraph);
        }
    });

    return view;
});
