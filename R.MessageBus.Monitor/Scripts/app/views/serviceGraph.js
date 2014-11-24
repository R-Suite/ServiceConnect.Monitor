define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/serviceGraph.html',
    "c3"
], function(Backbone, _, $, template, c3) {

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
            this.timestamps = [];
            
            var that = this;
            this.collection.each(function(model) {
                that.memoryPoints.push((model.get("LatestMemory") / Math.pow(1024, 2)).toFixed(2));
                that.cpuPoints.push(model.get("LatestCpu").toFixed(2));
                that.timestamps.push(moment.utc(model.get("Timestamp")).unix() * 1000);
            });
        },

        _renderGraph: function() {
            this._buildModel();

            this.cpuSeries = null;
            this.memorySeries = null;
            var that = this;
            
            this.$el.find('#serviceGraph').highcharts({
                chart: {
                    events: {
                        load: function () {
                            that.cpuSeries = this.series[0];
                            that.memorySeries = this.series[1];
                        }
                    }
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
                    yAxis: 1
                }, {
                    name: 'Memory',
                    data: this.memoryPoints,
                    yAxis: 2
                }]
            });
        },
        
        onClose: function() {
            this.collection.off("change", this._renderGraph);
            this.collection.off("reset", this._renderGraph);
        }
    });

    return view;
});
