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

        columns: function() {
            return [{
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
                    return "<a href='#route/" + val + "' >Session</a>";
                }
            }, {
                id: "Id",
                field: "Id",
                name: "",
                width: 40,
                formatter: function(row, col, val, colDefinition, model) {
                    return "<a href='#error/" + val + "' >Details</a>";
                }
            }];
        },

        initialize: function(options) {
            _.bindAll(this);
            this.onRetryComplete = options.onRetryComplete;
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

            var checkboxSelector = new Slick.CheckboxSelectColumn({
                cssClass: "slick-cell-checkboxsel"
            });
            var columns = this.columns();
            columns.unshift(checkboxSelector.getColumnDefinition());

            this.grid = new Slick.Grid(".grid", this.data, columns, options);

            this.grid.setSelectionModel(new Slick.RowSelectionModel({
                selectActiveRow: false
            }));
            this.grid.registerPlugin(checkboxSelector);
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

        getSelected: function() {
            var data = this.grid.getData();
            var rows = this.grid.getSelectedRows();
            var returnData = [];
            for (var i = 0; i < rows.length; i++) {
                returnData.push(data[rows[i]]);
            }
            return returnData;
        },

        removeMessages: function(from) {
            var dataRemoved = false;
            for (var i = this.data.length - 1; i >= 0; i--) {
                var model = this.data[i];
                if (moment.utc(model.TimeSent) < from) {
                    dataRemoved = true;
                    this.data.splice(i, 1);
                } else {
                    break;
                }
            }

            if (dataRemoved) {
                this.grid.setSelectedRows([]);
                this.grid.updateRowCount();
                this.grid.render();
            }
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
            this.grid.setSelectedRows([]);
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
