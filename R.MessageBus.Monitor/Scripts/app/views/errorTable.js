define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/auditTable.html',
    'backgrid',
    'slickgrid',
    'app/views/messageBodyModal',
    'app/views/exceptionModal'
], function(Backbone, _, $, template, Backgrid, Slick, MessageBodyModal, ExceptionModal) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .messageBody": "_showMessageBody",
            "click .exceptionBody": "_showException"
        },

        columns: [{
            id: "TypeName",
            field: "TypeName",
            name: "Message Type"
        }, {
            id: "TimeSent",
            name: "Time Sent",
            field: "TimeSent",
            formatter: function(row, col, val, colDefinition, model) {
                return moment.utc(val).format("DD/MM/YYYY HH:mm:ss.SSS");
            },
            width: 100
        }, {
            id: "SourceAddress",
            field: "SourceAddress",
            name: "Source Address"
        }, {
            id: "DestinationAddress",
            field: "DestinationAddress",
            name: "Destination Address"
        }, {
            id: "ExceptionType",
            field: "Exception",
            name: "Exception Type",
            formatter: function(row, col, val, colDefinition, model) {
                return val.ExceptionType;
            }
        }, {
            id: "Exception",
            field: "Exception",
            name: "Error",
            width: 20,
            formatter: function(row, col, val, colDefinition, model) {
                return "<div style='width: 100%; text-align: center'><i class='fa fa-exclamation-circle exceptionBody' id-attr='" + model.Id + "' style='cursor: pointer; cursor: hand;'></i></div>";
            }
        }, {
            id: "Body",
            field: "Body",
            name: "Body",
            width: 20,
            formatter: function(row, col, val, colDefinition, model) {
                return "<div style='width: 100%; text-align: center'><i class='fa fa-envelope-o messageBody' id-attr='" + model.Id + "' style='cursor: pointer; cursor: hand;'></i></div>";
            }
        }, {
            id: "Route",
            field: "CorrelationId",
            name: "",
            width: 40,
            formatter: function(row, col, val, colDefinition, model) {
                return "<a href='#route/" + val + "' >Route</a>";
            }
        }, {
            id: "Id",
            field: "Id",
            name: "",
            width: 40,
            formatter: function(row, col, val, colDefinition, model) {
                return "<a href='#error/" + val + "' >Details</a>";
            }
        }],

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.data = this.collection.fullCollection.toJSON();
            this._renderGrid();
            $(window).on("resize", this._resizeGrid);
            return this;
        },

        _renderGrid: function() {
            var options = {
                enableColumnReorder: false,
                defaultColumnWidth: 150,
                forceFitColumns: true
            };

            this.grid = new Slick.Grid(".grid", this.data, this.columns, options);
        },

        _showMessageBody: function(e) {
            var id = $(e.currentTarget).attr("id-attr");
            var model = this.collection.fullCollection.findWhere({
                Id: id
            });
            var modal = new MessageBodyModal({
                model: model
            });
            $("body").append(modal.render().$el);
        },

        _showException: function(e) {
            var id = $(e.currentTarget).attr("id-attr");
            var model = this.collection.fullCollection.findWhere({
                Id: id
            });
            var modal = new ExceptionModal({
                model: model
            });
            $("body").append(modal.render().$el);
        },

        removeMessages: function(from) {
            for (var i = this.data.length - 1; i >= 0; i--) {
                var model = this.data[i];
                if (moment.utc(model.TimeSent) < from) {
                    this.data.splice(i, 1);
                } else {
                    break;
                }
            }
            this.grid.updateRowCount();
            this.grid.render();
        },

        refresh: function() {
            while (this.data.length > 0) {
                this.data.pop();
            }
            this.grid.updateRowCount();
            var models = this.collection.fullCollection.toJSON();
            for (var i = 0; i < models.length; i++) {
                this.data.push(models[i]);
            }
            this.grid.updateRowCount();
            this.grid.render();
        },

        addErrors: function() {
            this.refresh();
        },

        _resizeGrid: function() {
            if (this.grid) {
                this.grid.resizeCanvas();
            }
        },

        onClose: function() {
            $(window).off("resize", this._resizeGrid);
        }
    });

    return view;
});
