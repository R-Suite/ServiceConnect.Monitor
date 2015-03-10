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
    'bower_components/requirejs-text/text!app/templates/endpoints.html',
    "app/collections/serviceMessages",
    "app/collections/services",
    "app/collections/endpoints",
    "app/views/services",
    "app/views/endpointGraph",
    "app/helpers/timer"
], function(Backbone, _, $, template, ServiceMessagesCollection, ServicesCollection,
    EndpointCollection, ServicesView, EndpointGraphView, Timer) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        initialize: function() {
            _.bindAll(this);
            this.tags = [];
        },

        render: function() {
            this.$el.html(template);

            this.serviceMessagesCollection = new ServiceMessagesCollection();
            this.endpointCollection = new EndpointCollection();
            this.serviceCollection = new ServicesCollection();
            this.serviceCollection.fetch({
                success: this._renderServices
            });
            var promise1 = this.endpointCollection.fetch();
            var promise2 = this.serviceMessagesCollection.fetch();
            $.when(promise1, promise2).done(this._renderEndpointGraph);

            var that = this;
            this.$el.find(".tags").select2({
                multiple: true,
                allowClear: true,
                query: function(query) {
                    $.ajax({
                        url: "/tags",
                        data: {
                            query: query.term
                        },
                        success: function(data) {
                            var results = [];
                            for (var i = 0; i < data.length; i++) {
                                results.push({
                                    id: data[i],
                                    text: data[i]
                                });
                            }
                            query.callback({
                                results: results
                            });
                        }
                    });
                }
            }).on("select2-selecting", function(e, d) {
                that.tags.push(e.object.id);
                that._filterServices();
            }).on("select2-removed", function(e, d) {
                var index;
                $.each(that.tags, function(i, tag) {
                    if (tag === e.val) {
                        index = i;
                    }
                });
                that.tags.splice(index, 1);
                that._filterServices();
            });

            this.timer = new Timer(this._refresh, 5000);
            this.timer.start();
            return this;
        },

        _filterServices: function() {
            var promise1 = this.endpointCollection.fetch({
                data: {
                    tags: this.tags.join()
                },
                dataType: "json"
            });
            var promise2 = this.serviceMessagesCollection.fetch();
            this.serviceCollection.fetch({
                data: {
                    tags: this.tags.join()
                },
                reset: true,
                dataType: "json"
            });
            $.when(promise1, promise2).done(this.endpointGraph.refresh);
        },

        _renderServices: function() {
            if (this.services) {
                this.services.close();
            }
            this.services = new ServicesView({
                collection: this.serviceCollection
            });
            this.renderView(this.services);
            this.$el.find(".services").html(this.services.$el);
        },

        _renderEndpointGraph: function() {
            this.endpointGraph = new EndpointGraphView({
                endpointCollection: this.endpointCollection,
                serviceMessagesCollection: this.serviceMessagesCollection
            });
            this.$el.find(".endpointGraph").html(this.endpointGraph.$el);
            this.renderView(this.endpointGraph);
        },

        _refresh: function() {
            this.serviceCollection.fetch({
                data: {
                    tags: this.tags.join()
                },
                reset: true,
                dataType: "json"
            });
        },

        onClose: function() {
            this.timer.stop();
        }
    });

    return view;
});
