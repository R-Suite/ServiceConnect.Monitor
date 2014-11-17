define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/services.html',
    'backgrid'
], function(Backbone, _, $, template, Backgrid) {

    "use strict";

    var view = Backbone.View.extend({

        columns: [{
            name: "Name",
            cell: "string",
            editable: false
        }, {
            Label: "Last Heartbeat",
            name: "LastHeartbeat",
            cell: "string",
            editable: false
        }, {
            name: "InstanceLocation",
            label: "Locations",
            cell: "string",
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    if (!rawValue) {
                        return "";
                    }
                    return rawValue.join(", ");
                }
            })
        }, {
            name: "LatestCpu",
            label: "CPU",
            cell: "string",
            editable: false
        }, {
            name: "LatestMemory",
            label: "Memory",
            cell: "string",
            editable: false
        }, {
            label: "",
            name: "id",
            cell: "html",
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    return "<a href='#services/details/" + rawValue + "' >Details</a>";
                }
            })
        }],

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.grid = new Backgrid.Grid({
                columns: this.columns,
                collection: this.collection
            });
            this.$el.find(".serviceGrid").html(this.grid.render().$el);

            var paginator = new Backgrid.Extension.Paginator({
                collection: this.collection
            });
            this.$el.find(".servicePaginator").html(paginator.render().$el);

            this.$el.find(".backgrid").addClass("table-hover");
            this.$el.find(".backgrid").addClass("table-bordered");

            return this;
        }
    });

    return view;
});
