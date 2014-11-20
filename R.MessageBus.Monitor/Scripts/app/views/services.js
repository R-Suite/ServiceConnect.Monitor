define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/services.html',
    'backgrid',
    "moment"
], function(Backbone, _, $, template, Backgrid, moment) {

    "use strict";

    var view = Backbone.View.extend({

        columns: [{
            label: "",
            name: "Status",
            cell: "html",
            editable: false,
            className: "activeServiceCol",
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    if (rawValue === "Yellow") {
                        return '<i class="fa fa-circle" style="color: #fecd5e;"></i>';
                    } else if (rawValue === "Green") {
                        return '<i class="fa fa-circle" style="color: #a1d26e;"></i>';
                    } else {
                        return '<i class="fa fa-circle" style="color: #fd685c;"></i>';
                    }
                }
            })
        }, {
            name: "Name",
            cell: "string",
            editable: false
        }, {
            Label: "Last Heartbeat",
            name: "LastHeartbeat",
            cell: "string",
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue) {
                    return moment.utc(rawValue).format("DD/MM/YYYY HH:mm:ss");
                }
            })
        }, {
            name: "InstanceLocation",
            label: "Location",
            cell: "string",
            editable: false
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
        }, {
            label: "",
            name: "Name",
            cell: "html",
            className: "detailsCol",
            editable: false,
            formatter: _.extend({}, Backgrid.CellFormatter.prototype, {
                fromRaw: function(rawValue, model) {
                    return "<a href='#endpoint/" + rawValue + "/" + model.get("InstanceLocation") + "' >Details</a>";
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
