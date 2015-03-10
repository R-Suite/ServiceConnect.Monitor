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
    'bower_components/requirejs-text/text!app/templates/route.html',
    "app/collections/auditMessages",
    "app/collections/errorMessages",
    "app/views/auditTable",
    "app/views/endpointGraph",
    'backbone-pageable'
], function(Backbone, _, $, template, AuditMessagesCollection, ErrorMessagesCollection, AuditTableView, EndpointGraphView) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        initialize: function(options) {
            _.bindAll(this);
            this.options = options;
        },

        render: function() {
            this.$el.html(template);

            this.auditCollection = new AuditMessagesCollection();
            this.errorCollection = new ErrorMessagesCollection();

            var auditPromise = this.auditCollection.fetch({
                data: {
                    correlationId: this.options.correlationId
                }
            });

            var errorPromise = this.errorCollection.fetch({
                data: {
                    correlationId: this.options.correlationId
                }
            });

            var that = this;
            $.when(auditPromise, errorPromise).done(function() {
                that.collection = new Backbone.PageableCollection(that.auditCollection.fullCollection.toJSON());
                that.collection.add(that.errorCollection.fullCollection.toJSON());
                that._renderViews();
            });

            return this;
        },

        _renderViews: function() {
            var endpointCollection = new Backbone.Collection();
            var serviceMessagesCollection = new Backbone.Collection();

            this.collection.each(function(model) {
                var match = serviceMessagesCollection.findWhere({
                    Out: model.get("SourceAddress"),
                    In: model.get("DestinationAddress"),
                    Type: model.get("TypeName")
                });

                if (match) {
                    match.set({
                        Count: match.get("Count") + 1
                    });
                } else {
                    serviceMessagesCollection.add(new Backbone.Model({
                        Out: model.get("SourceAddress"),
                        In: model.get("DestinationAddress"),
                        Type: model.get("TypeName"),
                        Count: 1
                    }));
                }
            });

            this.collection.each(function(model) {
                var sourceMatch = endpointCollection.findWhere({
                    Name: model.get("SourceAddress")
                });

                if (!sourceMatch) {
                    endpointCollection.add(new Backbone.Model({
                        Name: model.get("SourceAddress")
                    }));
                }

                var destinationMatch = endpointCollection.findWhere({
                    Name: model.get("DestinationAddress")
                });

                if (!destinationMatch) {
                    endpointCollection.add(new Backbone.Model({
                        Name: model.get("DestinationAddress")
                    }));
                }
            });

            this.endpointGraphView = new EndpointGraphView({
                serviceMessagesCollection: serviceMessagesCollection,
                endpointCollection: endpointCollection
            });
            this.$el.find(".endpointGraph").html(this.endpointGraphView.$el);
            this.renderView(this.endpointGraphView);
            this.auditTableView = new AuditTableView({
                collection: this.collection
            });
            for (var i = 0; i < this.auditTableView.columns.length; i++) {
                if (this.auditTableView.columns[i].id === "Route") {
                    this.auditTableView.columns.splice(i, 1);
                    break;
                }
            }
            this.$el.find(".routeTable").html(this.auditTableView.$el);
            this.renderView(this.auditTableView);
        }
    });

    return view;
});
