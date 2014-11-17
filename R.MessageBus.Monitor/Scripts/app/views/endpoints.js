define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/endpoints.html',
    "app/collections/services",
    "app/collections/serviceMessages",
    "app/views/services",
    "app/views/endpointGraph",
], function(Backbone, _, $, template, ServiceCollection, ServiceMessagesCollection, ServicesView, EndpointGraphView) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            this.serviceCollection = new ServiceCollection();
            var promise1 = this.serviceCollection.fetch({
                success: this._renderServices
            });
            this.serviceMessagesCollection = new ServiceMessagesCollection();
            var promise2 = this.serviceMessagesCollection.fetch();
            $.when(promise1, promise2).done(this._renderEndpointGraph);
            return this;
        },

        _renderServices: function() {
            var services = new ServicesView({
                collection: this.serviceCollection
            });
            this.renderView(services);
            this.$el.find(".services").html(services.$el);
        },

        _renderEndpointGraph: function() {
            var endpointGraph = new EndpointGraphView({
                serviceCollection: this.serviceCollection,
                serviceMessagesCollection: this.serviceMessagesCollection
            });
            this.$el.find(".endpointGraph").html(endpointGraph.$el);
            this.renderView(endpointGraph);
        }
    });

    return view;
});
