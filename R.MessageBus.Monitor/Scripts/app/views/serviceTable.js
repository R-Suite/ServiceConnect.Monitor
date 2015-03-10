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
    'bower_components/requirejs-text/text!app/templates/serviceTable.html',
    'backgrid'
], function(Backbone, _, $, template, Backgrid) {

    "use strict";

    var view = Backbone.View.extend({

        columns: [{
            Label: "Timestamp",
            name: "Timestamp",
            cell: "string",
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    return moment.utc(rawValue).format("DD/MM/YYYY HH:mm:ss");
                }
            })
        }, {
            name: "LatestMemory",
            label: "Memory (mb)",
            cell: "number",
            editable: false,
            decimals: 2,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    return (rawValue / Math.pow(1024, 2)).toFixed(2);
                }
            })
        }, {
            name: "LatestCpu",
            label: "Latest CPU",
            cell: "percent",
            editable: false,
            decimals: 2
        }],

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this._renderGrid();
            return this;
        },

        _renderGrid: function() {
            this.grid = new Backgrid.Grid({
                columns: this.columns,
                collection: this.collection
            });
            this.$el.find(".grid").html(this.grid.render().$el);

            var paginator = new Backgrid.Extension.Paginator({
                collection: this.collection
            });
            this.$el.find(".paginator").html(paginator.render().$el);

            this.$el.find(".backgrid").addClass("table-hover");
            this.$el.find(".backgrid").addClass("table-bordered");
        },

        refresh: function() {
            this.grid.remove();
            this._renderGrid();
        }
    });

    return view;
});
