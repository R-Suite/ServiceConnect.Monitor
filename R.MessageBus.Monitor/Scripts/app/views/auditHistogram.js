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
    'bower_components/requirejs-text/text!app/templates/auditHistogram.html',
    "highcharts"
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({
        initialize: function() {
            _.bindAll(this);
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

        refresh: function() {
            this._renderGraph();
        },

        _renderGraph: function() {
            this._buildModel();

            this.chart = this.$el.find('.auditChart').highcharts({
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

        onClose: function() {
            this.collection.off("change", this._renderGraph);
            this.collection.off("remove", this._renderGraph);
            this.collection.off("add", this._renderGraph);
            this.collection.off("reset", this._renderGraph);
        }
    });

    return view;
});
