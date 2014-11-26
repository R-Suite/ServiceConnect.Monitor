define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/auditTable.html',
    'backgrid',
    'slickgrid'
], function(Backbone, _, $, template, Backgrid, Slick) {

    "use strict";

    var view = Backbone.View.extend({

        columns: [{
            id: "TypeName",
            field: "TypeName",
            name: "Message Type"
        }, {
            id: "TimeSent",
            name: "Time Sent",
            field: "TimeSent",
            formatter: function (row, col, val, colDefinition, model) {
                return moment.utc(val).format("DD/MM/YYYY HH:mm.SSS");
            }
        }, {
            id: "TimeProcessed",
            field: "TimeProcessed",
            name: "Processing Time",
            formatter: function(row, col, val, colDefinition, model) {
                return moment.utc(moment.duration(moment.utc(val).diff(moment.utc(model.TimeReceived))).get("milliseconds")).format("HH:mm:ss.SSS");
            } 
        }, {
            id: "SourceAddress",
            field: "SourceAddress",
            name: "Source Address"
        }, {
            id: "DestinationAddress",
            field: "DestinationAddress",
            name: "Destination Address"
        }, {
            id: "Body",
            field: "Body",
            name: "Message"
        }, {
            id: "Id",
            field: "Id",
            name: "",
            formatter: function (row, col, val, colDefinition, model) {
                return "<a href='#audit/" + val + "' >Details</a>";
            }
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
            var options = {
                enableCellNavigation: true,
                enableColumnReorder: false,
                defaultColumnWidth: 150,
                forceFitColumns: true
            };

            this.grid = new Slick.Grid(".grid", this.collection.toJSON(), this.columns, options);
        },

        refresh: function() {
            this.grid.remove();
            this._renderGrid();
        }
    });

    return view;
});
