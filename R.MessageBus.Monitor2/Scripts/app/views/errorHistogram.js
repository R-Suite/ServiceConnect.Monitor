define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/errorHistogram.html',
    "highcharts"
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({
        initialize: function() {
            _.bindAll(this);
            this.collection.on("change", this._renderGraph);
            this.collection.on("reset", this._renderGraph);
            this.collection.on("remove", this._renderGraph);
            this.collection.on("add", this._renderGraph);
        },

        render: function() {
            this.$el.html(template);
            this._renderGraph();
            return this;
        },

        _buildModel: function() {
            this.x = [];

            var that = this;
            _.each(this.collection.fullCollection.last(this.collection.fullCollection.length).reverse(), function(model) {
                that.x.push(moment.utc(model.get("TimeSent")).unix() * 1000);
            });
        },

        _renderGraph: function() {
            this._buildModel();

            this.chart = this.$el.find('.errorChart').highcharts({
                title: {
                    text: ''
                },
                chart: {
                    type: 'histogram',
                    animation: false
                },
                series: [{
                    data: this.x
                }],
                xAxis: {
                    type: 'datetime',
                    max: moment.utc(this.collection.to).unix() * 1000,
                    min: moment.utc(this.collection.from).unix() * 1000
                },
                legend: {
                    enabled: false
                },
                loading: {
                    showDuration: 0,
                    hideDuration: 0
                }
            });
        },

        refresh: function() {
            this._renderGraph();
        },

        onClose: function() {
            this.collection.off("change", this._renderGraph);
            this.collection.off("remove", this._renderGraph);
            this.collection.off("add", this._renderGraph);
            this.collection.off("reset", this._renderGraph);
        }
    });

    return view;
});
