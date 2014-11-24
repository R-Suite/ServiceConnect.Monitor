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
