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
                query: function (query) {
                    $.ajax({
                        url: "/tags",
                        data: {
                            query: query.term
                        },
                        success: function (data) {
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
            }).on("select2-selecting", function (e, d) {
                that.tags.push(e.object.id);
                that._filterServices();
            }).on("select2-removed", function (e, d) {
                var index;
                $.each(that.tags, function (i, tag) {
                    if (tag === e.val) {
                        index = i;
                    }
                });
                that.tags.splice(index, 1);
                that._filterServices();
            }).select2('val', []);

            this.timer = new Timer(this._refresh, 5000);
            this.timer.start();
            return this;
        },
        
        _filterServices: function () {
            var promise1 = this.endpointCollection.fetch({
                data: {
                    tags: this.tags
                }
            });
            var promise2 = this.serviceMessagesCollection.fetch();
            this.serviceCollection.fetch({
                data: {
                    tags: this.tags
                },
                reset: true
            });
            $.when(promise1, promise2).done(this.endpointGraph.refresh);
        },

        _renderServices: function() {
            this.services = new ServicesView({
                collection: this.serviceCollection
            });
            this.renderView(this.services);
            this.$el.find(".services").html(this.services.$el);
        },

        _renderEndpointGraph: function() {
            this.endpointGraph = new EndpointGraphView({
                endpointCollection: this.endpointCollection.fullCollection,
                serviceMessagesCollection: this.serviceMessagesCollection
            });
            this.$el.find(".endpointGraph").html(this.endpointGraph.$el);
            this.renderView(this.endpointGraph);
        },
        
        _refresh: function() {
            this.serviceCollection.fetch({
                data: {
                    tags: this.tags
                },
                reset: true
            });
        },
        
        onClose: function() {
            this.timer.stop();
        }
    });

    return view;
});
