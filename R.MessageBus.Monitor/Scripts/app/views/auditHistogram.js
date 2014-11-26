define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/auditHistogram.html',
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

            return this;
        },

        refresh: function() {
            //this._renderGraph();
        },

        _buildModel: function() {
            //this.points = [];

            //var that = this;
            //_.each(this.collection.last(this.collection.length).reverse(), function(model) {
            //    var timestamp = moment.utc(model.get("Timestamp")).unix() * 1000;
            //    that.memoryPoints.push([timestamp, model.get("LatestMemory") / Math.pow(1024, 2)]);
            //    that.cpuPoints.push([timestamp, model.get("LatestCpu")]);
            //});
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

        addAudits: function(heartbeats) {
            //for (var i = 0; i < heartbeats.length; i++) {
            //    var timestamp = moment.utc(heartbeats[i].Timestamp).unix() * 1000;
            //    this.cpuSeries.addPoint([timestamp, heartbeats[i].LatestCpu], true, true);
            //    this.memorySeries.addPoint([timestamp, heartbeats[i].LatestMemory / Math.pow(1024, 2)], true, true);
            //}
        },

        onClose: function() {
            this.collection.off("change", this._renderGraph);
            this.collection.off("reset", this._renderGraph);
        }
    });

    return view;
});
